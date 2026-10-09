using System;
using System.Windows.Forms;

using WitcherScriptMerger.Theming;

using static WitcherScriptMerger.Program;

namespace WitcherScriptMerger.Forms;

internal partial class BaseForm : Form
{
	internal BaseForm() => InitializeComponent();

	protected override void OnShown(EventArgs e)
	{
		if (!DesignMode)
		{
			// The title bar matches the running theme, which is set at start: a theme changed since then, in Options or in
			// Windows, applies after a restart, to the title bar too.
			bool isUsingLightTheme = ThemeManager.CurrentTheme is { IsDark: false };

			if (!isUsingLightTheme)
			{
				// Title bar follow Windows Dark/Light theme
				// https://stackoverflow.com/questions/56312190/how-do-i-make-my-form-title-bar-follow-the-windows-dark-theme
				int preference = Convert.ToInt32(true);
				DwmSetWindowAttribute(Handle, DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE, ref preference, sizeof(uint));
			}
		}

		base.OnShown(e);
	}
}