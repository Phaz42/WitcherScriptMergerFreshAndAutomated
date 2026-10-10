using System.Drawing;

namespace WitcherScriptMerger.Theming;

internal class Theme
{
	internal string ThemeName;

	internal Color BackColor;
	internal Color ForeColor;
	internal Color ForeDimmedColor;
	internal Color ForeBrightColor;

	// Fainter than dimmed, still readable: a load order number mods.settings doesn't hold (ConflictTree).
	internal Color ForeFaintColor;

	// The accent, SM-FAE's blue or with "Use Windows accent color" the user's own (ThemeManager.UpdateAccentColors).
	// AccentColor is the accent as text - winners, section titles, headings, the selected menu item - readable on every
	// background. AccentFillColor is the accent as a fill - accent buttons (AccentButton*), checked checkboxes, the progress
	// bar, the search box's border. They are the same colour unless the user's accent needs lightening or darkening to read
	// as text.
	internal Color AccentColor;
	internal Color AccentFillColor;

	// The Options window's groups, lighter than the window behind them.
	internal Color OptionsPanelBackColor;
	internal Color BackgroundPanelBackColor;
	internal Color LinkColor;
	internal Color BorderColor;

	internal Color StandardButtonBackColor;
	internal Color StandardButtonForeColor;
	internal Color StandardButtonHoverColor;
	internal Color StandardButtonDownColor;

	// The buttons of each window's main action, filled with the accent (ThemeManager.ApplyThemeToControls says which).
	internal Color AccentButtonBackColor;
	internal Color AccentButtonForeColor;
	internal Color AccentButtonHoverColor;
	internal Color AccentButtonDownColor;

	internal Color MenuBackColor;
	internal Color MenuForeColor;
	internal Color MenuDisabledForeColor;
	internal Color MenuSelectedBackColor;

	internal Color TreeViewBackColor;

	// A thin dividing line: between the status bar's sections.
	internal Color SeparatorColor;

	// An input field: a dropdown, a path.
	internal Color FieldBackColor;

	// The progress bar's empty part; the filled part is the accent (AccentFillColor).
	internal Color ProgressTrackColor;

	// Something found or in order: a dependency's path that points at its file.
	internal Color SuccessColor;

	// A mod with a problem the mod check found (ModHealth): readable on the list's background in each theme.
	internal Color WarningColor;

	// Picks the "no conflicts" image for the theme (NoConflictsImage), a file beside SM-FAE.
	internal bool IsDark;
}
