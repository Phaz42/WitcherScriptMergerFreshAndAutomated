using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using WitcherScriptMerger.Theming;

namespace WitcherScriptMerger.Controls;

/// <summary>
/// A progress bar SM-FAE draws itself, in the theme's colours: Windows' own is a green bar on a bright grey track, in the
/// dark theme too. The filled part is the accent, on a rounded track; Marquee style runs a segment along the track, for
/// work whose length isn't known. Value, Maximum and Style work as on any progress bar.
/// </summary>
internal sealed class ThemedProgressBar : ProgressBar
{
	private readonly Timer _marquee = new() { Interval = 16 };
	private float _marqueePosition;

	internal ThemedProgressBar()
	{
		SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
		_marquee.Tick += (sender, e) =>
		{
			_marqueePosition = (_marqueePosition + 0.012f) % 1.4f;
			Invalidate();
		};
	}

	// The segment runs only while it can be seen.
	private void UpdateMarquee()
	{
		bool run = Style == ProgressBarStyle.Marquee && Visible;
		if (run && !_marquee.Enabled)
			_marqueePosition = 0;
		_marquee.Enabled = run;
	}

	protected override void OnVisibleChanged(EventArgs e)
	{
		base.OnVisibleChanged(e);
		UpdateMarquee();
	}

	// Style has no change event; each paint checks it, and the first paint after a change starts or stops the segment.
	protected override void OnPaint(PaintEventArgs e)
	{
		ArgumentNullException.ThrowIfNull(e);
		UpdateMarquee();

		Theme theme = ThemeManager.CurrentTheme;
		Graphics g = e.Graphics;
		g.Clear(Parent?.BackColor ?? theme.BackColor);
		g.SmoothingMode = SmoothingMode.AntiAlias;

		RectangleF track = new(0, 0, Width - 1, Height - 1);
		float radius = Math.Min(track.Height / 2, LogicalToDeviceUnits(15) / 2f);
		using GraphicsPath trackPath = Rounded(track, radius);
		using (SolidBrush trackBrush = new(theme.ProgressTrackColor))
			g.FillPath(trackBrush, trackPath);

		RectangleF fill;
		if (Style == ProgressBarStyle.Marquee)
		{
			float segment = track.Width * 0.4f;
			fill = new RectangleF(track.Left + ((_marqueePosition - 0.4f) * track.Width), track.Top, segment, track.Height);
		}
		else
		{
			float share = Maximum > Minimum ? (float)(Value - Minimum) / (Maximum - Minimum) : 0;
			fill = new RectangleF(track.Left, track.Top, track.Width * Math.Clamp(share, 0, 1), track.Height);
		}

		if (fill.Width <= 0)
			return;

		// The fill is rounded like the track, and clipped to it where the marquee segment runs past its ends.
		using Region clip = new(trackPath);
		g.SetClip(clip, CombineMode.Replace);
		using SolidBrush fillBrush = new(theme.AccentTextColor);
		if (fill.Width >= radius * 2)
		{
			using GraphicsPath fillPath = Rounded(fill, radius);
			g.FillPath(fillBrush, fillPath);
		}
		else
		{
			g.FillRectangle(fillBrush, fill);
		}

		g.ResetClip();
	}

	private static GraphicsPath Rounded(RectangleF r, float radius)
	{
		float d = radius * 2;
		GraphicsPath path = new();
		path.AddArc(r.Left, r.Top, d, d, 180, 90);
		path.AddArc(r.Right - d, r.Top, d, d, 270, 90);
		path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
		path.AddArc(r.Left, r.Bottom - d, d, d, 90, 90);
		path.CloseFigure();
		return path;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
			_marquee.Dispose();
		base.Dispose(disposing);
	}
}
