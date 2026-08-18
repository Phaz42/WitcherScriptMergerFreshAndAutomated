using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using WitcherScriptMerger.FileIndex;
using WitcherScriptMerger.LoadOrder;
using WitcherScriptMerger.UI;

using static WitcherScriptMerger.Program;

namespace WitcherScriptMerger.Inventory;

internal class InventoryMergeManager
{
	private readonly UIThreadManager uiThreadManager;
	private readonly ProgressManager progressManager;
	private readonly TreeManager treeManager;
	private bool _autoMergeStarted;

	internal InventoryMergeManager(UIThreadManager uiThreadManager, ProgressManager progressManager, TreeManager treeManager)
	{
		this.uiThreadManager = uiThreadManager;
		this.progressManager = progressManager;
		this.treeManager = treeManager;
	}

	internal bool GetIsInventoryInOrder()
	{
		bool mergedModDirExists = Directory.Exists(Paths.RetrieveMergedModDir());
		bool inventoryFileExists = File.Exists(Paths.Inventory);
		int mergeCount = inventoryFileExists ? MergeInventory.GetMergeCount(Paths.Inventory) : 0;

		// Inventory is in order if:
		// - Merged mod directory doesn't exist AND the inventory file also doesn't exist
		// - OR if the merged mod directory exists and the merge count in the inventory is at least 1.
		if ((!mergedModDirExists && !inventoryFileExists) ||
			(mergedModDirExists && inventoryFileExists && mergeCount >= 1))
		{
			return true;
		}

		// If merged mod dir doesn't exist but the inventory exists, delete the inventory.
		if (!mergedModDirExists && inventoryFileExists)
		{
			File.Delete(Paths.Inventory);
			return true;
		}

		// If Merged mod dir exists and inventory is empty/missing, remove old merges.
		return ConfirmRemoveOldMerges() && RemoveOldMerges();
	}

	private bool ConfirmRemoveOldMerges()
	{
		string removeOldMergesMsg = $"Old merges (not registered in the Script Merger inventory) were found " +
			$"in the \"merged mods\" directory:\n\n{Paths.RetrieveMergedModDir()}\n\nOld merges should be removed. " +
			"This is safe to do, unless you have made manual changes in this directory. Regular (non-manual) " +
			"merges will be re-created automatically. " +
			"If you do not remove the old merges, Script Merger will close.\n\n" +
			"Remove the old merges now?";

		return DialogResult.Yes == uiThreadManager.ShowMessage(
			removeOldMergesMsg,
			"Remove old merges?",
			MessageBoxButtons.YesNo,
			MessageBoxIcon.Question);
	}

	private bool RemoveOldMerges()
	{
		if (Paths.RetrieveMergedModDir() is null)
			return false;
#pragma warning disable CA1031 // Do not catch general exception types
		try
		{
			Directory.Delete(Paths.RetrieveMergedModDir(), true);
			return true;
		}
		catch (Exception Ex)
		{
			_ = uiThreadManager.ShowMessage($"Removing old merges failed:\n\n{Ex}\n\n" +
				"Script Merger will now close. Try to remove this directory manually:\n\n" +
				$"{Paths.RetrieveMergedModDir()}", "Removing old merges failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
			return false;
		}
#pragma warning restore CA1031 // Do not catch general exception types
	}

	internal async Task<bool> RefreshMergeInventory()
	{
		progressManager.InitializeProgressScreen("Checking existing Merges and Merged Mod Directory", ProgressBarStyle.Continuous);

		progressManager.lblProgressCurrentAction.Text = "Checking existing Merges and Merged Mod Directory";
		progressManager.progressBar.Value = 10;
		if (!GetIsInventoryInOrder())
			AutoExit(null);

		progressManager.lblProgressCurrentAction.Text = "Loading MergeInventory.xml file";
		Program.Inventory = await Task.Run(() =>
			MergeInventory.Load(Paths.Inventory)
		).ConfigureAwait(true);
		progressManager.progressBar.Value = 25;

		progressManager.lblProgressCurrentAction.Text = "Loading mods.settings file";
		Program.LoadOrder = await Task.Run(() =>
			new CustomLoadOrder()
		).ConfigureAwait(true);
		progressManager.progressBar.Value = 40;

		if (Settings.Get<bool>("ValidateCustomLoadOrder") && Program.Inventory.Merges.Any())
		{
			progressManager.lblProgressCurrentAction.Text = "Validating load order";
			await Task.Run(() =>
				LoadOrderValidator.ValidateAndFix(Program.LoadOrder)
			).ConfigureAwait(true);
		}

		progressManager.progressBar.Value = 65;

		if (Settings.Get<bool>("CheckDuplicatePrios"))
		{
			progressManager.lblProgressCurrentAction.Text = "Checking for duplicate priorities in Load Order";
			if (LoadOrderValidator.CheckForDuplicatePrios(Program.LoadOrder).Count > 0)
			{
				string dupes = string.Empty;
				foreach (KeyValuePair<int, string> mod in LoadOrderValidator.CheckForDuplicatePrios(Program.LoadOrder))
				{
					dupes += $"Prio {mod.Key}: {mod.Value}\r\n";
				}

				string msg = $"Mods with duplicate priorities were found in your mods.settings file. This can cause " +
					$"unexpected behavior and you have no real control over the order those mods are loaded in. " +
					$"Because {AppName} (SM-FAE) has no way to determine which of the duplicates should get the highest priority, " +
					$"you should resolve this manually by editing the following file, before making any priority changes " +
					$"with SM-FAE:\r\n\r\n{Program.LoadOrder.FilePath}\r\n\r\n" +
					$"Do you want to open the mods.settings file now?\r\n\r\n\r\n" +
					$"The mods with duplicate priorities are:\r\n\r\n{dupes}\r\n";

				MessageFrm.Text = "Duplicate mod priorities detected";
				MessageFrm.Message = msg;
				if (uiThreadManager.ShowModal(MessageFrm) == DialogResult.Yes)
				{ _ = TryOpenFile(Program.LoadOrder.FilePath); }
			}
		}

		progressManager.progressBar.Value = 75;

		progressManager.lblProgressCurrentAction.Text = "Refreshing merge tree";
		return await Task.Run(treeManager.RefreshMergeTree).ConfigureAwait(true);
	}

	internal void OnMergeProgressChanged(object sender, ProgressChangedEventArgs e)
	{
		MergeProgressInfo mergeProgress = (MergeProgressInfo)e.UserState;
		progressManager.lblProgressCurrentPhase.Text = mergeProgress.CurrentPhase;
		progressManager.lblProgressCurrentAction.Text = mergeProgress.CurrentAction;
	}

	internal void OnMergeComplete(object sender, RunWorkerCompletedEventArgs e)
	{
		if (Program.Inventory.HasChanged)
		{
			Program.Inventory.Save();
			if (Settings.Get<int>("CheckBundles") < 2)
				treeManager.RefreshTrees(Program.Inventory.BundleChanged);
			else
				treeManager.RefreshTrees(false);
		}
		else
		{
			progressManager.HideProgressScreen();
			uiThreadManager.SetMergeButtonTextIfValidSelection();
		}

		if (sender is FileMerger merger)
		{
			merger.Dispose();
		}
	}

	internal void RepackBundle()
	{
		IEnumerable<string> mergedBundles = Program.Inventory.Merges.Where(merge => merge.IsBundleContent).Select(merge => merge.GetMergedBundle()).Distinct();
		int mergedBundleCount = mergedBundles.Count();
		foreach (string bundlePath in mergedBundles)
		{
			progressManager.InitializeProgressScreen($"Repacking Bundle{mergedBundleCount.GetPluralS()}");

#pragma warning disable CA2000 // Dispose objects before losing scope
			// Disposed in OnMergeComplete
			FileMerger merger = new(Program.Inventory, OnMergeProgressChanged, OnMergeComplete);
#pragma warning restore CA2000 // Dispose objects before losing scope
			merger.RepackBundleAsync(bundlePath);
		}
	}

	internal void MergeFiles()
	{
		if (!IsValidMergeSelection())
			return;

		if (!Paths.ValidateModsDirectory() ||
			(treeManager.ConflictsTree.FileNodes.Any(node => ModFile.IsScript(node.Text)) && !Paths.ValidateScriptsDirectory()) ||
			(treeManager.ConflictsTree.FileNodes.Any(node => ModFile.IsBundle(node.Text)) && !Paths.ValidateBundlesDirectory()))
		{
			return;
		}

		string mergedModName = Paths.RetrieveMergedModName();
		if (mergedModName == null)
			return;

		progressManager.InitializeProgressScreen("Merging");

		Program.Inventory = MergeInventory.Load(Paths.Inventory);
		if (!GetIsInventoryInOrder())
			AutoExit(null);

		bool firstMerge = !Program.Inventory.Merges.Any();
#pragma warning disable CA2000 // Dispose objects before losing scope
		// Disposed in OnMergeComplete
		FileMerger merger = new(Program.Inventory, OnMergeProgressChanged, OnMergeComplete);
#pragma warning restore CA2000 // Dispose objects before losing scope
		IEnumerable<TreeNode> fileNodes = treeManager.ConflictsTree.FileNodes.Where(node => node.GetTreeNodes().Count(modNode => modNode.Checked) > 1);

		merger.MergeByTreeNodesAsync(fileNodes, mergedModName);

		if (firstMerge && Settings.Get<bool>("ValidateCustomLoadOrder"))
		{
			LoadOrderValidator.ValidateAndFix(Program.LoadOrder);
		}
	}

	internal bool IsValidMergeSelection()
	{
		int validFileNodeCount = treeManager.ConflictsTree.FileNodes.Count(node => node.GetTreeNodes().Count(modNode => modNode.Checked) > 1);
		if (validFileNodeCount > 0)
			return true;
		_ = uiThreadManager.ShowMessage("You have not selected any files to merge!",
			"No files selected",
			MessageBoxButtons.OK,
			MessageBoxIcon.Warning);
		return false;
	}

	internal bool IsValidUnMergeSelection()
	{
		int selectedCount = treeManager.MergesTree.FileNodes.Count(node => node.Checked);
		if (selectedCount > 0)
			return true;
		_ = uiThreadManager.ShowMessage("You have not selected any merges to delete!",
			"No files selected",
			MessageBoxButtons.OK,
			MessageBoxIcon.Warning);
		return false;
	}

	internal void CreateAllScriptMerges(bool manual = false)
	{
		if (_autoMergeStarted && !manual)
			return;
		_autoMergeStarted = true;
		TreeNode scriptCatNode = treeManager.ConflictsTree.GetCategoryNode(Categories.Script);

		if (scriptCatNode != null
			&& TreeManager.CheckSubNodesIfVisible(scriptCatNode)
			&& scriptCatNode.AreAllVisibleCheckboxesChecked())
		{
			uiThreadManager.SetMergeButtonTextIfValidSelection();
			MergeFiles();
		}
	}

	internal void ManualDeleteMerges()
	{
		if (!IsValidUnMergeSelection())
			return;

		IEnumerable<TreeNode> fileNodes = treeManager.MergesTree.FileNodes.Where(node => node.Checked);
		DeleteMerges(fileNodes);
	}

	internal void ManualDeleteAllMerges()
	{
		List<TreeNode> fileNodes = treeManager.MergesTree.FileNodes;
		DeleteMerges(fileNodes);
	}

	internal async Task ManualMergesRefresh()
	{
		OptionsFrm.CheckSetGameDir();

		if (Paths.ValidateModsDirectory())
			_ = await RefreshMergeInventory().ConfigureAwait(true);

		progressManager.HideProgressScreen();
	}

	internal void HandleDeletedBundleMerges(List<Merge> bundleMerges)
	{
		IEnumerable<string> affectedBundles = bundleMerges.Select(merge => merge.GetMergedBundle()).Distinct();
		foreach (string bundlePath in affectedBundles)
		{
			progressManager.InitializeProgressScreen("Merge Deleted");

#pragma warning disable CA2000 // Dispose objects before losing scope
			// Disposed in OnMergeComplete
			FileMerger merger = new(Program.Inventory, OnMergeProgressChanged, OnMergeComplete);
#pragma warning restore CA2000 // Dispose objects before losing scope
			merger.RepackBundleAsync(bundlePath);
		}
	}

	#region Deleting Merges

	internal void DeleteMerges(IEnumerable<TreeNode> fileNodes)
	{
		List<Merge> merges = fileNodes.Select(node =>
			Program.Inventory.Merges.First(merge =>
				merge.RelativePath.EqualsIgnoreCase(node.Text)))
				.ToList();
		_ = DeleteMerges(merges);
		if (Program.Inventory.Merges.Count == 0)
		{
			string gameDir = Settings.Get("GameDirectory");
			string mergeModPath = Path.Combine(gameDir, "Mods", Paths.RetrieveMergedModName());
			if (!Directory.Exists(mergeModPath))
			{
				// Folder is gone - nothing to do here.
				return;
			}

			// KDiff must've created a backup which is why the mod folder still exists.
			string[] filePaths = Directory.GetFiles(mergeModPath, "*", SearchOption.AllDirectories);
#pragma warning disable IDE0305 // Simplify collection initialization
			string[] dirPaths = Directory.GetDirectories(mergeModPath, "*", SearchOption.AllDirectories)
				.OrderByDescending(dir => dir.Length).ToArray();
#pragma warning restore IDE0305 // Simplify collection initialization
			string errorMsg = "Merges have been removed successfully, but we were unable to automatically "
							+ $"remove the generated merged mod folder: {mergeModPath}; please remove the folder manually.";
			Array.ForEach(filePaths, filePath =>
			{
#pragma warning disable CA1031 // Do not catch general exception types
				try
				{
					File.Delete(filePath);
				}
				catch
				{
					_ = uiThreadManager.ShowError(errorMsg);
					return;
				}
#pragma warning restore CA1031 // Do not catch general exception types
			});

			Array.ForEach(dirPaths, dirPath =>
			{
#pragma warning disable CA1031 // Do not catch general exception types
				try
				{
					Directory.Delete(dirPath);
				}
				catch
				{
					_ = uiThreadManager.ShowError(errorMsg);
					return;
				}
#pragma warning restore CA1031 // Do not catch general exception types
			});
		}
	}

	internal bool DeleteMerges(List<Merge> merges)
	{
		List<Merge> bundleMerges = [];
		string[] segments = Paths.ScriptsDirectory
		  .Split(Path.DirectorySeparatorChar)
		  .Where((whatever) => !whatever.EqualsIgnoreCase("content0"))
		  .ToArray();
		string stopPath = Path.Combine(segments);
		foreach (Merge merge in merges)
		{
			string mergePath = merge.GetMergedFile();
			if (File.Exists(mergePath))
			{
				File.Delete(mergePath);
				DeleteEmptyDirs(Path.GetDirectoryName(mergePath), stopPath);
			}

			if (merge.IsBundleContent)
			{
				IEnumerable<Merge> mergesForBundle = Program.Inventory.Merges.Where(m =>
				m.IsBundleContent &&
				m.MergedModName.EqualsIgnoreCase(merge.MergedModName) &&
				m.BundleName.EqualsIgnoreCase(merge.BundleName));
				if (mergesForBundle.All(merges.Contains))
				{
					string bundlePath = merge.GetMergedBundle();
					if (File.Exists(bundlePath))
						File.Delete(bundlePath);

					string metadataPath = Path.Combine(Path.GetDirectoryName(bundlePath), "metadata.store");
					if (File.Exists(metadataPath))
						File.Delete(metadataPath);

					DeleteEmptyDirs(Path.GetDirectoryName(bundlePath), stopPath);
				}
				else if (merge.IsBundleContent)
				{
					bundleMerges.Add(merge);
				}
			}

			_ = Program.Inventory.Merges.Remove(merge);
		}

		if (Program.Inventory.HasChanged)
		{
			Program.Inventory.Save();
			if (bundleMerges.Count > 0)
			{
				HandleDeletedBundleMerges(bundleMerges);
				return true;
			}
			// If mod index is null, we haven't refreshed it for the 1st time yet. Don't do it here.
			if (treeManager.ModIndex != null)
			{
				if (Settings.Get<int>("CheckBundles") == 0)
					treeManager.RefreshTrees(Program.Inventory.BundleChanged);
				else
					treeManager.RefreshTrees(false);
			}
		}

		return false;
	}

	internal static void DeleteEmptyDirs(string dirPath, string stopPath)
	{
		if (dirPath.EqualsIgnoreCase(stopPath))
			return;
		DirectoryInfo dirInfo = new(dirPath);
		if (!dirInfo.Exists || dirInfo.GetFiles().Length > 0 || dirInfo.GetDirectories().Length > 0)
			return;
		Directory.Delete(dirPath);
		DeleteEmptyDirs(dirInfo.Parent.FullName, stopPath);
	}

	#endregion
}
