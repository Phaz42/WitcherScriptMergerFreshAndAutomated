using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using WitcherScriptMerger.Controls;
using WitcherScriptMerger.Events;
using WitcherScriptMerger.FileIndex;
using WitcherScriptMerger.Inventory;
using WitcherScriptMerger.LoadOrder;
using WitcherScriptMerger.Theming;

using static WitcherScriptMerger.Controls.SMTree;
using static WitcherScriptMerger.Program;

namespace WitcherScriptMerger.UI;

internal class TreeManager
{
	internal ConflictTree ConflictsTree { get; }
	internal MergeTree MergesTree { get; }
	private UIThreadManager UIThreadManager { get; }
	private ProgressManager ProgressManager { get; }

	internal ModFileIndex ModIndex;

	private bool _manualConflictsRefreshRunning;

	private TreeNode _treConflictsPrevNode, _treMergesPrevNode;
	private TreeNode _prevToolTipNode;

	private List<TreeNodeColorFader> _treConflictsColorFaders = [];
	private List<TreeNodeColorFader> _treMergesColorFaders = [];

	private readonly ToolTip treConflictsToolTip;

	internal Action<bool> CreateAllScriptMergesDelegate { get; set; }
	internal Func<Task<bool>> RefreshMergeInventoryDelegate { get; set; }
	internal Func<List<Merge>, bool> DeleteMergesDelegate { get; set; }

	internal event EventHandler<InvokeRequiredEventArgs> InvokeRequired;

	internal TreeManager(ConflictTree treConflicts, MergeTree treMerges, UIThreadManager uiThreadManager, ProgressManager progressManager, ToolTip treConflictsToolTip)
	{
		ConflictsTree = treConflicts;
		MergesTree = treMerges;
		UIThreadManager = uiThreadManager;
		ProgressManager = progressManager;
		this.treConflictsToolTip = treConflictsToolTip;
	}

	/// <summary>
	/// Refreshes both the conflicts and merges trees.
	/// </summary>
	internal async void RefreshTrees(bool checkBundles)
	{
		// Validate required directories
		if (!Paths.ValidateModsDirectory() || (Settings.Get<bool>("CheckScripts") && !Paths.ValidateScriptsDirectory()) || (checkBundles && !Paths.ValidateBundlesDirectory()))
			return;

		// Refresh merge inventory if necessary
		if (Program.Inventory == null)
		{
			_ = await RefreshMergeInventoryDelegate().ConfigureAwait(true);
		}
		else
		{
			ProgressManager.InitializeProgressScreen("Loading Merges");
			Program.LoadOrder.Refresh();
			_ = RefreshMergeTree();
		}

		RefreshConflictsTree(checkBundles);
	}

	/// <summary>
	/// Refreshes the conflicts tree with the specified option to check bundles.
	/// </summary>
	internal void RefreshConflictsTree(bool checkBundles)
	{
#if DEBUG
		checkBundles = false; // Force checkBundles to be false in debug mode
#endif

		if (Settings.Get<int>("CheckBundles") == 0 || (Settings.Get<int>("CheckBundles") == 1 && checkBundles))
			ProgressManager.lblBundleScanInfo.Visible = true;

		ProgressManager.InitializeProgressScreen("Detecting Conflicts", ProgressBarStyle.Continuous);
		ProgressManager.lblStatusLeft1.Text = "Refreshing...";
		ProgressManager.lblStatusLeft2.Visible = ProgressManager.lblStatusLeft3.Visible = false;

		ConflictsTree.BeginUpdate();

		// Clear tree nodes or update specific nodes based on conditions
		if (OptionsFrm.BundleScanOptionChanged || (Program.Inventory.ScriptsChanged && Program.Inventory.BundleChanged))
		{
			ConflictsTree.Nodes.Clear();
		}
		else
		{
			List<TreeNode> nodesToUpdate = [];

			// Add category nodes to update
			TreeNode scriptCatNode = ConflictsTree.GetCategoryNode(Categories.Script);
			if (scriptCatNode != null)
				nodesToUpdate.Add(scriptCatNode);

			TreeNode xmlCatNode = ConflictsTree.GetCategoryNode(Categories.Xml);
			if (xmlCatNode != null)
				nodesToUpdate.Add(xmlCatNode);

			if (checkBundles)
			{
				TreeNode bundleTextCatNode = ConflictsTree.GetCategoryNode(Categories.BundleText);
				if (bundleTextCatNode != null)
					nodesToUpdate.Add(bundleTextCatNode);

				TreeNode bundleNotMergeableCatNode = ConflictsTree.GetCategoryNode(Categories.BundleNotMergeable);
				if (bundleNotMergeableCatNode != null)
					nodesToUpdate.Add(bundleNotMergeableCatNode);
			}

			// Add missing file nodes to update
			IEnumerable<TreeNode> missingFileNodes = ConflictsTree.FileNodes.Where(node =>
				node.GetTreeNodes().Any(modNode =>
					!File.Exists(modNode.GetMetadata().FilePath)
				)
			);
			nodesToUpdate.AddRange(missingFileNodes);

			// Remove and update nodes
			foreach (TreeNode node in nodesToUpdate)
				ConflictsTree.Nodes.Remove(node);

			// Hack-fix for bug: Empty category remained on refresh after resolving conflicts outside of SM
			foreach (TreeNode catNode in ConflictsTree.CategoryNodes)
			{
				if (catNode.Nodes.Count == 0)
					ConflictsTree.Nodes.Remove(catNode);
			}
		}

		// Build mod index asynchronously
		ModIndex = new ModFileIndex();
		ModIndex.BuildAsync(Settings.Get<bool>("CheckScripts"), Settings.Get<bool>("CheckXmlFiles"),
			checkBundles, OnRefreshConflictsProgressChanged, OnRefreshConflictsComplete);
	}

	/// <summary>
	/// Refreshes the merge tree.
	/// </summary>
	/// <returns><see langword="true"/> if the tree was refreshed due to deleted merges, <see langword="false"/> otherwise.</returns>
	internal bool RefreshMergeTree()
	{
		// Clear merge tree nodes
		InvokeRequired?.Invoke(this, new InvokeRequiredEventArgs(delegate
		{
			ConflictsTree.BeginUpdate();
			MergesTree.Nodes.Clear();
		}));

		bool changed = false;
		List<Merge> bundleMergesPruned = [];
		List<Merge> mergesToDelete = [];

		// Iterate through merges to check for missing files, disabled mods, or outdated hashes
		for (int i = Program.Inventory.Merges.Count - 1; i >= 0; --i)
		{
			Merge merge = Program.Inventory.Merges[i];

			// Check for missing merge file
			if (!File.Exists(merge.GetMergedFile()) && ConfirmPruneMissingMergeFile(merge))
			{
				Program.Inventory.Merges.RemoveAt(i);
				changed = true;

				if (merge.IsBundleContent)
					bundleMergesPruned.Add(merge);
				continue;
			}

			bool willDelete = false;

			// Check for missing or disabled mods, or outdated hashes
			foreach (FileHash mod in merge.Mods)
			{
				string modFilePath = merge.GetModFile(mod.Name);

				// Check for missing mod file
				if (!File.Exists(modFilePath) && ConfirmDeleteMergeForMissingMod(merge, mod.Name))
				{
					willDelete = true;
					break;
				}

				// Check for disabled mod
				ModLoadSetting modLoadSetting = Program.LoadOrder.GetModLoadSettingByName(mod.Name);
				if (modLoadSetting != null && !modLoadSetting.IsEnabled.Value && ConfirmDeleteMergeForDisabledMod(merge, mod.Name))
				{
					willDelete = true;
					break;
				}

				// Check for outdated hash
				string latestHash = Tools.Hasher.ComputeHash(modFilePath);
				if (latestHash != null && mod.Hash != latestHash)
				{
					mod.IsOutdated = true;

					// Auto delete old merges
					if (Settings.Get<bool>("AutoDeleteOldMerges"))
					{
						willDelete = true;
						break;
					}

					// Prompt to delete for changed hash
					if (Settings.Get<bool>("ValidateMergeSources"))
					{
						DialogResult choice = PromptToDeleteForChangedHash(merge, modFilePath, mod.Name);
						if (choice == DialogResult.Yes)
						{
							willDelete = true;
							break;
						}
						else if (choice == DialogResult.Cancel) // Never
						{
							Settings.Set("ValidateMergeSources", false);
							Settings.Save();
						}
					}
				}
			}

			if (willDelete)
			{
				mergesToDelete.Add(merge);
				continue;
			}

			// Add merge node to tree
			InvokeRequired?.Invoke(this, new InvokeRequiredEventArgs(delegate
			{
				TreeNode fileNode = new()
				{
					Text = merge.RelativePath,
					ForeColor = ThemeManager.CurrentTheme.ForeBrightColor,
					Tag = new NodeMetadata
					{
						FilePath = merge.GetMergedFile(),
						ModFile = merge
					}
				};

				TreeNode categoryNode = MergesTree.GetCategoryNode(merge.Category);
				if (categoryNode == null)
				{
					categoryNode = new TreeNode
					{
						Text = merge.Category.DisplayName,
						ToolTipText = merge.Category.ToolTipText,
						Tag = merge.Category
					};
					_ = MergesTree.Nodes.Add(categoryNode);
				}

				_ = categoryNode.Nodes.Add(fileNode);
				foreach (FileHash mod in merge.Mods)
				{
					_ = fileNode.Nodes.Add(
						new TreeNode
						{
							Text = mod.Name,
							ForeColor = ThemeManager.CurrentTheme.ForeDimmedColor,
							Tag = new NodeMetadata
							{
								FilePath = merge.GetModFile(mod.Name),
								FileHash = mod,
								ModFile = merge
							}
						}
					);
				}
			}));
		}

		ThemeManager.AttachColorFaderToTreeNodes(MergesTree, out _treMergesColorFaders);

		// Sort and expand merge tree nodes
		InvokeRequired?.Invoke(this, new InvokeRequiredEventArgs(delegate
		{
			MergesTree.Sort();
			MergesTree.ExpandAll();
			MergesTree.ScrollToTop();
			ConflictsTree.EndUpdate();
		}));

		// Delete merges if necessary
		if (mergesToDelete.Any())
		{
			if (DeleteMergesDelegate(mergesToDelete))
				return true;
		}

		if (changed)
		{
			Program.Inventory.Save();
			if (bundleMergesPruned.Any())
				return DeleteMergesDelegate(bundleMergesPruned);
		}

		// Update UI elements
		InvokeRequired?.Invoke(this, new InvokeRequiredEventArgs(delegate
		{
			foreach (TreeNode modNode in MergesTree.ModNodes)
				modNode.SetIsCheckBoxVisible(false);

			ProgressManager.UpdateStatusText(ModIndex);
			UIThreadManager.SetUnmergeButtonTextIfValidSelection();

			ProgressManager.progressBar.Value = 100;
		}));

		return false;
	}

	/// <summary>
	/// Performs a manual refresh of the conflicts tree.
	/// </summary>
	internal void ManualConflictsRefresh()
	{
		// Validate game directory
		if (string.IsNullOrWhiteSpace(Paths.GameDirectory) || !Directory.Exists(Paths.GameDirectory))
		{
			_ = UIThreadManager.ShowMessage(
				"Please set your 'The Witcher 3 Wild Hunt' game directory in the Options screen.");
			return;
		}

		_manualConflictsRefreshRunning = true;

		// Refresh trees with or without bundle checking based on settings
		if (Settings.Get<int>("CheckBundles") < 2)
			RefreshTrees(true);
		else
			RefreshTrees(false);
	}

	/// <summary>
	/// Updates the progress bar and label with the current progress of the conflicts refresh operation.
	/// </summary>
	private void OnRefreshConflictsProgressChanged(object sender, ProgressChangedEventArgs e)
	{
		ProgressManager.progressBar.Value = e.ProgressPercentage;
		ProgressManager.lblProgressCurrentAction.Text = e.UserState as string;

		TaskbarProgress.SetState(ProgressManager.Handle, TaskbarProgress.TaskbarStates.Normal);
		TaskbarProgress.SetValue(ProgressManager.Handle, e.ProgressPercentage, 100);
	}

	/// <summary>
	/// Finalizes the conflicts refresh operation, populates the conflicts tree with detected conflicts,
	/// and updates the UI accordingly. This includes sorting, expanding, and styling the tree nodes,
	/// updating status labels, and potentially triggering auto-creation of merges and auto-exit.
	/// </summary>
	private void OnRefreshConflictsComplete(object sender, RunWorkerCompletedEventArgs e)
	{
		ProgressManager.lblBundleScanInfo.Visible = false;

		if (ModIndex.HasConflict)
		{
			// Add conflict nodes to the tree
			foreach (ModFile conflict in ModIndex.Conflicts)
			{
				if (Program.Inventory.HasResolvedConflict(conflict))
					continue;

				TreeNode fileNode = ConflictsTree.FileNodes.FirstOrDefault(node =>
					node.Text.EqualsIgnoreCase(conflict.RelativePath));

				if (fileNode == null)
				{
					fileNode = new TreeNode
					{
						Text = conflict.RelativePath,
						ForeColor = ThemeManager.CurrentTheme.ForeBrightColor,
						Tag = new NodeMetadata
						{
							FilePath = conflict.Category == Categories.Script || conflict.Category == Categories.Xml
								? conflict.GetVanillaFile()
								: conflict.RelativePath,
							ModFile = conflict
						}
					};

					TreeNode categoryNode = ConflictsTree.GetCategoryNode(conflict.Category);
					if (categoryNode == null)
					{
						Color foreColor = Settings.Get<bool>("AccentColor")
							? ThemeManager.CurrentTheme.AcceptButtonBackColor
							: ThemeManager.DefaultAccentColor;
						categoryNode = new TreeNode
						{
							Text = conflict.Category.DisplayName,
							ToolTipText = conflict.Category.ToolTipText,
							NodeFont = new Font("Segoe UI", 14.0F, FontStyle.Bold),
							ForeColor = foreColor,
							Tag = conflict.Category
						};
						_ = ConflictsTree.Nodes.Add(categoryNode);
					}

					_ = categoryNode.Nodes.Add(fileNode);
				}

				Merge merge = Program.Inventory.Merges.FirstOrDefault(mrg =>
					mrg.RelativePath.EqualsIgnoreCase(conflict.RelativePath));
				foreach (FileHash mod in conflict.Mods)
				{
					FileHash mergeModHash = merge?.Mods.FirstOrDefault(m => m.Name.EqualsIgnoreCase(mod.Name));
					if (mergeModHash != null && !mergeModHash.IsOutdated)
						continue;

					TreeNode modNode = fileNode.GetTreeNodes().FirstOrDefault(node =>
						node.Text.EqualsIgnoreCase(mod.Name));

					if (modNode == null)
					{
						modNode = new TreeNode
						{
							Text = mod.Name,
							Tag = new NodeMetadata
							{
								FilePath = conflict.GetModFile(mod.Name),
								FileHash = mergeModHash,
								ModFile = conflict
							}
						};
						_ = fileNode.Nodes.Add(modNode);
					}
				}
			}

			// Configure conflict tree nodes
			ConflictsTree.Sort();
			ConflictsTree.ExpandAll();
			ConflictsTree.Select();
			foreach (TreeNode catNode in ConflictsTree.CategoryNodes)
			{
				if (!(catNode.Tag as ModFileCategory).IsSupported)
				{
					catNode.SetIsCheckBoxVisible(false, true);
					if (Settings.Get<bool>("CollapseNotMergeable"))
						catNode.Collapse();
				}
			}

			ConflictsTree.SetStylesForCustomLoadOrder();

			foreach (TreeNode fileNode in ConflictsTree.FileNodes)
			{
				if (Settings.Get<bool>("CollapseCustomLoadOrder"))
					fileNode.Collapse();
			}
		}

		// Finalize and update UI
		_ = ConflictsTree.CollapseIdenticalScriptNodesInTreeView();
		ConflictsTree.EndUpdate();
		ConflictsTree.ScrollToTop();
		ProgressManager.UpdateStatusText(ModIndex);
		ProgressManager.HideProgressScreen();
		UIThreadManager.SetMergeButtonTextIfValidSelection();

		ThemeManager.AttachColorFaderToTreeNodes(ConflictsTree, out _treConflictsColorFaders);

		// Auto Create Merges
		if (Settings.Get<bool>("AutoCreateScriptMerges"))
			CreateAllScriptMergesDelegate?.Invoke(false);

		// Show/hide "no conflicts" image if enabled
		UIThreadManager.ShowHideNoConflictsWitcher();

		// Auto Quit
		if (!_manualConflictsRefreshRunning)
		{
			if (Settings.Get<bool>("AutoExit"))
				UIThreadManager.ExitIfNoMoreConflicts();
		}
	}

	/// <summary>
	/// Recursively checks if the specified node or any of its sub-nodes are visible and checkable.
	/// If a visible and checkable node is found, it is checked.
	/// </summary>
	/// <param name="baseNode">The base node to start checking from.</param>
	/// <returns>
	/// <see langword="true"/> if a visible and checkable node is found in the hierarchy, 
	/// <see langword="false"/> otherwise.
	/// </returns>
	internal static bool CheckSubNodesIfVisible(TreeNode baseNode)
	{
		if (!CheckNodeIfVisible(baseNode))
			return false;

		foreach (TreeNode node in baseNode.Nodes)
		{
			if (!CheckSubNodesIfVisible(node))
				return false;
		}

		return true;
	}

	/// <summary>
	/// Checks if the specified node is visible and checkable. If it is, the node is checked.
	/// </summary>
	/// <param name="node">The node to check.</param>
	/// <returns>
	/// <see langword="true"/> if the node is visible and checkable, <see langword="false"/> otherwise.
	/// </returns>
	internal static bool CheckNodeIfVisible(TreeNode node)
	{
		if (node.IsCheckBoxVisible())
		{
			node.Checked = true;
			return true;
		}

		return false;
	}

	/// <summary>
	/// Handles the MouseMove event of both the ConflictsTree and MergesTree.
	/// This involves highlighting the node under the mouse cursor and showing tooltips for conflicts.
	/// </summary>
	internal void OnTreeViewMouseMove(object sender, MouseEventArgs e)
	{
		TreeView tv = (TreeView)sender;

		if (tv == ConflictsTree)
		{
			_treConflictsPrevNode = HandleTreeViewMouseMoves(tv, _treConflictsColorFaders, _treConflictsPrevNode, e);
			ShowConflictsToolTip(tv, e);
		}
		else
		{
			_treMergesPrevNode = tv == MergesTree
				? HandleTreeViewMouseMoves(tv, _treMergesColorFaders, _treMergesPrevNode, e)
				: throw new NotImplementedException($"Unknown Treeview: {tv.Name}");
		}
	}

	/// <summary>
	/// Shows tooltips for conflicts in the ConflictsTree, based on the user's settings for tooltip display.
	/// </summary>
	private void ShowConflictsToolTip(TreeView tv, MouseEventArgs e)
	{
		TreeNode node = tv.GetNodeAt(e.Location);
		bool isBundledNode = node?.Parent?.Parent?.Tag is ModFileCategory tag && tag.IsBundled == true;

		// Determine if tooltips are allowed based on settings
		bool ToolTipsAllowed = false;
		switch (Settings.Get<int>("PrioToolTips"))
		{
			// 0 Never (highest prio is still colored)
			// 1 Only for Bundled conflicts
			// 2 Only for Script conflicts
			// 3 Both Bundled and Script conflicts
			case 1: // Bundles
				ToolTipsAllowed = isBundledNode;
				break;
			case 2: // Scripts
				ToolTipsAllowed = !isBundledNode;
				break;
			case 3: // Both
				ToolTipsAllowed = true;
				break;
		}

		// Show or hide tooltip based on conditions
		if (node != null && node != _prevToolTipNode && !string.IsNullOrEmpty(node.ToolTipText) && ToolTipsAllowed)
		{
			int mostRightX = node.Bounds.Left + node.Bounds.Width + 15;
			int centerY = node.Bounds.Top + 3;

			Point ToolTipPos = new(mostRightX, centerY);
			treConflictsToolTip.Show(node.ToolTipText, tv, ToolTipPos);
		}
		else if (node is null || string.IsNullOrEmpty(node.ToolTipText))
		{
			treConflictsToolTip.SetToolTip(tv, null);
			treConflictsToolTip.Hide(tv);
		}

		_prevToolTipNode = node;
	}

	/// <summary>
	/// Handles mouse movements over a TreeView by highlighting the hovered node and restoring 
	/// the previous node's color.
	/// </summary>
	private static TreeNode HandleTreeViewMouseMoves(
		TreeView tv,
		List<TreeNodeColorFader> colorFaders,
		TreeNode previousNode,
		MouseEventArgs e)
	{
		TreeNode hoveredNode = tv.GetNodeAt(e.Location);
		TreeNodeColorFader previousHoveredColorFader = colorFaders.Find(f => f.OutputNode == previousNode);
		TreeNodeColorFader currentHoveredColorFader = colorFaders.Find(f => f.OutputNode == hoveredNode);

		if (hoveredNode != previousNode)
		{
			Color targetForeColor = ThemeManager.CurrentTheme.ForeDimmedColor;
			if (previousNode?.Tag is NodeMetadata tag && tag.ForeColor != Color.Empty)
				targetForeColor = tag.ForeColor;
			previousHoveredColorFader?.StartFadeOut(targetForeColor);
		}

		currentHoveredColorFader?.StartFadeIn(ThemeManager.CurrentTheme.ForeBrightColor);
		return hoveredNode;
	}

	#region Confirmations

	/// <summary>
	/// Prompts the user to confirm pruning a missing merge file from the inventory.
	/// </summary>
	/// <param name="merge">The merge with the missing file.</param>
	/// <returns><see langword="true"/> if the user confirms the pruning, <see langword="false"/> otherwise.</returns>
	private bool ConfirmPruneMissingMergeFile(Merge merge)
	{
		// Auto delete old merges
		if (Settings.Get<bool>("AutoDeleteOldMerges"))
			return true;

		string msg =
			"Can't find the merged version of the following file.\n\n" +
			merge.RelativePath + "\n            " + // Adjusted spacing for consistency
			string.Join("\n            ", merge.Mods.Select(mod => mod.Name)) + "\n\n" +
			"Expected path:\n" +
			merge.GetMergedFile() + "\n\n";

		msg += merge.IsBundleContent
			? "Remove from Merges list & repack merged bundle?"
			: "Remove from Merges list?";

		return DialogResult.Yes == UIThreadManager.ShowMessage(
			msg,
			"Missing Merge Inventory File",
			MessageBoxButtons.YesNo,
			MessageBoxIcon.Question);
	}

	/// <summary>
	/// Prompts the user to confirm deleting a merge due to a missing mod file.
	/// </summary>
	/// <param name="merge">The merge with the missing mod file.</param>
	/// <param name="modName">The name of the missing mod.</param>
	/// <returns><see langword="true"/> if the user confirms the deletion, <see langword="false"/> otherwise.</returns>
	private bool ConfirmDeleteMergeForMissingMod(Merge merge, string modName)
	{
		// Auto delete old merges
		if (Settings.Get<bool>("AutoDeleteOldMerges"))
			return true;

		string msg =
			$"Can't find the '{modName}' version of the following file, " +
			"perhaps because the mod was uninstalled or updated.\n\n" +
			merge.RelativePath + "\n            " + // Adjusted spacing for consistency
			string.Join("\n            ", merge.Mods.Select(mod => mod.Name)) + "\n\n" +
			"Expected path:\n" +
			merge.GetModFile(modName) + "\n\n";

		msg += merge.IsBundleContent
			? "Delete this affected merge & repack the merged bundle?"
			: "Delete this affected merge?";

		return DialogResult.Yes == UIThreadManager.ShowMessage(
			msg,
			"Missing Merge Inventory File",
			MessageBoxButtons.YesNo,
			MessageBoxIcon.Question);
	}

	/// <summary>
	/// Prompts the user to confirm deleting a merge due to a disabled mod.
	/// </summary>
	/// <param name="merge">The merge with the disabled mod.</param>
	/// <param name="modName">The name of the disabled mod.</param>
	/// <returns><see langword="true"/> if the user confirms the deletion, <see langword="false"/> otherwise.</returns>
	private bool ConfirmDeleteMergeForDisabledMod(Merge merge, string modName)
	{
		// Auto delete old merges
		if (Settings.Get<bool>("AutoDeleteOldMerges"))
			return true;

		string msg =
			$"In your custom load order, {modName} is disabled.\n" +
			"Delete the following merge that includes the disabled mod?\n\n" +
			merge.RelativePath + "\n            " + // Adjusted spacing for consistency
			string.Join("\n            ", merge.Mods.Select(mod => mod.Name));

		return DialogResult.Yes == UIThreadManager.ShowMessage(
			msg,
			"Disabled Mod in Merge",
			MessageBoxButtons.YesNo,
			MessageBoxIcon.Question);
	}

	/// <summary>
	/// Prompts the user to delete a merge due to a changed file hash, indicating a potential mod update.
	/// </summary>
	/// <param name="merge">The affected merge.</param>
	/// <param name="modFilePath">The path to the mod file with the changed hash.</param>
	/// <param name="modName">The name of the mod.</param>
	/// <returns>The <see cref="DialogResult"/> representing the user's choice.</returns>
	private DialogResult PromptToDeleteForChangedHash(Merge merge, string modFilePath, string modName)
	{
		string msg =
			$"The '{modName}' {(merge.IsBundleContent ? "bundle" : "version of the following file")} " +
			"is different from when it was used in a merge, perhaps because the mod has been updated.\n\n" +
			$"This file has changed:\n\n{modFilePath}\n\n" +
			$"This merge is affected:\n\n{merge.RelativePath}\n            " + // Adjusted spacing for consistency
			string.Join("\n            ", merge.Mods.Select(mod => mod.Name)) + "\n\n";

		msg += merge.IsBundleContent
			? "Delete this affected merge & repack the merged bundle?"
			: "Delete this affected merge?";

		msg += "\n\nYes:           Recommended\nNo:            Not recommended, the merges may be outdated\nCancel:    No, and never perform this check again";

		DialogResult choice = UIThreadManager.ShowMessage(
			msg,
			"Merged Mod File Changed",
			MessageBoxButtons.YesNoCancel,
			MessageBoxIcon.Exclamation,
			MessageBoxDefaultButton.Button1);
		return choice;
	}

	#endregion

}
