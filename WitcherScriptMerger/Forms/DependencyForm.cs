using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

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

	internal DependencyForm()
	{
		InitializeComponent();
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
		bool allValid =
			Color.DarkSeaGreen == txtKDiff3Path.BackColor &&
			Color.DarkSeaGreen == txtBmsPath.BackColor &&
			Color.DarkSeaGreen == txtBmsPluginPath.BackColor &&
			Color.DarkSeaGreen == txtWccLitePath.BackColor;

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

	private void Exe_TextChanged(object sender, EventArgs e) => ValidateTextBox(sender as TextBox, ".exe");

	private void Bms_TextChanged(object sender, EventArgs e) => ValidateTextBox(sender as TextBox, ".bms");

	private static void ValidateTextBox(TextBox txt, string validExtension)
	{
		string path = txt.Text;
		txt.BackColor = path.EndsWithIgnoreCase(validExtension) && File.Exists(path)
			? Color.DarkSeaGreen
			: Color.RosyBrown;
	}

	#endregion
}