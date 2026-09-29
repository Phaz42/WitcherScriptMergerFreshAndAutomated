using System;
using System.Diagnostics;
using System.IO;

using WitcherScriptMerger.Inventory;

using static WitcherScriptMerger.Program;

namespace WitcherScriptMerger.Tools;

internal static class KDiff3
{
	internal static string ExePath = Program.Settings.Get("KDiff3Path");

	internal static int Run(
		FileMerger.MergeSource source1,
		FileMerger.MergeSource source2,
		FileInfo vanillaFile,
		string outputPath, bool followFileLinks)
	{
		if (!File.Exists(ExePath))
		{
			_ = MainFrm.uiThreadManager.ShowError("Can't find KDiff3 at this location:\n\n" + ExePath, "Missing KDiff3");
			return 1;
		}

		string outputDir = Path.GetDirectoryName(outputPath);

		if (!Directory.Exists(outputDir))
			_ = Directory.CreateDirectory(outputDir);

		bool hasVanillaVersion = vanillaFile != null && vanillaFile.Exists;

		string args = hasVanillaVersion ? "\"" + vanillaFile.FullName + "\"" : "";

		// resolve any simlinked files
		string source1FullName = followFileLinks ? source1.TextFile.ResolveTargetFileFullName() : source1.TextFile.FullName;
		string source2FullName = followFileLinks ? source2.TextFile.ResolveTargetFileFullName() : source2.TextFile.FullName;

		args += $" \"{source1FullName}\" \"{source2FullName}\"";
		args += $" -o \"{outputPath}\"";
		args += " --cs \"WhiteSpace3FileMergeDefault=2\"";
		args += " --cs \"CreateBakFiles=0\"";
		args += " --cs \"LineEndStyle=1\"";
		args += " --cs \"FollowFileLinks=1\"";
		args += " --cs \"FollowDirLinks=1\"";

		// Auto skip KDiff3 info dialogs
		if (Program.Settings.Get<bool>("AutoSkipKDiff3InfoDialogs"))
			args += " --cs \"ShowInfoDialogs=0\"";

		// Make it easier for the user
		args += " --cs \"ShowWhiteSpace=0\"";
		args += " --cs \"ShowWhiteSpaceCharacters=0\"";
		args += " --cs \"EscapeKeyQuits=1\"";

		// On 5.x a merged script takes vanilla's form: UTF-8 without a byte order mark. Unless told otherwise,
		// KDiff3 reads a file without a mark in the local code page, which mangles non-ASCII text as soon as the
		// inputs differ in encoding (a UTF-8 mod beside a UTF-16 one), and picks the output encoding from the inputs.
		// So: UTF-8 is the fallback for unmarked files, UTF-16 is still detected by its mark, and the output is UTF-8.
		// 4.x and older, and bundle content, get exactly the arguments they always had.
		if (outputPath.EndsWithIgnoreCase(".ws") && Paths.HasRemasterScripts())
		{
			args += " --cs \"EncodingForA=UTF-8\" --cs \"EncodingForB=UTF-8\" --cs \"EncodingForC=UTF-8\"";
			args += " --cs \"AutoDetectUnicodeA=1\" --cs \"AutoDetectUnicodeB=1\" --cs \"AutoDetectUnicodeC=1\"";
			args += " --cs \"AutoSelectOutEncoding=0\" --cs \"EncodingForOutput=UTF-8\"";

			// Its own KDiff3 settings file too, beside SM-FAE. KDiff3 saves the settings it was given when its
			// window is closed; saved into the user's own settings file, these would change every later 4.x merge.
			// KDiff3 starts it from its defaults on first use, and whatever the user changes there is kept.
			args += $" --config \"{Path.Combine(AppContext.BaseDirectory, "KDiff3-5x.kdiff3rc")}\"";
		}

		if (hasVanillaVersion)
			args += $" --L1 Vanilla --L2 \"{source1.Name}\" --L3 \"{source2.Name}\"";
		else
			args += $" --L1 \"{source1.Name}\" --L2 \"{source2.Name}\"";

		if (!Settings.Get<bool>("ReviewEachMerge") && hasVanillaVersion)
		{
			if (source1FullName.EqualsIgnoreCase(outputPath)
				&& source2.Hash != null && source2.Hash.IsOutdated)
			{
				_ = MainFrm.uiThreadManager.ShowMessage(
					"You are merging an updated mod file into a merge created with a previous version of the file.\n\n" +
					"You should carefully inspect this merge, because KDiff3's auto-solving behavior KEEPS changes from the previous version of the mod file that have been REMOVED in the new version.",
					"Warning",
					System.Windows.Forms.MessageBoxButtons.OK,
					System.Windows.Forms.MessageBoxIcon.Warning);
			}
			else
			{
				args += " --auto";
			}
		}

		string kdiff3Path = Path.IsPathRooted(ExePath)
			? ExePath
			: Path.Combine(Environment.CurrentDirectory, ExePath);

		Process kdiff3Proc = Process.Start(kdiff3Path, args);
		kdiff3Proc.WaitForExit();

		return kdiff3Proc.ExitCode;
	}
}
