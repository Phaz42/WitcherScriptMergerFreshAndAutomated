using System;
using System.Drawing;
using System.Globalization;

using Microsoft.Win32;

namespace WitcherScriptMerger.Theming;

internal static class Colors
{
	internal static Color GetAccentColorFromRegistry()
	{
		try
		{
			using RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\DWM");
			if (key != null)
			{
				int accentColor = (int)key.GetValue("AccentColor");
				return Color.FromArgb(
					255, // Alpha
					accentColor & 0xFF, // Red
					(accentColor & 0xFF00) >> 8, // Green
					(accentColor & 0xFF0000) >> 16); // Blue
			}
		}
		catch (Exception) { }

		return Color.FromArgb(0, 120, 215); // Default blue
	}

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

	internal static bool IsLightColor(Color color)
	{
		double luminance = (0.299 * color.R) + (0.587 * color.G) + (0.114 * color.B);
		return luminance > 128;
	}

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

