using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

using WitcherScriptMerger.Inventory;
using WitcherScriptMerger.UI;

using static WitcherScriptMerger.Program;

namespace WitcherScriptMerger.Tools;

internal static class KDiff3
{
	internal static string ExePath = Program.Settings.Get("KDiff3Path");

	internal static int Run(
		FileMerger.MergeSource source1,
		FileMerger.MergeSource source2,
		FileInfo vanillaFile,
		string outputPath, bool followFileLinks, string relativePath)
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

		bool isRemaster = Paths.HasRemasterScripts();
		bool isXml = outputPath.EndsWithIgnoreCase(".xml");

		// On 5.x a merged XML file takes the form of the vanilla file it is merged against - UTF-8 with a byte order
		// mark for nearly every file, but not all, and a merge converts nothing - or, with no vanilla file, 5.x's usual
		// XML form. The mods' declarations are set to that encoding in temporary copies, never in the mods' own files,
		// so line 1 never conflicts over it and never carries a declaration that does not fit.
		string xmlOutputCodec = null;
		string xmlTempDir = null;
		string source1Arg = source1FullName;
		string source2Arg = source2FullName;
		if (isRemaster && isXml)
		{
			XmlDeclaration.TextForm outputForm = hasVanillaVersion
				? XmlDeclaration.DetectForm(File.ReadAllBytes(vanillaFile.FullName))
				: XmlDeclaration.TextForm.Utf8Bom;
			xmlOutputCodec = XmlDeclaration.KDiff3Codec(outputForm);
			string declaredName = XmlDeclaration.DeclaredName(outputForm, hasVanillaVersion ? vanillaFile.FullName : null);

			xmlTempDir = Path.Combine(Path.GetTempPath(), "SM-FAE-xml-" + Guid.NewGuid().ToString("N"));
			source1Arg = Path.Combine(xmlTempDir, "1", Path.GetFileName(source1FullName));
			source2Arg = Path.Combine(xmlTempDir, "2", Path.GetFileName(source2FullName));
			XmlDeclaration.CopyWithEncoding(source1FullName, source1Arg, declaredName);
			XmlDeclaration.CopyWithEncoding(source2FullName, source2Arg, declaredName);
		}

		args += $" \"{source1Arg}\" \"{source2Arg}\"";
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
		// XML on 5.x reads its inputs the same way, and is written in the form chosen above.
		// 4.x and older get exactly the arguments they always had.
		if (isRemaster && (outputPath.EndsWithIgnoreCase(".ws") || isXml))
		{
			args += " --cs \"EncodingForA=UTF-8\" --cs \"EncodingForB=UTF-8\" --cs \"EncodingForC=UTF-8\"";
			args += " --cs \"AutoDetectUnicodeA=1\" --cs \"AutoDetectUnicodeB=1\" --cs \"AutoDetectUnicodeC=1\"";
			args += $" --cs \"AutoSelectOutEncoding=0\" --cs \"EncodingForOutput={xmlOutputCodec ?? "UTF-8"}\"";

			// Its own KDiff3 settings file too, beside SM-FAE. KDiff3 saves the settings it was given when its
			// window is closed; saved into the user's own settings file, these would change every later 4.x merge.
			// KDiff3 starts it from its defaults on first use, and whatever the user changes there is kept.
			string config = Path.Combine(AppContext.BaseDirectory, "KDiff3-5x.kdiff3rc");
			args += $" --config \"{config}\"";

			// Except its window: from its defaults it opens at about 300 by 240 pixels, so it is given a size.
			if (NeedsWindowSize(config))
				args += FirstWindowArgs();
		}

		if (hasVanillaVersion)
			args += $" --L1 Vanilla --L2 \"{source1.Name}\" --L3 \"{source2.Name}\"";
		else
			args += $" --L1 \"{source1.Name}\" --L2 \"{source2.Name}\"";

		// Without --auto KDiff3 always opens, and why is known here; with it, it opens only for a conflict.
		KDiff3Reason reason = Settings.Get<bool>("ReviewEachMerge") ? KDiff3Reason.Review
			: hasVanillaVersion ? KDiff3Reason.Conflict
			: KDiff3Reason.NoGameFile;
		if (reason == KDiff3Reason.Conflict)
		{
			if (source1FullName.EqualsIgnoreCase(outputPath)
				&& source2.Hash != null && source2.Hash.IsOutdated)
			{
				reason = KDiff3Reason.UpdatedMod;
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

		// The first side is the file merged so far when earlier mods were merged into it already.
		KDiff3GuideContext guide = new(
			relativePath, source1.Name, source2.Name, hasVanillaVersion,
			source1FullName.EqualsIgnoreCase(outputPath) || source1FullName.StartsWithIgnoreCase(Paths.MergedBundleContentAbsolute),
			reason);

		using Process kdiff3Proc = Process.Start(kdiff3Path, args);
		bool windowShown = WaitForExit(kdiff3Proc, guide);
		int exitCode = kdiff3Proc.ExitCode;
		if (windowShown)
			KDiff3Guide.KDiff3Closed();

		if (xmlTempDir != null)
		{
			try
			{ Directory.Delete(xmlTempDir, true); }
			catch (IOException) { }  // a temporary copy left behind costs nothing
			catch (UnauthorizedAccessException) { }
		}

		// On every version, a merged XML file whose declaration names an encoding it is not in is never kept: the
		// normalising above prevents it on 5.x, and this catches whatever else produces one - a user's edit in the
		// KDiff3 window, a mod on 4.x saved in another encoding.
		if (exitCode == 0 && isXml && File.Exists(outputPath))
		{
			string mismatch = XmlDeclaration.FindMismatch(outputPath);
			if (mismatch != null)
			{
				File.Delete(outputPath);
				_ = MainFrm.uiThreadManager.ShowError(
					$"SM-FAE did not keep this merged file, because its XML declaration does not fit its encoding: it {mismatch}.\n\n{outputPath}",
					"Merged File Refused");
				return 2;
			}
		}

		return exitCode;
	}

	// KDiff3 0.9.98 saves its window's size only once the user has moved or resized it; otherwise it writes back its
	// default, 600,400, whatever size it was given (measured). So the size is given until the file holds another one.
	private static bool NeedsWindowSize(string config)
	{
		try
		{
			return !File.Exists(config)
				|| !File.ReadLines(config).Any(line => line.StartsWith("Geometry=", StringComparison.Ordinal) && line.Trim() != "Geometry=600,400");
		}
		catch (Exception e) when (e is IOException or UnauthorizedAccessException)
		{
			return false;  // KDiff3 reads it as it is
		}
	}

	// KDiff3's first window: most of the screen's height, with room on the right for the guide's panel beside it. KDiff3
	// takes both in physical pixels (0.9.98, measured).
	private static string FirstWindowArgs()
	{
		System.Drawing.Rectangle screen = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;
		int margin = screen.Width / 40;
		int panel = Forms.KDiff3GuideForm.PanelWidth * MainFrm.DeviceDpi / 96;
		int width = Math.Clamp(screen.Width - panel - (3 * margin), screen.Width * 6 / 10, screen.Width * 8 / 10);
		int height = screen.Height * 85 / 100;
		int top = screen.Top + ((screen.Height - height) / 2);
		return $" --cs \"Geometry={width},{height}\" --cs \"Position={screen.Left + margin},{top}\"";
	}

	// Waits for KDiff3 to close, and tells the guide when its main window appears: with --auto, only when something is
	// left for the user to decide. Its info dialogs belong to that window, so they are never taken for it (KDiff3 0.9.98).
	private static bool WaitForExit(Process kdiff3, KDiff3GuideContext guide)
	{
		bool windowShown = false;
		while (!kdiff3.WaitForExit(100))
		{
			if (windowShown)
				continue;

			try
			{
				kdiff3.Refresh();
				IntPtr window = kdiff3.MainWindowHandle;
				if (window == IntPtr.Zero)
					continue;

				windowShown = true;
				KDiff3Guide.KDiff3Opened(window, guide);
			}
			catch (InvalidOperationException)
			{
				// It closed between the wait and the look.
			}
		}

		return windowShown;
	}
}
