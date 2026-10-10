using System.Drawing;
using System.Windows.Forms;

using WitcherScriptMerger.Controls;

namespace WitcherScriptMerger.Forms;

/// <summary>
/// The keyboard shortcuts, from Menu: grouped by where they work, each key beside what it does, in the theme. It was a
/// Windows message box, always light, whose list had fallen behind the keys (MainForm.MainForm_KeyDown, SMTree.OnKeyDown).
/// </summary>
internal sealed class KeyboardShortcutsForm : BaseForm
{
	private static readonly (string Heading, string Note, (string Key, string Action)[] Keys)[] Groups =
	[
		("Main Window", "Letters and Esc: not while typing in a search box", [
			("F5", "Refresh the conflicts list"),
			("F6", "Refresh the merges list"),
			("C", "Create the selected merges"),
			("A", "Create all script merges"),
			("D", "Delete the selected merges"),
			("E", "Delete all merges"),
			("Esc", "Exit SM-FAE"),
		]),
		("Lists", null, [
			("Ctrl+A", "Select everything in the list"),
			("Ctrl+D", "Deselect everything in the list"),
			("Ctrl+F", "Search the list"),
		]),
		("Search Box", null, [
			("Enter", "Search now"),
			("Esc", "Clear the search; when it's empty, back to the list"),
		]),
	];

	private readonly Font _textFont = new("Segoe UI", 11F);
	private readonly Font _keyFont = new("Segoe UI", 11F, FontStyle.Bold);
	private readonly Font _noteFont = new("Segoe UI", 10F);

	internal KeyboardShortcutsForm()
	{
		// BaseForm, for its title bar in the theme; it scales by font, and this form sizes itself in device units.
		AutoScaleMode = AutoScaleMode.None;
		Text = "Keyboard Shortcuts";
		Icon = Program.MainFrm.Icon;
		Font = _textFont;
		FormBorderStyle = FormBorderStyle.FixedDialog;
		MaximizeBox = false;
		MinimizeBox = false;
		ShowInTaskbar = false;
		StartPosition = FormStartPosition.CenterParent;
		AutoSize = true;
		AutoSizeMode = AutoSizeMode.GrowAndShrink;

		TableLayoutPanel table = new()
		{
			AutoSize = true,
			ColumnCount = 2,
			Dock = DockStyle.Fill,
			Padding = Scaled(new Padding(24, 16, 32, 8))
		};
		_ = table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
		_ = table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

		bool first = true;
		foreach ((string heading, string note, (string Key, string Action)[] keys) in Groups)
		{
			AddRow(table, new Label { AutoSize = true, Font = _keyFont, Margin = Scaled(new Padding(0, first ? 0 : 16, 0, 2)), Tag = "AccentLabel", Text = heading });
			first = false;
			if (note != null)
				AddRow(table, new Label { AutoSize = true, Font = _noteFont, Margin = Scaled(new Padding(0, 0, 0, 4)), Tag = "DimmedLabel", Text = note });

			foreach ((string key, string action) in keys)
			{
				_ = table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
				table.Controls.Add(new Label { AutoSize = true, Font = _keyFont, Margin = Scaled(new Padding(0, 4, 24, 4)), Text = key });
				table.Controls.Add(new Label { AutoSize = true, Font = _textFont, Margin = Scaled(new Padding(0, 4, 0, 4)), Text = action });
			}
		}

		// A minimum size, not only a size: an auto-sized button shrinks to its text when it becomes the default button.
		RoundButton ok = new()
		{
			Anchor = AnchorStyles.Right,
			AutoSize = true,
			DialogResult = DialogResult.OK,
			Font = _textFont,
			Margin = Scaled(new Padding(0, 16, 0, 8)),
			MinimumSize = new Size(160, 50),
			Size = new Size(160, 50),
			Text = "OK"
		};
		_ = table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
		table.Controls.Add(ok, 0, table.RowStyles.Count - 1);
		table.SetColumnSpan(ok, 2);
		AcceptButton = ok;
		CancelButton = ok;

		Controls.Add(table);
	}

	// A heading or a note: one row across both columns.
	private static void AddRow(TableLayoutPanel table, Label label)
	{
		_ = table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
		table.Controls.Add(label, 0, table.RowStyles.Count - 1);
		table.SetColumnSpan(label, 2);
	}

	private Padding Scaled(Padding padding) => new(
		LogicalToDeviceUnits(padding.Left), LogicalToDeviceUnits(padding.Top),
		LogicalToDeviceUnits(padding.Right), LogicalToDeviceUnits(padding.Bottom));

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_textFont.Dispose();
			_keyFont.Dispose();
			_noteFont.Dispose();
		}

		base.Dispose(disposing);
	}
}
