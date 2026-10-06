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
/// </summary>
internal sealed class SearchBox : Panel
{
	// Rounded a little more than the header buttons (12, ThemeManager.ModernizeButton), and the room beside the text.
	private const int CornerRadius = 14;
	private const int HorizontalPadding = 10;

	internal TextBox TextBox { get; }

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
		Controls.Add(TextBox);
	}

	protected override void OnLayout(LayoutEventArgs levent)
	{
		base.OnLayout(levent);

		int padding = LogicalToDeviceUnits(HorizontalPadding);
		TextBox.SetBounds(padding, (ClientSize.Height - TextBox.Height) / 2, Math.Max(0, ClientSize.Width - (2 * padding)), TextBox.Height);
	}

	// A click beside the text still puts the cursor in it.
	protected override void OnMouseDown(MouseEventArgs e)
	{
		base.OnMouseDown(e);
		_ = TextBox.Focus();
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

		if (!TextBox.Focused)
			return;

		Color border = Settings.Get<bool>("AccentColor") ? ThemeManager.CurrentTheme.AcceptButtonBackColor : ThemeManager.DefaultAccentColor;
		using Pen pen = new(border, 1f);
		g.DrawPath(pen, path);
	}
}
