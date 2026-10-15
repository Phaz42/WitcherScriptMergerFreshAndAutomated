using System.Drawing;

using WitcherScriptMerger.Theming;

namespace WitcherScriptMerger.Controls;

/// <summary>
/// The round funnel left of the conflicts search box, which opens the filter (FilterPanel). While the filter hides any
/// mod, it is filled in the theme's green, so a filter left on is seen at a glance and not taken for the blue of a hovered
/// button; lighter while hovered, darker while pressed.
/// </summary>
internal sealed class FilterButton : CircleButton
{
	private bool _isActive;

	internal FilterButton() => AccessibleName = "Filter";

	/// <summary>Whether the filter hides any mod now.</summary>
	internal bool IsActive
	{
		get => _isActive;
		set
		{
			if (_isActive == value)
				return;

			_isActive = value;
			Invalidate();
		}
	}

	protected override (Color Fill, Color Mark) GetColors(Theme theme, bool hovered, bool pressed)
	{
		// At rest in the light theme, white as the search box beside it: the "?"'s faint blue, made for the box's white,
		// barely stands out from the window's gray (1.07:1).
		if (!_isActive)
		{
			(Color restFill, Color restMark) = base.GetColors(theme, hovered, pressed);
			return hovered || pressed || theme.IsDark ? (restFill, restMark) : (theme.FieldBackColor, restMark);
		}

		// The mark as on the green itself, so it does not change color with the state: dark on the dark theme's light
		// green, white on the light theme's deep one.
		Color green = theme.SuccessColor;
		Color fill = pressed ? Colors.Mix(Color.Black, green, 0.15) : hovered ? Colors.Mix(Color.White, green, 0.15) : green;
		return (fill, Colors.TextOn(green) == Color.White ? Color.White : theme.TreeViewBackColor);
	}

	// A funnel: a wide top narrowing to a short spout, its foot slanted. Drawn for 20 logical pixels and scaled.
	protected override void PaintMark(Graphics g, Rectangle bounds, Color color)
	{
		float unit = bounds.Width / 35f, cx = bounds.Left + (bounds.Width / 2f), cy = bounds.Top + (bounds.Height / 2f);
		PointF[] funnel =
		[
			new(cx - (9 * unit), cy - (8 * unit)),
			new(cx + (9 * unit), cy - (8 * unit)),
			new(cx + (2.5f * unit), cy + (1 * unit)),
			new(cx + (2.5f * unit), cy + (7 * unit)),
			new(cx - (2.5f * unit), cy + (9.5f * unit)),
			new(cx - (2.5f * unit), cy + (1 * unit)),
		];
		using SolidBrush brush = new(color);
		g.FillPolygon(brush, funnel);
	}
}
