using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using WitcherScriptMerger.FileIndex;
using WitcherScriptMerger.Inventory;
using WitcherScriptMerger.Theming;

using static WitcherScriptMerger.Program;

namespace WitcherScriptMerger.Controls;

internal abstract partial class SMTree : TreeView, IDisposable
{
	private bool disposedValue;

	protected override void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				// dispose managed state (managed objects)
				_checkBoxImages?.Dispose();
				_contextMenu.Dispose();
				_contextOpenModFile.Dispose();
				_contextOpenModFileDir.Dispose();
				_contextOpenModBundleDir.Dispose();
				_contextOpenVanillaFile.Dispose();
				_contextOpenVanillaFileDir.Dispose();
				_contextCopyPath.Dispose();
				_contextAllSeparator.Dispose();
				_contextExpandAll.Dispose();
				_contextCollapseAll.Dispose();
				ContextSelectAll.Dispose();
				ContextDeselectAll.Dispose();

				// Dispose ConflictTree-specific resources
				if (this is ConflictTree conflictTree)
				{
					conflictTree._contextCustomLoadOrderSeparator.Dispose();
					conflictTree._contextPrioritizeMod.Dispose();
					conflictTree._contextToggleMod.Dispose();
					conflictTree._contextRemoveFromCustomLoadOrder.Dispose();
					conflictTree._contextShowOnlyMod.Dispose();
				}

				// Dispose MergeTree-specific resources
				if (this is MergeTree mergeTree)
				{
					mergeTree._contextOpenMergedFile.Dispose();
					mergeTree._contextOpenMergedFileDir.Dispose();
					mergeTree._contextDeleteAssociatedMerges.Dispose();
					mergeTree._contextDeleteMerge.Dispose();
					mergeTree._contextDeleteSeparator.Dispose();
					mergeTree._contextShowOnlyMod.Dispose();
				}
			}

			// TODO: free unmanaged resources (unmanaged objects) and override finalizer
			// TODO: set large fields to null
			disposedValue = true;
		}

		base.Dispose(disposing);
	}

	public new void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	#region Types

	internal enum LevelType : int
	{
		Categories, Files, Mods
	}

	internal class NodeMetadata
	{
		internal string FilePath;
		internal FileHash FileHash;
		internal ModFile ModFile;
		internal Color ForeColor = ThemeManager.CurrentTheme.ForeDimmedColor;
		internal string ModName = string.Empty;
		internal bool IsOutdated;  // this mod's copy was made for an older game version (ModHealth)

		// Non-mergeable conflicts only (ConflictTree): the mod's number in the load order, shown before its name - its
		// priority in mods.settings, or, when it has none, the number it effectively has (IsEffectiveLoadOrderNumber).
		internal int? LoadOrderNumber;
		internal bool IsEffectiveLoadOrderNumber;
	}

	#endregion

	#region Members

	internal static readonly Color FileNodeForeColor = Color.Black;

	internal List<TreeNode> CategoryNodes => GetNodesAtLevel(LevelType.Categories);

	internal List<TreeNode> FileNodes => GetNodesAtLevel(LevelType.Files);

	internal List<TreeNode> ModNodes => GetNodesAtLevel(LevelType.Mods);

	protected TreeNode ClickedNode;
	protected bool IsUpdating;
	private readonly Theme currentTheme;

	#endregion

	#region Double-buffering

	// From http://stackoverflow.com/a/10364283/1641069
	// Pinvoke:
	private const int TVM_SETEXTENDEDSTYLE = 0x1100 + 44;
	private const int TVS_EX_DOUBLEBUFFER = 0x0004;
	[LibraryImport("user32.dll", EntryPoint = "SendMessageW")]
	private static partial IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);
	protected override void OnHandleCreated(EventArgs e)
	{
		_ = SendMessage(Handle, TVM_SETEXTENDEDSTYLE, TVS_EX_DOUBLEBUFFER, TVS_EX_DOUBLEBUFFER);
		ApplyWindowTheme();
		base.OnHandleCreated(e);
		ApplyThemedCheckBoxes();
	}

	/// <summary>Gives the control the Windows look of the current theme: dark scrollbars in the dark theme.</summary>
	internal void ApplyWindowTheme() => ThemeUtils.ApplyWindowTheme(this);

	#endregion

	#region Checkboxes

	// Windows draws a list's checkboxes as images, in a fixed blue whatever the theme or accent. They are SM-FAE's images
	// instead, the checkbox every window here shows (ThemeUtils.DrawCheckBox), in the accent colour.
	private ImageList _checkBoxImages;

	private const int TVM_GETIMAGELIST = 0x1100 + 8;
	private const int TVSIL_STATE = 2;

	[LibraryImport("comctl32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool ImageList_GetIconSize(IntPtr imageList, out int width, out int height);

	private void ApplyThemedCheckBoxes()
	{
		if (!CheckBoxes || StateImageList != null)
			return;

		// Windows' own checkbox images' size, where it has made them yet; otherwise its size at this scaling.
		int size = LogicalToDeviceUnits(13);
		IntPtr native = SendMessage(Handle, TVM_GETIMAGELIST, TVSIL_STATE, IntPtr.Zero);
		if (native != IntPtr.Zero && ImageList_GetIconSize(native, out int width, out _) && width > 0)
			size = width;

		_checkBoxImages = new ImageList { ColorDepth = ColorDepth.Depth32Bit, ImageSize = new Size(size, size) };
		_checkBoxImages.Images.Add(ThemeUtils.CheckBoxImage(size, false));  // index 0: unchecked
		_checkBoxImages.Images.Add(ThemeUtils.CheckBoxImage(size, true));   // index 1: checked
		StateImageList = _checkBoxImages;
	}

	#endregion

	#region Context Menu Members

	protected TreeNode RightClickedNode;
	private ContextMenuStrip _contextMenu;

	protected ToolStripRegion ContextOpenRegion;
	private readonly ToolStripMenuItem _contextOpenModFile = new();
	private readonly ToolStripMenuItem _contextOpenModFileDir = new();
	private readonly ToolStripMenuItem _contextOpenModBundleDir = new();
	private readonly ToolStripMenuItem _contextOpenVanillaFile = new();
	private readonly ToolStripMenuItem _contextOpenVanillaFileDir = new();
	private readonly ToolStripMenuItem _contextCopyPath = new();

	protected ToolStripRegion ContextNodeRegion;

	protected ToolStripRegion ContextAllRegion;
	private readonly ToolStripSeparator _contextAllSeparator = new();
	private readonly ToolStripMenuItem _contextExpandAll = new();
	private readonly ToolStripMenuItem _contextCollapseAll = new();
	protected ToolStripMenuItem ContextSelectAll = new();
	protected ToolStripMenuItem ContextDeselectAll = new();

	#endregion

	internal SMTree(Theme theme)
	{
		currentTheme = theme;
		InitializeContextMenu();
		TreeViewNodeSorter = new SMTreeSorter();
	}

	protected List<TreeNode> GetNodesAtLevel(LevelType level)
	{
		IEnumerable<TreeNode> nodes = Nodes.Cast<TreeNode>();
		for (int i = 0; i < (int)level; ++i)
			nodes = nodes.SelectMany(node => node.GetTreeNodes());
		return nodes.ToList();
	}

	internal TreeNode GetCategoryNode(ModFileCategory category)
	{
		return CategoryNodes.FirstOrDefault(node =>
			category == (ModFileCategory)node.Tag);
	}

	#region Search

	// Windows' tree control cannot hide a node, so a file node a search does not match is taken out of the tree and kept
	// here, with the category node it came from; a category left empty goes too. Only nodes in the tree are shown,
	// clicked and checked - FileNodes and CategoryNodes are those. Whatever must see every conflict or merge counts with
	// AllFileNodes, or runs while every node is back in the tree: TreeManager puts them back before any merge, deletion
	// or scan, because those read nodes' parents and checkbox states, which a node out of the tree does not have.
	private readonly List<(TreeNode Category, TreeNode File)> _hiddenFileNodes = [];
	private readonly List<TreeNode> _hiddenCategoryNodes = [];

	/// <summary>Every file node, shown or hidden by a search.</summary>
	internal List<TreeNode> AllFileNodes => [.. FileNodes, .. _hiddenFileNodes.Select(hidden => hidden.File)];

	/// <summary>The file nodes a search hides.</summary>
	internal IEnumerable<TreeNode> HiddenFileNodes => _hiddenFileNodes.Select(hidden => hidden.File);

	/// <summary>Whether the tree has no file node at all, shown or hidden.</summary>
	internal bool HasNoFileNodes => this.IsEmpty() && _hiddenFileNodes.Count == 0;

	/// <summary>Whether a category has any file node, shown or hidden.</summary>
	internal bool HasCategory(ModFileCategory category) =>
		GetCategoryNode(category) != null || _hiddenFileNodes.Any(hidden => category == (ModFileCategory)hidden.Category.Tag);

	/// <summary>Takes every file node not shown out of the tree, and every category node left empty.</summary>
	internal void HideFileNodes(Func<TreeNode, bool> isShown)
	{
		foreach (TreeNode categoryNode in Nodes.Cast<TreeNode>().ToList())
		{
			foreach (TreeNode fileNode in categoryNode.GetTreeNodes().ToList())
			{
				if (isShown(fileNode))
					continue;

				fileNode.Remove();
				_hiddenFileNodes.Add((categoryNode, fileNode));
			}

			if (categoryNode.Nodes.Count == 0)
			{
				categoryNode.Remove();
				_hiddenCategoryNodes.Add(categoryNode);
			}
		}
	}

	/// <summary>
	/// Puts every hidden node back where it was. The tree is sorted, so each goes back to its sorted place. A node put
	/// back has lost its expanded state and its checkbox visibility, which live in Windows' control, not in the node;
	/// its checked state is kept. False when nothing was hidden.
	/// </summary>
	internal bool ShowAllNodes()
	{
		if (_hiddenFileNodes.Count == 0 && _hiddenCategoryNodes.Count == 0)
			return false;

		foreach (TreeNode categoryNode in _hiddenCategoryNodes)
			_ = Nodes.Add(categoryNode);
		foreach ((TreeNode categoryNode, TreeNode fileNode) in _hiddenFileNodes)
			_ = categoryNode.Nodes.Add(fileNode);

		_hiddenCategoryNodes.Clear();
		_hiddenFileNodes.Clear();
		return true;
	}

	/// <summary>
	/// Restores what a node put back (ShowAllNodes) or a search lost: checkbox visibility, styles and the expanded
	/// state - all expanded while a search is shown, otherwise as after a refresh.
	/// </summary>
	internal abstract void RestyleAfterSearch(bool searching);

	#endregion

	protected override void OnKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);
		if (e.Control)
		{
			if (e.KeyCode == Keys.A)
				ContextSelectAll_Click(null, null);
			else if (e.KeyCode == Keys.D)
				ContextDeselectAll_Click(null, null);
		}
	}

	protected override void OnAfterSelect(TreeViewEventArgs e)
	{
		base.OnAfterSelect(e);
		SelectedNode = null;
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		base.OnMouseDown(e);
		ClickedNode = GetNodeAt(e.Location);
		if (ClickedNode != null && !ClickedNode.Bounds.Contains(e.Location))
		{
			ClickedNode = null;
		}
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);
		if (ClickedNode == null || RightClickedNode != null || e.Button == MouseButtons.Right)
			return;
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		base.OnMouseUp(e);

		TreeNode lastClicked = ClickedNode;
		ClickedNode = GetNodeAt(e.Location);
		if (ClickedNode != null &&
			(lastClicked != ClickedNode || !ClickedNode.Bounds.Contains(e.Location)))
		{
			ClickedNode = null;
		}

		if (e.Button == MouseButtons.Left)
		{
			OnLeftMouseUp(e);
			ClickedNode = null;
		}
		else if (e.Button == MouseButtons.Right)
		{
			OnRightMouseUp(e);
		}

		IsUpdating = false;
	}

	protected virtual void OnLeftMouseUp(MouseEventArgs e)
	{
		if (ClickedNode == null)
			return;
		if (ClickedNode.SetCheckedIfVisible(!ClickedNode.Checked))
			HandleCheckedChange();
	}

	protected virtual void OnRightMouseUp(MouseEventArgs e)
	{
		ResetContextItemAvailability();
		SetContextItemAvailability();

		if (_contextMenu.Items.OfType<ToolStripMenuItem>().Any(item => item.Available))
		{
			if (ClickedNode != null)
				SetContextMenuSize();

			_contextMenu.Show(this, e.X, e.Y);
		}
	}

	protected override void OnAfterCheck(TreeViewEventArgs e)
	{
		base.OnAfterCheck(e);
		if (e.Action != TreeViewAction.Unknown)  // Event was triggered programmatically
		{
			ClickedNode = e.Node;
			HandleCheckedChange();
		}
	}

	protected abstract void HandleCheckedChange();

	protected override void OnMouseLeave(EventArgs e) => base.OnMouseLeave(e);

	protected static bool IsCategoryNode(TreeNode node) => (LevelType)node.Level == LevelType.Categories;

	protected static bool IsFileNode(TreeNode node) => (LevelType)node.Level == LevelType.Files;

	protected static bool IsModNode(TreeNode node) => (LevelType)node.Level == LevelType.Mods;

	#region Context Menu

	private void InitializeContextMenu()
	{
		_contextMenu = new()
		{
			ShowImageMargin = false,
			Renderer = new MenuRenderer(currentTheme),
		};

		ContextOpenRegion = new ToolStripRegion(_contextMenu,
		[
			_contextCopyPath,
			_contextOpenModFileDir,
			_contextOpenModBundleDir,
			_contextOpenVanillaFileDir
		]);

		ContextNodeRegion = new ToolStripRegion(_contextMenu, []);

		ContextAllRegion = new ToolStripRegion(_contextMenu,
		[
			_contextAllSeparator,
			ContextSelectAll,
			ContextDeselectAll,
			_contextExpandAll,
			_contextCollapseAll
		]);

		_contextMenu.Name = "treeContextMenu";
		_contextMenu.Closing += ContextMenu_Closing;

		// contextOpenModFileDir
		_contextOpenModFileDir.Name = "contextOpenModFileDir";
		_contextOpenModFileDir.Size = new Size(225, 22);
		_contextOpenModFileDir.Text = "Open Mod File Folder";
		_contextOpenModFileDir.ToolTipText = "Opens the location of this mod's version of the file";
		_contextOpenModFileDir.Click += ContextOpenDirectory_Click;

		// contextOpenModBundleDir
		_contextOpenModBundleDir.Name = "contextOpenModBundleDir";
		_contextOpenModBundleDir.Size = new Size(225, 22);
		_contextOpenModBundleDir.Text = "Open Mod Bundle Folder";
		_contextOpenModBundleDir.ToolTipText = "Opens the location of this mod's bundle file";
		_contextOpenModBundleDir.Click += ContextOpenDirectory_Click;

		// contextOpenVanillaFileDir
		_contextOpenVanillaFileDir.Name = "contextOpenVanillaFileDir";
		_contextOpenVanillaFileDir.Size = new Size(225, 22);
		_contextOpenVanillaFileDir.Text = "Open Vanilla File Folder";
		_contextOpenVanillaFileDir.ToolTipText = "Opens the location of the unmodded version of the file";
		_contextOpenVanillaFileDir.Click += ContextOpenVanillaDirectory_Click;

		// contextCopyPath
		_contextCopyPath.Name = "contextCopyPath";
		_contextCopyPath.Size = new Size(225, 22);
		_contextCopyPath.Text = "Copy Path";
		_contextCopyPath.ToolTipText = "Copies the full path of this file to the clipboard";
		_contextCopyPath.Click += ContextCopyPath_Click;

		// contextAllSeparator
		_contextAllSeparator.Name = "contextAllSeparator";
		_contextAllSeparator.Size = new Size(235, 6);

		// contextSelectAll
		ContextSelectAll.Name = "contextSelectAll";
		ContextSelectAll.Size = new Size(225, 22);
		ContextSelectAll.Text = "Select All";
		ContextSelectAll.Click += ContextSelectAll_Click;

		// contextDeselectAll
		ContextDeselectAll.Name = "contextDeselectAll";
		ContextDeselectAll.Size = new Size(225, 22);
		ContextDeselectAll.Text = "Deselect All";
		ContextDeselectAll.Click += ContextDeselectAll_Click;

		// contextExpandAll
		_contextExpandAll.Name = "contextExpandAll";
		_contextExpandAll.Size = new Size(225, 22);
		_contextExpandAll.Text = "Expand All";
		_contextExpandAll.Click += ContextExpandAll_Click;

		// contextCollapseAll
		_contextCollapseAll.Name = "contextCollapseAll";
		_contextCollapseAll.Size = new Size(225, 22);
		_contextCollapseAll.Text = "Collapse All";
		_contextCollapseAll.Click += ContextCollapseAll_Click;
	}

	protected void BuildContextMenu()
	{
		_contextMenu.Items.Clear();

		// Context Open Region
		_contextMenu.Items.AddRange(new[] {
			_contextCopyPath,
			_contextOpenModFileDir,
			_contextOpenModBundleDir,
			_contextOpenVanillaFileDir
		});

		foreach (ToolStripItem item in ContextNodeRegion.Items)
		{
			_ = _contextMenu.Items.Add(item);
		}

		// Context All Region
		_ = _contextMenu.Items.Add(_contextAllSeparator);
		_contextMenu.Items.AddRange(new[] {
			ContextSelectAll,
			ContextDeselectAll,
			_contextExpandAll,
			_contextCollapseAll
		});
	}

	private void ResetContextItemAvailability()
	{
		foreach (ToolStripItem menuItem in _contextMenu.Items.OfType<ToolStripItem>())
			menuItem.Available = false;
	}

	protected virtual void SetContextItemAvailability()
	{
		foreach (ToolStripItem menuItem in _contextMenu.Items.OfType<ToolStripItem>())
			menuItem.Available = false;

		if (ClickedNode != null && ClickedNode.Tag is NodeMetadata)
		{
			_contextCopyPath.Available = true;
			if (IsFileNode(ClickedNode)
				&& !((ModFileCategory)ClickedNode.Parent.Tag).IsBundled
				&& File.Exists((ClickedNode.Tag as NodeMetadata).ModFile.GetVanillaFile()))
			{
				_contextOpenVanillaFile.Available = true;
				_contextOpenVanillaFileDir.Available = true;
			}
			else if (IsModNode(ClickedNode))
			{
				if (ClickedNode.GetMetadata().ModFile.IsBundleContent)
					_contextOpenModBundleDir.Available = true;
				else
					_contextOpenModFile.Available = _contextOpenModFileDir.Available = true;
			}
		}

		if (ClickedNode == null && !this.IsEmpty())
		{
			_contextExpandAll.Available =
				CategoryNodes.Any(catNode => !catNode.IsExpanded)
				|| FileNodes.Any(fileNode => !fileNode.IsExpanded);

			_contextCollapseAll.Available = CategoryNodes.Any(node => node.IsExpanded);

			_contextAllSeparator.Visible =
				(_contextExpandAll.Available || _contextCollapseAll.Available)
				&& (ContextOpenRegion.Available || ContextNodeRegion.Available);
		}
	}

	private void SetContextMenuSize()
	{
		if (_contextMenu.Items.OfType<ToolStripItem>().Any(item => item.Available))
		{
			int width = _contextMenu.Items.OfType<ToolStripMenuItem>().Where(item => item.Available)
				.Max(item => TextRenderer.MeasureText(item.Text, item.Font).Width);
			int height = _contextMenu.GetAvailableItems()
				.Sum(item => item.Height);
			_contextMenu.Width = width + 45;
			_contextMenu.Height = height + 5;
		}
	}

	protected void ContextOpenFile_Click(object sender, EventArgs e)
	{
		if (RightClickedNode == null)
			return;

		_ = TryOpenFile(RightClickedNode.GetMetadata().FilePath);

		RightClickedNode = null;
	}

	protected void ContextOpenDirectory_Click(object sender, EventArgs e)
	{
		if (RightClickedNode == null)
			return;

		_ = TryOpenFileLocation(RightClickedNode.GetMetadata().FilePath);

		RightClickedNode = null;
	}

	protected void ContextOpenVanillaFile_Click(object sender, EventArgs e)
	{
		if (RightClickedNode == null)
			return;

		_ = TryOpenFile(RightClickedNode.GetMetadata().ModFile.GetVanillaFile());

		RightClickedNode = null;
	}

	protected void ContextOpenVanillaDirectory_Click(object sender, EventArgs e)
	{
		if (RightClickedNode == null)
			return;

		_ = TryOpenFileLocation(RightClickedNode.GetMetadata().ModFile.GetVanillaFile());

		RightClickedNode = null;
	}

	private void ContextCopyPath_Click(object sender, EventArgs e)
	{
		if (RightClickedNode == null)
			return;

		Clipboard.SetText(RightClickedNode.GetMetadata().FilePath);

		RightClickedNode = null;
	}

	protected void ContextSelectAll_Click(object sender, EventArgs e) => SetAllChecked(true);

	protected void ContextDeselectAll_Click(object sender, EventArgs e) => SetAllChecked(false);

	protected abstract void SetAllChecked(bool isChecked);

	private void ContextExpandAll_Click(object sender, EventArgs e) => ExpandAll();

	private void ContextCollapseAll_Click(object sender, EventArgs e) => CollapseAll();

	private void ContextMenu_Closing(object sender, ToolStripDropDownClosingEventArgs e)
	{
		if (ClickedNode == null)
			return;

		RightClickedNode = ClickedNode;  // Preserve reference to clicked node so context item handlers can access,
		ClickedNode = null;              // but clear ClickedNode so mouseover doesn't change back color.
	}

	#endregion
}