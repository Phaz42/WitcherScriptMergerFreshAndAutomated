using System;
using System.Drawing;
using System.Windows.Forms;

namespace WitcherScriptMerger.Theming;

internal partial class MenuRenderer(Theme theme, int fontSize = 10) : ToolStripProfessionalRenderer, IDisposable
{
	private readonly Font baseFont = new("Segoe UI", fontSize);

	protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
	{
		e.Item.Padding = new Padding(fontSize / 2);
		e.Item.Font = baseFont;

		Rectangle rect = new(Point.Empty, e.Item.Size);

		if (e.Item.Selected)
		{
			e.Item.ForeColor = theme.MenuSelectedForeColor;

			using SolidBrush brush = new(theme.MenuSelectedBackColor);
			e.Graphics.FillRectangle(brush, rect);
		}
		else
		{
			e.Item.ForeColor = theme.MenuForeColor;
		}
	}

	protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
	{
		ToolStripItem item = e.Item;
		Size textSize = TextRenderer.MeasureText(item.Text, item.Font);
		Rectangle bounds = new(item.ContentRectangle.Location, item.ContentRectangle.Size);

		int left = item.Padding.Left;
		int top = bounds.Top + ((bounds.Height - textSize.Height) / 2);
		Point textLocation = new(left, top);

		Rectangle textRectangle = new(textLocation, textSize);
		ToolStripItemTextRenderEventArgs centeredTextEventArgs = new(e.Graphics, e.Item, e.Text, textRectangle, e.TextColor, e.TextFont, e.TextFormat);

		base.OnRenderItemText(centeredTextEventArgs);
	}

	protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e) => e.ToolStrip.BackColor = theme.MenuBackColor;

	protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
	{
		using Pen pen = new(ControlPaint.Dark(theme.BackColor, 0.0005f), 4);
		e.Graphics.DrawRectangle(pen, e.AffectedBounds);
	}

	protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
	{
		if (e.Item is ToolStripSeparator)
		{
			Rectangle bounds = new(Point.Empty, e.Item.Size);
			using Pen pen = new(ControlPaint.Light(theme.BackColor, 0.05f), 2);
			int y = bounds.Height / 2;
			e.Graphics.DrawLine(pen, bounds.Left + fontSize, y, bounds.Right - fontSize, y);
		}
	}

	public void Dispose() => baseFont.Dispose();
}
