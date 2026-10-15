using System;
using System.Drawing;
using System.Globalization;


namespace WitcherScriptMerger.Theming;

internal static class Colors
{
	internal static Color GetColorByHex(string hex)
	{
		if (!hex.StartsWith("#", StringComparison.Ordinal))
			hex = "#" + hex;
		ColorConverter converter = new();
		return (Color)converter.ConvertFromString(hex);
	}

	internal static string GetHexByColor(Color color) =>
		color.R.ToString("X2", CultureInfo.InvariantCulture) +
		color.G.ToString("X2", CultureInfo.InvariantCulture) +
		color.B.ToString("X2", CultureInfo.InvariantCulture);

	// WCAG 2's contrast ratio between two colors, 1 to 21. Normal text needs 4.5 to be read comfortably.
	internal static double ContrastRatio(Color a, Color b)
	{
		double la = RelativeLuminance(a), lb = RelativeLuminance(b);
		return (Math.Max(la, lb) + 0.05) / (Math.Min(la, lb) + 0.05);
	}

	private static double RelativeLuminance(Color color)
	{
		static double Channel(int value)
		{
			double c = value / 255.0;
			return c <= 0.03928 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
		}

		return (0.2126 * Channel(color.R)) + (0.7152 * Channel(color.G)) + (0.0722 * Channel(color.B));
	}

	/// <summary>
	/// The color as text: mixed towards white (on dark backgrounds) or black (on light ones) just far enough to reach 4.5:1
	/// on every background given, so its hue is kept. A Windows accent color is chosen to fill things, and as text it can
	/// be unreadable: Windows' default blue reaches only 3:1 on a dark list, a yellow accent 1.7:1 on white.
	/// </summary>
	internal static Color ReadableOn(Color color, bool darkBackgrounds, params Color[] backgrounds) =>
		ContrastingOn(color, darkBackgrounds, 4.5, backgrounds);

	/// <summary>
	/// The color mixed towards white (on dark backgrounds) or black (on light ones) just far enough to reach the given
	/// contrast on every background given - unchanged where it already does. 4.5:1 for text; 3:1 for a control's fill,
	/// the usual minimum for something that isn't text to stand out.
	/// </summary>
	internal static Color ContrastingOn(Color color, bool darkBackgrounds, double ratio, params Color[] backgrounds)
	{
		Color target = darkBackgrounds ? Color.White : Color.Black;
		for (int step = 0; step <= 100; step++)
		{
			float amount = step / 100f;
			Color mixed = Color.FromArgb(
				(int)Math.Round(color.R + ((target.R - color.R) * amount)),
				(int)Math.Round(color.G + ((target.G - color.G) * amount)),
				(int)Math.Round(color.B + ((target.B - color.B) * amount)));
			if (Array.TrueForAll(backgrounds, background => ContrastRatio(mixed, background) >= ratio))
				return mixed;
		}

		return target;
	}

	/// <summary>The color laid over another at the given strength, from 0 (none of it) to 1: a hover's tint, a dimmed row.</summary>
	internal static Color Mix(Color color, Color under, double amount) => Color.FromArgb(
		(int)Math.Round(under.R + ((color.R - under.R) * amount)),
		(int)Math.Round(under.G + ((color.G - under.G) * amount)),
		(int)Math.Round(under.B + ((color.B - under.B) * amount)));

	// Text on a fill of this color: white, as Windows writes on its accent, where that reads; otherwise the better of the two.
	internal static Color TextOn(Color fill) =>
		ContrastRatio(Color.White, fill) >= 4.5 || ContrastRatio(Color.White, fill) >= ContrastRatio(Color.Black, fill) ? Color.White : Color.Black;

	internal static Color MakeCooler(Color originalColor, float percentage)
	{
		// Convert RGB to HSB
		RGBtoHSB(originalColor, out float hue, out float saturation, out float brightness);

		// Decrease saturation and increase brightness
		saturation -= saturation * percentage;
		brightness += brightness * percentage;

		// Ensure saturation and brightness stay within valid range
		saturation = Math.Max(0, Math.Min(1, saturation));
		brightness = Math.Max(0, Math.Min(1, brightness));

		// Convert HSB back to RGB
		Color coolerColor = HSBtoRGB(hue, saturation, brightness);

		return coolerColor;
	}

	private static void RGBtoHSB(Color color, out float hue, out float saturation, out float brightness)
	{
		hue = color.GetHue() / 360f; // GetHue returns degrees, convert to float between 0 and 1
		saturation = color.GetSaturation();
		brightness = color.GetBrightness();
	}

	private static Color HSBtoRGB(float hue, float saturation, float brightness)
	{
		int r = 0;
		int g = 0;
		int b = 0;
		if (saturation == 0)
		{
			r = g = b = (int)((brightness * 255.0f) + 0.5f);
		}
		else
		{
			float h = (hue - (float)Math.Floor(hue)) * 6.0f;
			float f = h - (float)Math.Floor(h);
			float p = brightness * (1.0f - saturation);
			float q = brightness * (1.0f - (saturation * f));
			float t = brightness * (1.0f - (saturation * (1.0f - f)));
			switch ((int)h)
			{
				case 0:
					r = (int)((brightness * 255.0f) + 0.5f);
					g = (int)((t * 255.0f) + 0.5f);
					b = (int)((p * 255.0f) + 0.5f);
					break;
				case 1:
					r = (int)((q * 255.0f) + 0.5f);
					g = (int)((brightness * 255.0f) + 0.5f);
					b = (int)((p * 255.0f) + 0.5f);
					break;
				case 2:
					r = (int)((p * 255.0f) + 0.5f);
					g = (int)((brightness * 255.0f) + 0.5f);
					b = (int)((t * 255.0f) + 0.5f);
					break;
				case 3:
					r = (int)((p * 255.0f) + 0.5f);
					g = (int)((q * 255.0f) + 0.5f);
					b = (int)((brightness * 255.0f) + 0.5f);
					break;
				case 4:
					r = (int)((t * 255.0f) + 0.5f);
					g = (int)((p * 255.0f) + 0.5f);
					b = (int)((brightness * 255.0f) + 0.5f);
					break;
				case 5:
					r = (int)((brightness * 255.0f) + 0.5f);
					g = (int)((p * 255.0f) + 0.5f);
					b = (int)((q * 255.0f) + 0.5f);
					break;
			}
		}

		return Color.FromArgb(255, (byte)r, (byte)g, (byte)b);
	}
}

