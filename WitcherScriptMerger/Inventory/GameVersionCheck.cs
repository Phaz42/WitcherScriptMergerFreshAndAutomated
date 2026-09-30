using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using static WitcherScriptMerger.Program;

namespace WitcherScriptMerger.Inventory;

/// <summary>
/// Whether a mod's copy of a file was made for another version of the game than the one installed.
/// <para>
/// A three-way merge takes the installed vanilla file as its base. A copy made from an older vanilla differs from it
/// wherever the game changed, so the merge reads each of those changes as the mod taking it back - and drops it from the
/// result, silently, with no conflict to show. The Witcher 3 5.x changed about 300 scripts and 30 bundled XML files, and
/// most mods made for 4.04 replace whole files.
/// </para>
/// </summary>
internal static class GameVersionCheck
{
	// Measured on real mods against 5.00b: copies made for 5.x lack at most 3 of the installed vanilla file's lines;
	// copies made for 4.04 lack tens to hundreds (r4Player.ws 614 - only 9.5% of it - playerWitcher.ws 564,
	// playerInput.ws 136; a small mountHorse.ws 16 of 79). So many missing lines alone is enough, and on a small file a
	// few lines that are also a large share of it. A mod that deliberately removes vanilla code is asked about too;
	// the question costs a click, a silent loss costs a broken game.
	private const int ManyLines = 50;
	private const int SomeLines = 10;
	private const double LargeShare = 0.10;

	/// <summary>
	/// Asks before a merge when any of the mods' copies looks made for another game version. True to go on: nothing
	/// looked out of place, or the user chose to merge anyway. Only on 5.x, so 4.x merges behave exactly as before.
	/// </summary>
	internal static bool Confirm(string relativePath, FileInfo vanillaFile, IEnumerable<(string ModName, FileInfo Copy)> modCopies)
	{
		if (vanillaFile == null || !vanillaFile.Exists || !Paths.HasRemasterScripts())
			return true;

		HashSet<string> vanillaLines = MeaningfulLines(vanillaFile.FullName);
		if (vanillaLines.Count == 0)
			return true;

		List<string> suspects = [];
		foreach ((string modName, FileInfo copy) in modCopies)
		{
			if (copy == null || !copy.Exists)
				continue;  // the merge itself reports a missing copy

			HashSet<string> modLines = MeaningfulLines(copy.FullName);
			int missing = vanillaLines.Count(line => !modLines.Contains(line));
			if (missing >= ManyLines || (missing >= SomeLines && missing >= LargeShare * vanillaLines.Count))
			{
				suspects.Add(string.Format(CultureInfo.InvariantCulture,
					"{0} is missing {1:N0} of the game's {2:N0} lines", modName, missing, vanillaLines.Count));
			}
		}

		if (suspects.Count == 0)
			return true;

		// On 5.x a mismatched copy is nearly always made for 4.04, hence "older".
		// No is the default: proceeding is the choice that can break the game, so it must be made deliberately.
		bool one = suspects.Count == 1;
		return DialogResult.Yes == MainFrm.uiThreadManager.ShowMessage(
			(one ? "This mod contains" : "These mods contain")
			+ $" a copy of {relativePath} made for an older version of the game:\n\n  "
			+ string.Join("\n  ", suspects)
			+ "\n\nMerging would remove those lines and will likely cause problems, such as compilation errors when the game "
			+ "starts, or things quietly going wrong in-game.\n\n"
			+ "It's recommended not to proceed, and to get "
			+ (one ? "a version of the mod that has" : "versions of these mods that have")
			+ " been updated for The Witcher 3 Remastered.\n\nMerge anyway?",
			"Made for an Older Game Version",
			MessageBoxButtons.YesNo,
			MessageBoxIcon.Warning,
			MessageBoxDefaultButton.Button2);
	}

	// The lines that carry meaning: trimmed, and neither blank, a comment, an XML declaration, nor only punctuation
	// such as a brace - those differ freely between versions and between editors, and would count as missing noise.
	private static HashSet<string> MeaningfulLines(string path)
	{
		HashSet<string> lines = new(StringComparer.Ordinal);
		foreach (string raw in File.ReadAllLines(path))  // detects a UTF-16 or UTF-8 byte order mark; UTF-8 otherwise
		{
			string line = raw.Trim();
			if (line.Length == 0
				|| line.StartsWith("//", StringComparison.Ordinal)
				|| line.StartsWith("/*", StringComparison.Ordinal)
				|| line.StartsWith('*')
				|| line.StartsWith("<!--", StringComparison.Ordinal)
				|| line.StartsWith("<?xml", StringComparison.OrdinalIgnoreCase)
				|| !line.Any(char.IsLetterOrDigit))
			{
				continue;
			}

			_ = lines.Add(line);
		}

		return lines;
	}
}
