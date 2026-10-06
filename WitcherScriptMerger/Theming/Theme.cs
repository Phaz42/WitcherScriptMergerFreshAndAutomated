using System.Drawing;

namespace WitcherScriptMerger.Theming;

internal class Theme
{
	internal string ThemeName;

	internal Color BackColor;
	internal Color ForeColor;
	internal Color ForeDimmedColor;
	internal Color ForeBrightColor;

	internal Color OptionsPanelBackColor;
	internal Color BackgroundPanelBackColor;
	internal Color LinkColor;
	internal Color BorderColor;

	internal Color StandardButtonBackColor;
	internal Color StandardButtonForeColor;
	internal Color StandardButtonHoverColor;
	internal Color StandardButtonDownColor;

	internal Color AcceptButtonBackColor;
	internal Color AcceptButtonForeColor;
	internal Color AcceptButtonHoverColor;
	internal Color AcceptButtonDownColor;

	internal Color MenuBackColor;
	internal Color MenuForeColor;
	internal Color MenuSelectedBackColor;
	internal Color MenuSelectedForeColor;

	internal Color TreeViewBackColor;

	// A mod with a problem the mod check found (ModHealth): readable on the list's background in each theme.
	internal Color WarningColor;

	// Picks the "no conflicts" image for the theme (NoConflictsImage), a file beside SM-FAE.
	internal bool IsDark;
}
