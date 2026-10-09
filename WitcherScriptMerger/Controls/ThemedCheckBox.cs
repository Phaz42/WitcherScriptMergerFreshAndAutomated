using System;
using System.Drawing;
using System.Windows.Forms;

using WitcherScriptMerger.Theming;

namespace WitcherScriptMerger.Controls;

/// <summary>
/// A checkbox drawn by SM-FAE: the box the lists show (ThemeUtils.DrawCheckBox), in the accent colour, and its text.
/// Windows' own box is a picture in a fixed blue, light or dark, which stood out once the accent was another colour.
/// Checking, keys and events are a checkbox's as ever; only the drawing is SM-FAE's.
/// </summary>
internal sealed class ThemedCheckBox : CheckBox
{
	internal ThemedCheckBox() =>
		SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);

	// The box's cell and the gap after it, at this control's scaling. The cell is the lists' box as it shows: Windows
	// scales the lists' images up a little, so 13 here showed about 15% smaller beside them, and 15 still 2 px smaller.
	private int BoxSize => LogicalToDeviceUnits(16);
	private int Gap => LogicalToDeviceUnits(5);

	private TextFormatFlags TextFlags => TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine
		| (ShowKeyboardCues ? TextFormatFlags.Default : TextFormatFlags.HidePrefix);

	// As wide as this drawing needs, so the text is never cut off; as tall as Windows' own checkbox, so rows keep their
	// spacing.
	public override Size GetPreferredSize(Size proposedSize)
	{
		Size text = TextRenderer.MeasureText(Text, Font, Size.Empty, TextFlags);
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

		Rectangle text = new(content.Left + BoxSize + Gap, content.Top, content.Width - BoxSize - Gap, content.Height);
		Color textColor = Enabled ? ForeColor : ThemeManager.CurrentTheme.MenuDisabledForeColor;
		TextRenderer.DrawText(g, Text, Font, text, textColor, TextFlags);

		// The keyboard's focus, where Windows would show it, as a thin line under the text.
		if (Focused && ShowFocusCues)
		{
			Size measured = TextRenderer.MeasureText(g, Text, Font, text.Size, TextFlags);
			int y = Math.Min(Height - 1, text.Top + ((text.Height + measured.Height) / 2));
			using Pen pen = new(textColor, 1);
			g.DrawLine(pen, text.Left, y, text.Left + measured.Width, y);
		}
	}
}
