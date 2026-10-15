using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using WitcherScriptMerger.Theming;

namespace WitcherScriptMerger.Controls;

/// <summary>
/// A small round "?" that opens a help topic (HelpPanel): at rest a faint blue disc with the "?" in the accent color, so it
/// does not draw the eye; hovered, filled as the accent buttons are; pressed, as they are pressed. Drawn by SM-FAE;
/// whatever is around the circle shows the color behind it (Backdrop).
/// </summary>
internal sealed class HelpButton : Control
{
	/// <summary>Its width and height at 96 dpi.</summary>
	internal const int LogicalSize = 20;

	private bool _hovered, _pressed;

	/// <summary>The color around the circle: when not set, the nearest container's own color; a search box gives its fill.</summary>
	internal Func<Color> Backdrop { get; set; }

	internal HelpButton()
	{
		SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint
			| ControlStyles.ResizeRedraw, true);
		Cursor = Cursors.Hand;
		Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point);
		TabStop = false;
		AccessibleName = "Help";
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

	protected override void OnPaint(PaintEventArgs e)
	{
		Theme theme = ThemeManager.CurrentTheme;
		Graphics g = e.Graphics;
		g.Clear(Backdrop?.Invoke() ?? OpaqueBackColor());

		Color fill = _pressed ? theme.AccentButtonDownColor : _hovered ? theme.AccentButtonBackColor : theme.AccentSubtleColor;
		Color mark = _pressed || _hovered ? theme.AccentButtonForeColor : theme.AccentColor;
		g.SmoothingMode = SmoothingMode.AntiAlias;
		using (SolidBrush brush = new(fill))
			g.FillEllipse(brush, 0, 0, Width - 1, Height - 1);

		TextRenderer.DrawText(g, "?", Font, ClientRectangle, mark,
			TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
	}
}
