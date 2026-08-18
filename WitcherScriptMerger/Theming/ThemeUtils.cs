using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WitcherScriptMerger.Theming;

internal static partial class ThemeUtils
{
	[LibraryImport("User32.dll", EntryPoint = "GetDC")]
	private static partial IntPtr GetDC(IntPtr hWnd);

	[LibraryImport("User32.dll", EntryPoint = "GetDpiForWindow")]
	private static partial int GetDpiForWindow(IntPtr hWnd);

	[LibraryImport("User32.dll", EntryPoint = "ReleaseDC")]
	private static partial int ReleaseDC(IntPtr hWnd, IntPtr hDC);

	internal static float GetDpiScalingFactor(IntPtr handle)
	{
		IntPtr hdc = GetDC(handle);
		int dpi = GetDpiForWindow(handle);
		_ = ReleaseDC(handle, hdc);

		// DPI scaling factor = DPI / 96 (96 DPI is 100% scaling)
		return dpi / 96f;
	}

	internal static Size GetDpiAwareSize(int width, int height, Form form) => new(GetDpiAwarePixels(width, form), GetDpiAwarePixels(height, form));

	internal static int GetDpiAwarePixels(int pixels, Form form)
	{
		float dpiX;
		using (Graphics g = form.CreateGraphics())
			dpiX = g.DpiX;
		return (int)(pixels * (dpiX / 96));
	}

	internal static void DrawRoundedBorderF(Panel panel, int borderWidth, Color borderColor, int cornerRadius)
	{
		using Graphics graphics = panel.CreateGraphics();
		using Pen pen = new(borderColor, borderWidth);
		pen.LineJoin = LineJoin.Round;
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		Rectangle borderRect = new(
			panel.ClientRectangle.X + (borderWidth / 2),
			panel.ClientRectangle.Y + (borderWidth / 2),
			panel.ClientRectangle.Width - borderWidth - 1,
			panel.ClientRectangle.Height - borderWidth - 1);
		graphics.DrawRoundedRectangle(pen, borderRect, cornerRadius);
	}

	internal static void DrawRoundedBorder(Panel panel, int borderWidth, Color borderColor, int cornerRadius)
	{
		using Graphics graphics = panel.CreateGraphics();
		using Pen pen = new(borderColor, borderWidth);
		pen.LineJoin = LineJoin.Round;
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		Rectangle borderRect = new(
			panel.ClientRectangle.X + (borderWidth / 2),
			panel.ClientRectangle.Y + (borderWidth / 2),
			panel.ClientRectangle.Width - borderWidth - 1,
			panel.ClientRectangle.Height - borderWidth - 1);
		graphics.DrawRoundedRectangle(pen, borderRect, cornerRadius);
	}

	internal static void DrawRoundedBorder2(Panel panel, int borderWidth, int cornerRadius, Color borderColor, Color? insideColor)
	{
		using Graphics graphics = panel.CreateGraphics();
		using Pen pen = new(borderColor, borderWidth);
		pen.LineJoin = LineJoin.Round;
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		Rectangle borderRect = new(
			panel.ClientRectangle.X + (borderWidth / 2),
			panel.ClientRectangle.Y + (borderWidth / 2),
			panel.ClientRectangle.Width - borderWidth - 1,
			panel.ClientRectangle.Height - borderWidth - 1);

		// Create a GraphicsPath for the rounded rectangle
		using GraphicsPath path = new();
		path.AddArc(borderRect.X, borderRect.Y, cornerRadius * 2, cornerRadius * 2, 180, 90);
		path.AddArc(borderRect.X + borderRect.Width - (cornerRadius * 2), borderRect.Y, cornerRadius * 2, cornerRadius * 2, 270, 90);
		path.AddArc(borderRect.X + borderRect.Width - (cornerRadius * 2), borderRect.Y + borderRect.Height - (cornerRadius * 2), cornerRadius * 2, cornerRadius * 2, 0, 90);
		path.AddArc(borderRect.X, borderRect.Y + borderRect.Height - (cornerRadius * 2), cornerRadius * 2, cornerRadius * 2, 90, 90);
		path.CloseFigure();

		// Draw the rounded rectangle border
		graphics.DrawPath(pen, path);

		// Fill the area inside the rounded rectangle with red
		using Brush redBrush = new SolidBrush(insideColor ?? Color.Transparent);
		graphics.FillPath(redBrush, path);
	}

	internal static void DrawRoundedBorder4(Panel panel, int borderWidth, Color borderColor, int cornerRadius)
	{
		using Graphics graphics = panel.CreateGraphics();
		graphics.SmoothingMode = SmoothingMode.AntiAlias;

		// Outer rectangle for the border (with rounded corners)
		Rectangle outerRect = new(
			panel.ClientRectangle.X + (borderWidth / 2),
			panel.ClientRectangle.Y + (borderWidth / 2),
			panel.ClientRectangle.Width - borderWidth - 1,
			panel.ClientRectangle.Height - borderWidth - 1);

		// Inner rectangle for the inside area (with rounded corners)
		Rectangle innerRect = new(
			outerRect.X + cornerRadius,
			outerRect.Y + cornerRadius,
			outerRect.Width - (2 * cornerRadius),
			outerRect.Height - (2 * cornerRadius));

		// Create a GraphicsPath for the rounded border
		using GraphicsPath borderPath = new();
		borderPath.AddArc(outerRect.X, outerRect.Y, cornerRadius * 2, cornerRadius * 2, 180, 90);
		borderPath.AddArc(outerRect.X + outerRect.Width - (cornerRadius * 2), outerRect.Y, cornerRadius * 2, cornerRadius * 2, 270, 90);
		borderPath.AddArc(outerRect.X + outerRect.Width - (cornerRadius * 2), outerRect.Y + outerRect.Height - (cornerRadius * 2), cornerRadius * 2, cornerRadius * 2, 0, 90);
		borderPath.AddArc(outerRect.X, outerRect.Y + outerRect.Height - (cornerRadius * 2), cornerRadius * 2, cornerRadius * 2, 90, 90);
		borderPath.CloseFigure();

		// Create a GraphicsPath for the inside area (rounded)
		using GraphicsPath insidePath = new();
		insidePath.AddArc(innerRect.X, innerRect.Y, cornerRadius * 2, cornerRadius * 2, 180, 90);
		insidePath.AddArc(innerRect.X + innerRect.Width - (cornerRadius * 2), innerRect.Y, cornerRadius * 2, cornerRadius * 2, 270, 90);
		insidePath.AddArc(innerRect.X + innerRect.Width - (cornerRadius * 2), innerRect.Y + innerRect.Height - (cornerRadius * 2), cornerRadius * 2, cornerRadius * 2, 0, 90);
		insidePath.AddArc(innerRect.X, innerRect.Y + innerRect.Height - (cornerRadius * 2), cornerRadius * 2, cornerRadius * 2, 90, 90);
		insidePath.CloseFigure();

		// Draw the rounded rectangle border with the specified border color
		using Pen borderPen = new(borderColor, borderWidth);
		graphics.DrawPath(borderPen, borderPath);

		// Fill the inside area with red
		using Brush redBrush = new SolidBrush(Color.Red);
		graphics.FillPath(redBrush, insidePath);
	}
}

internal static class GraphicsExtensions
{
	internal static void DrawRoundedRectangle(this Graphics graphics, Pen pen, Rectangle rectangle, int cornerRadius)
	{
		using GraphicsPath path = new();
		path.AddArc(rectangle.X, rectangle.Y, cornerRadius, cornerRadius, 180, 90);
		path.AddArc(rectangle.X + rectangle.Width - cornerRadius, rectangle.Y, cornerRadius, cornerRadius, 270, 90);
		path.AddArc(rectangle.X + rectangle.Width - cornerRadius, rectangle.Y + rectangle.Height - cornerRadius, cornerRadius, cornerRadius, 0, 90);
		path.AddArc(rectangle.X, rectangle.Y + rectangle.Height - cornerRadius, cornerRadius, cornerRadius, 90, 90);
		path.CloseFigure();

		graphics.DrawPath(pen, path);
	}
}
