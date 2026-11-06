using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WitcherScriptMerger.Controls;

/// <summary>
/// A button as the theme rounds it (ThemeManager.ModernizeButton), whose focus shows as a rounded ring in its text colour -
/// always, whether the keyboard or the mouse moved it there. Windows' own focus mark on a flat button is a thin square
/// frame, which the rounded corners cut, shown only once a key was pressed; in the dark theme it read as a misdrawn outline.
/// </summary>
internal class RoundButton : Button
{
	/// <summary>The corner radius the theme gave it, in pixels.</summary>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	internal int CornerRadius { get; set; } = 12;

	// Windows' frame is replaced by the ring below.
	protected override bool ShowFocusCues => false;

	// Never drawn as the default button: Windows marks it with the same square frame, on the form's AcceptButton while
	// another control has the focus, and on the last clicked button while another window does. Enter still presses the
	// form's default button - the form keeps track of it, not the button.
	public override void NotifyDefault(bool value) => base.NotifyDefault(false);

	protected override void OnPaint(PaintEventArgs pevent)
	{
		ArgumentNullException.ThrowIfNull(pevent);
		base.OnPaint(pevent);

		if (!Focused)
			return;

		float width = Math.Max(1f, DeviceDpi / 96f);
		float inset = width * 2;
		RectangleF ring = new(inset, inset, ClientSize.Width - (inset * 2) - 1, ClientSize.Height - (inset * 2) - 1);
		float diameter = Math.Max(2f, (CornerRadius - inset) * 2);

		using GraphicsPath path = new();
		path.AddArc(ring.Left, ring.Top, diameter, diameter, 180, 90);
		path.AddArc(ring.Right - diameter, ring.Top, diameter, diameter, 270, 90);
		path.AddArc(ring.Right - diameter, ring.Bottom - diameter, diameter, diameter, 0, 90);
		path.AddArc(ring.Left, ring.Bottom - diameter, diameter, diameter, 90, 90);
		path.CloseFigure();

		SmoothingMode smoothing = pevent.Graphics.SmoothingMode;
		pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
		using Pen pen = new(ForeColor, width);
		pevent.Graphics.DrawPath(pen, path);
		pevent.Graphics.SmoothingMode = smoothing;
	}
}
