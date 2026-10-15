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

	// The accent, SM-FAE's own blue (ThemeManager.UpdateAccentColors). AccentColor is the accent as text - winners,
	// section titles, headings, the selected menu item - readable on every background. AccentFillColor is the accent as a
	// fill - accent buttons (AccentButton*), checked checkboxes, the progress bar, the search box's border - deep enough
	// for white text on it. AccentSubtleColor is a faint blue on the lists' color: under the help's "?" and around its panel.
	internal Color AccentColor;
	internal Color AccentFillColor;
	internal Color AccentSubtleColor;

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

	// Delete All Merges, the one button that removes every merge: red, as a fill readable in each theme.
	internal Color DangerButtonBackColor;
	internal Color DangerButtonForeColor;
	internal Color DangerButtonHoverColor;
	internal Color DangerButtonDownColor;

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
