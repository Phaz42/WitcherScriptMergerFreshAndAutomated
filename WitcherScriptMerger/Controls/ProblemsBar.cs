using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using WitcherScriptMerger.Theming;

namespace WitcherScriptMerger.Controls;

/// <summary>
/// The bar above the lists saying that SM-FAE found mods with problems (ModHealth): the status bar's small link was easy
/// to miss. What was found, "Show details" for the report, and × to hide it until a scan finds a problem that was not
/// showing when it was hidden (MainForm.UpdateProblemsBar). Drawn by SM-FAE in the theme's warning color; the message
/// wraps onto more lines rather than being cut short, and the bar grows with it.
/// </summary>
internal sealed class ProblemsBar : Control
{
	internal event EventHandler ShowDetailsClicked;
	internal event EventHandler DismissClicked;

	private const string WarningSign = "⚠";
	private const string DetailsText = "Show details";
	private const string CloseSign = "✕";
	private const TextFormatFlags LineFlags =
		TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding | TextFormatFlags.VerticalCenter;
	private const TextFormatFlags MessageFlags = TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding;

	private readonly Font _detailsHoverFont;
	private Rectangle _sign, _message, _details, _close;
	private bool _overDetails, _overClose;

	internal ProblemsBar()
	{
		SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint
			| ControlStyles.ResizeRedraw, true);
		Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
		_detailsHoverFont = new Font(Font, FontStyle.Underline);
		TabStop = false;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
			_detailsHoverFont.Dispose();

		base.Dispose(disposing);
	}

	/// <summary>Shows this message.</summary>
	internal void SetMessage(string message)
	{
		Text = message;
		LayOutParts();
		Invalidate();
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		base.OnSizeChanged(e);
		LayOutParts();
	}

	// Where each part goes at the bar's width, and the height that takes: the sign on the left; the close button, then
	// "Show details", on the right, each centered on the bar's height; the message between them, wrapped to fit.
	private void LayOutParts()
	{
		int pad = LogicalToDeviceUnits(12), gap = LogicalToDeviceUnits(10), inset = LogicalToDeviceUnits(4), padY = LogicalToDeviceUnits(9);
		int line = TextRenderer.MeasureText("Ag", Font, Size.Empty, LineFlags).Height;
		int signWidth = TextRenderer.MeasureText(WarningSign, Font, Size.Empty, LineFlags).Width;
		int closeWidth = TextRenderer.MeasureText(CloseSign, Font, Size.Empty, LineFlags).Width + (2 * LogicalToDeviceUnits(9));
		int detailsWidth = TextRenderer.MeasureText(DetailsText, Font, Size.Empty, LineFlags).Width;

		int messageLeft = pad + signWidth + gap;
		int messageWidth = Math.Max(1, Width - inset - closeWidth - gap - detailsWidth - (2 * gap) - messageLeft);
		Size message = TextRenderer.MeasureText(Text ?? string.Empty, Font, new Size(messageWidth, int.MaxValue), MessageFlags);
		int height = Math.Max(line, message.Height) + (2 * padY);

		_sign = new Rectangle(pad, 0, signWidth, height);
		int closeHeight = line + (2 * (padY - inset));
		_close = new Rectangle(Width - inset - closeWidth, (height - closeHeight) / 2, closeWidth, closeHeight);
		_details = new Rectangle(_close.Left - gap - detailsWidth, 0, detailsWidth, height);
		_message = new Rectangle(messageLeft, (height - message.Height) / 2, messageWidth, message.Height);

		if (Height != height)
			Height = height;
	}

	// The theme's warning color, faintly, over the window's background - over white in the light theme, whose gray
	// window would leave the warning color too dark to read on it.
	private static Color FillColor(Theme theme) =>
		Mix(theme.WarningColor, theme.IsDark ? theme.BackColor : Color.White, theme.IsDark ? 0.14 : 0.08);

	private static Color Mix(Color color, Color under, double amount) => Color.FromArgb(
		(int)Math.Round(under.R + ((color.R - under.R) * amount)),
		(int)Math.Round(under.G + ((color.G - under.G) * amount)),
		(int)Math.Round(under.B + ((color.B - under.B) * amount)));

	protected override void OnPaint(PaintEventArgs e)
	{
		Theme theme = ThemeManager.CurrentTheme;
		Graphics g = e.Graphics;
		Color fill = FillColor(theme);
		Color warning = Colors.ReadableOn(theme.WarningColor, theme.IsDark, fill);
		g.Clear(Parent?.BackColor ?? theme.BackColor);

		g.SmoothingMode = SmoothingMode.AntiAlias;
		using (GraphicsPath frame = RoundedRectangle(new Rectangle(0, 0, Width - 1, Height - 1), LogicalToDeviceUnits(6)))
		using (SolidBrush brush = new(fill))
		using (Pen pen = new(theme.WarningColor))
		{
			g.FillPath(brush, frame);
			g.DrawPath(pen, frame);
		}

		g.SmoothingMode = SmoothingMode.Default;

		TextRenderer.DrawText(g, WarningSign, Font, _sign, warning, LineFlags);

		if (_overClose)
		{
			using SolidBrush hover = new(Mix(theme.WarningColor, fill, 0.2));
			g.FillRectangle(hover, _close);
		}

		TextRenderer.DrawText(g, CloseSign, Font, _close, _overClose ? theme.ForeBrightColor : theme.ForeColor,
			LineFlags | TextFormatFlags.HorizontalCenter);
		TextRenderer.DrawText(g, DetailsText, _overDetails ? _detailsHoverFont : Font, _details, warning, LineFlags);
		TextRenderer.DrawText(g, Text, Font, _message, theme.ForeColor, MessageFlags);
	}

	private static GraphicsPath RoundedRectangle(Rectangle bounds, int radius)
	{
		int diameter = 2 * radius;
		GraphicsPath path = new();
		path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
		path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
		path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
		path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
		path.CloseFigure();
		return path;
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);
		SetHover(_details.Contains(e.Location), _close.Contains(e.Location));
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		base.OnMouseLeave(e);
		SetHover(false, false);
	}

	private void SetHover(bool overDetails, bool overClose)
	{
		if (overDetails == _overDetails && overClose == _overClose)
			return;

		_overDetails = overDetails;
		_overClose = overClose;
		Cursor = overDetails || overClose ? Cursors.Hand : Cursors.Default;
		Invalidate();
	}

	protected override void OnMouseClick(MouseEventArgs e)
	{
		base.OnMouseClick(e);
		if (e.Button != MouseButtons.Left)
			return;

		if (_close.Contains(e.Location))
			DismissClicked?.Invoke(this, EventArgs.Empty);
		else if (_details.Contains(e.Location))
			ShowDetailsClicked?.Invoke(this, EventArgs.Empty);
	}
}
