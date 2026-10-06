using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using WitcherScriptMerger.FileIndex;
using WitcherScriptMerger.Forms;
using WitcherScriptMerger.Inventory;
using WitcherScriptMerger.Theming;

using static WitcherScriptMerger.Program;

namespace WitcherScriptMerger.Controls;

internal partial class ConflictTree : SMTree
{
	#region Members

#pragma warning disable CA2213 // Disposable fields should be disposed
	// Disposed in SMTree
	internal readonly ToolStripSeparator _contextCustomLoadOrderSeparator = new();
	internal readonly ToolStripMenuItem _contextPrioritizeMod = new();
	internal readonly ToolStripMenuItem _contextToggleMod = new();
	internal readonly ToolStripMenuItem _contextRemoveFromCustomLoadOrder = new();
	internal readonly ToolStripMenuItem _contextShowOnlyMod = new();
#pragma warning restore CA2213 // Disposable fields should be disposed

	#endregion

	internal ConflictTree(Theme theme) : base(theme)
	{
		ContextNodeRegion.Items.AddRange(
		[
			_contextCustomLoadOrderSeparator,
			_contextPrioritizeMod,
			_contextToggleMod,
			_contextRemoveFromCustomLoadOrder,
			_contextShowOnlyMod
		]);
		BuildContextMenu();

		// contextCustomLoadOrderSeparator
		_contextCustomLoadOrderSeparator.Name = "contextCustomLoadOrderSeparator";
		_contextCustomLoadOrderSeparator.Size = new Size(235, 6);

		// contextPrioritizeMod
		_contextPrioritizeMod.Name = "contextPrioritizeMod";
		_contextPrioritizeMod.Size = new Size(225, 22);
		_contextPrioritizeMod.Text = "Set Mod Load Order";
		_contextPrioritizeMod.ToolTipText = "Lets you define the load order of your mods";
		_contextPrioritizeMod.Click += ContextPrioritizeMod;

		// contextToggleMod
		_contextToggleMod.Name = "contextToggleMod";
		_contextToggleMod.Size = new Size(225, 22);
		_contextToggleMod.ToolTipText = "Tells the game whether to load any of this mod's files";
		_contextToggleMod.Click += ContextToggleMod;

		// contextRemoveFromCustomLoadOrder
		_contextRemoveFromCustomLoadOrder.Name = "contextRemoveFromCustomLoadOrder";
		_contextRemoveFromCustomLoadOrder.Size = new Size(225, 22);
		_contextRemoveFromCustomLoadOrder.ToolTipText = "Removes this mod's custom load order settings";
		_contextRemoveFromCustomLoadOrder.Click += ContextRemoveFromCustomLoadOrder;

		// contextShowOnlyMod
		_contextShowOnlyMod.Name = "contextShowOnlyMod";
		_contextShowOnlyMod.Size = new Size(225, 22);
		_contextShowOnlyMod.Text = "Show Only This Mod's Conflicts";
		_contextShowOnlyMod.ToolTipText = "Searches the conflicts for this mod";
		_contextShowOnlyMod.Click += (sender, e) => MainFrm.treeManager.SearchForMod(this, RightClickedNode.Text);
	}

	protected override void HandleCheckedChange()
	{
		if (IsCategoryNode(ClickedNode))
		{
			foreach (TreeNode fileNode in ClickedNode.GetTreeNodes())
			{
				_ = fileNode.SetCheckedIfVisible(ClickedNode.Checked);
				foreach (TreeNode modNode in fileNode.GetTreeNodes())
					_ = modNode.SetCheckedIfVisible(ClickedNode.Checked);
			}
		}
		else if (IsFileNode(ClickedNode))
		{
			foreach (TreeNode modNode in ClickedNode.GetTreeNodes())
				_ = modNode.SetCheckedIfVisible(ClickedNode.Checked);

			TreeNode catNode = ClickedNode.Parent;
			catNode.Checked = catNode.AreAllVisibleCheckboxesChecked();
		}
		else if (IsModNode(ClickedNode))
		{
			TreeNode fileNode = ClickedNode.Parent;
			fileNode.Checked = fileNode.AreAllVisibleCheckboxesChecked();

			TreeNode catNode = fileNode.Parent;
			catNode.Checked = catNode.AreAllVisibleCheckboxesChecked();
		}

		MainFrm.uiThreadManager.SetMergeButtonTextIfValidSelection();
	}

	protected override void OnLeftMouseUp(MouseEventArgs e)
	{
		if (ClickedNode == null)
			return;

		TreeNode catNode = IsCategoryNode(ClickedNode) ? ClickedNode : IsFileNode(ClickedNode) ? ClickedNode.Parent : ClickedNode.Parent.Parent;
		ModFileCategory category = catNode.Tag as ModFileCategory;
		if (category.IsSupported)
			base.OnLeftMouseUp(e);
	}

	protected override void SetAllChecked(bool isChecked)
	{
		foreach (TreeNode catNode in CategoryNodes)
		{
			ModFileCategory category = catNode.Tag as ModFileCategory;
			if (!category.IsSupported)
				continue;
			catNode.Checked = isChecked;
			foreach (TreeNode fileNode in catNode.GetTreeNodes())
			{
				fileNode.Checked = isChecked;
				foreach (TreeNode modNode in fileNode.GetTreeNodes())
					_ = modNode.SetCheckedIfVisible(isChecked);
			}
		}

		MainFrm.uiThreadManager.SetMergeButtonTextIfValidSelection();
	}

	protected override void SetContextItemAvailability()
	{
		base.SetContextItemAvailability();

		if (ClickedNode != null)
		{
			if (IsModNode(ClickedNode))
			{
				_contextCustomLoadOrderSeparator.Available = true;
				_contextPrioritizeMod.Available = true;

				foreach (ToolStripItem item in ContextNodeRegion.Items.Cast<ToolStripItem>())
				{
					item.Enabled = Program.LoadOrder.IsValid;
				}

				bool isDisabled = Program.LoadOrder.IsModDisabledByName(ClickedNode.Text);

				_contextToggleMod.Available = true;
				_contextToggleMod.Text =
					isDisabled
					? "Enable Mod in Load Order"
					: "Disable Mod in Load Order";

				_contextRemoveFromCustomLoadOrder.Available = Program.LoadOrder.ContainsMod(ClickedNode.Text);

				// Not a load order setting, so not disabled with them when the load order is invalid.
				_contextShowOnlyMod.Available = true;
				_contextShowOnlyMod.Enabled = true;
				_contextRemoveFromCustomLoadOrder.Text =
					isDisabled
					? "Clear Set Load Order && Disabled State"
					: "Clear Set Load Order";
			}
		}
		else if (!this.IsEmpty())
		{
			ContextSelectAll.Available = CategoryNodes.Any(catNode => !catNode.Checked && (catNode.Tag as ModFileCategory).IsSupported);

			ContextDeselectAll.Available = ModNodes.Any(modNode => modNode.Checked);
		}
	}

	private void ContextPrioritizeMod(object sender, EventArgs e)
	{
		string modName = RightClickedNode.Text;
		int? inputVal;

		using (PriorityPromptForm prompt = new())
		{
			inputVal = prompt.ShowDialog(modName, Program.LoadOrder.GetPriorityByName(modName));
		}

		if (!inputVal.HasValue)
			return;

		Program.LoadOrder.SetPriorityByName(modName, inputVal.Value);
		SetStylesForCustomLoadOrder();
	}

	/// <summary>
	/// Allow setting priority via double click on mod node (as alternative for context menu)
	/// </summary>
	protected override void OnDoubleClick(EventArgs e)
	{
		base.OnDoubleClick(e);
		if (ClickedNode != null && IsModNode(ClickedNode))
		{
			RightClickedNode = ClickedNode;
			ContextPrioritizeMod(RightClickedNode, e);
		}
	}

	private void ContextToggleMod(object sender, EventArgs e)
	{
		string modName = RightClickedNode.Text;

		Program.LoadOrder.ToggleModByName(modName);

		SetStylesForCustomLoadOrder();

		TreeNode fileNode = RightClickedNode.Parent;
		if ((fileNode.Parent.Tag as ModFileCategory).IsSupported)
		{
			fileNode.Checked = fileNode.GetTreeNodes()
				.Where(modNode => modNode.IsCheckBoxVisible())
				.All(modNode => modNode.Checked);
		}

		MainFrm.uiThreadManager.SetMergeButtonTextIfValidSelection();
	}

	private void ContextRemoveFromCustomLoadOrder(object sender, EventArgs e)
	{
		Program.LoadOrder.RemoveMod(RightClickedNode.Text);
		SetStylesForCustomLoadOrder();
	}

	internal void SetStylesForCustomLoadOrder()
	{
		ModHealth health = MainFrm.treeManager?.ModIndex?.Health;
		foreach (TreeNode fileNode in FileNodes)
		{
			IEnumerable<string> modNames = fileNode.GetTreeNodes().Select(modNode => modNode.Text);
			bool isResolved = Program.LoadOrder.HasResolvedConflict(modNames);
			string topPriorityMod = isResolved ? Program.LoadOrder.GetTopPriorityEnabledMod(modNames) : null;

			foreach (TreeNode modNode in fileNode.GetTreeNodes())
			{
				modNode.ToolTipText = "";
				int priority = Program.LoadOrder.GetPriorityByName(modNode.Text);

				if (modNode.Text.EqualsIgnoreCase(topPriorityMod))
				{
					if (priority > -1)
					{
						modNode.ToolTipText += $"Top priority in this conflict. Prio: {priority}";
						modNode.ForeColor = ((NodeMetadata)modNode.Tag).ForeColor = Settings.Get<bool>("AccentColor") ? ThemeManager.CurrentTheme.AcceptButtonBackColor : ThemeManager.DefaultAccentColor;
					}
				}
				else
				{
					modNode.ToolTipText += $"Prio: {priority}";
					modNode.ForeColor = ((NodeMetadata)modNode.Tag).ForeColor = ThemeManager.CurrentTheme.ForeDimmedColor;
				}

				if (Program.LoadOrder.IsModDisabledByName(modNode.Text))
				{
					// Add underline
					modNode.ToolTipText = "This mod is disabled in your custom load order";
					modNode.SetFontStyle(FontStyle.Strikeout);
					modNode.Checked = false;
					modNode.SetIsCheckBoxVisible(false);
				}
				else
				{
					// Remove underline
					modNode.NodeFont = null;
					if ((fileNode.Parent.Tag as ModFileCategory).IsSupported)
						modNode.SetIsCheckBoxVisible(true);
				}

				// A copy made for an older game version stands out, whatever its priority; the tooltip says why.
				NodeMetadata metadata = (NodeMetadata)modNode.Tag;
				GameVersionCheck.Result outdated = default;
				metadata.IsOutdated = health != null && health.IsOutdated(fileNode.Text, modNode.Text, out outdated);
				if (metadata.IsOutdated)
				{
					modNode.ForeColor = metadata.ForeColor = ThemeManager.CurrentTheme.WarningColor;
					modNode.ToolTipText = $"Made for an older game version: missing {outdated.Describe()}\n{modNode.ToolTipText}";
				}
				else if (health != null && health.IsOldMerge(fileNode.Text, out string mergedModName, out outdated)
					&& modNode.Text.EqualsIgnoreCase(mergedModName))
				{
					// The existing merge, listed here because another mod now conflicts with it.
					metadata.IsOutdated = true;
					modNode.ForeColor = metadata.ForeColor = ThemeManager.CurrentTheme.WarningColor;
					modNode.ToolTipText = $"An old merge, made against an older game version (missing {outdated.Describe()}): delete it and merge again.\n{modNode.ToolTipText}";
				}
			}
		}
	}

	internal override void RestyleAfterSearch(bool searching)
	{
		// Non-mergeable categories show no checkboxes; disabled mods are struck through and unchecked (load order styles).
		foreach (TreeNode catNode in CategoryNodes)
		{
			if (!(catNode.Tag as ModFileCategory).IsSupported)
				catNode.SetIsCheckBoxVisible(false, true);
		}

		SetStylesForCustomLoadOrder();

		// A category is checked when all its shown files are.
		foreach (TreeNode catNode in CategoryNodes)
		{
			if ((catNode.Tag as ModFileCategory).IsSupported)
				catNode.Checked = catNode.AreAllVisibleCheckboxesChecked();
		}

		if (searching)
			ExpandAll();
		else
			ApplyDefaultExpansion();
	}

	/// <summary>
	/// Expands the tree as a refresh leaves it: all expanded, except what the options collapse - non-mergeable files,
	/// files with a custom load order, and identical conflicts.
	/// </summary>
	internal void ApplyDefaultExpansion()
	{
		ExpandAll();
		foreach (TreeNode catNode in CategoryNodes)
		{
			if (!(catNode.Tag as ModFileCategory).IsSupported && Settings.Get<bool>("CollapseNotMergeable"))
				catNode.Collapse();
		}

		if (Settings.Get<bool>("CollapseCustomLoadOrder"))
		{
			foreach (TreeNode fileNode in FileNodes)
				fileNode.Collapse();
		}

		_ = CollapseIdenticalScriptNodesInTreeView();
	}

	internal int CollapseIdenticalScriptNodesInTreeView()
	{
		if (!Settings.Get<bool>("CollapseIdenticalConflicts"))
			return 0;

		// No Application.DoEvents() here: this also runs when an action puts a search's hidden nodes back before reading
		// the trees (TreeManager.SearchSuspended), and handling queued input then - a second click, the search's timer -
		// could run another action, or hide nodes again, before the first reads them.
		int totalCollapsedCount = 0;

		foreach (TreeNode rootLevelNode in Nodes)
		{
			if (rootLevelNode.Text == "Scripts")
				continue;

			int collapsedCount = 0;
			List<string> prevModNodeNames = null;

			foreach (TreeNode scriptNode in rootLevelNode.Nodes)
			{
				List<string> currentModNodeNames = scriptNode.Nodes
					.Cast<TreeNode>()
					.Select(node => node.Text)
					.OrderBy(name => name)
					.ToList();

				if (prevModNodeNames != null && prevModNodeNames.SequenceEqual(currentModNodeNames))
				{
					scriptNode.Collapse();
					collapsedCount++;
				}

				prevModNodeNames = currentModNodeNames;
			}

			totalCollapsedCount += collapsedCount;
		}

		return totalCollapsedCount;
	}
}
