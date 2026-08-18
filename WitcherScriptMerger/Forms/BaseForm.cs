using System;
using System.Windows.Forms;

using Microsoft.Win32;

using static WitcherScriptMerger.Program;

namespace WitcherScriptMerger.Forms;

internal partial class BaseForm : Form
{
	internal BaseForm() => InitializeComponent();

	protected override void OnShown(EventArgs e)
	{
		if (!DesignMode)
		{
			bool isUsingLightTheme = false;
			switch (Settings.Get<int>("ColorTheme"))
			{
				default:
				case 0:
					// Follow Windows
					try
					{
						isUsingLightTheme = (int)Registry.GetValue(
						"HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize",
						"AppsUseLightTheme", false) == 1;
					}
					catch (Exception) { }

					break;

				case 1:
					// Light Theme
					isUsingLightTheme = true;
					break;

				case 2:
					// Dark Theme
					isUsingLightTheme = false;
					break;
			}


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