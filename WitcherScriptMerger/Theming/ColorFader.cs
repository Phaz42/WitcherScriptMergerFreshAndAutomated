using System;
using System.Drawing;
using System.Reflection;

using System.Windows.Forms;

namespace WitcherScriptMerger.Theming;

internal partial class ColorFader : IDisposable
{
	private Timer fadeTimer;
	private Color targetColor;
	private readonly Control outputControl;
	private readonly PropertyInfo colorProperty;
	private readonly string colorPropertyName;
	private readonly int fadeInDuration = 700; // Separate fade-in duration
	private readonly int fadeOutDuration = 700; // Separate fade-out duration
	private int stepR;
	private int stepG;
	private int stepB;

	internal Color OutputColor
	{
		get => GetNestedColorProperty(outputControl, colorPropertyName);
		set => SetNestedProperty(outputControl, colorPropertyName, value);
	}

	internal ColorFader(Control control, string colorPropertyName, int fadeInDuration = 700, int fadeOutDuration = 700)
	{
		colorProperty = FindNestedProperty(control.GetType(), colorPropertyName);


		if (colorProperty is null || colorProperty.PropertyType != typeof(Color))
		{
			throw new ArgumentException($"Invalid color property name or unsupported " +
				$"type: {colorPropertyName}", nameof(colorPropertyName));
		}

		outputControl = control;
		this.fadeInDuration = fadeInDuration;
		this.fadeOutDuration = fadeOutDuration;
		this.colorPropertyName = colorPropertyName;

	}

	/// <summary>
	/// Gets the value of a nested Color property on the target object using the provided property name.
	/// This method is used to navigate through nested properties and retrieve the final Color value.
	/// </summary>
	/// <param name="target">The object from which to retrieve the nested Color property.</param>
	/// <param name="propertyFullName">The full name of the nested Color property, including any intermediate properties separated by dots ('.').</param>
	/// <returns>The value of the final Color property in the nested chain.</returns>
	/// <exception cref="ArgumentException">
	/// Thrown when the provided property name is invalid, or when the property chain contains a non-existent property.
	/// Also thrown when the final property in the chain is not of type Color.
	/// </exception>
	internal static Color GetNestedColorProperty(object target, string propertyFullName)
	{
		string[] propertyNames = propertyFullName.Split('.');
		int lastIndex = propertyNames.Length - 1;
		PropertyInfo property;
		for (int i = 0; i < lastIndex; i++)
		{
			property = target.GetType().GetProperty(propertyNames[i], BindingFlags.Public | BindingFlags.Instance);

			if (property == null)
			{
				throw new ArgumentException($"Invalid property name: {propertyFullName}");
			}

			target = property.GetValue(target);
		}

		property = target.GetType().GetProperty(propertyNames[lastIndex], BindingFlags.Public | BindingFlags.Instance);

		return property != null && property.PropertyType == typeof(Color)
			? (Color)property.GetValue(target)
			: throw new ArgumentException($"Invalid property name or unsupported type: {propertyFullName}");
	}

	/// <summary>
	/// Sets the value of a nested property on the target object using the provided property name.
	/// This method is used to navigate through nested properties and set the final Color property.
	/// </summary>
	/// <param name="target">The object on which to set the nested property.</param>
	/// <param name="propertyFullName">The full name of the nested property, including any intermediate properties separated by dots ('.').</param>
	/// <param name="newColor">The new Color value to set for the final Color property.</param>
	/// <exception cref="ArgumentException">
	/// Thrown when the provided property name is invalid, or when the property chain contains a non-existent property.
	/// Also thrown when the final property in the chain is not of type Color.
	/// </exception>
	private static void SetNestedProperty(object target, string propertyFullName, Color newColor)
	{
		string[] propertyNames = propertyFullName.Split('.');
		int lastIndex = propertyNames.Length - 1;
		PropertyInfo property;
		for (int i = 0; i < lastIndex; i++)
		{
			property = target.GetType().GetProperty(propertyNames[i], BindingFlags.Public | BindingFlags.Instance);

			if (property == null)
			{
				throw new ArgumentException($"Invalid property name: {propertyFullName}");
			}

			target = property.GetValue(target);
		}

		property = target.GetType().GetProperty(propertyNames[lastIndex], BindingFlags.Public | BindingFlags.Instance);

		if (property != null && property.PropertyType == typeof(Color))
		{
			property.SetValue(target, newColor);
		}
		else
		{
			throw new ArgumentException($"Invalid color property name or unsupported type: {propertyFullName}");
		}
	}

	private static PropertyInfo FindNestedProperty(Type type, string propertyFullName)
	{
		string[] propertyNames = propertyFullName.Split('.');

		PropertyInfo nestedProperty = null;
		Type currentType = type;

		foreach (string propertyName in propertyNames)
		{
			nestedProperty = currentType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

			if (nestedProperty == null)
			{
				return null; // One of the properties in the chain does not exist
			}

			currentType = nestedProperty.PropertyType;
		}

		return nestedProperty;
	}

	internal void StartFadeIn(Color targetColor)
	{
		this.targetColor = targetColor;
		StartFadeEffect(fadeInDuration);
	}

	internal void StartFadeOut(Color targetColor)
	{
		this.targetColor = targetColor;
		StartFadeEffect(fadeOutDuration);
	}

	private void CalculateSteps(int fadeDuration)
	{
		int steps = fadeDuration / fadeTimer.Interval;
		stepR = (targetColor.R - OutputColor.R) / steps;
		stepG = (targetColor.G - OutputColor.G) / steps;
		stepB = (targetColor.B - OutputColor.B) / steps;
	}

	private void StartFadeEffect(int fadeDuration)
	{
		fadeTimer?.Stop(); // Stop the timer if it's already running
		fadeTimer = new Timer();

		CalculateSteps(fadeDuration); // Calculate the steps before setting the timer interval

		int maxStep = Math.Max(Math.Abs(stepR), Math.Max(Math.Abs(stepG), Math.Abs(stepB)));
		maxStep = Math.Max(maxStep, 1); // Set a minimum value for maxStep

		fadeTimer.Interval = fadeDuration / maxStep;

		fadeTimer.Tick += FadeEffectTick;
		fadeTimer.Start();
	}

	private void FadeEffectTick(object sender, EventArgs e)
	{
		int r = OutputColor.R + stepR;
		int g = OutputColor.G + stepG;
		int b = OutputColor.B + stepB;

		// Check if the target color is reached
		if ((stepR > 0 && r >= targetColor.R) || (stepR < 0 && r <= targetColor.R))
		{
			r = targetColor.R;
		}

		if ((stepG > 0 && g >= targetColor.G) || (stepG < 0 && g <= targetColor.G))
		{
			g = targetColor.G;
		}

		if ((stepB > 0 && b >= targetColor.B) || (stepB < 0 && b <= targetColor.B))
		{
			b = targetColor.B;
		}

		// Ensure the color components stay within valid range (0 to 255)
		r = Math.Max(Math.Min(r, 255), 0);
		g = Math.Max(Math.Min(g, 255), 0);
		b = Math.Max(Math.Min(b, 255), 0);

		OutputColor = Color.FromArgb(r, g, b);

		// Check if the target color is reached for all components (R, G, B)
		if (r == targetColor.R && g == targetColor.G && b == targetColor.B)
		{
			fadeTimer.Stop();
		}
	}

	public void Dispose() => ((IDisposable)fadeTimer).Dispose();
}
