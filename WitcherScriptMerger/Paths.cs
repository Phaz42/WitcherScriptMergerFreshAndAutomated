using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using WitcherScriptMerger.Forms;
using WitcherScriptMerger.Tools;

using static WitcherScriptMerger.Program;

namespace WitcherScriptMerger;

/// <summary>
/// Provides methods and constants for managing file and directory paths.
/// </summary>
internal static class Paths
{
	internal const string TempBundleContent = "tempbundlecontent";
	internal static string MergedBundleContent = "Merged Bundle Content";
	internal static string MergedBundleContentAbsolute = Path.Combine(Environment.CurrentDirectory, MergedBundleContent);
	internal const string Inventory = "MergeInventory.xml";
	internal static string ModScriptBase = Path.Combine("content", "scripts");
	internal static string VanillaScriptBase = Path.Combine("content", "content0", "scripts");
	internal static string BundleBase = "content";
	internal static string GameDirectory => OptionsFrm.GameDirectory;
	internal static string GameExeDX11 => Path.Combine(GameDirectory, "bin", "x64", "witcher3.exe");
	internal static string GameExeDX12 => Path.Combine(GameDirectory, "bin", "x64_dx12", "witcher3.exe");
	internal static string BundlesDirectory => Path.Combine(GameDirectory, BundleBase);
	internal static string DlcDirectory => Path.Combine(GameDirectory, "DLC");
	internal static string ScriptsDirectory => Path.Combine(GameDirectory, VanillaScriptBase);
	internal static string ModsDirectory => Path.Combine(GameDirectory, "Mods");

	/// <summary>
	/// Determines whether the game's vanilla scripts are in the form The Witcher 3 5.x ships them in.
	/// 4.x and older ship every vanilla script as UTF-16LE with a byte order mark; 5.x ships them as UTF-8
	/// without one. Checked on one script both versions ship. A missing or unreadable file counts as not 5.x,
	/// so an older installation always keeps the behaviour it had.
	/// </summary>
	/// <returns><see langword="true"/> if the vanilla scripts are in 5.x's form, <see langword="false"/> otherwise.</returns>
	internal static bool HasRemasterScripts()
	{
		try
		{
			using FileStream stream = File.OpenRead(Path.Combine(ScriptsDirectory, "core", "array.ws"));
			Span<byte> head = stackalloc byte[2];
			return stream.ReadAtLeast(head, head.Length, throwOnEndOfStream: false) == head.Length
				&& !(head[0] == 0xFF && head[1] == 0xFE);
		}
		catch (IOException)
		{
			return false;
		}
		catch (UnauthorizedAccessException)
		{
			return false;
		}
	}

	/// <summary>
	/// Gets the relative path of a file or directory within a base path.
	/// </summary>
	internal static string GetRelativePath(string fullPath, string basePath)
	{
		int startIndex = fullPath.IndexOfIgnoreCase(basePath) + basePath.Length + 1;
		return fullPath[startIndex..];
	}

	/// <summary>
	/// Validates the paths of required dependencies.
	/// </summary>
	/// <returns><see langword="true"/> if all dependency paths are valid, <see langword="false"/> otherwise.</returns>
	internal static bool ValidateDependencyPaths()
	{
		string[] dependencyPaths = [KDiff3.ExePath, QuickBms.ExePath, QuickBms.PluginPath, WccLite.ExePath];

		if (dependencyPaths.All(File.Exists))
			return true;

		using DependencyForm dependencyForm = new();
		return dependencyForm.ShowDialog() == DialogResult.OK;
	}

	/// <summary>
	/// Validates all required directories.
	/// </summary>
	/// <returns><see langword="true"/> if all directories are valid, <see langword="false"/> otherwise.</returns>
	/// <remarks>
	/// The game folder is checked first: validating the Mods directory creates it when missing, which must only happen
	/// inside a real game folder - never beside SM-FAE when no game folder is set, nor in a wrongly chosen folder.
	/// </remarks>
	internal static bool ValidateAllDirectories() =>
		ValidateScriptsDirectory() && ValidateBundlesDirectory() && ValidateModsDirectory();

	/// <summary>
	/// Validates the Mods directory.
	/// </summary>
	/// <returns><see langword="true"/> if the directory is valid, <see langword="false"/> otherwise.</returns>
	internal static bool ValidateModsDirectory()
	{
		if (!Directory.Exists(ModsDirectory))
		{
			_ = Directory.CreateDirectory(ModsDirectory);
			return ValidateModsDirectory();
		}

		return true;
	}

	/// <summary>
	/// Validates the Scripts directory.
	/// </summary>
	/// <returns><see langword="true"/> if the directory is valid, <see langword="false"/> otherwise.</returns>
	internal static bool ValidateScriptsDirectory()
	{
		if (!Directory.Exists(ScriptsDirectory))
		{
			_ = MainFrm.uiThreadManager.ShowMessage(
				$"Can't find the Scripts folder at the expected location: {ScriptsDirectory}\n\n" +
				"This indicates that either the wrong game folder is selected or the game isn't installed correctly. " +
				"Select the correct game folder in Options, verify the game files in Steam, GOG Galaxy or Epic, " +
				"or reinstall the game.");
			return false;
		}

		return true;
	}

	/// <summary>
	/// Validates the Bundles directory.
	/// </summary>
	/// <returns><see langword="true"/> if the directory is valid, <see langword="false"/> otherwise.</returns>
	internal static bool ValidateBundlesDirectory()
	{
		if (!Directory.Exists(BundlesDirectory))
		{
			_ = MainFrm.uiThreadManager.ShowMessage(
				$"Can't find the \"content\" folder at the expected location: {BundlesDirectory}\n\n" +
				"This indicates that either the wrong game folder is selected or the game isn't installed correctly. " +
				"Select the correct game folder in Options, verify the game files in Steam, GOG Galaxy or Epic, " +
				"or reinstall the game.");
			return false;
		}

		return true;
	}

	/// <summary>
	/// Retrieves the path to the merged bundle file.
	/// </summary>
	/// <returns>The path to the merged bundle file, or <see langword="null"/> if not found.</returns>
	internal static string RetrieveMergedBundlePath()
	{
		string mergedModName = RetrieveMergedModName();
		return mergedModName != null ? Path.Combine(ModsDirectory, mergedModName, BundleBase, "blob0.bundle") : null;
	}

	/// <summary>
	/// Retrieves the name of the merged mod.
	/// </summary>
	/// <returns>The name of the merged mod, or <see langword="null"/> if not found or invalid.</returns>
	internal static string RetrieveMergedModName()
	{
		string mergedModName = Settings.Get("MergedModName");
		if (string.IsNullOrWhiteSpace(mergedModName))
		{
			_ = MainFrm.uiThreadManager.ShowMessage("The MergedModName setting isn't configured in the .config file.");
			return null;
		}

		if (mergedModName.Length > 64)
			mergedModName = mergedModName[..64];

		if (!mergedModName.IsAlphaNumeric() || !mergedModName.StartsWith("mod", StringComparison.OrdinalIgnoreCase))
		{
			if (!ConfirmInvalidModName(mergedModName))
				return null;
		}

		return mergedModName;
	}

	/// <summary>
	/// Retrieves the directory of the merged mod.
	/// </summary>
	/// <returns>The directory of the merged mod, or <see langword="null"/> if not found.</returns>
	internal static string RetrieveMergedModDir()
	{
		string modName = RetrieveMergedModName();
		return modName != null ? Path.Combine(ModsDirectory, modName) : null;
	}

	/// <summary>
	/// Confirms with the user if they want to use an invalid mod name.
	/// </summary>
	/// <param name="mergedModName">The invalid mod name.</param>
	/// <returns><see langword="true"/> if the user confirms, <see langword="false"/> otherwise.</returns>
	private static bool ConfirmInvalidModName(string mergedModName)
	{
		return DialogResult.Yes == MainFrm.uiThreadManager.ShowMessage(
			"The Witcher 3 won't load the merged file if the mod name isn't \"mod\" followed by numbers, letters, or underscores."
			+ "\n\nUse this name anyway?\n" + mergedModName
			+ "\n\nTo change the name: Click No, then edit \"MergedModName\" in the .config file.",
			"Warning",
			MessageBoxButtons.YesNo,
			MessageBoxIcon.Exclamation);
	}
}