using System;
using System.Diagnostics;
using System.IO;

using static WitcherScriptMerger.Program;

namespace WitcherScriptMerger.Tools;

internal static class WccLite
{
	internal static string ExePath = Program.Settings.Get("WccLitePath");

	internal static int PackBundle(string sourceDir, string outputDir)
	{
		if (!Directory.Exists(sourceDir))
		{
			_ = MainFrm.uiThreadManager.ShowError("Can't find content directory to pack into bundle:\n\n" + sourceDir, "Missing Directory");
			return 1;
		}

		return Run(
			$"pack -dir=\"{sourceDir}\" -outdir=\"{outputDir}\"",
			"Error packing merged content into a new bundle using wcc_lite.\nIts error output is below."
		);
	}

	internal static int GenerateMetadata(string bundleDir)
	{
		return Run(
			$"metadatastore -path=\"{bundleDir}\"",
			"Error generating metadata.store for new merged bundle using wcc_lite.\nIts error output is below."
		);
	}

	internal static int Run(string arguments, string failureMsg)
	{
		if (!File.Exists(ExePath))
		{
			_ = MainFrm.uiThreadManager.ShowError("Can't find wcc_lite at this location:\n\n" + ExePath, "Missing wcc_lite");
			return 1;
		}

		ProcessStartInfo procInfo = new()
		{
			FileName = ExePath,
			Arguments = arguments,
			WorkingDirectory = Path.GetDirectoryName(ExePath),
			UseShellExecute = false,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			CreateNoWindow = true
		};

		using Process wccLiteProc = new()
		{ StartInfo = procInfo };
		_ = wccLiteProc.Start();
		string stdOutput = wccLiteProc.StandardOutput.ReadToEnd().Trim();
		string stdError = wccLiteProc.StandardError.ReadToEnd().Trim();

		string errorMsg = null;
		if (!string.IsNullOrWhiteSpace(stdError))
			errorMsg = stdError;
		else if (stdOutput.EndsWith("Wcc operation failed", StringComparison.OrdinalIgnoreCase))
			errorMsg = stdOutput;
		if (errorMsg != null)
		{
			_ = MainFrm.uiThreadManager.ShowError(failureMsg + "\n\n" + errorMsg);
			return 1;
		}

		return 0;
	}
}
