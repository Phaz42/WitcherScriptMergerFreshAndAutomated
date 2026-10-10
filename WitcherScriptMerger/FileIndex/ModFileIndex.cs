using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

using WitcherScriptMerger.Inventory;
using WitcherScriptMerger.Tools;

using static WitcherScriptMerger.Program;

namespace WitcherScriptMerger.FileIndex;

internal class ModFileIndex
{
	internal List<ModFile> Files;

	internal IEnumerable<ModFile> Conflicts => Files.Where(f => f.HasConflict);

	internal bool HasConflict => Files.Any(f => f.HasConflict);

	internal int ModCount { get; private set; }

	internal int ScriptCount { get; private set; }

	internal int XmlCount { get; private set; }

	internal int BundleCount { get; private set; }

	/// <summary>Problems found with the mods at the end of the scan: outdated scripts, mods installed wrongly.</summary>
	internal ModHealth Health { get; private set; } = new();

	internal ModFileIndex() => Files = [];

	internal void BuildAsync(
		bool checkScripts, bool checkXml, bool checkBundles,
		ProgressChangedEventHandler progressHandler,
		RunWorkerCompletedEventHandler completedHandler)
	{
		IEnumerable<string> ignoredModNames = GetIgnoredModNames();
		List<string> modDirPaths = Directory.Exists(Paths.ModsDirectory)
		  ? Directory.GetDirectories(Paths.ModsDirectory, "mod*", SearchOption.TopDirectoryOnly)
			.Where(path => !ignoredModNames.Any(name => name.EqualsIgnoreCase(new DirectoryInfo(path).Name)))
			.ToList()
		  : [];

		if (!Paths.ValidateModsDirectory())
			return;

		ModCount = modDirPaths.Count;

		// For the problem check, read here on the UI thread: which files are the merged mod's, and what each merge holds.
		List<Merge> merges = Program.Inventory?.Merges.ToList() ?? [];
		HashSet<string> mergedModNames = new(merges.Select(merge => merge.MergedModName).Where(name => !string.IsNullOrWhiteSpace(name)), StringComparer.OrdinalIgnoreCase);
		string mergedModSetting = Settings.Get("MergedModName");
		if (!string.IsNullOrWhiteSpace(mergedModSetting))
			_ = mergedModNames.Add(mergedModSetting.Trim());
		if (ModCount == 0)
		{
			_ = MainFrm.uiThreadManager.ShowMessage("There are no mods in the Mods folder: nothing to do.");
		}

		using BackgroundWorker bgWorker = new()
		{
			WorkerReportsProgress = true
		};

		bgWorker.DoWork += (sender, e) =>
		{
			int i = 0;
			ScriptCount = XmlCount = BundleCount = 0;
			foreach (string modDirPath in modDirPaths)
			{
				string modName = Path.GetFileName(modDirPath);

				// Only what the game loads: a mod's files in its content folder, its scripts in content\scripts. A mod
				// installed a folder too deep (Mods\modX\modX\content) or with a Mods folder of its own has its files
				// elsewhere; the game ignores them, and ModFile.GetModFile, which builds the path from these two folders,
				// could not find them to merge. The mod check (ModHealth) reports such a mod instead.
				string contentDir = Path.Combine(modDirPath, Paths.BundleBase);
				string scriptDir = Path.Combine(modDirPath, Paths.ModScriptBase) + Path.DirectorySeparatorChar;
				string[] filePaths = Directory.Exists(contentDir)
					? Directory.GetFiles(contentDir, "*", SearchOption.AllDirectories)
					: [];
				IEnumerable<string> scriptPaths = filePaths.Where(path => ModFile.IsScript(path) && path.StartsWithIgnoreCase(scriptDir));
				IEnumerable<string> xmlPaths = filePaths.Where(path => ModFile.IsXml(path));
				IEnumerable<string> bundlePaths = filePaths.Where(path => ModFile.IsBundle(path));

				ScriptCount += scriptPaths.Count();
				XmlCount += xmlPaths.Count();
				BundleCount += bundlePaths.Count();

				if (checkScripts)
				{
					Files.AddRange(GetModFilesFromPaths(scriptPaths, Categories.Script, modName));
				}

				if (checkXml)
				{
					Files.AddRange(GetModFilesFromPaths(xmlPaths, Categories.Xml, modName));
				}

				if (checkBundles)
				{
					foreach (string bundlePath in bundlePaths)
					{
						string[] contentPaths = QuickBms.GetBundleContentPaths(bundlePath);
						Files.AddRange(GetModFilesFromPaths(contentPaths, Categories.BundleText, modName, bundlePath));
					}
				}

				int progressPct = (int)(++i * 100f / modDirPaths.Count);
				bgWorker.ReportProgress(progressPct, modName);
			}

			bgWorker.ReportProgress(100, "Checking mods for problems");
			Health = ModHealth.Check(modDirPaths, Files, Paths.HasRemasterScripts(), merges, mergedModNames);

			if (checkBundles)
				System.Threading.Thread.Sleep(500);  // Wait for progress bar to fill completely
		};

		bgWorker.RunWorkerCompleted += completedHandler;
		bgWorker.ProgressChanged += progressHandler;
		bgWorker.RunWorkerAsync();
	}

	private List<ModFile> GetModFilesFromPaths(
		IEnumerable<string> filePaths,
		ModFileCategory category,
		string modName, string bundlePath = null)
	{
		List<ModFile> fileList = [];

		foreach (string filePath in filePaths)
		{
			string relPath = category == Categories.Script
				? Paths.GetRelativePath(filePath, Paths.ModScriptBase)
				: category == Categories.Xml
				? Paths.GetRelativePath(filePath, modName)
				: category == Categories.BundleText ? filePath : throw new InvalidOperationException();

			ModFile existingFile = Files.FirstOrDefault(file =>
				file.RelativePath.EqualsIgnoreCase(relPath));

			if (existingFile == null)
			{
				ModFile newFile = bundlePath != null
					? new ModFile(relPath, bundlePath)
					: new ModFile(relPath);
				newFile.Mods.Add(new FileHash { Name = modName });
				fileList.Add(newFile);
			}
			else
			{
				existingFile.Mods.Add(new FileHash { Name = modName });
			}
		}

		return fileList;
	}

	private static IEnumerable<string> GetIgnoredModNames()
	{
		string ignoredNames = Settings.Get("IgnoreModNames");
		return ignoredNames.Split(',')
			.Where(name => !string.IsNullOrWhiteSpace(name))
			.Select(name => name.Trim());
	}
}
