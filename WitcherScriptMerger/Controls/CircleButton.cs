using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using WitcherScriptMerger.Theming;

namespace WitcherScriptMerger.Controls;

/// <summary>
/// A small round button with a mark on it, drawn by SM-FAE: the help's "?" (HelpButton) and the conflict list's filter
/// (FilterButton). At rest a faint blue disc with the mark in the accent color, so it does not draw the eye; hovered,
/// filled as the accent buttons are; pressed, as they are pressed. Whatever is around the circle shows the color behind
/// it (Backdrop).
/// </summary>
internal abstract class CircleButton : Control
{
	/// <summary>Its width and height at 96 dpi.</summary>
	internal const int LogicalSize = 20;

	private bool _hovered, _pressed;

	/// <summary>The color around the circle: when not set, the nearest container's own color; a search box gives its fill.</summary>
	internal Func<Color> Backdrop { get; set; }

	protected CircleButton()
	{
		SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint
			| ControlStyles.ResizeRedraw, true);
		Cursor = Cursors.Hand;
		TabStop = false;
		AccessibleRole = AccessibleRole.PushButton;
		Size = new Size(LogicalToDeviceUnits(LogicalSize), LogicalToDeviceUnits(LogicalSize));
	}

	protected override void OnMouseEnter(EventArgs e)
	{
		base.OnMouseEnter(e);
		SetState(true, _pressed);
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		base.OnMouseLeave(e);
		SetState(false, false);
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		base.OnMouseDown(e);
		if (e.Button == MouseButtons.Left)
			SetState(_hovered, true);
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		base.OnMouseUp(e);
		SetState(ClientRectangle.Contains(e.Location), false);
	}

	private void SetState(bool hovered, bool pressed)
	{
		if (hovered == _hovered && pressed == _pressed)
			return;

		_hovered = hovered;
		_pressed = pressed;
		Invalidate();
	}

	// A container may be transparent and painted by the theme instead; its color would then be black here.
	private Color OpaqueBackColor()
	{
		for (Control control = Parent; control != null; control = control.Parent)
		{
			if (control.BackColor.A == 255)
				return control.BackColor;
		}

		return ThemeManager.CurrentTheme.BackColor;
	}

	/// <summary>The disc's color and the mark's, in this state: the accent buttons' when hovered or pressed.</summary>
	protected virtual (Color Fill, Color Mark) GetColors(Theme theme, bool hovered, bool pressed) =>
		pressed ? (theme.AccentButtonDownColor, theme.AccentButtonForeColor)
		: hovered ? (theme.AccentButtonBackColor, theme.AccentButtonForeColor)
		: (theme.AccentSubtleColor, theme.AccentColor);

	/// <summary>Draws the mark, centered in the bounds given.</summary>
	protected abstract void PaintMark(Graphics g, Rectangle bounds, Color color);

	protected override void OnPaint(PaintEventArgs e)
	{
		Graphics g = e.Graphics;
		g.Clear(Backdrop?.Invoke() ?? OpaqueBackColor());

		(Color fill, Color mark) = GetColors(ThemeManager.CurrentTheme, _hovered, _pressed);
		g.SmoothingMode = SmoothingMode.AntiAlias;
		using (SolidBrush brush = new(fill))
			g.FillEllipse(brush, 0, 0, Width - 1, Height - 1);

		PaintMark(g, ClientRectangle, mark);
	}
}
