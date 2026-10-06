using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using WitcherScriptMerger.Inventory;

namespace WitcherScriptMerger.FileIndex;

/// <summary>
/// Problems with the installed mods that SM-FAE can see, whether or not they conflict with another mod:
/// <list type="bullet">
/// <item>on 5.x, a mod's script made for an older game version (GameVersionCheck's measure, as asked before merging) -
/// replacing the game's file, it removes what the Remastered version changed in it, conflict or not;</item>
/// <item>a mod the game cannot load as installed: an empty folder, no content folder, or a Mods folder of its own.</item>
/// </list>
/// Built at the end of each scan (ModFileIndex), on the scan's thread.
/// </summary>
internal sealed class ModHealth
{
	/// <param name="MergedAnyway">The active merge of this file includes this copy: it was merged despite the warning.</param>
	/// <param name="OldMerge">A merged file made against an older game version, by no outdated mod listed here.</param>
	internal sealed record OutdatedScript(string ModName, string RelativePath, GameVersionCheck.Result Result, bool MergedAnyway, bool OldMerge);

	internal sealed record InstallProblem(string ModName, string Problem);

	private readonly Dictionary<(string RelativePath, string ModName), GameVersionCheck.Result> _outdatedByFile = [];

	internal List<OutdatedScript> OutdatedScripts { get; } = [];

	internal List<InstallProblem> InstallProblems { get; } = [];

	/// <summary>The mods with any problem.</summary>
	internal int ModCount =>
		OutdatedScripts.Select(script => script.ModName).Concat(InstallProblems.Select(problem => problem.ModName))
			.Distinct(StringComparer.OrdinalIgnoreCase).Count();

	internal bool HasProblems => OutdatedScripts.Count > 0 || InstallProblems.Count > 0;

	/// <summary>Whether a mod's copy of a script was found made for an older game version - and how far it differs.</summary>
	internal bool IsOutdated(string relativePath, string modName, out GameVersionCheck.Result result) =>
		_outdatedByFile.TryGetValue(Key(relativePath, modName), out result);

	private static (string, string) Key(string relativePath, string modName) => (relativePath.ToUpperInvariant(), modName.ToUpperInvariant());

	/// <summary>
	/// Checks the mods: their folders always, their scripts on 5.x. <paramref name="scripts"/> is every script the scan
	/// found, each with the mods that carry it; <paramref name="merges"/> and <paramref name="mergedModNames"/> tell the
	/// merged mod's files from the mods' own.
	/// </summary>
	internal static ModHealth Check(IEnumerable<string> modDirPaths, IEnumerable<ModFile> scripts, bool isRemaster,
		IReadOnlyCollection<Merge> merges, IReadOnlySet<string> mergedModNames)
	{
		ModHealth health = new();

		foreach (string modDirPath in modDirPaths)
		{
			string problem = InstallProblemOf(modDirPath);
			if (problem != null)
				health.InstallProblems.Add(new InstallProblem(Path.GetFileName(modDirPath), problem));
		}

		// On 4.x nothing is compared, as before 1.0: the measure was made for 5.x, against mods made for 4.04.
		if (isRemaster)
		{
			List<(ModFile Script, string MergedModName, GameVersionCheck.Result Result)> outdatedMerged = [];
			HashSet<(string, string)> mergedAnyway = [];

			foreach (ModFile script in scripts.Where(file => file.Category == Categories.Script))
			{
				string gameFile = script.GetVanillaFile();
				if (!File.Exists(gameFile))
					continue;  // a script of the mod's own: nothing of the game's to lose

				foreach (FileHash mod in script.Mods)
				{
					if (GameVersionCheck.Compare(gameFile, script.GetModFile(mod.Name)) is not { IsOutdated: true } result)
						continue;

					if (mergedModNames.Contains(mod.Name))
						outdatedMerged.Add((script, mod.Name, result));
					else
						health._outdatedByFile[Key(script.RelativePath, mod.Name)] = result;
				}
			}

			// A merged file is outdated because an outdated copy was merged into it - then that mod is the problem, and the
			// report says it is in the active merge - or because the merge was made against an older game: an old merge.
			foreach ((ModFile script, string mergedModName, GameVersionCheck.Result result) in outdatedMerged)
			{
				Merge merge = merges.FirstOrDefault(m =>
					m.MergedModName.EqualsIgnoreCase(mergedModName) && m.RelativePath.EqualsIgnoreCase(script.RelativePath));
				List<string> causes = merge?.Mods.Select(m => m.Name)
					.Where(name => health._outdatedByFile.ContainsKey(Key(script.RelativePath, name))).ToList() ?? [];

				if (causes.Count == 0)
					health.OutdatedScripts.Add(new OutdatedScript(mergedModName, script.RelativePath, result, false, true));
				foreach (string cause in causes)
					_ = mergedAnyway.Add(Key(script.RelativePath, cause));
			}

			foreach (KeyValuePair<(string RelativePath, string ModName), GameVersionCheck.Result> outdated in health._outdatedByFile)
			{
				ModFile script = scripts.First(file => file.RelativePath.EqualsIgnoreCase(outdated.Key.RelativePath));
				string modName = script.Mods.First(mod => mod.Name.EqualsIgnoreCase(outdated.Key.ModName)).Name;
				health.OutdatedScripts.Add(new OutdatedScript(modName, script.RelativePath, outdated.Value, mergedAnyway.Contains(outdated.Key), false));
			}

			health.OutdatedScripts.Sort((a, b) =>
				string.Compare(a.ModName, b.ModName, StringComparison.OrdinalIgnoreCase) is int byMod and not 0
					? byMod
					: string.Compare(a.RelativePath, b.RelativePath, StringComparison.OrdinalIgnoreCase));
		}

		health.InstallProblems.Sort((a, b) => string.Compare(a.ModName, b.ModName, StringComparison.OrdinalIgnoreCase));
		return health;
	}

	// Each of these means the game loads nothing of the mod - or loads it from the wrong place - while it looks installed.
	private static string InstallProblemOf(string modDirPath)
	{
		DirectoryInfo modDir = new(modDirPath);
		string modName = modDir.Name;

		try
		{
			if (!modDir.EnumerateFileSystemInfos("*", SearchOption.AllDirectories).Any(info => info is FileInfo))
				return "Empty folder: the game loads nothing from it.";

			// A mod unpacked with its own Mods folder: Mods\modX\Mods\modX\content.
			if (modDir.EnumerateDirectories().Any(dir => dir.Name.Equals("Mods", StringComparison.OrdinalIgnoreCase)))
				return $"Contains its own Mods folder: installed one level too deep. Move the folders in Mods\\{modName}\\Mods into the game's Mods folder.";

			// A mod unpacked into a folder of the same name: Mods\modX\modX\content - or something that is no mod at all.
			if (!modDir.EnumerateDirectories().Any(dir => dir.Name.Equals("content", StringComparison.OrdinalIgnoreCase)))
				return $"No content folder, so the game loads nothing from it. Probably installed one folder too deep: the content folder belongs directly in Mods\\{modName}.";
		}
		catch (IOException)
		{
			return null;  // unreadable now; the scan reports what it could not read
		}
		catch (UnauthorizedAccessException)
		{
			return null;
		}

		return null;
	}

	/// <summary>How a line of the report is shown: the report window sets headings, the opening line and mod names in bold, details indented.</summary>
	internal enum LineStyle { Text, Heading, Bold, Detail }

	/// <summary>The report the status bar opens: what was found, what it means, and what to do.</summary>
	internal List<(string Text, LineStyle Style)> ReportLines()
	{
		List<(string, LineStyle)> lines = [];
		void Add(string text, LineStyle style = LineStyle.Text) => lines.Add((text, style));

		int mods = ModCount;
		Add($"SM-FAE found problems with {mods} mod{(mods == 1 ? string.Empty : "s")}", LineStyle.Bold);
		Add("The Conflicts and Merges lists only show mods that share a file with another mod; this problem check covers all installed mods.");
		Add(string.Empty);

		if (OutdatedScripts.Count > 0)
		{
			Add("Made for an Older Game Version", LineStyle.Heading);
			Add("These scripts were made for an older game version. Each replaces the game's own script and removes what the "
				+ "Remastered version changed in it, even without a conflict. This can cause script compilation errors or "
				+ "problems in-game. Versions of these mods updated for The Witcher 3 Remastered are recommended.");
			Add("A mod that removes game code on purpose can appear here too; if it supports the Remastered version, it's probably fine.");
			Add(string.Empty);

			foreach (IGrouping<string, OutdatedScript> mod in OutdatedScripts.GroupBy(script => script.ModName, StringComparer.OrdinalIgnoreCase))
			{
				Add(mod.Key, LineStyle.Bold);
				foreach (OutdatedScript script in mod)
				{
					Add($"{script.RelativePath} - missing {script.Result.Describe()}", LineStyle.Detail);
					if (script.MergedAnyway)
						Add("Merged anyway: the active merge includes it, so it affects the game.", LineStyle.Detail);
					if (script.OldMerge)
						Add("An old merge, made against an older game version: delete it and merge again.", LineStyle.Detail);
				}

				Add(string.Empty);
			}
		}

		if (InstallProblems.Count > 0)
		{
			Add("Installed Incorrectly", LineStyle.Heading);
			Add(string.Empty);
			foreach (InstallProblem problem in InstallProblems)
			{
				Add(problem.ModName, LineStyle.Bold);
				Add(problem.Problem, LineStyle.Detail);
				Add(string.Empty);
			}
		}

		while (lines.Count > 0 && lines[^1].Item1.Length == 0)
			lines.RemoveAt(lines.Count - 1);
		return lines;
	}

	/// <summary>The report as plain text, for the clipboard: details indented with spaces.</summary>
	internal string Report() =>
		string.Join(Environment.NewLine, ReportLines().Select(line => line.Style == LineStyle.Detail ? "    " + line.Text : line.Text))
		+ Environment.NewLine;
}
