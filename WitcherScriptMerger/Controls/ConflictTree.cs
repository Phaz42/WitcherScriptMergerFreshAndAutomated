using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
		Dictionary<string, (int Number, bool Effective)> loadOrderNumbers = Program.LoadOrder.GetLoadOrderNumbers(InstalledModNames());
		foreach (TreeNode fileNode in FileNodes)
		{
			IEnumerable<string> modNames = fileNode.GetTreeNodes().Select(modNode => modNode.Text);
			bool isResolved = Program.LoadOrder.HasResolvedConflict(modNames);
			string topPriorityMod = isResolved ? Program.LoadOrder.GetTopPriorityEnabledMod(modNames) : null;
			bool isMergeable = (fileNode.Parent.Tag as ModFileCategory).IsSupported;

			foreach (TreeNode modNode in fileNode.GetTreeNodes())
			{
				// The number before the mod's name, in non-mergeable conflicts only (OnDrawNode).
				NodeMetadata numbered = (NodeMetadata)modNode.Tag;
				numbered.LoadOrderNumber = null;
				if (!isMergeable && loadOrderNumbers.TryGetValue(modNode.Text, out (int Number, bool Effective) loadOrder))
				{
					numbered.LoadOrderNumber = loadOrder.Number;
					numbered.IsEffectiveLoadOrderNumber = loadOrder.Effective;
				}

				// The load order shows in the numbers and the winner's colour; only what the colour means is said in words.
				modNode.ToolTipText = "";
				int priority = Program.LoadOrder.GetPriorityByName(modNode.Text);

				if (modNode.Text.EqualsIgnoreCase(topPriorityMod))
				{
					if (priority > -1)
					{
						modNode.ToolTipText = isMergeable
							? "Wins this conflict while it isn't merged: without a merge, the game uses only this mod's copy."
							: "Wins this conflict: the game uses this mod's copy.";
						modNode.ForeColor = ((NodeMetadata)modNode.Tag).ForeColor = ThemeManager.CurrentTheme.AccentColor;
					}
				}
				else
				{
					modNode.ForeColor = ((NodeMetadata)modNode.Tag).ForeColor = ThemeManager.CurrentTheme.ForeDimmedColor;
				}

				if (Program.LoadOrder.IsModDisabledByName(modNode.Text))
				{
					// Add underline
					modNode.ToolTipText = "Disabled in your load order: the game doesn't load this mod.";
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
					modNode.ToolTipText = WithLine($"Made for an older game version: missing {outdated.Describe()}", modNode.ToolTipText);
				}
				else if (health != null && health.IsOldMerge(fileNode.Text, out string mergedModName, out outdated)
					&& modNode.Text.EqualsIgnoreCase(mergedModName))
				{
					// The existing merge, listed here because another mod now conflicts with it.
					metadata.IsOutdated = true;
					modNode.ForeColor = metadata.ForeColor = ThemeManager.CurrentTheme.WarningColor;
					modNode.ToolTipText = WithLine($"An old merge, made against an older game version (missing {outdated.Describe()}): delete it and merge again.", modNode.ToolTipText);
				}
			}

			if (!isMergeable)
				ReorderByLoadOrder(fileNode);
		}

		Invalidate();  // the numbers
	}

	private static string WithLine(string first, string rest) => string.IsNullOrEmpty(rest) ? first : first + "\n" + rest;

	// A changed priority can change a non-mergeable conflict's winner, and with it the order its mods are listed in: the
	// mods are added again, which puts each in its sorted place. They show no checkboxes, and adding loses that state.
	private void ReorderByLoadOrder(TreeNode fileNode)
	{
		TreeNode[] mods = fileNode.GetTreeNodes().ToArray();
		if (mods.Zip(mods.Skip(1)).All(pair => Program.LoadOrder.CompareByLoadOrder(pair.First.Text, pair.Second.Text) <= 0))
			return;

		BeginUpdate();
		foreach (TreeNode modNode in mods)
			modNode.Remove();
		foreach (TreeNode modNode in mods)
		{
			_ = fileNode.Nodes.Add(modNode);
			modNode.SetIsCheckBoxVisible(false);
		}

		EndUpdate();
	}

	private static IEnumerable<string> InstalledModNames()
	{
		try
		{
			return Directory.Exists(Paths.ModsDirectory)
				? Directory.GetDirectories(Paths.ModsDirectory, "mod*", SearchOption.TopDirectoryOnly).Select(Path.GetFileName).ToArray()
				: [];
		}
		catch (Exception e) when (e is IOException or UnauthorizedAccessException)
		{
			return [];
		}
	}

	#region Load order numbers

	private readonly Font _numberFont = new("Segoe UI", 10F, FontStyle.Bold);
	private readonly Font _effectiveNumberFont = new("Segoe UI", 10F, FontStyle.Bold | FontStyle.Italic);

	// The number sits in the empty indent before a mod's name, right-aligned against it, on the name's baseline: Windows
	// centres the name in its row, and the number's own font is a different size.
	private Rectangle NumberBounds(TreeNode modNode, NodeMetadata metadata)
	{
		Font font = metadata.IsEffectiveLoadOrderNumber ? _effectiveNumberFont : _numberFont;
		Font nameFont = modNode.NodeFont ?? Font;
		Size size = TextRenderer.MeasureText(metadata.LoadOrderNumber.Value.ToString(CultureInfo.InvariantCulture), font,
			Size.Empty, TextFormatFlags.NoPadding);
		Rectangle text = modNode.Bounds;
		int nameTop = text.Top + ((text.Height - TextRenderer.MeasureText("Ag", nameFont, Size.Empty, TextFormatFlags.NoPadding).Height) / 2);
		int top = nameTop + (int)Math.Round(AscentPixels(nameFont) - AscentPixels(font));
		int gap = LogicalToDeviceUnits(6);
		return new Rectangle(text.Left - gap - size.Width, top, size.Width, size.Height);
	}

	// The numbers are painted after Windows paints the tree, not by owner drawing: with owner-drawn text, WinForms draws
	// each node's text itself, clipped to the width Windows measured with the tree's font - cutting off the larger
	// category names - and fills a background that overlaps the checkboxes. Painted afterwards, the tree looks exactly as
	// before. Each number's box is filled with the background first, so painting it twice changes nothing.
	// Windows paints the tree into a buffer, the numbers are added there, and only then is it shown: painted onto the
	// screen after the tree, the numbers blinked on every repaint - and each hovered row's colour fade repaints.
	protected override void WndProc(ref Message m)
	{
		if (m.Msg == WmEraseBackground && IsHandleCreated)
		{
			m.Result = 1;  // the buffer below is cleared to the background instead
			return;
		}

		if (m.Msg == WmPaint && m.WParam == IntPtr.Zero && IsHandleCreated)
		{
			PaintBuffered();
			return;
		}

		base.WndProc(ref m);
	}

	private const int WmPaint = 0x000F;
	private const int WmEraseBackground = 0x0014;

	private void PaintBuffered()
	{
		IntPtr screen = BeginPaint(Handle, out PaintStruct paint);
		try
		{
			using BufferedGraphics buffer = BufferedGraphicsManager.Current.Allocate(screen, ClientRectangle);
			Graphics g = buffer.Graphics;
			g.Clear(BackColor);

			// The tree paints into the device context a paint message carries, when it carries one.
			IntPtr hdc = g.GetHdc();
			try
			{
				Message treePaint = Message.Create(Handle, WmPaint, hdc, IntPtr.Zero);
				base.WndProc(ref treePaint);
			}
			finally
			{
				g.ReleaseHdc(hdc);
			}

			PaintLoadOrderNumbers(g);
			buffer.Render();
		}
		finally
		{
			_ = EndPaint(Handle, ref paint);
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	private unsafe struct PaintStruct
	{
		internal IntPtr Hdc;
		internal int Erase;
		internal int Left, Top, Right, Bottom;
		internal int Restore;
		internal int IncUpdate;
		internal fixed byte Reserved[32];
	}

	[LibraryImport("user32.dll")]
	private static partial IntPtr BeginPaint(IntPtr hWnd, out PaintStruct paint);

	[LibraryImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool EndPaint(IntPtr hWnd, ref PaintStruct paint);

	private void PaintLoadOrderNumbers(Graphics g)
	{
		Theme theme = ThemeManager.CurrentTheme;
		using SolidBrush background = new(BackColor);

		for (TreeNode node = TopNode; node != null && node.Bounds.Top < ClientSize.Height; node = node.NextVisibleNode)
		{
			if (node.Tag is not NodeMetadata { LoadOrderNumber: int number } metadata)
				continue;

			// A number the file does not hold is set apart: italic, and fainter - but still readable.
			Color color = metadata.IsEffectiveLoadOrderNumber ? theme.ForeFaintColor : theme.ForeDimmedColor;
			Rectangle box = NumberBounds(node, metadata);
			g.FillRectangle(background, box);
			TextRenderer.DrawText(g, number.ToString(CultureInfo.InvariantCulture),
				metadata.IsEffectiveLoadOrderNumber ? _effectiveNumberFont : _numberFont, box, color,
				TextFormatFlags.Right | TextFormatFlags.Top | TextFormatFlags.NoPadding);
		}
	}

	// Where the number answers the mouse: the whole indent before the name, at the row's full height. The number alone was
	// a target a few pixels across, missed as often as hit, and rows' numbers had gaps between them; rows' indents touch.
	private Rectangle NumberHoverBounds(TreeNode modNode, NodeMetadata metadata)
	{
		Rectangle number = NumberBounds(modNode, metadata);
		Rectangle row = modNode.Bounds;
		return Rectangle.FromLTRB(Math.Max(0, number.Left - LogicalToDeviceUnits(8)), row.Top, row.Left, row.Bottom);
	}

	// A font's ascent in pixels: the distance from the top of its line to the baseline.
	private float AscentPixels(Font font) =>
		font.SizeInPoints * DeviceDpi / 72f * font.FontFamily.GetCellAscent(font.Style) / font.FontFamily.GetEmHeight(font.Style);

	/// <summary>The tooltip of the load order number under the mouse, if it points at one; it explains the number.</summary>
	internal string LoadOrderNumberToolTip(TreeNode node, Point location)
	{
		if (node?.Tag is not NodeMetadata { LoadOrderNumber: int number } metadata || !NumberHoverBounds(node, metadata).Contains(location))
			return null;

		return metadata.IsEffectiveLoadOrderNumber
			? $"Not numbered in mods.settings: this mod loads after every numbered mod, in name order. {number} is the number it effectively has."
			: "Load order number from mods.settings. The lowest number wins.";
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_numberFont.Dispose();
			_effectiveNumberFont.Dispose();
		}

		base.Dispose(disposing);
	}

	#endregion

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

		// Only the conflicts the load order resolves - the ones with a winner coloured - as the option's label says.
		if (Settings.Get<bool>("CollapseCustomLoadOrder") && Program.LoadOrder != null)
		{
			foreach (TreeNode fileNode in FileNodes)
			{
				if (Program.LoadOrder.HasResolvedConflict(fileNode.GetTreeNodes().Select(modNode => modNode.Text)))
					fileNode.Collapse();
			}
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
