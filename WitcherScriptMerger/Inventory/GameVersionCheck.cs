using System;
using System.Collections.Concurrent;
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

	/// <summary>What to do with a file some of whose copies look made for another game version.</summary>
	/// <param name="Merge">False: skip the file.</param>
	/// <param name="LeaveOut">The mods whose copies are left out of the merge; empty to merge every copy.</param>
	internal sealed record Decision(bool Merge, IReadOnlySet<string> LeaveOut);

	private static readonly Decision MergeAll = new(true, new HashSet<string>());

	/// <summary>
	/// Asks before a merge when any of the mods' copies looks made for another game version: leave those copies out and
	/// merge the others, merge anyway, or skip the file. Only on 5.x, so 4.x merges behave exactly as before.
	/// </summary>
	/// <param name="mergedModName">The merged mod. Its copy is the existing merge, which the new one continues: it is
	/// reported when outdated, but never left out - leaving it out would overwrite the merge while the inventory still
	/// lists its mods as merged.</param>
	internal static Decision Ask(string relativePath, FileInfo vanillaFile, IReadOnlyCollection<(string ModName, FileInfo Copy)> modCopies,
		string mergedModName)
	{
		if (vanillaFile == null || !vanillaFile.Exists || !Paths.HasRemasterScripts())
			return MergeAll;

		List<string> suspects = [];
		HashSet<string> outdated = new(StringComparer.OrdinalIgnoreCase);
		bool oldMerge = false;
		foreach ((string modName, FileInfo copy) in modCopies)
		{
			if (copy == null || !copy.Exists)
				continue;  // the merge itself reports a missing copy

			if (Compare(vanillaFile.FullName, copy.FullName) is not { IsOutdated: true } result)
				continue;

			if (modName.EqualsIgnoreCase(mergedModName))
			{
				suspects.Add($"{modName} (the existing merge) is missing {result.Describe()}");
				oldMerge = true;
			}
			else
			{
				suspects.Add($"{modName} is missing {result.Describe()}");
				_ = outdated.Add(modName);
			}
		}

		if (suspects.Count == 0)
			return MergeAll;

		// Leaving them out helps only when two or more copies remain: their merge is then loaded instead of every mod's
		// own copy. With one left there is no merge, and the outdated copy may still be the one the load order picks.
		bool one = suspects.Count == 1;
		string them = outdated.Count == 1 ? modCopies.First(copy => outdated.Contains(copy.ModName)).ModName : "these mods";
		bool canLeaveOut = outdated.Count > 0 && modCopies.Count - outdated.Count >= 2;

		// On 5.x a mismatched copy is nearly always made for 4.04, hence "older".
		string text = (one ? "This mod's copy" : "These mods' copies") + $" of {relativePath} "
			+ (one ? "was" : "were") + " made for an older game version:\n\n  "
			+ string.Join("\n  ", suspects)
			+ "\n\nMerging " + (one ? "it" : "them") + " removes those lines, which can cause script compilation errors or "
			+ "problems in-game. " + (one ? "A version" : "Versions") + " updated for The Witcher 3 Remastered "
			+ (one ? "is" : "are") + " recommended."
			+ (oldMerge ? "\n\nThe existing merge was made for an older game version, or includes a copy merged anyway: delete it and merge again." : string.Empty);

		TaskDialogCommandLinkButton leaveOut = new(
			$"Leave out {them} and merge the others",
			"Keeps the Remastered code. " + (outdated.Count == 1 ? $"{them}'s" : "Their") + " changes to this file are not used.");
		TaskDialogCommandLinkButton mergeAnyway = new("Merge anyway", "Not recommended: removes the Remastered changes to this file.");
		TaskDialogCommandLinkButton skip = new("Don't merge this file", "Other files are still merged.");

		TaskDialogPage page = new()
		{
			Caption = "Made for an Older Game Version",
			Heading = "Made for an older game version",
			Text = text,
			Icon = TaskDialogIcon.Warning,
			AllowCancel = true
		};
		if (canLeaveOut)
			page.Buttons.Add(leaveOut);
		page.Buttons.Add(mergeAnyway);
		page.Buttons.Add(skip);

		// The default is never the choice that can break the game.
		page.DefaultButton = canLeaveOut ? leaveOut : skip;

		TaskDialogButton choice = MainFrm.uiThreadManager.ShowTaskDialog(page, () =>
			// Where the task dialog is unavailable, the question as before 1.0: merge anyway, or skip the file.
			DialogResult.Yes == MainFrm.uiThreadManager.ShowMessage(
				text + "\n\nMerge anyway?",
				"Made for an Older Game Version",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Warning,
				MessageBoxDefaultButton.Button2) ? mergeAnyway : skip);

		return choice == leaveOut ? new Decision(true, outdated)
			: choice == mergeAnyway ? MergeAll
			: new Decision(false, outdated);
	}

	/// <summary>How many of the installed game file's meaningful lines a mod's copy lacks, and whether that marks it as made for another game version.</summary>
	internal readonly record struct Result(int Missing, int GameLines)
	{
		internal bool IsOutdated => Missing >= ManyLines || (Missing >= SomeLines && Missing >= LargeShare * GameLines);

		internal string Describe() => string.Format(CultureInfo.InvariantCulture, "{0:N0} of the game's {1:N0} lines", Missing, GameLines);
	}

	// Read once per run of SM-FAE while a file's length and last write time are unchanged: the mod check (ModHealth)
	// compares every script of every mod at each refresh, and most of them have not changed. Kept: the game files' lines,
	// each compared with every mod's copy, and the result of each comparison. A mod's copy is read only to be compared -
	// keeping its lines would hold every mod script in memory for the whole run.
	private static readonly ConcurrentDictionary<FileStamp, HashSet<string>> GameLinesCache = new();
	private static readonly ConcurrentDictionary<(FileStamp Game, FileStamp Mod), Result?> ResultCache = new();

	private readonly record struct FileStamp(string Path, long Length, DateTime LastWriteUtc)
	{
		internal static FileStamp Of(string path)
		{
			FileInfo file = new(path);
			return new FileStamp(file.FullName.ToUpperInvariant(), file.Length, file.LastWriteTimeUtc);
		}
	}

	/// <summary>
	/// Compares a mod's copy with the installed game file. Null when the game file has no meaningful lines, or when either
	/// file cannot be read - then there is nothing to say about the copy.
	/// </summary>
	internal static Result? Compare(string gameFile, string modCopy)
	{
		try
		{
			FileStamp game = FileStamp.Of(gameFile);
			FileStamp mod = FileStamp.Of(modCopy);
			return ResultCache.GetOrAdd((game, mod), _ =>
			{
				HashSet<string> gameLines = GameLinesCache.GetOrAdd(game, __ => MeaningfulLines(gameFile));
				if (gameLines.Count == 0)
					return null;

				HashSet<string> modLines = MeaningfulLines(modCopy);
				return new Result(gameLines.Count(line => !modLines.Contains(line)), gameLines.Count);
			});
		}
		catch (IOException)
		{
			return null;
		}
		catch (UnauthorizedAccessException)
		{
			return null;
		}
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
