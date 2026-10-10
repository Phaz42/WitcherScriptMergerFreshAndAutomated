using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WitcherScriptMerger.Theming;

internal static partial class ThemeUtils
{
	// A control's scrollbars are Windows' own, and Windows Forms has no setting for their colours. Windows' Explorer look
	// has a dark variant (Windows 10 1809 and later) with dark scrollbars; SetWindowTheme gives it to a control. It is not
	// documented, but File Explorer and many applications rely on it. Where it does not exist - older Windows, Wine - the
	// control keeps its usual look. The light theme takes the light Explorer look, so both themes draw alike.
	[LibraryImport("uxtheme.dll", StringMarshalling = StringMarshalling.Utf16)]
	private static partial int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);

	/// <summary>Gives a control the Windows look of the current theme: dark scrollbars in the dark theme.</summary>
	internal static void ApplyWindowTheme(Control control)
	{
		if (control != null && control.IsHandleCreated)
			_ = SetWindowTheme(control.Handle, ThemeManager.CurrentTheme.IsDark ? "DarkMode_Explorer" : "Explorer", null);
	}

	/// <summary>
	/// In the dark theme, Windows' dark style for one of its own controls: "DarkMode_CFD" for a dropdown (its box, arrow and
	/// open list). The light theme keeps Windows' usual look. Applied again if the control's window is recreated.
	/// </summary>
	internal static void ApplyDarkStyle(Control control, string darkStyle)
	{
		if (control == null || !ThemeManager.CurrentTheme.IsDark)
			return;

		if (control.IsHandleCreated)
			_ = SetWindowTheme(control.Handle, darkStyle, null);
		control.HandleCreated += (sender, e) => _ = SetWindowTheme(((Control)sender).Handle, darkStyle, null);
	}

	#region Checkboxes

	/// <summary>
	/// SM-FAE's checkbox, in the lists (SMTree) and in its windows (ThemedCheckBox): a rounded box, filled with the accent
	/// and ticked when checked. Windows' own is a picture in a fixed blue, which no accent colour or theme reaches.
	/// </summary>
	internal static void DrawCheckBox(Graphics g, RectangleF cell, bool isChecked, bool enabled = true)
	{
		ArgumentNullException.ThrowIfNull(g);
		Theme theme = ThemeManager.CurrentTheme;
		SmoothingMode smoothing = g.SmoothingMode;
		g.SmoothingMode = SmoothingMode.AntiAlias;

		float inset = cell.Width * 0.06f;
		RectangleF box = new(cell.Left + inset, cell.Top + inset, cell.Width - (inset * 2), cell.Height - (inset * 2));
		float diameter = cell.Width * 0.36f;
		using GraphicsPath path = new();
		path.AddArc(box.Left, box.Top, diameter, diameter, 180, 90);
		path.AddArc(box.Right - diameter, box.Top, diameter, diameter, 270, 90);
		path.AddArc(box.Right - diameter, box.Bottom - diameter, diameter, diameter, 0, 90);
		path.AddArc(box.Left, box.Bottom - diameter, diameter, diameter, 90, 90);
		path.CloseFigure();

		if (isChecked)
		{
			Color fillColor = enabled ? theme.AccentFillColor : theme.MenuDisabledForeColor;
			using SolidBrush fill = new(fillColor);
			g.FillPath(fill, path);
			// A slightly darker edge sets the box off its background: the fill, a fifth of the way to black.
			Color edgeColor = Color.FromArgb((int)(fillColor.R * 0.8f), (int)(fillColor.G * 0.8f), (int)(fillColor.B * 0.8f));
			using (Pen edge = new(edgeColor, Math.Max(1f, cell.Width * 0.04f)))
				g.DrawPath(edge, path);
			using Pen tick = new(Colors.TextOn(fillColor), cell.Width * 0.09f)
			{
				StartCap = LineCap.Round,
				EndCap = LineCap.Round,
				LineJoin = LineJoin.Round
			};
			g.DrawLines(tick, [
				new PointF(box.Left + (box.Width * 0.22f), box.Top + (box.Height * 0.52f)),
				new PointF(box.Left + (box.Width * 0.42f), box.Top + (box.Height * 0.72f)),
				new PointF(box.Left + (box.Width * 0.78f), box.Top + (box.Height * 0.30f))
			]);
		}
		else
		{
			// Darker than the background in the dark theme, a white field in the light one, as Windows' own.
			using SolidBrush fill = new(theme.IsDark ? theme.BackColor : theme.FieldBackColor);
			g.FillPath(fill, path);
			using Pen border = new(enabled ? theme.ForeDimmedColor : theme.MenuDisabledForeColor, Math.Max(1f, cell.Width * 0.06f));
			g.DrawPath(border, path);
		}

		g.SmoothingMode = smoothing;
	}

	internal static Bitmap CheckBoxImage(int size, bool isChecked)
	{
		Bitmap image = null;
		try
		{
			image = new Bitmap(size, size);
			using (Graphics g = Graphics.FromImage(image))
				DrawCheckBox(g, new RectangleF(0, 0, size, size), isChecked);

			Bitmap drawn = image;
			image = null;
			return drawn;
		}
		finally
		{
			image?.Dispose();
		}
	}

	#endregion

	#region Dark tooltips

	// Every tooltip window SM-FAE has - its own ToolTips, and those Windows Forms makes for menus and the status bar, which
	// no code here can reach - takes Windows' dark tooltip style as it is created: Windows reports each window it creates
	// in this process (a WinEvent hook), and a tooltip's class name says what it is.
	private const uint EventObjectCreate = 0x8000;
	private const int ObjIdWindow = 0;

	private delegate void WinEventProc(IntPtr hook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint thread, uint time);

	// Held for the life of the program: Windows keeps a pointer to it.
	private static readonly WinEventProc ToolTipCreated = OnWindowCreated;
	private static IntPtr _toolTipHook;

	/// <summary>In the dark theme, makes every tooltip created from now on dark. Called once, before any window exists.</summary>
	internal static void DarkenToolTips()
	{
		if (!ThemeManager.CurrentTheme.IsDark || _toolTipHook != IntPtr.Zero)
			return;

		_toolTipHook = SetWinEventHook(EventObjectCreate, EventObjectCreate, IntPtr.Zero,
			Marshal.GetFunctionPointerForDelegate(ToolTipCreated), (uint)Environment.ProcessId, 0, 0);
	}

	private static unsafe void OnWindowCreated(IntPtr hook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint thread, uint time)
	{
		if (idObject != ObjIdWindow || hwnd == IntPtr.Zero)
			return;

		const int capacity = 128;
		char* name = stackalloc char[capacity];
		int length = GetClassNameW(hwnd, name, capacity);
		if (length > 0 && new string(name, 0, length).Contains("tooltips_class32", StringComparison.Ordinal))
			_ = SetWindowTheme(hwnd, "DarkMode_Explorer", null);
	}

	[LibraryImport("user32.dll")]
	private static partial IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr module, IntPtr proc, uint process, uint thread, uint flags);

	[LibraryImport("user32.dll", EntryPoint = "GetClassNameW")]
	private static unsafe partial int GetClassNameW(IntPtr hWnd, char* className, int maxCount);

	#endregion

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
