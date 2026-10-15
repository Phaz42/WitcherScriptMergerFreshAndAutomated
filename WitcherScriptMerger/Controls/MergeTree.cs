using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using WitcherScriptMerger.Theming;

using static WitcherScriptMerger.Program;

namespace WitcherScriptMerger.Controls;

internal partial class MergeTree : SMTree
{
	#region Members

	internal static new readonly Color FileNodeForeColor = Color.Blue;

#pragma warning disable CA2213 // Disposable fields should be disposed
	// Disposed in SMTree
	internal readonly ToolStripMenuItem _contextOpenMergedFile = new();
	internal readonly ToolStripMenuItem _contextOpenMergedFileDir = new();
	internal readonly ToolStripMenuItem _contextDeleteAssociatedMerges = new();
	internal readonly ToolStripMenuItem _contextDeleteMerge = new();
	internal readonly ToolStripSeparator _contextDeleteSeparator = new();
	internal readonly ToolStripMenuItem _contextShowOnlyMod = new();
#pragma warning restore CA2213 // Disposable fields should be disposed

	#endregion

	internal MergeTree(Theme theme) : base(theme)
	{

		ContextOpenRegion.Items.AddRange([_contextOpenMergedFile, _contextOpenMergedFileDir]);
		ContextNodeRegion.Items.AddRange([_contextDeleteSeparator, _contextDeleteMerge, _contextDeleteAssociatedMerges, _contextShowOnlyMod]);
		BuildContextMenu();

		// contextOpenMergedFile
		_contextOpenMergedFile.Name = "contextOpenMergedFile";
		_contextOpenMergedFile.Size = new Size(225, 22);
		_contextOpenMergedFile.Text = "Open Merged File";
		_contextOpenMergedFile.ToolTipText = "Opens the merged version of the file";
		_contextOpenMergedFile.Click += ContextOpenFile_Click;

		// contextOpenMergedFileDir
		_contextOpenMergedFileDir.Name = "contextOpenMergedFileDir";
		_contextOpenMergedFileDir.Size = new Size(225, 22);
		_contextOpenMergedFileDir.Text = "Open Merged File Folder";
		_contextOpenMergedFileDir.ToolTipText = "Opens the location of the merged version of the file";
		_contextOpenMergedFileDir.Click += ContextOpenDirectory_Click;

		// contextDeleteSeparator
		_contextDeleteSeparator.Name = "contextDeleteSeparator";
		_contextDeleteSeparator.Size = new Size(235, 6);

		// contextDeleteMerge
		_contextDeleteMerge.Name = "contextDeleteMerge";
		_contextDeleteMerge.Size = new Size(225, 22);
		_contextDeleteMerge.Text = "Delete This Merge";
		_contextDeleteMerge.ToolTipText = "Deletes the merged version of the file";
		_contextDeleteMerge.Click += ContextDeleteMerge_Click;

		// contextDeleteAssociatedMerges
		_contextDeleteAssociatedMerges.Name = "contextDeleteAssociatedMerges";
		_contextDeleteAssociatedMerges.Size = new Size(225, 22);
		_contextDeleteAssociatedMerges.Text = "Delete All {0} Merges";
		_contextDeleteAssociatedMerges.ToolTipText = "Deletes all merges that contain this mod's files";
		_contextDeleteAssociatedMerges.Click += ContextDeleteAssociatedMerges_Click;

		// contextShowOnlyMod
		_contextShowOnlyMod.Name = "contextShowOnlyMod";
		_contextShowOnlyMod.Size = new Size(225, 22);
		_contextShowOnlyMod.Text = "Show Only This Mod's Merges";
		_contextShowOnlyMod.ToolTipText = "Searches the merges for this mod";
		_contextShowOnlyMod.Click += (sender, e) => MainFrm.treeManager.SearchForMod(this, RightClickedNode.Text);
	}

	protected override void HandleCheckedChange()
	{
		if (IsCategoryNode(ClickedNode))
		{
			foreach (TreeNode fileNode in ClickedNode.GetTreeNodes())
				fileNode.Checked = ClickedNode.Checked;
		}
		else if (IsFileNode(ClickedNode))
		{
			TreeNode catNode = ClickedNode.Parent;
			catNode.Checked = catNode.GetTreeNodes().All(node => node.Checked);
		}

		MainFrm.uiThreadManager.SetUnmergeButtonTextIfValidSelection();
	}

	protected override void OnLeftMouseUp(MouseEventArgs e)
	{
		if (ClickedNode != null && IsModNode(ClickedNode))
			ClickedNode = ClickedNode.Parent;

		base.OnLeftMouseUp(e);
	}

	protected override void SetAllChecked(bool isChecked)
	{
		foreach (TreeNode catNode in CategoryNodes)
		{
			catNode.Checked = isChecked;
			foreach (TreeNode fileNode in catNode.GetTreeNodes())
				fileNode.Checked = isChecked;
		}

		MainFrm.uiThreadManager.SetUnmergeButtonTextIfValidSelection();
	}

	internal override void RestyleAfterSearch(bool searching)
	{
		// Merges are selected by file; their mods show no checkboxes.
		foreach (TreeNode modNode in ModNodes)
		{
			modNode.NodeFont = null;
			if (modNode.Tag is NodeMetadata metadata)
			{
				metadata.IsFound = false;  // a search's mark (TreeManager.MarkFoundMods)
				modNode.ForeColor = metadata.ForeColor;
			}

			modNode.SetIsCheckBoxVisible(false);
		}

		foreach (TreeNode catNode in CategoryNodes)
			catNode.Checked = catNode.GetTreeNodes().All(node => node.Checked);

		ExpandAll();
	}

	private void ContextDeleteMerge_Click(object sender, EventArgs e)
	{
		if (RightClickedNode == null)
			return;

		if (IsModNode(RightClickedNode))
			MainFrm.inventoryMergeManager.DeleteMerges(new TreeNode[] { RightClickedNode.Parent });
		else
			MainFrm.inventoryMergeManager.DeleteMerges(new TreeNode[] { RightClickedNode });
	}

	private void ContextDeleteAssociatedMerges_Click(object sender, EventArgs e)
	{
		if (RightClickedNode == null || !IsModNode(RightClickedNode))
			return;

		// All of this mod's merges, including those a search hides: every node is put back first, because deleting reads them.
		using IDisposable searchSuspended = MainFrm.treeManager.SearchSuspended();

		// Find all file nodes that contain a node matching the clicked node
		System.Collections.Generic.IEnumerable<TreeNode> fileNodes = FileNodes.Where(node =>
			node.GetTreeNodes().Any(modNode =>
				modNode.Text == RightClickedNode.Text));

		MainFrm.inventoryMergeManager.DeleteMerges(fileNodes);
	}

	protected override void SetContextItemAvailability()
	{
		base.SetContextItemAvailability();

		if (ClickedNode != null)
		{
			if (ClickedNode.Tag != null && IsFileNode(ClickedNode))
			{
				_contextOpenMergedFile.Available = _contextOpenMergedFileDir.Available = true;
			}

			if (!IsCategoryNode(ClickedNode))
			{
				_contextDeleteMerge.Available = _contextDeleteSeparator.Available = true;
				if (IsModNode(ClickedNode))
				{
					_contextDeleteAssociatedMerges.Available = true;
					_contextDeleteAssociatedMerges.Text = $"Delete All {ClickedNode.Text} Merges";
					_contextShowOnlyMod.Available = true;
				}
			}
		}
		else if (!this.IsEmpty())
		{
			ContextSelectAll.Available = CategoryNodes.Any(catNode => !catNode.Checked);

			ContextDeselectAll.Available = FileNodes.Any(fileNode => fileNode.Checked);
		}
	}
}
