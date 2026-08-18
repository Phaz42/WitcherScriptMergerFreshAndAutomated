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
		if (ModCount == 0)
		{
			_ = MainFrm.uiThreadManager.ShowMessage("There are no mods in the Mods directory, nothing to do...");
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
				string[] filePaths = Directory.GetFiles(modDirPath, "*", SearchOption.AllDirectories);
				IEnumerable<string> scriptPaths = filePaths.Where(path => ModFile.IsScript(path));
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
