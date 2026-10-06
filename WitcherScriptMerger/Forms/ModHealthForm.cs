using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using WitcherScriptMerger.FileIndex;
using WitcherScriptMerger.Theming;

using static WitcherScriptMerger.Program;

namespace WitcherScriptMerger.Forms;

/// <summary>
/// The problem check's report (ModHealth), opened from the status bar: headings and mod names in bold, details indented,
/// to scroll through and copy - as plain text - into a forum post or bug report.
/// </summary>
internal sealed class ModHealthForm : Form
{
	private readonly ModHealth _health;
	private readonly RichTextBox _text;
	private readonly Font _textFont = new("Segoe UI", 10.5F);
	private readonly Font _boldFont = new("Segoe UI", 10.5F, FontStyle.Bold);
	private readonly Font _headingFont = new("Segoe UI", 12F, FontStyle.Bold);

	internal ModHealthForm(ModHealth health)
	{
		_health = health;

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

		Button copy = new() { AutoSize = true, Font = new Font("Segoe UI", 11F), Margin = new Padding(0, 0, 8, 0), Size = new Size(160, 50), Text = "&Copy" };
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

		Button close = new() { AutoSize = true, DialogResult = DialogResult.OK, Font = new Font("Segoe UI", 11F), Margin = new Padding(0, 0, 16, 0), Size = new Size(160, 50), Text = "OK" };
		AcceptButton = close;
		CancelButton = close;

		// The theme colours a TableLayoutPanel and its children, and scales the button row's height (designed at 125%).
		TableLayoutPanel buttons = new() { ColumnCount = 3, Dock = DockStyle.Fill, Margin = new Padding(0), RowCount = 1 };
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

	// Filled once the theme is applied: each line's colour is set as it is written, from the theme in force.
	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);

		Color textColor = ThemeManager.CurrentTheme.ForeColor;
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
				ModHealth.LineStyle.Heading => _headingFont,
				ModHealth.LineStyle.Bold => _boldFont,
				_ => _textFont
			};
			_text.SelectionColor = textColor;
			_text.SelectionIndent = style == ModHealth.LineStyle.Detail ? indent : 0;
			_text.AppendText(line);
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
			_textFont.Dispose();
			_boldFont.Dispose();
			_headingFont.Dispose();
		}

		base.Dispose(disposing);
	}
}
