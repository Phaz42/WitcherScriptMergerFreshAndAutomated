using System;
using System.Drawing;
using System.Windows.Forms;

using WitcherScriptMerger.Theming;

namespace WitcherScriptMerger.Forms;

/// <summary>
/// A panel over the window it was opened from, with a title, a × at the top right, and a body each kind fills: a help
/// topic (HelpPanel), the conflict list's filter (FilterPanel). The ×, Esc or a click on the window it opened over closes
/// it; switching to another program does not. One at a time; a click on the button that opened the one already open
/// closes it rather than opening it again.
/// </summary>
internal abstract class OverlayPanel : Form
{
	private static OverlayPanel _open;
	private static (string Key, DateTime When) _lastClosed;

	private readonly Form _owner;
	private readonly string _key;
	private readonly Font _titleFont = new("Segoe UI", 14F, FontStyle.Bold);
	private readonly Font _closeFont = new("Segoe UI", 12F);

	/// <summary>Where each kind puts its content, below the title.</summary>
	protected Panel Body { get; }

	/// <summary>
	/// Opens a panel over the owner, closing any other one first - unless the click that asks for it is the one that just
	/// closed it, by taking the focus from it: then it stays closed.
	/// </summary>
	protected static void Open(Form owner, string key, Func<OverlayPanel> create)
	{
		ArgumentNullException.ThrowIfNull(owner);
		ArgumentNullException.ThrowIfNull(create);

		if (_lastClosed.Key == key && (DateTime.UtcNow - _lastClosed.When).TotalMilliseconds < 300)
			return;

		_open?.Close();
		_open = create();
		_open.Show(owner);
	}

	/// <param name="widthShare">The share of the owner's width it takes, at least <paramref name="minWidth"/> at 96 dpi.</param>
	protected OverlayPanel(Form owner, string key, string title, double widthShare, int minWidth)
	{
		ArgumentNullException.ThrowIfNull(owner);
		_owner = owner;
		_key = key;
		_owner.Activated += OwnerActivated;
		Theme theme = ThemeManager.CurrentTheme;

		AutoScaleMode = AutoScaleMode.None;  // sized in device units below
		FormBorderStyle = FormBorderStyle.None;
		ShowInTaskbar = false;
		StartPosition = FormStartPosition.Manual;
		KeyPreview = true;
		Text = title;
		// The frame: the padding shows it around the content, solid and wide enough to set the panel off from the window.
		BackColor = theme.AccentSubtleColor;
		Padding = new Padding(owner.LogicalToDeviceUnits(5));

		// Large: most of the window it opens over, within the screen.
		Rectangle screen = Screen.FromControl(owner).WorkingArea;
		int width = Math.Min(Math.Max((int)(owner.Width * widthShare), owner.LogicalToDeviceUnits(minWidth)), screen.Width - owner.LogicalToDeviceUnits(40));
		int height = Math.Min(Math.Max(owner.Height * 3 / 4, owner.LogicalToDeviceUnits(420)), screen.Height - owner.LogicalToDeviceUnits(40));
		Bounds = new Rectangle(
			Math.Clamp(owner.Left + ((owner.Width - width) / 2), screen.Left, screen.Right - width),
			Math.Clamp(owner.Top + ((owner.Height - height) / 2), screen.Top, screen.Bottom - height),
			width, height);

		Color back = theme.TreeViewBackColor;
		int pad = owner.LogicalToDeviceUnits(20);

		Label titleLabel = new()
		{
			AutoEllipsis = true,
			BackColor = back,
			Dock = DockStyle.Fill,
			Font = _titleFont,
			ForeColor = theme.AccentColor,
			Text = title,
			TextAlign = ContentAlignment.MiddleLeft,
			UseMnemonic = false
		};
		CloseButton close = new(back) { Dock = DockStyle.Right, Font = _closeFont, Width = owner.LogicalToDeviceUnits(44) };
		close.Click += (sender, e) => Close();

		Panel header = new() { BackColor = back, Dock = DockStyle.Top, Height = owner.LogicalToDeviceUnits(52), Padding = new Padding(pad, 0, owner.LogicalToDeviceUnits(4), 0) };
		header.Controls.Add(titleLabel);
		header.Controls.Add(close);

		Body = new Panel { BackColor = back, Dock = DockStyle.Fill, Padding = new Padding(pad, 0, owner.LogicalToDeviceUnits(4), pad / 2) };

		Controls.Add(Body);
		Controls.Add(header);
	}

	// Opened, nothing has the focus: no caret in a text, and Esc reaches the panel.
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
		_lastClosed = (_key, DateTime.UtcNow);
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

	// The fonts after the controls that use them.
	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		if (disposing)
		{
			_titleFont.Dispose();
			_closeFont.Dispose();
		}
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
				using SolidBrush hover = new(Colors.Mix(theme.ForeColor, _back, _pressed ? 0.24 : 0.12));
				g.FillRectangle(hover, square);
			}

			TextRenderer.DrawText(g, "✕", Font, square, _hovered ? theme.ForeBrightColor : theme.ForeDimmedColor,
				TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
		}
	}
}
