using System.Drawing;
using System.Windows.Forms;

namespace WitcherScriptMerger.Controls;

/// <summary>A round "?" that opens a help topic (HelpPanel).</summary>
internal sealed class HelpButton : CircleButton
{
	internal HelpButton()
	{
		Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point);
		AccessibleName = "Help";
	}

	protected override void PaintMark(Graphics g, Rectangle bounds, Color color) =>
		TextRenderer.DrawText(g, "?", Font, bounds, color,
			TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
}
