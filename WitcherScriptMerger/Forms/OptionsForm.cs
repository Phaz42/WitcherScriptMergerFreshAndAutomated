using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using Microsoft.Win32;

using WitcherScriptMerger.Theming;

#pragma warning disable CS8981
using def = WitcherScriptMerger.Utilities.DefaultSettings;
#pragma warning restore CS8981

using static WitcherScriptMerger.Program;

namespace WitcherScriptMerger.Forms;

internal partial class OptionsForm : BaseForm
{
	private string gameDirectory = string.Empty;
	internal string GameDirectory { get => gameDirectory; set => SetGameDirectory(value); }

	internal bool BundleScanOptionChanged;

	private List<ColorFader> colorFaders;

	private void SetGameDirectory(string path)
	{
		gameDirectory = path;
		txtGameDir.Text = path;
		Settings.Set("GameDirectory", path);
		Settings.Save();
	}

	internal OptionsForm()
	{
		InitializeComponent();
		UpdateWindowSizeFromWorkingArea(this);
		ThemeMngr.ApplyThemeOnForm(this);
	}

	private static void ColorizeGroupBoxLine(GroupBox groupBox, Color lineColor)
	{
		using Graphics graphics = groupBox.CreateGraphics();
		Rectangle borderRect = new(0, 0, groupBox.Width - 1, groupBox.Height - 1);
		ControlPaint.DrawBorder(graphics, borderRect, lineColor, ButtonBorderStyle.Solid);
	}

	private void Options_Load(object sender, EventArgs e)
	{
		ThemeMngr.AttachColorFaderToCheckBoxes(this, out colorFaders, 300, 700);
		CheckSetGameDir();
		LoadSettings();

		btnOK.Select();
	}

	private void Save()
	{
		Settings.Set("GameDirectory", GameDirectory);

		Settings.Set("CheckScripts", chkCheckScripts.Checked);
		Settings.Set("CheckXmlFiles", chkCheckXmlFiles.Checked);
		Settings.Set("CheckBundles", cbCheckBundles.SelectedIndex);

		Settings.Set("CollapseIdenticalConflicts", chkCollapseIdenticalConflicts.Checked);
		Settings.Set("CollapseNotMergeable", chkCollapseNotMergeable.Checked);
		Settings.Set("CollapseCustomLoadOrder", chkCollapseCustomLoadOrder.Checked);

		Settings.Set("ValidateMergeSources", chkPromptOutdatedMerge.Checked);
		Settings.Set("ValidateCustomLoadOrder", chkPromptPrioritize.Checked);

		Settings.Set("ReviewEachMerge", chkReviewEachMerge.Checked);
		Settings.Set("CheckDuplicatePrios", chkCheckDuplicatePrios.Checked);
		Settings.Set("PlayCompletionSounds", chkCompletionSounds.Checked);
		Settings.Set("ReportAfterMerge", chkMergeReport.Checked);
		Settings.Set("ReportAfterPack", chkPackReport.Checked);

		Settings.Set("AutoCreateScriptMerges", chkAutoCreateScriptMerges.Checked);
		Settings.Set("AutoDeleteOldMerges", chkAutoDeleteOldMerges.Checked);
		Settings.Set("AutoOverwriteOldMerges", chkAutoOverwriteOldMerges.Checked);
		Settings.Set("AutoSkipKDiff3InfoDialogs", chkAutoSkipKDiff3InfoDialogs.Checked);
		Settings.Set("AutoExit", chkAutoExit.Checked);
		Settings.Set("AutoBackupLoadOrder", chkAutoBackupLoadOrder.Checked);

		Settings.Set("ColorTheme", cbColorTheme.SelectedIndex);
		Settings.Set("AccentColor", chkAccentColor.Checked);
		Settings.Set("PrioTooltips", cbPrioTooltips.SelectedIndex);
		Settings.Set("NoConflictsWitcher", chkNoConflictsWitcher.Checked);

		Settings.Save();
	}

	private void BtnOK_Click(object sender, EventArgs e)
	{
		Save();
		MainFrm.uiThreadManager.ShowHideNoConflictsWitcher();
		DialogResult = DialogResult.OK;
	}

	private void BtnCancel_Click(object sender, EventArgs e) => DialogResult = DialogResult.Cancel;

	private void BtnApply_Click(object sender, EventArgs e)
	{
		Save();
		MainFrm.uiThreadManager.ShowHideNoConflictsWitcher();
		DialogResult = DialogResult.None;
	}

	private void BtnSelectGameDirectory_Click(object sender, EventArgs e) => SelectGameDirectory();

	private void TxtGameDir_TextChanged(object sender, EventArgs e) => Settings.Set("GameDirectory", GameDirectory);

	internal void CheckSetGameDir()
	{
		GameDirectory = Settings.Get("GameDirectory");
		if (Directory.Exists(GameDirectory))
			return;

		string SteamPath = string.Empty;
		string GogPath = string.Empty;
		try
		{
			SteamPath = Registry.GetValue(
				"HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Steam App 292030",
				"InstallLocation", null).ToString();
			GogPath = Registry.GetValue(
				"HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\GOG.com\\Games\\1207664643",
				"path", null).ToString();
		}
		catch (Exception) { } // Don't care

		if (Directory.Exists(SteamPath))
		{
			if (ConfirmAutoGameDir(SteamPath))
				GameDirectory = SteamPath;
		}

		if (Directory.Exists(GogPath))
		{
			if (ConfirmAutoGameDir(GogPath))
				GameDirectory = GogPath;
		}

		if (string.IsNullOrEmpty(GameDirectory))
		{
			_ = MainFrm.uiThreadManager.ShowMessage("In the following screen, please select your main Witcher 3 game directory.",
				"Select Witcher 3 game directory", MessageBoxButtons.OK, MessageBoxIcon.Information);
			SelectGameDirectory();
		}
	}

	private static bool ConfirmAutoGameDir(string gameDir)
	{
		string msg = "Your Witcher 3 Game directory was found at the following location, is this correct?\n\n" + gameDir;

		return DialogResult.Yes == MainFrm.uiThreadManager.ShowMessage(
			msg,
			"Check Witcher 3 game directory location",
			MessageBoxButtons.YesNo,
			MessageBoxIcon.Question);
	}

	private void SelectGameDirectory()
	{
		string dirChoice = GetUserDirectoryChoice();
		if (Directory.Exists(dirChoice))
		{
			if (dirChoice.EndsWithIgnoreCase("The Witcher 3 Wild Hunt\\Mods"))
				GameDirectory = Path.GetDirectoryName(dirChoice);
		}
		else
		{
			_ = MainFrm.uiThreadManager.ShowMessage("Invalid or no game directory selected, Script Merger will now exit.\n\n" +
				"Restart the program to try again.", "Invalid game directory", MessageBoxButtons.OK, MessageBoxIcon.Error);
			AutoExit(null);
		}
	}

	private static string GetUserDirectoryChoice()
	{
		using FolderBrowserDialog dlgSelectRoot = new();
		if (Directory.Exists(Paths.GameDirectory))
			dlgSelectRoot.SelectedPath = Paths.GameDirectory;
		return DialogResult.OK == dlgSelectRoot.ShowDialog() ? dlgSelectRoot.SelectedPath : null;
	}

	private void GrpCheckForConflicts_Paint(object sender, PaintEventArgs e)
	{
		GroupBox groupBox = (GroupBox)sender;
		ColorizeGroupBoxLine(groupBox, Color.Red);
	}

	private void BtnDefaultOptions_Click(object sender, EventArgs e)
	{
		LoadSettings(true);
		Save();
	}

	private void CbCheckBundles_SelectionChangeCommitted(object sender, EventArgs e) => BundleScanOptionChanged = true;

	private void LoadSettings(bool useDefaults = false)
	{
		bool useDef = useDefaults;
		chkCheckScripts.Checked = LoadSetting("CheckScripts", useDef, def.CheckScripts);
		chkCheckXmlFiles.Checked = LoadSetting("CheckXmlFiles", useDef, def.CheckXmlFiles);
		cbCheckBundles.SelectedIndex = LoadSetting("CheckBundles", useDef, def.CheckBundles);
		chkCollapseIdenticalConflicts.Checked = LoadSetting("CollapseIdenticalConflicts", useDef, def.CollapseIdenticalConflicts);
		chkCollapseNotMergeable.Checked = LoadSetting("CollapseNotMergeable", useDef, def.CollapseNotMergeable);
		chkCollapseCustomLoadOrder.Checked = LoadSetting("CollapseCustomLoadOrder", useDef, def.CollapseCustomLoadOrder);
		chkPromptOutdatedMerge.Checked = LoadSetting("ValidateMergeSources", useDef, def.ValidateMergeSources);
		chkPromptPrioritize.Checked = LoadSetting("ValidateCustomLoadOrder", useDef, def.ValidateCustomLoadOrder);
		chkReviewEachMerge.Checked = LoadSetting("ReviewEachMerge", useDef, def.ReviewEachMerge);
		chkCheckDuplicatePrios.Checked = LoadSetting("CheckDuplicatePrios", useDef, def.CheckDuplicatePrios);
		chkCompletionSounds.Checked = LoadSetting("PlayCompletionSounds", useDef, def.PlayCompletionSounds);
		chkMergeReport.Checked = LoadSetting("ReportAfterMerge", useDef, def.ReportAfterMerge);
		chkPackReport.Checked = LoadSetting("ReportAfterPack", useDef, def.ReportAfterPack);
		chkAutoCreateScriptMerges.Checked = LoadSetting("AutoCreateScriptMerges", useDef, def.AutoCreateScriptMerges);
		chkAutoDeleteOldMerges.Checked = LoadSetting("AutoDeleteOldMerges", useDef, def.AutoDeleteOldMerges);
		chkAutoOverwriteOldMerges.Checked = LoadSetting("AutoOverwriteOldMerges", useDef, def.AutoOverwriteOldMerges);
		chkAutoSkipKDiff3InfoDialogs.Checked = LoadSetting("AutoSkipKDiff3InfoDialogs", useDef, def.AutoSkipKDiff3InfoDialogs);
		chkAutoExit.Checked = LoadSetting("AutoExit", useDef, def.AutoExit);
		chkAutoBackupLoadOrder.Checked = LoadSetting("AutoBackupLoadOrder", useDef, def.AutoBackupLoadOrder);
		cbColorTheme.SelectedIndex = LoadSetting("ColorTheme", useDef, def.ColorTheme);
		chkAccentColor.Checked = LoadSetting("AccentColor", useDef, def.AccentColor);
		cbPrioTooltips.SelectedIndex = LoadSetting("PrioTooltips", useDef, def.PrioTooltips);
		chkNoConflictsWitcher.Checked = LoadSetting("NoConflictsWitcher", useDef, def.NoConflictsWitcher);

		Save();
	}

	private static T LoadSetting<T>(string settingKey, bool useDefaults, T defaultValue) => useDefaults ? defaultValue : Settings.Get(settingKey, defaultValue);
}
