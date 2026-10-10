using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using WitcherScriptMerger.Theming;
using WitcherScriptMerger.Tools;

using static WitcherScriptMerger.Program;

namespace WitcherScriptMerger.Forms;

internal partial class DependencyForm : BaseForm
{
	private bool AreAnyPathsChanged
	{
		get
		{
			return !txtKDiff3Path.Text.EqualsIgnoreCase(KDiff3.ExePath) ||
					!txtBmsPath.Text.EqualsIgnoreCase(QuickBms.ExePath) ||
					!txtBmsPluginPath.Text.EqualsIgnoreCase(QuickBms.PluginPath) ||
					!txtWccLitePath.Text.EqualsIgnoreCase(WccLite.ExePath);
		}
	}

	// Each path field and the file type it must point at.
	private readonly Dictionary<TextBox, string> _extensions;

	internal DependencyForm()
	{
		InitializeComponent();
		_extensions = new()
		{
			[txtKDiff3Path] = ".exe",
			[txtBmsPath] = ".exe",
			[txtBmsPluginPath] = ".bms",
			[txtWccLitePath] = ".exe"
		};
		foreach (TextBox txt in _extensions.Keys)
			txt.Parent.Paint += PaintPathMarks;
		ThemeMngr.ApplyThemeOnForm(this);
	}

	private void DependencyForm_Load(object sender, EventArgs e)
	{
		txtKDiff3Path.Text = KDiff3.ExePath;
		txtBmsPath.Text = QuickBms.ExePath;
		txtBmsPluginPath.Text = QuickBms.PluginPath;
		txtWccLitePath.Text = WccLite.ExePath;
		btnOK.Select();
	}

	private void BtnOK_Click(object sender, EventArgs e)
	{
		bool allValid = true;
		foreach (TextBox txt in _extensions.Keys)
			allValid &= IsValid(txt);

		if (!allValid &&
			DialogResult.No == MessageBox.Show(
				"Not all the files are located & valid. Save settings anyway?",
				"Missing Dependency",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Warning))
		{
			DialogResult = DialogResult.None;
			return;
		}

		if (AreAnyPathsChanged)
		{
			KDiff3.ExePath = UpdatePathSetting(KDiff3.ExePath, txtKDiff3Path.Text, "KDiff3Path");
			QuickBms.ExePath = UpdatePathSetting(QuickBms.ExePath, txtBmsPath.Text, "QuickBmsPath");
			QuickBms.PluginPath = UpdatePathSetting(QuickBms.PluginPath, txtBmsPluginPath.Text, "QuickBmsPluginPath");
			WccLite.ExePath = UpdatePathSetting(WccLite.ExePath, txtWccLitePath.Text, "WccLitePath");
			Settings.Save();
		}

		DialogResult = allValid
			? DialogResult.OK
			: DialogResult.Cancel;
	}

	private static string UpdatePathSetting(string oldPath, string newPath, string settingName)
	{
		if (oldPath.EqualsIgnoreCase(newPath))
			return oldPath;
		Settings.Set(settingName, newPath);
		return newPath;
	}

	private void BtnCancel_Click(object sender, EventArgs e) => DialogResult = DialogResult.Cancel;

	#region Selecting Files

	private void BtnKDiff3Path_Click(object sender, EventArgs e) => GetUserFileChoice(txtKDiff3Path, "Executables|*.exe");

	private void BtnBmsPath_Click(object sender, EventArgs e) => GetUserFileChoice(txtBmsPath, "Executables|*.exe");

	private void BtnBmsPluginPath_Click(object sender, EventArgs e) => GetUserFileChoice(txtBmsPluginPath, "QuickBMS Plugins|*.bms");

	private void BtnWccLitePath_Click(object sender, EventArgs e) => GetUserFileChoice(txtWccLitePath, "Executables|*.exe");

	private static void GetUserFileChoice(TextBox txt, string filter)
	{
		using OpenFileDialog dlgSelectFile = new()
		{
			Filter = filter
		};
		if (!string.IsNullOrWhiteSpace(txt.Text) && File.Exists(txt.Text))
			dlgSelectFile.FileName = txt.Text;
		if (DialogResult.OK == dlgSelectFile.ShowDialog())
			txt.Text = dlgSelectFile.FileName.Replace(Environment.CurrentDirectory + "\\", "", StringComparison.OrdinalIgnoreCase);
	}

	#endregion

	#region Clicking Links

	private void LnkKDiff3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) => Process.Start("cmd", "/c start http://kdiff3.sourceforge.net/");

	private void LnkBms_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) => Process.Start("cmd", "/c start http://aluigi.altervista.org/quickbms.htm");

	private void LnkWccLite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) => Process.Start("cmd", "/c start http://www.nexusmods.com/witcher3/news/12625/?");

	#endregion

	#region Validation

	// Checked when the path changes, never while painting: on a disconnected network drive each File.Exists can stall the
	// window for seconds. OK checks again, for a file deleted or created since.
	private readonly Dictionary<TextBox, bool> _valid = [];

	private void Path_TextChanged(object sender, EventArgs e)
	{
		TextBox txt = (TextBox)sender;
		_valid[txt] = IsValid(txt);
		txt.Parent?.Invalidate();
	}

	private bool IsValid(TextBox txt) => txt.Text.EndsWithIgnoreCase(_extensions[txt]) && File.Exists(txt.Text);

	// Under each path, a line: green when it points at its file, amber when not. The field keeps the theme's colours - a
	// field filled green or brown, with white text, read at about 2:1.
	private void PaintPathMarks(object sender, PaintEventArgs e)
	{
		foreach (TextBox txt in _extensions.Keys)
		{
			if (txt.Parent != sender)
				continue;

			Theme theme = ThemeManager.CurrentTheme;
			using SolidBrush brush = new(_valid.GetValueOrDefault(txt) ? theme.SuccessColor : theme.WarningColor);
			e.Graphics.FillRectangle(brush, txt.Left, txt.Bottom + LogicalToDeviceUnits(1), txt.Width, Math.Max(2, LogicalToDeviceUnits(3)));
		}
	}

	#endregion
}