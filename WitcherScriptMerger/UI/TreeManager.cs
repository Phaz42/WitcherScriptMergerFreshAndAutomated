using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "One TreeManager lives as long as the main window; its lists' tooltips and their timers are released with the process.")]
internal class TreeManager
{
	internal ConflictTree ConflictsTree { get; }
	internal MergeTree MergesTree { get; }
	private UIThreadManager UIThreadManager { get; }
	private ProgressManager ProgressManager { get; }

	internal ModFileIndex ModIndex;

	private bool _manualConflictsRefreshRunning;

	private TreeNode _treConflictsPrevNode, _treMergesPrevNode;
	private readonly ListToolTips _conflictsToolTips, _mergesToolTips;

	// A mod node's hover fader is made the first time it is hovered, and goes with its node. A list attached when a tree
	// was built missed nodes: the merges tree is built in queued steps that ran after the list was made (so no fader
	// worked after a merge was created or deleted), and a search can hide nodes as a refresh ends.
	private readonly ConditionalWeakTable<TreeNode, TreeNodeColorFader> _colorFaders = new();

	// The search above each tree (TreeSearch), its text box and its "12 of 340" count. Null: nothing searched.
	private TreeSearch _conflictsSearch, _mergesSearch;
	private string _conflictsSearchText = string.Empty, _mergesSearchText = string.Empty;
	private TextBox _conflictsSearchBox, _mergesSearchBox;
	private Label _conflictsSearchCount, _mergesSearchCount;
	private bool _applyingSearch;
	private int _searchSuspensions;
	private readonly HashSet<SMTree> _searchApplied = [];

	// The conflict list's filter (FilterPanel): the mods it hides, saved by name, separated by |, which no folder name
	// can hold. A conflict is hidden when a filtered mod is in it and fewer than two other mods are; in the others, the
	// filtered mod is dimmed (ConflictTree), as their merges include it. Applied with the search, by the same hiding.
	private const string FilteredModsKey = "FilteredMods";
	private HashSet<string> _filteredMods = new(
		(Settings.Get(FilteredModsKey) ?? string.Empty).Split('|', StringSplitOptions.RemoveEmptyEntries),
		StringComparer.OrdinalIgnoreCase);

	/// <summary>After the conflict list's search and filter were applied: what the filter hides may have changed.</summary>
	internal event EventHandler FilterApplied;

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
		_conflictsToolTips = new ListToolTips(treConflicts, treConflictsToolTip, treConflicts.LoadOrderNumberToolTip);
		_mergesToolTips = new ListToolTips(treMerges, new ToolTip());
	}

	// A tooltip stays until the mouse moves; when a list is rebuilt or searched under a resting mouse, it would float over
	// rows that are no longer there.
	private void HideToolTips()
	{
		_conflictsToolTips.Hide();
		_mergesToolTips.Hide();
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

		if (Settings.ScanBundlesOnEveryScan || (Settings.ScanBundles && checkBundles))
			ProgressManager.lblBundleScanInfo.Visible = true;

		ProgressManager.InitializeProgressScreen("Detecting Conflicts", ProgressBarStyle.Continuous);
		ProgressManager.lblStatusLeft1.Text = "Refreshing...";
		ProgressManager.lblStatusLeft2.Visible = ProgressManager.lblStatusLeft3.Visible = false;

		HideToolTips();
		ConflictsTree.BeginUpdate();

		// Clear tree nodes or update specific nodes based on conditions
		if (OptionsFrm.BundleScanOptionChanged || (Program.Inventory.ScriptsChanged && Program.Inventory.BundleChanged))
		{
			ConflictsTree.Nodes.Clear();
			OptionsFrm.BundleScanOptionChanged = false; // rebuilt once for the change
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
			HideToolTips();
			ConflictsTree.BeginUpdate();
			// Every merge node is rebuilt below; any a search hid would otherwise come back beside the new ones.
			_ = MergesTree.ShowAllNodes();
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
					categoryNode = CreateCategoryNode(merge.Category, true);
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

			MarkOutdatedMergeMods();

			// This runs queued, possibly after the progress screen was hidden: the search is applied to the new nodes.
			ApplySearch(MergesTree);
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
				"Please set the game folder of The Witcher 3 in Options.");
			return;
		}

		_manualConflictsRefreshRunning = true;

		// Refresh trees with or without bundle checking based on settings
		if (Settings.ScanBundles)
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

		// An exception inside the scan worker arrives here, not as a crash. The conflict list is then incomplete, so
		// the user is told, and nothing automatic acts on it: no merges created, no exit as if nothing conflicted.
		bool scanFailed = e.Error != null;
		if (scanFailed)
		{
			_ = UIThreadManager.ShowError(
				"The conflict scan stopped because of an unexpected error. The list of conflicts may be incomplete.\n\n" + e.Error.Message,
				"Scan Stopped");
		}

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
						categoryNode = CreateCategoryNode(conflict.Category);
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
			ConflictsTree.Select();
			foreach (TreeNode catNode in ConflictsTree.CategoryNodes)
			{
				if (!(catNode.Tag as ModFileCategory).IsSupported)
					catNode.SetIsCheckBoxVisible(false, true);
			}

			ConflictsTree.SetStylesForCustomLoadOrder();
		}

		// Finalize and update UI
		ConflictsTree.ApplyDefaultExpansion();
		MarkOutdatedMergeMods();  // the merges tree was built before this scan found which copies are outdated
		ConflictsTree.EndUpdate();
		ProgressManager.UpdateStatusText(ModIndex);
		ProgressManager.HideProgressScreen();
		ConflictsTree.ScrollToTop();  // after hiding the progress screen applies the search and the filter, which rebuild the list
		UIThreadManager.SetMergeButtonTextIfValidSelection();

		// Auto Create Merges
		if (Settings.Get<bool>("AutoCreateScriptMerges") && !scanFailed)
			CreateAllScriptMergesDelegate?.Invoke(false);

		// Show/hide "no conflicts" image if enabled
		UIThreadManager.ShowHideNoConflictsWitcher();

		// Auto Quit
		if (!_manualConflictsRefreshRunning && !scanFailed)
		{
			if (Settings.Get<bool>("AutoExit"))
				UIThreadManager.ExitIfNoMoreConflicts();
		}
	}

	#region Search

	/// <summary>Whether the conflict list's filter hides this mod.</summary>
	internal bool IsFiltered(string modName) => _filteredMods.Contains(modName);

	/// <summary>The mods the filter hides, as saved: also those with no conflict now, which it hides again when they have one.</summary>
	internal IReadOnlyCollection<string> FilteredMods => _filteredMods;

	/// <summary>Every mod in the conflict list, shown or hidden, by name, with the number of conflicts it is in.</summary>
	internal IEnumerable<(string Mod, int Conflicts)> ConflictModCounts() =>
		ConflictsTree.AllFileNodes
			.SelectMany(fileNode => fileNode.GetTreeNodes().Select(modNode => modNode.Text).Distinct(StringComparer.OrdinalIgnoreCase))
			.GroupBy(modName => modName, StringComparer.OrdinalIgnoreCase)
			.OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
			.Select(group => (group.Key, group.Count()));

	/// <summary>The mods the filter hides from now on, saved, and applied to the conflict list at once.</summary>
	internal void SetFilteredMods(IEnumerable<string> modNames)
	{
		_filteredMods = new HashSet<string>(modNames, StringComparer.OrdinalIgnoreCase);
		Settings.Set(FilteredModsKey, string.Join('|', _filteredMods));
		Settings.Save();
		ApplySearch(ConflictsTree);

		// As a new search does, it starts at the top of the list.
		if (ConflictsTree.Nodes.Count > 0)
			ConflictsTree.TopNode = ConflictsTree.Nodes[0];
	}

	/// <summary>How many mods in the conflict list the filter hides, and how many conflicts it hides for them.</summary>
	internal (int Mods, int Conflicts) FilterHides()
	{
		List<TreeNode> fileNodes = ConflictsTree.AllFileNodes;
		return (ConflictModCounts().Count(mod => IsFiltered(mod.Mod)), fileNodes.Count(fileNode => !ShownByFilter(fileNode)));
	}

	// Whether the filter hides any mod in the conflict list now; a filtered mod no longer there changes nothing.
	private bool IsFilterApplied() =>
		_filteredMods.Count > 0 && ConflictsTree.AllFileNodes.Any(fileNode => fileNode.GetTreeNodes().Any(modNode => IsFiltered(modNode.Text)));

	// A conflict none of whose mods the filter hides, or one with two or more other mods: its merge includes the others.
	private bool ShownByFilter(TreeNode fileNode)
	{
		int mods = 0, filtered = 0;
		foreach (TreeNode modNode in fileNode.GetTreeNodes())
		{
			mods++;
			if (IsFiltered(modNode.Text))
				filtered++;
		}

		return filtered == 0 || mods - filtered >= 2;
	}

	/// <summary>The search boxes and their counts, from the main form.</summary>
	internal void SetSearchControls(TextBox conflictsBox, Label conflictsCount, TextBox mergesBox, Label mergesCount)
	{
		_conflictsSearchBox = conflictsBox;
		_conflictsSearchCount = conflictsCount;
		_mergesSearchBox = mergesBox;
		_mergesSearchCount = mergesCount;
	}

	/// <summary>Searches a tree for what was typed above it.</summary>
	internal void SetSearchText(SMTree tree, string text)
	{
		text ??= string.Empty;
		if (tree == ConflictsTree)
		{
			if (text == _conflictsSearchText)
				return;
			_conflictsSearchText = text;
			_conflictsSearch = TreeSearch.Parse(text);
		}
		else
		{
			if (text == _mergesSearchText)
				return;
			_mergesSearchText = text;
			_mergesSearch = TreeSearch.Parse(text);
		}

		ApplySearch(tree);

		// A new search, or none, starts at the top of the list; the tree is left wherever rebuilding it scrolled it.
		if (tree.Nodes.Count > 0)
			tree.TopNode = tree.Nodes[0];
	}

	/// <summary>Shows only the conflicts or merges with this mod: "Show Only This Mod's ..." in the context menu.</summary>
	internal void SearchForMod(SMTree tree, string modName)
	{
		string text = $"mod:\"{modName}\"";
		TextBox box = tree == ConflictsTree ? _conflictsSearchBox : _mergesSearchBox;
		if (box != null)
			box.Text = text;
		SetSearchText(tree, text);
	}

	/// <summary>
	/// Puts every node a search hides back into both trees, for whatever must act on every conflict or merge: merging,
	/// deleting, scanning, exporting. It is called when the progress screen is shown, which every merge and refresh does,
	/// and before the actions that read the trees without it. ResumeSearch, or the progress screen being hidden, applies
	/// the search again.
	/// </summary>
	internal void SuspendSearch()
	{
		foreach (SMTree tree in new SMTree[] { ConflictsTree, MergesTree })
		{
			if (!tree.ShowAllNodes())
				continue;

			tree.BeginUpdate();
			tree.Search = null;
			tree.RestyleAfterSearch(false);
			tree.EndUpdate();
		}
	}

	/// <summary>
	/// Every node in both trees while the returned scope lasts, for an action that reads them all; the search is applied
	/// again when it ends, also when the action throws. Scopes nest, and while one lasts nothing re-applies the search -
	/// not the typing delay's timer, nor a click handled meanwhile.
	/// </summary>
	internal IDisposable SearchSuspended()
	{
		++_searchSuspensions;
		SuspendSearch();
		return new SearchScope(this);
	}

	private sealed class SearchScope(TreeManager owner) : IDisposable
	{
		private bool _ended;

		public void Dispose()
		{
			if (_ended)
				return;
			_ended = true;

			if (--owner._searchSuspensions == 0)
				owner.ResumeSearch();
		}
	}

	/// <summary>Applies the searches again, unless an operation is still running; its end applies them.</summary>
	internal void ResumeSearch()
	{
		ApplySearch(ConflictsTree);
		ApplySearch(MergesTree);
	}

	private void ApplySearch(SMTree tree)
	{
		// While the progress screen is shown, or an action holds the search suspended, a merge, deletion or scan is using
		// the trees and needs every node in them.
		if (_applyingSearch || _searchSuspensions > 0 || ProgressManager.pnlProgress.Visible)
			return;

		_applyingSearch = true;
		try
		{
			TreeSearch search = tree == ConflictsTree ? _conflictsSearch : _mergesSearch;
			bool filtering = tree == ConflictsTree && IsFilterApplied();

			// A search that hid nothing still marked the found mods and expanded the tree, and a filter dims the mods it
			// hides, so clearing either restyles too.
			HideToolTips();
			tree.BeginUpdate();
			tree.Search = search;
			bool wasSearched = _searchApplied.Remove(tree);
			bool changed = tree.ShowAllNodes() | wasSearched;
			if (search != null || filtering)
			{
				tree.HideFileNodes(fileNode =>
					(search == null || search.MatchesFileNode(fileNode)) && (!filtering || ShownByFilter(fileNode)));
				_ = _searchApplied.Add(tree);
				changed = true;
			}

			if (changed)
			{
				tree.RestyleAfterSearch(search != null);
				if (search != null)
					MarkFoundMods(tree, search);
			}

			tree.EndUpdate();
			ShowSearchCount(tree, search, filtering);
		}
		finally
		{
			_applyingSearch = false;
		}

		if (tree == ConflictsTree)
			FilterApplied?.Invoke(this, EventArgs.Empty);
	}

	// The mods a search found are green among the other mods of their conflict or merge, as is what it found in a file's
	// path (SMTree.PaintSearchFinds); RestyleAfterSearch gives them their own color back. Not bold, as they were: WinForms
	// makes a new font handle on every repaint of a row with a font of its own and frees it only when the garbage
	// collector runs, so scrolling a long search ran out of handles and closed SM-FAE. A struck-through (disabled)
	// mod keeps that font.
	private static void MarkFoundMods(SMTree tree, TreeSearch search)
	{
		foreach (TreeNode modNode in tree.ModNodes)
		{
			if (modNode.Tag is NodeMetadata metadata && search.MatchesModName(modNode.Text))
			{
				metadata.IsFound = true;
				modNode.ForeColor = metadata.RestColor;
			}
		}
	}

	// "12 of 340", and how many selected ones the search or the filter hides: they are still merged or deleted with the
	// selection.
	private void ShowSearchCount(SMTree tree, TreeSearch search, bool filtering)
	{
		Label count = tree == ConflictsTree ? _conflictsSearchCount : _mergesSearchCount;
		if (count == null)
			return;

		if (search == null && !filtering)
		{
			count.Text = string.Empty;
			return;
		}

		int hiddenSelected = tree == ConflictsTree
			? tree.HiddenFileNodes.Count(node => node.GetTreeNodes().Count(modNode => modNode.Checked) > 1)
			: tree.HiddenFileNodes.Count(node => node.Checked);

		count.Text = $"{tree.FileNodes.Count} of {tree.AllFileNodes.Count}"
			+ (hiddenSelected > 0 ? $" \u00B7 {hiddenSelected} selected not shown" : string.Empty);
	}

	#endregion

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
			_treConflictsPrevNode = HandleTreeViewMouseMoves(tv, GetColorFader, _treConflictsPrevNode, e);
			_conflictsToolTips.OnMouseMove(e.Location);
		}
		else
		{
			_treMergesPrevNode = tv == MergesTree
				? HandleTreeViewMouseMoves(tv, GetColorFader, _treMergesPrevNode, e)
				: throw new NotImplementedException($"Unknown Treeview: {tv.Name}");
			_mergesToolTips.OnMouseMove(e.Location);
		}
	}

	/// <summary>
	/// Marks the mods in the merges tree whose copy was made for an older game version, as the conflicts tree does: merged,
	/// such a copy has already removed what the Remastered version changed in that file. Hidden merges too.
	/// </summary>
	private void MarkOutdatedMergeMods()
	{
		ModHealth health = ModIndex?.Health;
		foreach (TreeNode fileNode in MergesTree.AllFileNodes)
		{
			// An old merge has no outdated mod in it to color, so its file stands out instead.
			if (health != null && health.IsOldMerge(fileNode.Text, out _, out GameVersionCheck.Result oldMerge))
			{
				fileNode.ForeColor = ThemeManager.CurrentTheme.WarningColor;
				fileNode.ToolTipText = $"An old merge, made against an older game version (missing {oldMerge.Describe()}): delete it and merge again.";
			}
			else
			{
				fileNode.ForeColor = ThemeManager.CurrentTheme.ForeBrightColor;
				fileNode.ToolTipText = null;
			}

			foreach (TreeNode modNode in fileNode.GetTreeNodes())
			{
				if (modNode.Tag is not NodeMetadata metadata)
					continue;

				GameVersionCheck.Result outdated = default;
				metadata.IsOutdated = health != null && health.IsOutdated(fileNode.Text, modNode.Text, out outdated);
				metadata.ForeColor = metadata.IsOutdated ? ThemeManager.CurrentTheme.WarningColor : ThemeManager.CurrentTheme.ForeDimmedColor;
				modNode.ForeColor = metadata.ForeColor;
				modNode.ToolTipText = metadata.IsOutdated ? $"Made for an older game version: missing {outdated.Describe()}" : null;
			}
		}
	}

	// Mod nodes fade - nodes with no nodes below them, as before; categories and files do not. A category a search takes
	// out of the tree has none below it either, and faded to a mod's gray when the mouse left it (the metadata check).
	private TreeNodeColorFader GetColorFader(TreeNode node) =>
		node == null || node.Nodes.Count > 0 || node.Tag is not NodeMetadata
			? null
			: _colorFaders.GetValue(node, mod => new TreeNodeColorFader(mod, "ForeColor"));

	/// <summary>
	/// A category node, as both trees show it: large, bold, in the accent color.
	/// </summary>
	internal static TreeNode CreateCategoryNode(ModFileCategory category, bool forMerges = false)
	{
		return new TreeNode
		{
			Text = category.DisplayName,
			ToolTipText = forMerges ? category.MergesToolTipText : category.ToolTipText,
			NodeFont = new Font("Segoe UI", 14.0F, FontStyle.Bold),
			ForeColor = ThemeManager.CurrentTheme.AccentColor,
			Tag = category
		};
	}

	/// <summary>
	/// Handles mouse movements over a TreeView by highlighting the hovered node and restoring 
	/// the previous node's color.
	/// </summary>
	private static TreeNode HandleTreeViewMouseMoves(
		TreeView tv,
		Func<TreeNode, TreeNodeColorFader> colorFader,
		TreeNode previousNode,
		MouseEventArgs e)
	{
		TreeNode hoveredNode = tv.GetNodeAt(e.Location);
		TreeNodeColorFader previousHoveredColorFader = colorFader(previousNode);
		TreeNodeColorFader currentHoveredColorFader = colorFader(hoveredNode);

		if (hoveredNode != previousNode)
			FadeOut(previousNode, previousHoveredColorFader);

		currentHoveredColorFader?.StartFadeIn(ThemeManager.CurrentTheme.ForeBrightColor);
		return hoveredNode;
	}

	// A row the mouse has left fades back to the color it rests in - green while a search found it.
	private static void FadeOut(TreeNode node, TreeNodeColorFader colorFader)
	{
		Color targetForeColor = ThemeManager.CurrentTheme.ForeDimmedColor;
		if (node?.Tag is NodeMetadata tag && tag.ForeColor != Color.Empty)
			targetForeColor = tag.RestColor;
		colorFader?.StartFadeOut(targetForeColor);
	}

	/// <summary>
	/// The mouse left a list: the row it was over fades back, as when it moves on to another row. Without this, a row
	/// left straight out of the list - onto the window, a button, another program - stayed bright.
	/// </summary>
	internal void OnTreeViewMouseLeave(object sender, EventArgs e)
	{
		if (sender == ConflictsTree)
		{
			FadeOut(_treConflictsPrevNode, GetColorFader(_treConflictsPrevNode));
			_treConflictsPrevNode = null;
		}
		else if (sender == MergesTree)
		{
			FadeOut(_treMergesPrevNode, GetColorFader(_treMergesPrevNode));
			_treMergesPrevNode = null;
		}
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
			? "Remove from the Merges list and repack the merged bundle?"
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
			? "Delete this affected merge and repack the merged bundle?"
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
			? "Delete this affected merge and repack the merged bundle?"
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
