using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using Microsoft.Win32;

using WitcherScriptMerger.Forms;
using WitcherScriptMerger.Utilities;

using static WitcherScriptMerger.Program;
using static WitcherScriptMerger.Theming.Colors;
using static WitcherScriptMerger.Theming.ThemeUtils;

using Button = System.Windows.Forms.Button;
using Control = System.Windows.Forms.Control;
using Panel = System.Windows.Forms.Panel;
using TextBox = System.Windows.Forms.TextBox;
using TreeView = System.Windows.Forms.TreeView;

namespace WitcherScriptMerger.Theming;

internal class ThemeManager
{
	internal Theme LightTheme;
	internal Theme DarkTheme;
	internal static Theme CurrentTheme;
	// SM-FAE's own blue, the accent when "Use Windows accent color" is off, and always the links' colour. On light
	// backgrounds Windows' own shade of its default blue (AccentDark1). On dark ones the same hue, 209 degrees, at full
	// saturation, made just light enough to read as text (4.5:1 on 383838): Windows' dark shade (AccentLight2, 60CDFF)
	// drifts towards cyan, far from the light theme's blue.
	private static readonly Color SmfaeAccentOnDark = GetColorByHex("44A5FF");
	private static readonly Color SmfaeAccentOnLight = GetColorByHex("005FB8");

	/// <summary>
	/// Whether the running theme's accent is the Windows accent: the "Use Windows accent color" setting (key AccentColor)
	/// it was built with. Like the theme, it changes only at the next start.
	/// </summary>
	internal static bool UseWindowsAccentAtStart { get; private set; }

	/// <summary>
	/// The ColorTheme setting the running theme was built from.
	/// </summary>
	private static int ColorThemeAtStart;

	internal ThemeManager()
	{
		LightTheme = new()
		{
			ThemeName = "Light",

			ForeColor = GetColorByHex("1E1E1E"),
			ForeDimmedColor = GetColorByHex("646464"),
			ForeBrightColor = GetColorByHex("000000"),
			ForeFaintColor = GetColorByHex("8C8C8C"),
			BackColor = GetColorByHex("dedede"),

			OptionsPanelBackColor = Color.White,
			BackgroundPanelBackColor = GetColorByHex("f9f9f9"),
			BorderColor = GetColorByHex("D3D3D3"),

			StandardButtonBackColor = GetColorByHex("f8f8f8"),
			StandardButtonForeColor = GetColorByHex("000000"),
			StandardButtonHoverColor = GetColorByHex("EFEFEF"),
			StandardButtonDownColor = GetColorByHex("DCDCDC"),

			MenuBackColor = GetColorByHex("f9f9f9"),
			MenuForeColor = GetColorByHex("000000"),
			MenuDisabledForeColor = GetColorByHex("A0A0A0"),
			MenuSelectedBackColor = GetColorByHex("f0eced"),

			TreeViewBackColor = Color.White,
			FieldBackColor = Color.White,
			SeparatorColor = GetColorByHex("B0B0B0"),
			ProgressTrackColor = GetColorByHex("C4C4C4"),
			SuccessColor = GetColorByHex("0F7B0F"),
			WarningColor = GetColorByHex("B45309"),

			IsDark = false
		};

		DarkTheme = new()
		{
			ThemeName = "Dark",

			ForeColor = GetColorByHex("F1F1F1"),
			ForeDimmedColor = GetColorByHex("9e9e9e"),
			ForeBrightColor = GetColorByHex("ffffff"),
			ForeFaintColor = GetColorByHex("8A8A8A"),
			BackColor = GetColorByHex("202020"),

			BackgroundPanelBackColor = GetColorByHex("383838"),
			BorderColor = GetColorByHex("141414"),

			StandardButtonBackColor = GetColorByHex("2d2d2d"),
			StandardButtonForeColor = GetColorByHex("f2f2f2"),
			StandardButtonHoverColor = GetColorByHex("454545"),
			StandardButtonDownColor = GetColorByHex("373737"),

			MenuBackColor = GetColorByHex("383838"),
			MenuForeColor = GetColorByHex("D4D4D4"),
			MenuDisabledForeColor = GetColorByHex("7A7A7A"),
			MenuSelectedBackColor = GetColorByHex("202020"),

			TreeViewBackColor = GetColorByHex("2d2d2d"),
			OptionsPanelBackColor = GetColorByHex("2B2B2B"),
			FieldBackColor = GetColorByHex("383838"),
			SeparatorColor = GetColorByHex("505050"),
			ProgressTrackColor = GetColorByHex("383838"),
			SuccessColor = GetColorByHex("6CCB5F"),
			WarningColor = GetColorByHex("F0A040"),

			IsDark = true
		};

		ColorThemeAtStart = Settings.Get("ColorTheme", DefaultSettings.ColorTheme);
		CurrentTheme = IsDarkTheme(ColorThemeAtStart) ? DarkTheme : LightTheme;
		// Read with the default the Options window shows, so a settings file without the key agrees with it.
		UseWindowsAccentAtStart = Settings.Get("AccentColor", DefaultSettings.AccentColor);
		UpdateAccentColors();
		DarkenToolTips();
	}

	/// <summary>
	/// Whether a ColorTheme choice is a change from the setting the running theme was built from, and gives a different
	/// theme. Windows switching its own theme meanwhile, with Follow Windows kept, is not a change made here.
	/// </summary>
	internal static bool DiffersFromCurrentTheme(int colorTheme) =>
		colorTheme != ColorThemeAtStart && IsDarkTheme(colorTheme) != CurrentTheme.IsDark;

	// ColorTheme: 0 follows Windows, 1 is light, 2 is dark.
	private static bool IsDarkTheme(int colorTheme) => colorTheme switch
	{
		1 => false,
		2 => true,
		_ => !WindowsAppsUseLightTheme(),
	};

	// Dark when Windows' setting can't be read, as before.
	private static bool WindowsAppsUseLightTheme()
	{
		try
		{
			return Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize",
				"AppsUseLightTheme", null) is int value && value == 1;
		}
		catch (Exception ex) when (ex is System.Security.SecurityException or System.IO.IOException or UnauthorizedAccessException)
		{
			return false;
		}
	}

	// The accent, as text and as a fill - the accent buttons too, whichever colour it is: the option chooses the colour, not
	// whether there is one.
	// Off: SM-FAE's own blue, one colour for both.
	// On: the accent as the user picked it. As a fill it is kept as it is, unless it doesn't stand out from the window
	// (3:1); as text it is mixed until it reads (4.5:1) on every background it is written on - two shades of the user's
	// colour. Windows' own controls use a safer shade of it (AccentLight2 on dark, AccentDark1 on light), often far paler
	// than the colour picked; the user's choice should show.
	private static void UpdateAccentColors()
	{
		Theme theme = CurrentTheme;
		Color[] textBackgrounds = [theme.TreeViewBackColor, theme.BackColor, theme.OptionsPanelBackColor,
			theme.BackgroundPanelBackColor, theme.MenuBackColor, theme.MenuSelectedBackColor];
		Color smfaeAccent = theme.IsDark ? SmfaeAccentOnDark : SmfaeAccentOnLight;
		Color? picked = UseWindowsAccentAtStart ? GetAccentColorFromRegistry() : null;

		// Tried and rejected (2026-10-10): the accent unchanged everywhere - a dark accent such as 6E1420 read at 1.2:1 as
		// text on the dark lists.
		theme.AccentColor = ReadableOn(picked ?? smfaeAccent, theme.IsDark, textBackgrounds);
		theme.AccentFillColor = picked is Color own ? ContrastingOn(own, theme.IsDark, 3.0, theme.BackColor) : theme.AccentColor;

		Color fill = theme.AccentFillColor;
		theme.AccentButtonBackColor = fill;
		theme.AccentButtonHoverColor = ControlPaint.Light(fill, 0.25f);
		theme.AccentButtonDownColor = ControlPaint.Light(fill, 0.5f);
		theme.AccentButtonForeColor = TextOn(fill);

		// A link stays SM-FAE's blue, whatever the accent: a link should look like one.
		theme.LinkColor = theme.IsDark ? SmfaeAccentOnDark : SmfaeAccentOnLight;
	}

	private static void ModernizeButton(Button button, Form form, bool standardButton = true)
	{
		int cornerRadius = 12;
		if (form is OptionsForm or PriorityPromptForm)
			cornerRadius = GetDpiAwarePixels(cornerRadius, form);

		if (button.Text.Contains("merge", StringComparison.OrdinalIgnoreCase))
		{
			button.MinimumSize = GetDpiAwareSize(180, 50, form);
			cornerRadius = GetDpiAwarePixels(cornerRadius, form);
		}

		// The Menu button is bigger and more prominent than the others on purpose, and a quarter rounder.
		if (form is MainForm mainForm && button == mainForm.btnMainMenu)
			cornerRadius = cornerRadius * 5 / 4;

		button.BackColor = standardButton ? CurrentTheme.StandardButtonBackColor : CurrentTheme.AccentButtonBackColor;
		button.ForeColor = standardButton ? CurrentTheme.StandardButtonForeColor : CurrentTheme.AccentButtonForeColor;
		button.FlatAppearance.BorderSize = 0;
		button.FlatStyle = FlatStyle.Flat;
		if (button is Controls.RoundButton round)
			round.CornerRadius = cornerRadius;
		button.AutoSize = true;
		button.FlatAppearance.MouseOverBackColor = standardButton ? CurrentTheme.StandardButtonHoverColor : CurrentTheme.AccentButtonHoverColor;
		button.FlatAppearance.MouseDownBackColor = standardButton ? CurrentTheme.StandardButtonDownColor : CurrentTheme.AccentButtonDownColor;

		// Create a rounded rectangle using a GraphicsPath
		using GraphicsPath path = new();
		path.AddArc(0, 0, cornerRadius * 2, cornerRadius * 2, 180, 90);
		path.AddArc(button.Width - (cornerRadius * 2), 0, cornerRadius * 2, cornerRadius * 2, 270, 90);
		path.AddArc(button.Width - (cornerRadius * 2), button.Height - (cornerRadius * 2), cornerRadius * 2, cornerRadius * 2, 0, 90);
		path.AddArc(0, button.Height - (cornerRadius * 2), cornerRadius * 2, cornerRadius * 2, 90, 90);
		path.CloseFigure();

		// Set the region of the button to achieve rounded corners
		button.Region = new Region(path);

		// Set up the Graphics object to enable anti-aliasing
		using Graphics g = button.CreateGraphics();
		g.SmoothingMode = SmoothingMode.AntiAlias;

		// Fill the rounded rectangle with the button's background color
		using (Brush brush = new SolidBrush(button.BackColor))
		{
			g.FillPath(brush, path);
		}

		// Draw the rounded rectangle border with a pen
		using Pen pen = new(Color.Black, 1f);
		g.DrawPath(pen, path);
	}

	internal void PanelPaint(object sender, PaintEventArgs e) => DrawRoundedBorder2((Panel)sender, 2, 15, CurrentTheme.BorderColor, CurrentTheme.OptionsPanelBackColor);

	// The status bar's separators: a thin line in the theme, half the bar's height, at a section's left edge. Windows Forms
	// draws a status label's border in 3D - a white highlight beside a dark shadow - whatever the colours around it.
	private static void PaintStatusSeparator(object sender, PaintEventArgs e)
	{
		ToolStripItem label = (ToolStripItem)sender;
		int height = label.Height / 2;
		int top = (label.Height - height) / 2;
		using Pen pen = new(CurrentTheme.SeparatorColor, 1);
		e.Graphics.DrawLine(pen, 0, top, 0, top + height);
	}

	// A control's background: an Options group's colour inside one (Tag "OptionsPanel"), otherwise the window's.
	private static Color BackColorFor(Control control) =>
		control.Parent?.Tag?.ToString() == "OptionsPanel" ? CurrentTheme.OptionsPanelBackColor : CurrentTheme.BackColor;

#pragma warning disable CA1822 // Mark members as static
	internal void AttachColorFaderToCheckBoxes(Control parentControl, out List<ColorFader> colorFaders, int? fadeInDuration = null, int? fadeOutDuration = null)
#pragma warning restore CA1822 // Mark members as static
	{
		colorFaders = [];
		foreach (Control control in parentControl.Controls)
		{
			if (control is Panel panel)
			{
				AttachColorFaderToCheckBoxes(panel, out colorFaders, fadeInDuration, fadeOutDuration);
			}
			else if (control is CheckBox checkBox)
			{
				ColorFader colorFader = new(checkBox, "ForeColor", fadeInDuration ?? 700, fadeOutDuration ?? 700);
				checkBox.MouseEnter += (sender, e) => colorFader.StartFadeIn(CurrentTheme.ForeBrightColor);
				checkBox.MouseLeave += (sender, e) => colorFader.StartFadeOut(CurrentTheme.ForeDimmedColor);

				colorFaders.Add(colorFader);
			}
		}
	}

#pragma warning disable CA1822 // Mark members as static
	internal void ApplyThemeOnForm(Form form)
#pragma warning restore CA1822 // Mark members as static
	{
		if (form == null)
		{ return; }

		if (form.InvokeRequired)
		{
			form.Invoke(new Action(() => ApplyThemeOnForm(form)));
			return;
		}

		form.ForeColor = CurrentTheme.ForeColor;
		form.BackColor = CurrentTheme.BackColor;

		ApplyThemeToControls(form.Controls, form);

		if (form is MainForm main)
		{

			int extraMinWidth = GetDpiAwarePixels(57, form);
			int buttonWidth = main.btnCreateAllScriptMerges.Width + main.btnCreateMerges.Width + main.btnDeleteAllMerges.Width + main.btnDeleteMerges.Width;

			main.MinimumSize = new Size(extraMinWidth + buttonWidth, GetDpiAwarePixels(500, form));
		}
	}

	private static void ApplyThemeToControls(Control.ControlCollection controls, Form form)
	{
		foreach (Control control in controls)
		{
			if (control.InvokeRequired)
			{
				control.Invoke(new Action(() => ApplyThemeToControls(control.Controls, form)));
				continue;
			}

			if (control is TableLayoutPanel tbPnl)
			{
				int count = tbPnl.RowCount;
				int index = 0;
				foreach (RowStyle row in tbPnl.RowStyles)
				{
					index++;
					if (index == 1 || index == count)
					{
						float dpiScalingFactor = GetDpiScalingFactor(form.Handle);
						row.Height = (int)(row.Height / 1.25 * dpiScalingFactor); // Original row.Height was designed at 125%
					}
				}
			}

			if (control is Button button)
			{
				// The accent marks each window's main action: its default button, which Enter presses - OK in every dialog -
				// and in the main window, which has none, Menu, Create All Script Merges and the countdown's Exit Now.
				bool accent = button == form.AcceptButton
					|| (form is MainForm main && (button == main.btnMainMenu || button == main.btnCreateAllScriptMerges || button == main.btnExitNow));
				ModernizeButton(button, form, !accent);
			}
			else if (control is Label && (string)control.Tag == "AccentLabel")
			{
				control.ForeColor = CurrentTheme.AccentColor;
			}
			else if (control is Label && control.Tag?.ToString() == "DimmedLabel")
			{
				control.ForeColor = CurrentTheme.ForeDimmedColor;
				control.BackColor = BackColorFor(control);
			}
			else if (control is LinkLabel link)
			{
				link.BackColor = BackColorFor(link);
				link.ForeColor = CurrentTheme.ForeColor;
				link.LinkColor = link.ActiveLinkColor = link.VisitedLinkColor = CurrentTheme.LinkColor;
			}
			else if (control is ComboBox comboBox)
			{
				comboBox.BackColor = CurrentTheme.FieldBackColor;
				comboBox.ForeColor = CurrentTheme.ForeColor;
				ApplyDarkStyle(comboBox, "DarkMode_CFD");
			}
			else if (control as TreeView is TreeView tv)
			{
				tv.ForeColor = CurrentTheme.ForeDimmedColor;
				tv.BackColor = CurrentTheme.TreeViewBackColor;
				(tv as Controls.SMTree)?.ApplyWindowTheme();  // its scrollbars follow the theme

				float dpiScalingFactor = GetDpiScalingFactor(form.Handle);
				int baseItemHeight = 25;
				int adjustedItemHeight = (int)(baseItemHeight * dpiScalingFactor);
				tv.ItemHeight = adjustedItemHeight;
			}
			else if (control is StatusStrip strip)
			{
				strip.ForeColor = CurrentTheme.ForeDimmedColor;
				strip.BackColor = CurrentTheme.BackColor;

				// A section's left border becomes the theme's separator, drawn here (PaintStatusSeparator).
				foreach (ToolStripItem item in strip.Items)
				{
					if (item is ToolStripStatusLabel { BorderSides: ToolStripStatusLabelBorderSides.Left } label)
					{
						label.BorderSides = ToolStripStatusLabelBorderSides.None;
						label.Paint += PaintStatusSeparator;
					}
				}
			}
			else if (control is TextBox searchBox && searchBox.Tag?.ToString() == "SearchBox")
			{
				// The search boxes sit directly above the lists, so they take the lists' colours.
				searchBox.BackColor = CurrentTheme.TreeViewBackColor;
				searchBox.ForeColor = CurrentTheme.ForeColor;
			}
			else if (control.Parent.Parent is DependencyForm && control is TextBox tb)
			{
				tb.BackColor = CurrentTheme.FieldBackColor;
				tb.ForeColor = CurrentTheme.ForeColor;
			}
			else if (control is Panel pnl && pnl is not TableLayoutPanel && pnl.Name != "pnlProgress")
			{
				if (pnl.Tag?.ToString() == "DialogBar")
				{
					pnl.BackColor = CurrentTheme.BackColor;
				}

				ApplyThemeToControls(pnl.Controls, form);
			}
			else if (control is CheckBox && form is OptionsForm)
			{
				control.ForeColor = CurrentTheme.ForeDimmedColor;
				control.BackColor = BackColorFor(control);
			}
			else
			{
				control.BackColor = BackColorFor(control);
				control.ForeColor = CurrentTheme.ForeColor;

				if (control.HasChildren)
				{
					ApplyThemeToControls(control.Controls, form);
				}
			}
		}
	}
}