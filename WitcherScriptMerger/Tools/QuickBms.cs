using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using static WitcherScriptMerger.Program;

namespace WitcherScriptMerger.Tools;

internal static class QuickBms
{
	internal static string ExePath = Settings.Get("QuickBmsPath");
	internal static string PluginPath = Settings.Get("QuickBmsPluginPath");

	/// <summary>
	/// Unpacks one file from a bundle. <paramref name="modName"/> is the mod the bundle belongs to, or null for the
	/// game's own bundle; it only changes what the error message tells the user to do.
	/// </summary>
	internal static int UnpackFile(string bundlePath, string contentRelativePath, string outputDir, string modName)
	{
		if (!ValidateResources(bundlePath))
			return 1;

		if (!Directory.Exists(outputDir))
			_ = Directory.CreateDirectory(outputDir);

		ProcessStartInfo startInfo = BuildStartInfo($"-Y -f \"{contentRelativePath}\" \"{PluginPath}\" \"{bundlePath}\" \"{outputDir}\"");

		using Process bmsProc = new()
		{ StartInfo = startInfo };
		_ = bmsProc.Start();
		bmsProc.StandardInput.Close();
		Task<string> stdOutput = bmsProc.StandardOutput.ReadToEndAsync();
		string output = bmsProc.StandardError.ReadToEnd();  // QuickBMS prints results to std error, even if successful
		_ = stdOutput.Result;
		bmsProc.WaitForExit();

		// "0 files found" is not the only failure. A bundle QuickBMS misreads ends in an error with exit code 3, and
		// may leave an empty file behind, without ever printing that line. So the exit code and the file are checked
		// too: merging an empty or missing file would silently drop content from the merge.
		string unpackedFile = Path.Combine(outputDir, contentRelativePath);
		if (output.Contains("- 0 files found", StringComparison.OrdinalIgnoreCase)
			|| bmsProc.ExitCode != 0
			|| !File.Exists(unpackedFile))
		{
			// What a user can act on comes first; QuickBMS's own output, which only helps someone diagnosing it, is
			// collapsed under the details. Whether the game's bundle or a mod's failed decides what the user should do.
			string details = $"QuickBMS exit code {bmsProc.ExitCode}.";
			int outputStart = output.IndexOf("- filter string", StringComparison.OrdinalIgnoreCase);
			if (outputStart != -1)
				details += "\n\n" + output[outputStart..].Trim();

			string bundleName = Path.GetFileName(bundlePath);
			string text = modName == null
				? $"SM-FAE couldn't unpack {contentRelativePath} from the game's files ({bundleName}).\n\n" +
					"Without the game's own copy, this file can't be merged. Nothing has been changed.\n\n" +
					"Your game's files may be damaged or incomplete. Verify the game files in Steam, GOG Galaxy or Epic, then try again."
				: $"SM-FAE couldn't unpack {contentRelativePath} from the mod {modName} ({bundleName}).\n\n" +
					"Without this mod's copy, its changes to this file can't be merged. Nothing has been changed.\n\n" +
					"The mod's bundle may be damaged. Try reinstalling the mod, or ask its author.";

			MainFrm.uiThreadManager.ShowErrorWithDetails("Couldn't unpack a file", text, details, "Couldn't Unpack File");
			return 1;
		}

		return 0;
	}

	internal static string[] GetBundleContentPaths(string bundlePath)
	{
		if (!ValidateResources(bundlePath))
			return [];

		List<string> contentPaths = [];

		ProcessStartInfo startInfo = BuildStartInfo($"-l \"{PluginPath}\" \"{bundlePath}\"");

		using (Process bmsProc = new()
		{ StartInfo = startInfo })
		{
			_ = bmsProc.Start();
			bmsProc.StandardInput.Close();
			Task<string> stdError = bmsProc.StandardError.ReadToEndAsync();
			string output = bmsProc.StandardOutput.ReadToEnd() + "\n\n" + stdError.Result;
			bmsProc.WaitForExit();

			// The listing is everything before QuickBMS's banner. Without the banner, or with a non-zero exit code,
			// QuickBMS did not list the bundle, and what it printed is not a list of paths.
			int footerPos = output.LastIndexOf("QuickBMS generic", StringComparison.OrdinalIgnoreCase);
			if (footerPos < 0 || bmsProc.ExitCode != 0)
			{
				_ = MainFrm.uiThreadManager.ShowError(
					$"QuickBMS could not list the content of this bundle (exit code {bmsProc.ExitCode}):\n\n{bundlePath}",
					"Bundle Not Read");
				return [];
			}

			string[] outputLines = output[..footerPos].Split('\n');
			IEnumerable<string> paths = outputLines
				.Where(line => line.Length > 5)
				.Select(line => line[line.LastIndexOf(' ')..].Trim());
			contentPaths.AddRange(paths);
		}

		return contentPaths.ToArray();
	}

	private static bool ValidateResources(string bundlePath)
	{
		if (!File.Exists(bundlePath))
		{
			_ = MainFrm.uiThreadManager.ShowError("Can't find bundle file:\n\n" + bundlePath, "Missing Bundle");
			return false;
		}

		if (!File.Exists(ExePath))
		{
			_ = MainFrm.uiThreadManager.ShowError("Can't find QuickBMS at this location:\n\n" + ExePath, "Missing QuickBMS");
			return false;
		}

		if (!File.Exists(PluginPath))
		{
			_ = MainFrm.uiThreadManager.ShowError("Can't find QuickBMS plugin at this location:\n\n" + PluginPath, "Missing QuickBMS Plugin");
			return false;
		}

		return true;
	}

	private static ProcessStartInfo BuildStartInfo(string arguments)
	{
		// Standard input is redirected and closed at once: QuickBMS asks on it before opening a file it cannot
		// handle, and with nothing to read it declines instead of waiting for an answer nobody can give.
		return new ProcessStartInfo
		{
			FileName = PreferLargeFileBuild(ExePath),
			Arguments = arguments,
			UseShellExecute = false,
			RedirectStandardInput = true,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			CreateNoWindow = true
		};
	}

	/// <summary>
	/// The Witcher 3 5.x ships bundles over 4 GB (buffers.bundle, movies.bundle). quickbms.exe refuses them and asks
	/// whether to go on anyway; quickbms_4gb_files.exe, shipped beside it, reads them, and on smaller bundles lists
	/// the same paths. A settings file kept from an older version still names quickbms.exe, so the 4 GB build is used
	/// whenever it sits next to the configured one.
	/// </summary>
	private static string PreferLargeFileBuild(string configuredPath)
	{
		if (string.IsNullOrEmpty(configuredPath) || !Path.GetFileName(configuredPath).EqualsIgnoreCase("quickbms.exe"))
			return configuredPath;

		string largeFileBuild = Path.Combine(Path.GetDirectoryName(configuredPath) ?? string.Empty, "quickbms_4gb_files.exe");
		return File.Exists(largeFileBuild) ? largeFileBuild : configuredPath;
	}
}
