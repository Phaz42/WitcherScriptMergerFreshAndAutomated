using System;
using System.Drawing;
using System.Windows.Forms;

using WitcherScriptMerger.Controls;
using WitcherScriptMerger.Theming;
using WitcherScriptMerger.UI;

namespace WitcherScriptMerger.Forms;

/// <summary>
/// A help topic (HelpTopics), opened by a "?" (HelpButton): a large panel over the window it was opened from, with the
/// topic's title, its formatted text, which scrolls, and a × at the top right. The ×, Esc or a click on the window it
/// opened over closes it; switching to another program does not. One at a time; a click on the "?" of the topic already
/// open closes it rather than opening it again.
/// </summary>
internal sealed class HelpPanel : Form
{
	private static HelpPanel _open;
	private static (string Title, DateTime When) _lastClosed;

	private readonly HelpTopic _topic;
	private readonly RichTextBox _text;
	private readonly Font _textFont = new("Segoe UI", 10.5F);
	private readonly Font _headingFont = new("Segoe UI", 12F, FontStyle.Bold);
	private readonly Font _exampleFont = new("Consolas", 10.5F);
	private readonly Font _titleFont = new("Segoe UI", 14F, FontStyle.Bold);

	/// <summary>Opens the topic over the owner, closing any other one first.</summary>
	internal static void ShowTopic(Form owner, HelpTopic topic)
	{
		ArgumentNullException.ThrowIfNull(owner);
		ArgumentNullException.ThrowIfNull(topic);

		// The click on a "?" that just closed this topic's panel - by taking the focus from it - must not open it again.
		if (_lastClosed.Title == topic.Title && (DateTime.UtcNow - _lastClosed.When).TotalMilliseconds < 300)
			return;

		_open?.Close();
		_open = new HelpPanel(owner, topic);
		_open.Show(owner);
	}

	private readonly Form _owner;

	private HelpPanel(Form owner, HelpTopic topic)
	{
		_topic = topic;
		_owner = owner;
		_owner.Activated += OwnerActivated;
		Theme theme = ThemeManager.CurrentTheme;

		AutoScaleMode = AutoScaleMode.None;  // sized in device units below
		FormBorderStyle = FormBorderStyle.None;
		ShowInTaskbar = false;
		StartPosition = FormStartPosition.Manual;
		KeyPreview = true;
		Text = topic.Title;
		// The frame: the padding shows it around the content, solid and wide enough to set the panel off from the window.
		BackColor = theme.AccentSubtleColor;
		Padding = new Padding(owner.LogicalToDeviceUnits(5));

		// Large: most of the window it opens over, within the screen.
		Rectangle screen = Screen.FromControl(owner).WorkingArea;
		int width = Math.Min(Math.Max(owner.Width * 6 / 10, owner.LogicalToDeviceUnits(560)), screen.Width - owner.LogicalToDeviceUnits(40));
		int height = Math.Min(Math.Max(owner.Height * 3 / 4, owner.LogicalToDeviceUnits(420)), screen.Height - owner.LogicalToDeviceUnits(40));
		Bounds = new Rectangle(
			Math.Clamp(owner.Left + ((owner.Width - width) / 2), screen.Left, screen.Right - width),
			Math.Clamp(owner.Top + ((owner.Height - height) / 2), screen.Top, screen.Bottom - height),
			width, height);

		Color back = theme.TreeViewBackColor;
		int pad = owner.LogicalToDeviceUnits(20);

		Label title = new()
		{
			AutoEllipsis = true,
			BackColor = back,
			Dock = DockStyle.Fill,
			Font = _titleFont,
			ForeColor = theme.AccentColor,
			Text = topic.Title,
			TextAlign = ContentAlignment.MiddleLeft,
			UseMnemonic = false
		};
		CloseButton close = new(back) { Dock = DockStyle.Right, Width = owner.LogicalToDeviceUnits(44) };
		close.Click += (sender, e) => Close();

		Panel header = new() { BackColor = back, Dock = DockStyle.Top, Height = owner.LogicalToDeviceUnits(52), Padding = new Padding(pad, 0, owner.LogicalToDeviceUnits(4), 0) };
		header.Controls.Add(title);
		header.Controls.Add(close);

		_text = new RichTextBox
		{
			BackColor = back,
			BorderStyle = BorderStyle.None,
			DetectUrls = false,
			Dock = DockStyle.Fill,
			Font = _textFont,
			ForeColor = theme.ForeColor,
			ReadOnly = true,
			ScrollBars = RichTextBoxScrollBars.Vertical,
			TabStop = false,
			WordWrap = true
		};
		_text.HandleCreated += (sender, e) => ThemeUtils.ApplyWindowTheme(_text);  // dark scrollbar in the dark theme
		SpaceWordBreak.Apply(_text);  // a path or a name stays on one line
		_text.BulletIndent = owner.LogicalToDeviceUnits(16);  // from a bullet to its text

		Panel body = new() { BackColor = back, Dock = DockStyle.Fill, Padding = new Padding(pad, 0, owner.LogicalToDeviceUnits(4), pad / 2) };
		body.Controls.Add(_text);

		Controls.Add(body);
		Controls.Add(header);
	}

	// Filled once its handle exists; each run's color and font set as it is written, as in the problems report.
	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);

		Theme theme = ThemeManager.CurrentTheme;
		int indent = LogicalToDeviceUnits(22);
		bool first = true;
		foreach (HelpLine line in _topic.Lines)
		{
			// A new paragraph takes the previous one's format: it is set for each line, and a heading's blank line before
			// it is a plain paragraph, not a bullet.
			if (!first)
			{
				Append(Environment.NewLine, _textFont, theme.ForeColor);
				if (line.Style == HelpStyle.Heading)
				{
					SetParagraph(HelpStyle.Text, indent);
					Append(Environment.NewLine, _textFont, theme.ForeColor);
				}
			}

			first = false;
			SetParagraph(line.Style, indent);

			switch (line.Style)
			{
				case HelpStyle.Heading:
					Append(line.Text, _headingFont, theme.AccentColor);
					break;
				case HelpStyle.Example:
					Append(line.Example, _exampleFont, theme.ForeBrightColor);
					if (!string.IsNullOrEmpty(line.Text))
						Append("   " + line.Text, _textFont, theme.ForeDimmedColor);
					break;
				default:
					Append(line.Text, _textFont, theme.ForeColor);
					break;
			}
		}

		_text.SelectionStart = 0;
		_text.SelectionLength = 0;
	}

	// Examples and bullets indented alike; a bullet's text, wrapped lines included, starts after its bullet.
	private void SetParagraph(HelpStyle style, int indent)
	{
		_text.SelectionStart = _text.TextLength;
		_text.SelectionLength = 0;
		_text.SelectionBullet = style == HelpStyle.Bullet;
		_text.SelectionIndent = style is HelpStyle.Example or HelpStyle.Bullet ? indent : 0;
	}

	private void Append(string text, Font font, Color color)
	{
		_text.SelectionStart = _text.TextLength;
		_text.SelectionLength = 0;
		_text.SelectionFont = font;
		_text.SelectionColor = color;
		_text.AppendText(text);
	}

	// Opened, nothing has the focus: no caret in the text, and Esc reaches the panel.
	protected override void OnShown(EventArgs e)
	{
		base.OnShown(e);
		ActiveControl = null;
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);
		if (e.KeyCode == Keys.Escape)
		{
			e.Handled = e.SuppressKeyPress = true;
			Close();
		}
	}

	// A click on the window it opened over makes that window the active one, and closes it. Another program taking the
	// focus does not: the panel is there again when SM-FAE is.
	private void OwnerActivated(object sender, EventArgs e)
	{
		if (!IsDisposed)
			Close();
	}

	protected override void OnFormClosed(FormClosedEventArgs e)
	{
		_owner.Activated -= OwnerActivated;
		_lastClosed = (_topic.Title, DateTime.UtcNow);
		if (_open == this)
			_open = null;
		base.OnFormClosed(e);
	}

	// Rounded corners where Windows draws them for a window of its own (Windows 11); elsewhere it stays square.
	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);
		int round = 2;  // DWMWCP_ROUND
		Color frame = ThemeManager.CurrentTheme.AccentSubtleColor;
		int frameColor = frame.R | (frame.G << 8) | (frame.B << 16);  // COLORREF: Windows' own 1-pixel edge, in the frame's color
		try
		{
			Program.DwmSetWindowAttribute(Handle, Program.DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE, ref round, sizeof(int));
			Program.DwmSetWindowAttribute(Handle, Program.DWMWINDOWATTRIBUTE.DWMWA_BORDER_COLOR, ref frameColor, sizeof(int));
		}
		catch (Exception ex) when (ex is System.Runtime.InteropServices.COMException or ArgumentException)
		{
			// Not supported by this Windows: square corners.
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_text.Dispose();
			_textFont.Dispose();
			_headingFont.Dispose();
			_exampleFont.Dispose();
			_titleFont.Dispose();
		}

		base.Dispose(disposing);
	}

	// The × at the top right: the dimmed text color, brighter on a faint square while hovered, darker while pressed.
	private sealed class CloseButton : Control
	{
		private readonly Color _back;
		private bool _hovered, _pressed;

		internal CloseButton(Color back)
		{
			_back = back;
			SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint
				| ControlStyles.ResizeRedraw, true);
			Cursor = Cursors.Hand;
			Font = new Font("Segoe UI", 12F);
			TabStop = false;
			AccessibleName = "Close";
			AccessibleRole = AccessibleRole.PushButton;
		}

		protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); _hovered = true; Invalidate(); }

		protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); _hovered = _pressed = false; Invalidate(); }

		protected override void OnMouseDown(MouseEventArgs e) { base.OnMouseDown(e); _pressed = e.Button == MouseButtons.Left; Invalidate(); }

		protected override void OnMouseUp(MouseEventArgs e) { base.OnMouseUp(e); _pressed = false; Invalidate(); }

		protected override void OnPaint(PaintEventArgs e)
		{
			Theme theme = ThemeManager.CurrentTheme;
			Graphics g = e.Graphics;
			g.Clear(_back);

			int side = Math.Min(Width, Height) - LogicalToDeviceUnits(12);
			Rectangle square = new((Width - side) / 2, (Height - side) / 2, side, side);
			if (_hovered)
			{
				using SolidBrush hover = new(Mix(theme.ForeColor, _back, _pressed ? 0.24 : 0.12));
				g.FillRectangle(hover, square);
			}

			TextRenderer.DrawText(g, "✕", Font, square, _hovered ? theme.ForeBrightColor : theme.ForeDimmedColor,
				TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
		}

		private static Color Mix(Color color, Color under, double amount) => Color.FromArgb(
			(int)Math.Round(under.R + ((color.R - under.R) * amount)),
			(int)Math.Round(under.G + ((color.G - under.G) * amount)),
			(int)Math.Round(under.B + ((color.B - under.B) * amount)));
	}
}
