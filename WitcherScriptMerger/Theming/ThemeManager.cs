using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using Microsoft.Win32;

using WitcherScriptMerger.Forms;

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
	internal static Color DefaultAccentColor = GetColorByHex("0078D7");

	internal ThemeManager()
	{
		LightTheme = new()
		{
			ThemeName = "Light",

			ForeColor = GetColorByHex("1E1E1E"),
			ForeDimmedColor = GetColorByHex("646464"),
			ForeBrightColor = GetColorByHex("000000"),
			BackColor = GetColorByHex("dedede"),

			OptionsPanelBackColor = Color.White,
			BackgroundPanelBackColor = GetColorByHex("f9f9f9"),
			LinkColor = GetColorByHex("0E70C0"),
			BorderColor = GetColorByHex("D3D3D3"),

			StandardButtonBackColor = GetColorByHex("f8f8f8"),
			StandardButtonForeColor = GetColorByHex("000000"),
			StandardButtonHoverColor = GetColorByHex("fdfdfd"),
			StandardButtonDownColor = Color.White,

			AcceptButtonBackColor = GetColorByHex("f8f8f8"),
			AcceptButtonForeColor = GetColorByHex("000000"),
			AcceptButtonHoverColor = GetColorByHex("fdfdfd"),
			AcceptButtonDownColor = Color.White,

			MenuBackColor = GetColorByHex("f9f9f9"),
			MenuForeColor = GetColorByHex("000000"),
			MenuSelectedBackColor = GetColorByHex("f0eced"),
			MenuSelectedForeColor = DefaultAccentColor,

			TreeViewBackColor = Color.White,

			NoConflictsImage = Properties.Resources.thumbsuplight
		};

		DarkTheme = new()
		{
			ThemeName = "Dark",

			ForeColor = GetColorByHex("F1F1F1"),
			ForeDimmedColor = GetColorByHex("9e9e9e"),
			ForeBrightColor = GetColorByHex("ffffff"),
			BackColor = GetColorByHex("202020"),

			BackgroundPanelBackColor = GetColorByHex("383838"),
			LinkColor = GetColorByHex("0E70C0"),
			BorderColor = GetColorByHex("141414"),

			StandardButtonBackColor = GetColorByHex("2d2d2d"),
			StandardButtonForeColor = GetColorByHex("f2f2f2"),
			StandardButtonHoverColor = GetColorByHex("454545"),
			StandardButtonDownColor = GetColorByHex("373737"),

			AcceptButtonBackColor = GetColorByHex("2d2d2d"),
			AcceptButtonForeColor = GetColorByHex("f2f2f2"),
			AcceptButtonHoverColor = GetColorByHex("454545"),
			AcceptButtonDownColor = GetColorByHex("373737"),

			MenuBackColor = GetColorByHex("383838"),
			MenuForeColor = GetColorByHex("9e9e9e"),
			MenuSelectedBackColor = GetColorByHex("202020"),
			MenuSelectedForeColor = DefaultAccentColor,

			TreeViewBackColor = GetColorByHex("2d2d2d"),

			NoConflictsImage = Properties.Resources.thumbsupdark
		};

		SetCurrentTheme();
		UpdateThemeWithAccentColor();
	}

	private void SetCurrentTheme()
	{
		switch (Settings.Get<int>("ColorTheme"))
		{
			default:
			case 0:
				// Follow Windows
				CurrentTheme = DarkTheme;
				try
				{
					bool isUsingLightTheme = (int)Registry.GetValue(
					"HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize",
					"AppsUseLightTheme", false) == 1;
					CurrentTheme = isUsingLightTheme ? LightTheme : DarkTheme;
				}
				catch (Exception) { }

				break;

			case 1:
				// Light Theme
				CurrentTheme = LightTheme;
				break;

			case 2:
				// Dark Theme
				CurrentTheme = DarkTheme;
				break;
		}
	}

	private static void UpdateThemeWithAccentColor()
	{
		if (Environment.OSVersion.Version.Major < 10)
			return;
		if (!Settings.Get<bool>("AccentColor"))
			return;

		Color accentColor = GetAccentColorFromRegistry();
		CurrentTheme.AcceptButtonBackColor = accentColor;
		CurrentTheme.AcceptButtonHoverColor = ControlPaint.Light(accentColor, 0.25f);
		CurrentTheme.AcceptButtonDownColor = ControlPaint.Light(accentColor, 0.5f);
		CurrentTheme.AcceptButtonForeColor = IsLightColor(accentColor) ? Color.Black : Color.White;
		CurrentTheme.MenuSelectedForeColor = accentColor;
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

		button.BackColor = standardButton ? CurrentTheme.StandardButtonBackColor : CurrentTheme.AcceptButtonBackColor;
		button.ForeColor = standardButton ? CurrentTheme.StandardButtonForeColor : CurrentTheme.AcceptButtonForeColor;
		button.FlatAppearance.BorderSize = 0;
		button.FlatStyle = FlatStyle.Flat;
		button.AutoSize = true;
		button.FlatAppearance.MouseOverBackColor = standardButton ? CurrentTheme.StandardButtonHoverColor : CurrentTheme.AcceptButtonHoverColor;
		button.FlatAppearance.MouseDownBackColor = standardButton ? CurrentTheme.StandardButtonDownColor : CurrentTheme.AcceptButtonDownColor;

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

	internal void PanelPaint(object sender, PaintEventArgs e) => DrawRoundedBorder2((Panel)sender, 2, 15, CurrentTheme.BorderColor, CurrentTheme.BackColor);

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

	internal static void AttachColorFaderToTreeNodes(TreeView treeView, out List<TreeNodeColorFader> treeNodeColorFaders)
	{
		List<TreeNodeColorFader> treeNodeColorFadersLocal = [];

		void TraverseTreeNodes(TreeNodeCollection nodes)
		{
			foreach (TreeNode node in nodes)
			{
				if (node.Nodes.Count == 0)
				{
					TreeNodeColorFader treeNodeColorFader = new(node, "ForeColor");
					treeNodeColorFadersLocal.Add(treeNodeColorFader);
				}

				TraverseTreeNodes(node.Nodes);
			}
		}

		TraverseTreeNodes(treeView.Nodes);
		treeNodeColorFaders = treeNodeColorFadersLocal;
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

		if (form is OptionsForm)
		{
			form.BackColor = CurrentTheme.BackgroundPanelBackColor;
		}

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
				bool standard = true;
				string buttonText = button.Text; // Cache the button text

				if (buttonText.StartsWith("exit", StringComparison.OrdinalIgnoreCase) ||
					buttonText.StartsWith("menu", StringComparison.OrdinalIgnoreCase) ||
					buttonText.Replace("&", "", StringComparison.OrdinalIgnoreCase).StartsWith("create all", StringComparison.OrdinalIgnoreCase) ||
					buttonText.Equals("ok", StringComparison.OrdinalIgnoreCase))
				{
					standard = false;
				}

				ModernizeButton(button, form, standard);
			}
			else if (control is Label && (string)control.Tag == "AccentLabel")
			{
				control.ForeColor = Settings.Get<bool>("AccentColor") ? CurrentTheme.AcceptButtonBackColor : DefaultAccentColor;
			}
			else if (control is Label && control.Tag?.ToString() == "DimmedLabel")
			{
				control.ForeColor = CurrentTheme.ForeDimmedColor;
				control.BackColor = CurrentTheme.BackColor;
			}
			else if (control as TreeView is TreeView tv)
			{
				tv.ForeColor = CurrentTheme.ForeDimmedColor;
				tv.BackColor = CurrentTheme.TreeViewBackColor;

				float dpiScalingFactor = GetDpiScalingFactor(form.Handle);
				int baseItemHeight = 25;
				int adjustedItemHeight = (int)(baseItemHeight * dpiScalingFactor);
				tv.ItemHeight = adjustedItemHeight;
			}
			else if (control is StatusStrip strip)
			{
				strip.ForeColor = CurrentTheme.ForeDimmedColor;
				strip.BackColor = CurrentTheme.BackColor;

			}
			else if (control.Parent.Parent is DependencyForm && control is TextBox tb)
			{
				tb.ForeColor = Color.White;
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
				control.BackColor = CurrentTheme.BackColor;
			}
			else
			{
				control.BackColor = CurrentTheme.BackColor;
				control.ForeColor = CurrentTheme.ForeColor;

				if (control.HasChildren)
				{
					ApplyThemeToControls(control.Controls, form);
				}
			}
		}
	}
}