using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using WitcherScriptMerger.Controls;
using WitcherScriptMerger.FileIndex;
using WitcherScriptMerger.Theming;

using static WitcherScriptMerger.Program;

namespace WitcherScriptMerger.Forms;

/// <summary>
/// The problem check's report (ModHealth), opened from the status bar: headings and mod names in bold, details indented,
/// to scroll through and copy - as plain text - into a forum post or bug report. It looks like the help and filter panels
/// (OverlayPanel): their background, the headings in the accent color, the window's edge in their frame's color; its
/// opening line larger, in the warning bar's amber.
/// </summary>
internal sealed class ModHealthForm : BaseForm
{
	private readonly ModHealth _health;
	private readonly RichTextBox _text;
	private readonly TableLayoutPanel _buttonRow;
	private readonly Font _textFont = new("Segoe UI", 10.5F);
	private readonly Font _boldFont = new("Segoe UI", 10.5F, FontStyle.Bold);
	private readonly Font _headingFont = new("Segoe UI", 12F, FontStyle.Bold);
	private readonly Font _titleFont = new("Segoe UI", 13F, FontStyle.Bold);

	internal ModHealthForm(ModHealth health)
	{
		_health = health;

		// BaseForm, for its title bar in the theme; it scales by font, and this form sizes itself in device units.
		AutoScaleMode = AutoScaleMode.None;
		Text = "Mods with Problems";
		Icon = MainFrm.Icon;
		StartPosition = FormStartPosition.CenterParent;
		ShowInTaskbar = false;
		MinimizeBox = false;
		Font = new Font("Segoe UI", 10F);
		ClientSize = LogicalToDeviceUnits(new Size(760, 540));
		MinimumSize = LogicalToDeviceUnits(new Size(420, 300));

		_text = new RichTextBox
		{
			BorderStyle = BorderStyle.None,
			DetectUrls = false,
			Dock = DockStyle.Fill,
			Font = _textFont,
			Margin = new Padding(16, 16, 8, 8),
			ReadOnly = true,
			ScrollBars = RichTextBoxScrollBars.Vertical,
			TabStop = false,
			WordWrap = true
		};
		_text.HandleCreated += (sender, e) => ThemeUtils.ApplyWindowTheme(_text);  // dark scrollbars in the dark theme
		SpaceWordBreak.Apply(_text);  // a path or mod name stays on one line

		// A minimum size, not only a size: an auto-sized button shrinks to its text when it becomes the default button.
		RoundButton copy = new() { AutoSize = true, Font = new Font("Segoe UI", 11F), Margin = new Padding(0, 0, 8, 0), MinimumSize = new Size(160, 50), Size = new Size(160, 50), Text = "&Copy" };
		copy.Click += (sender, e) =>
		{
			try
			{
				Clipboard.SetText(_health.Report());
			}
			catch (ExternalException)
			{
				// Another program holds the clipboard; the text can still be selected and copied by hand.
			}
		};

		RoundButton close = new() { AutoSize = true, DialogResult = DialogResult.OK, Font = new Font("Segoe UI", 11F), Margin = new Padding(0, 0, 16, 0), MinimumSize = new Size(160, 50), Size = new Size(160, 50), Text = "OK" };
		AcceptButton = close;
		CancelButton = close;

		// The theme colors a TableLayoutPanel and its children, and scales the button row's height (designed at 125%).
		TableLayoutPanel buttons = _buttonRow = new() { ColumnCount = 3, Dock = DockStyle.Fill, Margin = new Padding(0), RowCount = 1 };
		_ = buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
		_ = buttons.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
		_ = buttons.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
		_ = buttons.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
		buttons.Controls.Add(copy, 1, 0);
		buttons.Controls.Add(close, 2, 0);
		copy.Anchor = close.Anchor = AnchorStyles.Right;

		TableLayoutPanel layout = new() { ColumnCount = 1, Dock = DockStyle.Fill, Margin = new Padding(0), RowCount = 2 };
		_ = layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
		_ = layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
		_ = layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 84F));
		layout.Controls.Add(_text, 0, 0);
		layout.Controls.Add(buttons, 0, 1);
		Controls.Add(layout);
	}

	// Filled once the theme is applied: each line's color is set as it is written, from the theme in force. The panels'
	// background replaces the theme's window color on the report; the button row keeps it, since a button's face is the
	// panels' color.
	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);

		Theme theme = ThemeManager.CurrentTheme;
		SetBackColor(this, theme.TreeViewBackColor);
		Color textColor = theme.ForeColor;
		Color titleColor = Colors.ReadableOn(theme.WarningColor, theme.IsDark, theme.TreeViewBackColor);  // the warning bar's amber
		int indent = LogicalToDeviceUnits(24);
		bool first = true;
		foreach ((string line, ModHealth.LineStyle style) in _health.ReportLines())
		{
			if (!first)
				_text.AppendText(Environment.NewLine);
			first = false;

			_text.SelectionStart = _text.TextLength;
			_text.SelectionLength = 0;
			_text.SelectionFont = style switch
			{
				ModHealth.LineStyle.Title => _titleFont,
				ModHealth.LineStyle.Heading => _headingFont,
				ModHealth.LineStyle.Bold => _boldFont,
				_ => _textFont
			};
			_text.SelectionColor = style switch
			{
				ModHealth.LineStyle.Title => titleColor,
				ModHealth.LineStyle.Heading => theme.AccentColor,
				_ => textColor
			};
			_text.SelectionIndent = style == ModHealth.LineStyle.Detail ? indent : 0;
			_text.AppendText(line);
		}
	}

	private void SetBackColor(Control control, Color color)
	{
		if (control is Button || control == _buttonRow)
			return;

		control.BackColor = color;
		foreach (Control child in control.Controls)
			SetBackColor(child, color);
	}

	// The window's 1-pixel edge in the panels' frame color, where Windows draws it in a color of the program's (Windows 11).
	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);
		Color frame = ThemeManager.CurrentTheme.AccentSubtleColor;
		int frameColor = frame.R | (frame.G << 8) | (frame.B << 16);  // COLORREF
		try
		{
			DwmSetWindowAttribute(Handle, DWMWINDOWATTRIBUTE.DWMWA_BORDER_COLOR, ref frameColor, sizeof(int));
		}
		catch (Exception ex) when (ex is COMException or ArgumentException)
		{
			// Not supported by this Windows: its own edge.
		}
	}

	// Opened, the text is not selected - a whole selected report reads as a mistake.
	protected override void OnShown(EventArgs e)
	{
		base.OnShown(e);
		_text.SelectionStart = 0;
		_text.SelectionLength = 0;
		ActiveControl = null;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_text.Dispose();
			_buttonRow.Dispose();
			_textFont.Dispose();
			_boldFont.Dispose();
			_headingFont.Dispose();
			_titleFont.Dispose();
		}

		base.Dispose(disposing);
	}
}
