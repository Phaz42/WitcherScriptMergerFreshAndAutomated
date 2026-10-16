using System;
using System.Drawing;
using System.Windows.Forms;

using WitcherScriptMerger.Theming;

namespace WitcherScriptMerger.Controls;

/// <summary>
/// A checkbox drawn by SM-FAE: the box the lists show (ThemeUtils.DrawCheckBox), in the accent color, and its text.
/// Windows' own box is a picture in a fixed blue, light or dark, which stood out once the accent was another color.
/// Checking, keys and events are a checkbox's as ever; only the drawing is SM-FAE's. A text too long for the control ends
/// in an ellipsis (IsTextCut).
/// </summary>
internal sealed class ThemedCheckBox : CheckBox
{
	internal ThemedCheckBox() =>
		SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);

	// The box's cell and the gap after it, at this control's scaling. The cell is the lists' box as it shows: Windows
	// scales the lists' images up a little, so 13 here showed about 15% smaller beside them, and 15 still 2 px smaller.
	private int BoxSize => LogicalToDeviceUnits(16);
	private int Gap => LogicalToDeviceUnits(5);

	// Measured whole; drawn ending in an ellipsis where it does not fit. Measured with the ellipsis and no width given, a
	// text measures as next to nothing (every auto-sized checkbox showed a letter and "...").
	private TextFormatFlags MeasureFlags => TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine
		| (ShowKeyboardCues ? TextFormatFlags.Default : TextFormatFlags.HidePrefix);

	private TextFormatFlags DrawFlags => MeasureFlags | TextFormatFlags.EndEllipsis;

	// The text's place: right of the box and the gap, inside the padding.
	private Rectangle TextBounds => new(Padding.Left + BoxSize + Gap, Padding.Top, Width - Padding.Horizontal - BoxSize - Gap,
		Height - Padding.Vertical);

	/// <summary>Whether the text is wider than its place, and so shown ending in an ellipsis.</summary>
	internal bool IsTextCut => TextRenderer.MeasureText(Text, Font, Size.Empty, MeasureFlags).Width > TextBounds.Width;

	// As wide as this drawing needs, so the text is never cut off; as tall as Windows' own checkbox, so rows keep their
	// spacing.
	public override Size GetPreferredSize(Size proposedSize)
	{
		Size text = TextRenderer.MeasureText(Text, Font, Size.Empty, MeasureFlags);
		return new Size(Padding.Horizontal + BoxSize + Gap + text.Width, base.GetPreferredSize(proposedSize).Height);
	}

	protected override void OnPaint(PaintEventArgs pevent)
	{
		ArgumentNullException.ThrowIfNull(pevent);
		Graphics g = pevent.Graphics;
		g.Clear(BackColor.A == 255 ? BackColor : Parent?.BackColor ?? ThemeManager.CurrentTheme.BackColor);

		Rectangle content = new(Padding.Left, Padding.Top, Width - Padding.Horizontal, Height - Padding.Vertical);
		RectangleF box = new(content.Left, content.Top + ((content.Height - BoxSize) / 2f), BoxSize, BoxSize);
		ThemeUtils.DrawCheckBox(g, box, Checked, Enabled);

		Rectangle text = TextBounds;
		Color textColor = Enabled ? ForeColor : ThemeManager.CurrentTheme.MenuDisabledForeColor;
		TextRenderer.DrawText(g, Text, Font, text, textColor, DrawFlags);

		// The keyboard's focus, where Windows would show it, as a thin line under the text.
		if (Focused && ShowFocusCues)
		{
			Size measured = TextRenderer.MeasureText(g, Text, Font, text.Size, DrawFlags);
			int y = Math.Min(Height - 1, text.Top + ((text.Height + measured.Height) / 2));
			using Pen pen = new(textColor, 1);
			g.DrawLine(pen, text.Left, y, text.Left + measured.Width, y);
		}
	}
}
