using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using WitcherScriptMerger.Controls;
using WitcherScriptMerger.Theming;
using WitcherScriptMerger.UI;

namespace WitcherScriptMerger.Forms;

/// <summary>
/// The conflict list's filter, opened by the funnel left of its search box (FilterButton): every mod in the conflict list,
/// each with a checkbox and the number of conflicts it is in. Unchecking a mod hides the conflicts that are there only
/// because of it; in the others it stays, dimmed, since their merges include it (TreeManager). Nothing in the load order
/// or the merges changes. Each change applies at once and is remembered.
/// </summary>
internal sealed class FilterPanel : OverlayPanel
{
	private const string PanelKey = "filter";

	private readonly TreeManager _trees;
	private readonly List<ThemedCheckBox> _boxes = [];
	private readonly Label _hint;
	private readonly Font _hintFont = new("Segoe UI", 10F);
	private readonly Font _listFont = new("Segoe UI", 10.5F);
	private bool _settingAll;

	/// <summary>Opens the filter over the owner, closing any other panel first.</summary>
	internal static void ShowFilter(Form owner, TreeManager trees) =>
		Open(owner, PanelKey, () => new FilterPanel(owner, trees));

	private FilterPanel(Form owner, TreeManager trees) : base(owner, PanelKey, "Filter the conflict list", 0.4, 460)
	{
		_trees = trees;
		Theme theme = ThemeManager.CurrentTheme;
		Color back = theme.TreeViewBackColor;

		List<(string Mod, int Conflicts)> mods = trees.ConflictModCounts().ToList();
		_hint = new Label
		{
			BackColor = back,
			Dock = DockStyle.Top,
			Font = _hintFont,
			ForeColor = theme.ForeDimmedColor,
			Padding = new Padding(0, 0, 0, owner.LogicalToDeviceUnits(12)),
			Text = mods.Count == 0
				? "There are no conflicts to filter."
				: "Uncheck a mod to hide the conflicts that are only there because of it. A conflict with two or more other mods "
					+ "stays, with that mod dimmed: its merge includes it. Your load order and your merges don't change.",
			UseMnemonic = false
		};

		TableLayoutPanel list = new()
		{
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			BackColor = back,
			ColumnCount = 2,
			Dock = DockStyle.Top,
			Padding = new Padding(0, 0, owner.LogicalToDeviceUnits(12), 0)  // clear of the scrollbar
		};
		_ = list.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
		_ = list.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
		list.SuspendLayout();
		foreach ((string mod, int conflicts) in mods)
		{
			ThemedCheckBox box = new()
			{
				AutoSize = true,
				BackColor = back,
				Checked = !trees.IsFiltered(mod),
				Font = _listFont,
				ForeColor = theme.ForeColor,
				Margin = new Padding(0, 0, 0, owner.LogicalToDeviceUnits(2)),
				Text = mod,
				UseMnemonic = false
			};
			box.CheckedChanged += (sender, e) => Apply();
			Label count = new()
			{
				Anchor = AnchorStyles.Right,
				AutoSize = true,
				BackColor = back,
				Font = _hintFont,
				ForeColor = theme.ForeDimmedColor,
				Text = conflicts == 1 ? "1 conflict" : $"{conflicts} conflicts"
			};

			_ = list.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			list.Controls.Add(box, 0, list.RowCount);
			list.Controls.Add(count, 1, list.RowCount);
			list.RowCount++;
			_boxes.Add(box);
		}

		list.ResumeLayout();

		Panel scroller = new() { AutoScroll = true, BackColor = back, Dock = DockStyle.Fill };
		scroller.HandleCreated += (sender, e) => ThemeUtils.ApplyWindowTheme(scroller);  // dark scrollbar in the dark theme
		scroller.Controls.Add(list);

		LinkLabel showAll = new()
		{
			ActiveLinkColor = theme.LinkColor,
			AutoSize = true,
			BackColor = back,
			Dock = DockStyle.Left,
			Font = _hintFont,
			LinkBehavior = LinkBehavior.HoverUnderline,
			LinkColor = theme.LinkColor,
			Text = "Show all mods",
			TextAlign = ContentAlignment.MiddleLeft,
			Visible = mods.Count > 0
		};
		showAll.LinkClicked += (sender, e) => ShowAll();
		Panel footer = new() { BackColor = back, Dock = DockStyle.Bottom, Height = owner.LogicalToDeviceUnits(40) };
		footer.Controls.Add(showAll);

		// Docked from the last added: the hint at the top, the link at the bottom, the list between them.
		Body.Controls.Add(scroller);
		Body.Controls.Add(footer);
		Body.Controls.Add(_hint);
	}

	// The hint wraps to the panel's width.
	protected override void OnLayout(LayoutEventArgs levent)
	{
		if (_hint != null)
		{
			int width = Body.ClientSize.Width - Body.Padding.Horizontal;
			int height = TextRenderer.MeasureText(_hint.Text, _hint.Font, new Size(width, 0), TextFormatFlags.WordBreak).Height
				+ _hint.Padding.Vertical;
			if (_hint.Height != height)
				_hint.Height = height;
		}

		base.OnLayout(levent);
	}

	private void ShowAll()
	{
		_settingAll = true;
		foreach (ThemedCheckBox box in _boxes)
			box.Checked = true;
		_settingAll = false;
		Apply();
	}

	// The mods unchecked now: those the filter hides from here on. One no longer in the list is no longer kept.
	private void Apply()
	{
		if (!_settingAll)
			_trees.SetFilteredMods(_boxes.Where(box => !box.Checked).Select(box => box.Text));
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_hint.Dispose();
			_hintFont.Dispose();
			_listFont.Dispose();
		}

		base.Dispose(disposing);
	}
}
