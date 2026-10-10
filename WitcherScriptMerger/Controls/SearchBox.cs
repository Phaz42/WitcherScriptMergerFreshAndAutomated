using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using WitcherScriptMerger.Theming;

using static WitcherScriptMerger.Program;

namespace WitcherScriptMerger.Controls;

/// <summary>
/// A search box with rounded corners, matching the buttons. A Windows text box cannot be made taller, padded or rounded,
/// so this draws the box itself - rounded, filled with the list's colour, with a border in the accent colour only while focused -
/// and holds a borderless text box inside, centred vertically with room left and right. The text box's height follows
/// its font; the box's height is the layout cell's.
/// While there is text, a cross at the right end clears it, as in a browser's search field; its room is kept when hidden,
/// so the text does not reflow as it appears.
/// </summary>
internal sealed class SearchBox : Panel
{
	// Rounded a little more than the header buttons (12, ThemeManager.ModernizeButton), and the room beside the text.
	private const int CornerRadius = 14;
	private const int HorizontalPadding = 10;

	// The clear button: its round hover area, and the cross inside it, at 96 dpi.
	private const int ClearButtonSize = 22;
	private const float ClearCrossHalf = 4.5f;

	private bool _clearHovered;

	internal TextBox TextBox { get; }

	/// <summary>The clear button was clicked; the text is already cleared.</summary>
	internal event EventHandler Cleared;

	private bool ClearButtonVisible => TextBox.TextLength > 0;

	private Rectangle ClearButtonBounds
	{
		get
		{
			int size = LogicalToDeviceUnits(ClearButtonSize);
			int right = ClientSize.Width - LogicalToDeviceUnits(HorizontalPadding / 2);
			return new Rectangle(right - size, (ClientSize.Height - size) / 2, size, size);
		}
	}

	internal SearchBox(TextBox textBox)
	{
		ArgumentNullException.ThrowIfNull(textBox);

		DoubleBuffered = true;
		ResizeRedraw = true;
		Cursor = Cursors.IBeam;

		TextBox = textBox;
		TextBox.BorderStyle = BorderStyle.None;
		TextBox.Enter += (sender, e) => Invalidate();
		TextBox.Leave += (sender, e) => Invalidate();
		TextBox.BackColorChanged += (sender, e) => Invalidate();
		TextBox.TextChanged += (sender, e) => Invalidate(ClearButtonBounds);
		Controls.Add(TextBox);
	}

	protected override void OnLayout(LayoutEventArgs levent)
	{
		base.OnLayout(levent);

		int padding = LogicalToDeviceUnits(HorizontalPadding);
		int width = ClearButtonBounds.Left - padding;
		TextBox.SetBounds(padding, (ClientSize.Height - TextBox.Height) / 2, Math.Max(0, width), TextBox.Height);
	}

	private bool OverClearButton(Point location) => ClearButtonVisible && ClearButtonBounds.Contains(location);

	// A click beside the text still puts the cursor in it; a click on the cross clears instead.
	protected override void OnMouseDown(MouseEventArgs e)
	{
		ArgumentNullException.ThrowIfNull(e);
		base.OnMouseDown(e);
		if (!OverClearButton(e.Location))
			_ = TextBox.Focus();
	}

	protected override void OnMouseClick(MouseEventArgs e)
	{
		ArgumentNullException.ThrowIfNull(e);
		base.OnMouseClick(e);
		if (e.Button != MouseButtons.Left || !OverClearButton(e.Location))
			return;

		TextBox.Clear();
		SetClearHovered(false);
		Cleared?.Invoke(this, EventArgs.Empty);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		ArgumentNullException.ThrowIfNull(e);
		base.OnMouseMove(e);
		SetClearHovered(OverClearButton(e.Location));
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		base.OnMouseLeave(e);
		SetClearHovered(false);
	}

	private void SetClearHovered(bool hovered)
	{
		Cursor = hovered ? Cursors.Hand : Cursors.IBeam;
		if (hovered == _clearHovered)
			return;
		_clearHovered = hovered;
		Invalidate(ClearButtonBounds);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		ArgumentNullException.ThrowIfNull(e);
		base.OnPaint(e);

		Graphics g = e.Graphics;
		g.SmoothingMode = SmoothingMode.AntiAlias;

		// Outside the rounded corners, the header shows through.
		g.Clear(Parent?.BackColor ?? BackColor);

		Rectangle bounds = new(0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
		int diameter = Math.Min(2 * CornerRadius, Math.Min(bounds.Width, bounds.Height));
		if (diameter <= 0)
			return;

		using GraphicsPath path = new();
		path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
		path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
		path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
		path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
		path.CloseFigure();

		using (SolidBrush fill = new(TextBox.BackColor))
			g.FillPath(fill, path);

		if (ClearButtonVisible)
			PaintClearButton(g);

		if (!TextBox.Focused)
			return;

		Color border = ThemeManager.CurrentTheme.AccentColor;
		using Pen pen = new(border, 1f);
		g.DrawPath(pen, path);
	}

	// A thin diagonal cross in the dimmed text colour; hovered, it takes the text colour on a faint round background.
	private void PaintClearButton(Graphics g)
	{
		Rectangle bounds = ClearButtonBounds;
		Theme theme = ThemeManager.CurrentTheme;

		if (_clearHovered)
		{
			using SolidBrush hover = new(Blend(TextBox.BackColor, theme.ForeColor, 0.15f));
			g.FillEllipse(hover, bounds);
		}

		float half = LogicalToDeviceUnits(10) * ClearCrossHalf / 10f;
		float cx = bounds.Left + (bounds.Width / 2f);
		float cy = bounds.Top + (bounds.Height / 2f);
		using Pen cross = new(_clearHovered ? theme.ForeColor : theme.ForeDimmedColor, Math.Max(1.5f, LogicalToDeviceUnits(10) * 0.15f))
		{
			StartCap = LineCap.Round,
			EndCap = LineCap.Round
		};
		g.DrawLine(cross, cx - half, cy - half, cx + half, cy + half);
		g.DrawLine(cross, cx - half, cy + half, cx + half, cy - half);
	}

	private static Color Blend(Color from, Color to, float amount) => Color.FromArgb(
		(int)(from.R + ((to.R - from.R) * amount)),
		(int)(from.G + ((to.G - from.G) * amount)),
		(int)(from.B + ((to.B - from.B) * amount)));
}
