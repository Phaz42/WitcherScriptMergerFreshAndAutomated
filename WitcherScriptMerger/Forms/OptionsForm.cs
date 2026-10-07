using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using Microsoft.Win32;

using WitcherScriptMerger.Theming;
using WitcherScriptMerger.UI;

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
		SetToolTips();
		UpdateWindowSizeFromWorkingArea(this);
		ThemeMngr.ApplyThemeOnForm(this);
	}

	// What each option does, and what the less obvious terms mean. The tooltip control lives in the designer's container,
	// which the form disposes.
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Added to the form's components container, which disposes it with the form.")]
	private void SetToolTips()
	{
		components ??= new System.ComponentModel.Container();
		ToolTip tips = new(components) { AutoPopDelay = 20000, InitialDelay = 500, ReshowDelay = 200 };
		void Tip(string text, params Control[] controls)
		{
			foreach (Control control in controls)
				tips.SetToolTip(control, text);
		}

		Tip("The folder The Witcher 3 is installed in, which holds the bin, content and Mods folders. SM-FAE reads the game's own files and your mods from there.",
			lblGameDir, txtGameDir, btnSelectGameDirectory);
		Tip("Looks for script files (.ws) that more than one mod changes. Unmerged, only one mod's version is used, which can break the other mod or stop the game from starting.",
			chkCheckScripts);
		Tip("Looks for loose XML files, outside bundles, that more than one mod changes. They hold game data such as items, skills and settings.",
			chkCheckXmlFiles);
		Tip("Also looks inside mods' bundles, the packed files in their content folders: on every scan, only when you click Refresh, or never. Checking bundles takes longer, because every bundle has to be read; only on Refresh keeps SM-FAE's start fast.",
			lblCheckBundles, cbCheckBundles);
		Tip("Collapses a conflict when the same mods conflict in the file listed just above it, which is common for mods that change many files. Scripts are never collapsed this way.",
			chkCollapseIdenticalConflicts);
		Tip("Collapses the not-mergeable sections, so the conflicts you can merge come first.",
			chkCollapseNotMergeable);
		Tip("Collapses conflicts where your load order decides which mod wins, so the ones still needing a decision stand out.",
			chkCollapseCustomLoadOrder);
		Tip("After merging, checks that the merged mod has the top load order number. If another mod outranks it, the game may use that mod's file instead of the merge, and SM-FAE offers to fix that.",
			chkPromptPrioritize);
		Tip("Asks before deleting an outdated merge. A merge is outdated when a mod in it was updated, removed or disabled since merging: it no longer matches your mods.",
			chkPromptOutdatedMerge);
		Tip("Opens every merge in KDiff3 for you to check before it's saved, even when SM-FAE could merge it automatically.",
			chkReviewEachMerge);
		Tip("Shows a short guide when KDiff3 opens for a merge you have to finish by hand: what's happening, how to read KDiff3, and the easy way out.",
			chkShowKDiff3Guide);
		Tip("Shows which files went into each merge and where the result was saved.",
			chkMergeReport);
		Tip("Shows what went into the merged bundle after it's packed.",
			chkPackReport);
		Tip("Warns when two mods have the same load order number in mods.settings. Then the game decides their order itself, and you can't control which one wins.",
			chkCheckDuplicatePrios);
		Tip("Merges all script conflicts by itself after each scan, without you selecting them. Conflicts that need a decision still open KDiff3.",
			chkAutoCreateScriptMerges);
		Tip("Deletes outdated merges without asking. A merge is outdated when a mod in it was updated, removed or disabled, or its merged file is missing; merge again afterwards.",
			chkAutoDeleteOldMerges);
		Tip("When a merged file already exists, replaces it without asking.",
			chkAutoOverwriteOldMerges);
		Tip("Hides KDiff3's own information windows, such as its conflict count, which add nothing in SM-FAE.",
			chkAutoSkipKDiff3InfoDialogs);
		Tip("Closes SM-FAE when merging is done and no conflicts remain.",
			chkAutoExit);
		Tip("Light, dark, or the same as Windows. A change takes effect after SM-FAE restarts.",
			lblColorTheme, cbColorTheme);
		Tip("Uses your Windows accent colour for highlights, such as the winning mod and the main buttons, instead of SM-FAE's blue.",
			chkAccentColor);
		Tip("Shows a picture when there are no conflicts. You can use your own; see Images\\Custom in SM-FAE's folder.",
			chkNoConflictsWitcher);
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
		KDiff3Guide.SetGuidesOn(chkShowKDiff3Guide.Checked);
		Settings.Set("ReportAfterMerge", chkMergeReport.Checked);
		Settings.Set("ReportAfterPack", chkPackReport.Checked);

		Settings.Set("AutoCreateScriptMerges", chkAutoCreateScriptMerges.Checked);
		Settings.Set("AutoDeleteOldMerges", chkAutoDeleteOldMerges.Checked);
		Settings.Set("AutoOverwriteOldMerges", chkAutoOverwriteOldMerges.Checked);
		Settings.Set("AutoSkipKDiff3InfoDialogs", chkAutoSkipKDiff3InfoDialogs.Checked);
		Settings.Set("AutoExit", chkAutoExit.Checked);

		Settings.Set("ColorTheme", cbColorTheme.SelectedIndex);
		Settings.Set("AccentColor", chkAccentColor.Checked);
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

		// Each store's registry entry is read on its own: a missing entry is normal (most players own the game in one
		// store only), and must not stop the other from being found.
		string steamPath = ReadRegistryPath(
			"HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Steam App 292030", "InstallLocation");
		string gogPath = ReadRegistryPath(
			"HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\GOG.com\\Games\\1207664643", "path");

		// With both installed, the second is offered only if the first was declined.
		if (Directory.Exists(steamPath) && ConfirmAutoGameDir(steamPath))
			GameDirectory = steamPath;
		else if (Directory.Exists(gogPath) && ConfirmAutoGameDir(gogPath))
			GameDirectory = gogPath;

		// Also when a saved folder no longer exists: it is not a game folder, so the user is asked for one.
		if (!Directory.Exists(GameDirectory))
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

	// A registry value holding a folder, or empty when the key or value does not exist or cannot be read.
	private static string ReadRegistryPath(string keyName, string valueName)
	{
		try
		{
			return Registry.GetValue(keyName, valueName, null) as string ?? string.Empty;
		}
		catch (Exception ex) when (ex is System.Security.SecurityException or IOException or UnauthorizedAccessException)
		{
			return string.Empty;
		}
	}

	/// <summary>
	/// Lets the user pick the game folder and takes it. Cancelling exits only when no game folder exists to keep.
	/// </summary>
	internal void SelectGameDirectory()
	{
		string dirChoice = GetUserDirectoryChoice();
		if (Directory.Exists(dirChoice))
		{
			// Picking the Mods folder inside the game folder is taken to mean the game folder.
			if (Path.GetFileName(dirChoice.TrimEnd('\\', '/')).EqualsIgnoreCase("Mods"))
				dirChoice = Path.GetDirectoryName(dirChoice.TrimEnd('\\', '/'));

			GameDirectory = dirChoice;
		}
		else if (!Directory.Exists(GameDirectory))
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
		chkShowKDiff3Guide.Checked = useDef ? def.ShowKDiff3Guide : KDiff3Guide.GuidesOn;
		chkMergeReport.Checked = LoadSetting("ReportAfterMerge", useDef, def.ReportAfterMerge);
		chkPackReport.Checked = LoadSetting("ReportAfterPack", useDef, def.ReportAfterPack);
		chkAutoCreateScriptMerges.Checked = LoadSetting("AutoCreateScriptMerges", useDef, def.AutoCreateScriptMerges);
		chkAutoDeleteOldMerges.Checked = LoadSetting("AutoDeleteOldMerges", useDef, def.AutoDeleteOldMerges);
		chkAutoOverwriteOldMerges.Checked = LoadSetting("AutoOverwriteOldMerges", useDef, def.AutoOverwriteOldMerges);
		chkAutoSkipKDiff3InfoDialogs.Checked = LoadSetting("AutoSkipKDiff3InfoDialogs", useDef, def.AutoSkipKDiff3InfoDialogs);
		chkAutoExit.Checked = LoadSetting("AutoExit", useDef, def.AutoExit);
		cbColorTheme.SelectedIndex = LoadSetting("ColorTheme", useDef, def.ColorTheme);
		chkAccentColor.Checked = LoadSetting("AccentColor", useDef, def.AccentColor);
		chkNoConflictsWitcher.Checked = LoadSetting("NoConflictsWitcher", useDef, def.NoConflictsWitcher);

		Save();
	}

	private static T LoadSetting<T>(string settingKey, bool useDefaults, T defaultValue) => useDefaults ? defaultValue : Settings.Get(settingKey, defaultValue);
}
