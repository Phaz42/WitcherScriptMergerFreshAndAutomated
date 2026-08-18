using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using static WitcherScriptMerger.Program;

namespace WitcherScriptMerger.Tools;

internal static class QuickBms
{
	internal static string ExePath = Settings.Get("QuickBmsPath");
	internal static string PluginPath = Settings.Get("QuickBmsPluginPath");

	internal static int UnpackFile(string bundlePath, string contentRelativePath, string outputDir)
	{
		if (!ValidateResources(bundlePath))
			return 1;

		if (!Directory.Exists(outputDir))
			_ = Directory.CreateDirectory(outputDir);

		ProcessStartInfo startInfo = BuildStartInfo($"-Y -f \"{contentRelativePath}\" \"{PluginPath}\" \"{bundlePath}\" \"{outputDir}\"");

		using Process bmsProc = new()
		{ StartInfo = startInfo };
		_ = bmsProc.Start();
		string output = bmsProc.StandardError.ReadToEnd();  // QuickBMS prints results to std error, even if successful

		if (output.Contains("- 0 files found", StringComparison.OrdinalIgnoreCase))
		{
			string errorMsg = "Error unpacking bundle content file using QuickBMS.\nIts output is below.";
			int outputStart = output.IndexOf("- filter string", StringComparison.OrdinalIgnoreCase);
			if (outputStart != -1)
			{
				output = output[outputStart..];
				errorMsg += "\n\n" + output;
			}

			_ = MainFrm.uiThreadManager.ShowError(errorMsg);
			return 1;
		}

		return 0;
	}

	internal static string[] GetBundleContentPaths(string bundlePath)
	{
		if (!ValidateResources(bundlePath))
			return null;

		List<string> contentPaths = [];

		ProcessStartInfo startInfo = BuildStartInfo($"-l \"{PluginPath}\" \"{bundlePath}\"");

		using (Process bmsProc = new()
		{ StartInfo = startInfo })
		{
			_ = bmsProc.Start();
			string output = bmsProc.StandardOutput.ReadToEnd() + "\n\n" + bmsProc.StandardError.ReadToEnd();
			int footerPos = output.LastIndexOf("QuickBMS generic", StringComparison.OrdinalIgnoreCase);
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
		return new ProcessStartInfo
		{
			FileName = ExePath,
			Arguments = arguments,
			UseShellExecute = false,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			CreateNoWindow = true
		};
	}
}
