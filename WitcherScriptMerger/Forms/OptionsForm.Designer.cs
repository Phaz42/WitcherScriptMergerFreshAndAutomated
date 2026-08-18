using System.Windows.Forms;

using WitcherScriptMerger.Controls;
using WitcherScriptMerger.Theming;

using static WitcherScriptMerger.Program;

namespace WitcherScriptMerger.Forms
{
	partial class OptionsForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		internal void PanelPaintForwarder(object sender, PaintEventArgs e)
		{
			// This "forwarder" is a workaround for a Visual Studio 2022 bug, where the Form designer will
			// crash because MainFrm and/or ThemeManager are not initialized during design time.
			Program.ThemeMngr.PanelPaint(sender, e);

		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OptionsForm));
			chkPromptPrioritize = new CheckBox();
			chkPromptOutdatedMerge = new CheckBox();
			chkPackReport = new CheckBox();
			chkReviewEachMerge = new CheckBox();
			chkCompletionSounds = new CheckBox();
			chkMergeReport = new CheckBox();
			chkCollapseCustomLoadOrder = new CheckBox();
			chkCollapseNotMergeable = new CheckBox();
			btnCancel = new Button();
			btnOK = new Button();
			btnApply = new Button();
			chkAutoSkipKDiff3InfoDialogs = new CheckBox();
			chkAutoCreateScriptMerges = new CheckBox();
			chkAutoExit = new CheckBox();
			chkAutoDeleteOldMerges = new CheckBox();
			chkAutoOverwriteOldMerges = new CheckBox();
			lblGameDir = new Label();
			txtGameDir = new TextBox();
			btnSelectGameDirectory = new Button();
			btnDefaultOptions = new Button();
			chkCheckXmlFiles = new CheckBox();
			chkCheckScripts = new CheckBox();
			pnlConflicts = new Panel();
			cbCheckBundles = new ComboBox();
			lblCheckBundles = new Label();
			lblConflicts = new Label();
			lblCollapse = new Label();
			pnlCollapse = new Panel();
			chkCollapseIdenticalConflicts = new CheckBox();
			lblMerging = new Label();
			pnlMerging = new Panel();
			chkCheckDuplicatePrios = new CheckBox();
			lblAutomation = new Label();
			pnlAutomation = new Panel();
			pnlGameDirectory = new Panel();
			pnlButtonBar = new Panel();
			pnlTheming = new Panel();
			chkNoConflictsWitcher = new CheckBox();
			lblPrioTooltips = new Label();
			cbPrioTooltips = new ComboBox();
			lblColorTheme = new Label();
			cbColorTheme = new ComboBox();
			lblTheming = new Label();
			chkAccentColor = new CheckBox();
			pnlOptionsMain = new Panel();
			chkAutoBackupLoadOrder = new CheckBox();
			pnlConflicts.SuspendLayout();
			pnlCollapse.SuspendLayout();
			pnlMerging.SuspendLayout();
			pnlAutomation.SuspendLayout();
			pnlGameDirectory.SuspendLayout();
			pnlButtonBar.SuspendLayout();
			pnlTheming.SuspendLayout();
			pnlOptionsMain.SuspendLayout();
			SuspendLayout();
			// 
			// chkPromptPrioritize
			// 
			chkPromptPrioritize.AutoSize = true;
			chkPromptPrioritize.Font = new System.Drawing.Font("Segoe UI", 11F);
			chkPromptPrioritize.Location = new System.Drawing.Point(36, 432);
			chkPromptPrioritize.Margin = new Padding(11, 11, 11, 11);
			chkPromptPrioritize.Name = "chkPromptPrioritize";
			chkPromptPrioritize.Size = new System.Drawing.Size(549, 45);
			chkPromptPrioritize.TabIndex = 4;
			chkPromptPrioritize.Text = "Check priority of master merged mod";
			chkPromptPrioritize.UseVisualStyleBackColor = true;
			// 
			// chkPromptOutdatedMerge
			// 
			chkPromptOutdatedMerge.AutoSize = true;
			chkPromptOutdatedMerge.Font = new System.Drawing.Font("Segoe UI", 11F);
			chkPromptOutdatedMerge.Location = new System.Drawing.Point(36, 363);
			chkPromptOutdatedMerge.Margin = new Padding(11, 11, 11, 11);
			chkPromptOutdatedMerge.Name = "chkPromptOutdatedMerge";
			chkPromptOutdatedMerge.Size = new System.Drawing.Size(723, 45);
			chkPromptOutdatedMerge.TabIndex = 3;
			chkPromptOutdatedMerge.Text = "Ask to delete outdated merges (unless automated)";
			chkPromptOutdatedMerge.UseVisualStyleBackColor = true;
			// 
			// chkPackReport
			// 
			chkPackReport.AutoSize = true;
			chkPackReport.Font = new System.Drawing.Font("Segoe UI", 11F);
			chkPackReport.Location = new System.Drawing.Point(36, 294);
			chkPackReport.Margin = new Padding(11, 11, 11, 11);
			chkPackReport.Name = "chkPackReport";
			chkPackReport.Size = new System.Drawing.Size(494, 45);
			chkPackReport.TabIndex = 7;
			chkPackReport.Text = "Show report after packing bundle";
			chkPackReport.UseVisualStyleBackColor = true;
			// 
			// chkReviewEachMerge
			// 
			chkReviewEachMerge.AutoSize = true;
			chkReviewEachMerge.Font = new System.Drawing.Font("Segoe UI", 11F);
			chkReviewEachMerge.Location = new System.Drawing.Point(36, 88);
			chkReviewEachMerge.Margin = new Padding(11, 11, 11, 11);
			chkReviewEachMerge.Name = "chkReviewEachMerge";
			chkReviewEachMerge.Size = new System.Drawing.Size(553, 45);
			chkReviewEachMerge.TabIndex = 3;
			chkReviewEachMerge.Text = "Review each succesful merge in KDiff3";
			chkReviewEachMerge.UseVisualStyleBackColor = true;
			// 
			// chkCompletionSounds
			// 
			chkCompletionSounds.AutoSize = true;
			chkCompletionSounds.Font = new System.Drawing.Font("Segoe UI", 11F);
			chkCompletionSounds.Location = new System.Drawing.Point(36, 157);
			chkCompletionSounds.Margin = new Padding(11, 11, 11, 11);
			chkCompletionSounds.Name = "chkCompletionSounds";
			chkCompletionSounds.Size = new System.Drawing.Size(367, 45);
			chkCompletionSounds.TabIndex = 4;
			chkCompletionSounds.Text = "Play completion sounds";
			chkCompletionSounds.UseVisualStyleBackColor = true;
			// 
			// chkMergeReport
			// 
			chkMergeReport.AutoSize = true;
			chkMergeReport.Font = new System.Drawing.Font("Segoe UI", 11F);
			chkMergeReport.Location = new System.Drawing.Point(36, 226);
			chkMergeReport.Margin = new Padding(11, 11, 11, 11);
			chkMergeReport.Name = "chkMergeReport";
			chkMergeReport.Size = new System.Drawing.Size(445, 45);
			chkMergeReport.TabIndex = 6;
			chkMergeReport.Text = "Show report after each merge";
			chkMergeReport.UseVisualStyleBackColor = true;
			// 
			// chkCollapseCustomLoadOrder
			// 
			chkCollapseCustomLoadOrder.AutoSize = true;
			chkCollapseCustomLoadOrder.Font = new System.Drawing.Font("Segoe UI", 11F);
			chkCollapseCustomLoadOrder.Location = new System.Drawing.Point(36, 226);
			chkCollapseCustomLoadOrder.Margin = new Padding(11, 11, 11, 11);
			chkCollapseCustomLoadOrder.Name = "chkCollapseCustomLoadOrder";
			chkCollapseCustomLoadOrder.Size = new System.Drawing.Size(577, 45);
			chkCollapseCustomLoadOrder.TabIndex = 1;
			chkCollapseCustomLoadOrder.Text = "Conflicts resolved by custom load order";
			chkCollapseCustomLoadOrder.UseVisualStyleBackColor = true;
			// 
			// chkCollapseNotMergeable
			// 
			chkCollapseNotMergeable.AutoSize = true;
			chkCollapseNotMergeable.Font = new System.Drawing.Font("Segoe UI", 11F);
			chkCollapseNotMergeable.Location = new System.Drawing.Point(36, 157);
			chkCollapseNotMergeable.Margin = new Padding(11, 11, 11, 11);
			chkCollapseNotMergeable.Name = "chkCollapseNotMergeable";
			chkCollapseNotMergeable.Size = new System.Drawing.Size(367, 45);
			chkCollapseNotMergeable.TabIndex = 0;
			chkCollapseNotMergeable.Text = "Not mergeable conflicts";
			chkCollapseNotMergeable.UseVisualStyleBackColor = true;
			// 
			// btnCancel
			// 
			btnCancel.BackColor = System.Drawing.SystemColors.Control;
			btnCancel.DialogResult = DialogResult.Cancel;
			btnCancel.FlatAppearance.BorderColor = System.Drawing.SystemColors.ButtonShadow;
			btnCancel.Font = new System.Drawing.Font("Segoe UI", 11F);
			btnCancel.Location = new System.Drawing.Point(1209, 37);
			btnCancel.Margin = new Padding(8, 37, 8, 37);
			btnCancel.Name = "btnCancel";
			btnCancel.Size = new System.Drawing.Size(176, 64);
			btnCancel.TabIndex = 12;
			btnCancel.Text = "Cancel";
			btnCancel.UseVisualStyleBackColor = false;
			btnCancel.Click += BtnCancel_Click;
			// 
			// btnOK
			// 
			btnOK.BackColor = System.Drawing.SystemColors.Control;
			btnOK.DialogResult = DialogResult.OK;
			btnOK.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlDarkDark;
			btnOK.Font = new System.Drawing.Font("Segoe UI", 11F);
			btnOK.Location = new System.Drawing.Point(1017, 37);
			btnOK.Margin = new Padding(37, 37, 8, 37);
			btnOK.Name = "btnOK";
			btnOK.Size = new System.Drawing.Size(176, 64);
			btnOK.TabIndex = 11;
			btnOK.Text = "OK";
			btnOK.UseVisualStyleBackColor = false;
			btnOK.Click += BtnOK_Click;
			// 
			// btnApply
			// 
			btnApply.BackColor = System.Drawing.SystemColors.Control;
			btnApply.FlatAppearance.BorderColor = System.Drawing.SystemColors.ButtonShadow;
			btnApply.Font = new System.Drawing.Font("Segoe UI", 11F);
			btnApply.Location = new System.Drawing.Point(1401, 37);
			btnApply.Margin = new Padding(8, 37, 37, 37);
			btnApply.Name = "btnApply";
			btnApply.Size = new System.Drawing.Size(176, 64);
			btnApply.TabIndex = 13;
			btnApply.Text = "Apply";
			btnApply.UseVisualStyleBackColor = false;
			btnApply.Click += BtnApply_Click;
			// 
			// chkAutoSkipKDiff3InfoDialogs
			// 
			chkAutoSkipKDiff3InfoDialogs.AutoSize = true;
			chkAutoSkipKDiff3InfoDialogs.Font = new System.Drawing.Font("Segoe UI", 11F);
			chkAutoSkipKDiff3InfoDialogs.Location = new System.Drawing.Point(44, 302);
			chkAutoSkipKDiff3InfoDialogs.Margin = new Padding(11, 11, 11, 11);
			chkAutoSkipKDiff3InfoDialogs.Name = "chkAutoSkipKDiff3InfoDialogs";
			chkAutoSkipKDiff3InfoDialogs.Size = new System.Drawing.Size(541, 45);
			chkAutoSkipKDiff3InfoDialogs.TabIndex = 7;
			chkAutoSkipKDiff3InfoDialogs.Text = "Automatically skip KDiff3 info dialogs";
			chkAutoSkipKDiff3InfoDialogs.UseVisualStyleBackColor = true;
			// 
			// chkAutoCreateScriptMerges
			// 
			chkAutoCreateScriptMerges.AutoSize = true;
			chkAutoCreateScriptMerges.Font = new System.Drawing.Font("Segoe UI", 11F);
			chkAutoCreateScriptMerges.Location = new System.Drawing.Point(44, 96);
			chkAutoCreateScriptMerges.Margin = new Padding(11, 11, 11, 11);
			chkAutoCreateScriptMerges.Name = "chkAutoCreateScriptMerges";
			chkAutoCreateScriptMerges.Size = new System.Drawing.Size(543, 45);
			chkAutoCreateScriptMerges.TabIndex = 3;
			chkAutoCreateScriptMerges.Text = "Automatically create all script merges";
			chkAutoCreateScriptMerges.UseVisualStyleBackColor = true;
			// 
			// chkAutoExit
			// 
			chkAutoExit.AutoSize = true;
			chkAutoExit.Font = new System.Drawing.Font("Segoe UI", 11F);
			chkAutoExit.Location = new System.Drawing.Point(44, 371);
			chkAutoExit.Margin = new Padding(11, 11, 11, 11);
			chkAutoExit.Name = "chkAutoExit";
			chkAutoExit.Size = new System.Drawing.Size(642, 45);
			chkAutoExit.TabIndex = 4;
			chkAutoExit.Text = "Automatically exit when all merges complete";
			chkAutoExit.UseVisualStyleBackColor = true;
			// 
			// chkAutoDeleteOldMerges
			// 
			chkAutoDeleteOldMerges.AutoSize = true;
			chkAutoDeleteOldMerges.Font = new System.Drawing.Font("Segoe UI", 11F);
			chkAutoDeleteOldMerges.Location = new System.Drawing.Point(44, 165);
			chkAutoDeleteOldMerges.Margin = new Padding(11, 11, 11, 11);
			chkAutoDeleteOldMerges.Name = "chkAutoDeleteOldMerges";
			chkAutoDeleteOldMerges.Size = new System.Drawing.Size(558, 45);
			chkAutoDeleteOldMerges.TabIndex = 5;
			chkAutoDeleteOldMerges.Text = "Automatically delete outdated merges";
			chkAutoDeleteOldMerges.UseVisualStyleBackColor = true;
			// 
			// chkAutoOverwriteOldMerges
			// 
			chkAutoOverwriteOldMerges.AutoSize = true;
			chkAutoOverwriteOldMerges.Font = new System.Drawing.Font("Segoe UI", 11F);
			chkAutoOverwriteOldMerges.Location = new System.Drawing.Point(44, 234);
			chkAutoOverwriteOldMerges.Margin = new Padding(11, 11, 11, 11);
			chkAutoOverwriteOldMerges.Name = "chkAutoOverwriteOldMerges";
			chkAutoOverwriteOldMerges.Size = new System.Drawing.Size(519, 45);
			chkAutoOverwriteOldMerges.TabIndex = 6;
			chkAutoOverwriteOldMerges.Text = "Automatically overwrite old merges";
			chkAutoOverwriteOldMerges.UseVisualStyleBackColor = true;
			// 
			// lblGameDir
			// 
			lblGameDir.AutoSize = true;
			lblGameDir.Font = new System.Drawing.Font("Segoe UI", 12F);
			lblGameDir.Location = new System.Drawing.Point(39, 38);
			lblGameDir.Margin = new Padding(24, 24, 8, 24);
			lblGameDir.Name = "lblGameDir";
			lblGameDir.Size = new System.Drawing.Size(387, 45);
			lblGameDir.TabIndex = 14;
			lblGameDir.Text = "Witcher 3 game directory:";
			// 
			// txtGameDir
			// 
			txtGameDir.BorderStyle = BorderStyle.None;
			txtGameDir.Enabled = false;
			txtGameDir.Font = new System.Drawing.Font("Segoe UI", 12F);
			txtGameDir.Location = new System.Drawing.Point(442, 38);
			txtGameDir.Margin = new Padding(8, 24, 8, 24);
			txtGameDir.Name = "txtGameDir";
			txtGameDir.Size = new System.Drawing.Size(956, 43);
			txtGameDir.TabIndex = 15;
			txtGameDir.TextChanged += TxtGameDir_TextChanged;
			// 
			// btnSelectGameDirectory
			// 
			btnSelectGameDirectory.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			btnSelectGameDirectory.BackColor = System.Drawing.SystemColors.Control;
			btnSelectGameDirectory.FlatAppearance.BorderColor = System.Drawing.SystemColors.ButtonShadow;
			btnSelectGameDirectory.Font = new System.Drawing.Font("Segoe UI", 11F);
			btnSelectGameDirectory.Location = new System.Drawing.Point(1396, 34);
			btnSelectGameDirectory.Margin = new Padding(8, 24, 24, 24);
			btnSelectGameDirectory.Name = "btnSelectGameDirectory";
			btnSelectGameDirectory.Size = new System.Drawing.Size(176, 64);
			btnSelectGameDirectory.TabIndex = 16;
			btnSelectGameDirectory.Text = "Browse";
			btnSelectGameDirectory.UseVisualStyleBackColor = false;
			btnSelectGameDirectory.Click += BtnSelectGameDirectory_Click;
			// 
			// btnDefaultOptions
			// 
			btnDefaultOptions.BackColor = System.Drawing.SystemColors.Control;
			btnDefaultOptions.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlDarkDark;
			btnDefaultOptions.Font = new System.Drawing.Font("Segoe UI", 11F);
			btnDefaultOptions.Location = new System.Drawing.Point(52, 37);
			btnDefaultOptions.Margin = new Padding(37, 37, 37, 37);
			btnDefaultOptions.Name = "btnDefaultOptions";
			btnDefaultOptions.Size = new System.Drawing.Size(176, 64);
			btnDefaultOptions.TabIndex = 17;
			btnDefaultOptions.Text = "Defaults";
			btnDefaultOptions.UseVisualStyleBackColor = false;
			btnDefaultOptions.Click += BtnDefaultOptions_Click;
			// 
			// chkCheckXmlFiles
			// 
			chkCheckXmlFiles.AutoSize = true;
			chkCheckXmlFiles.Font = new System.Drawing.Font("Segoe UI", 11F);
			chkCheckXmlFiles.Location = new System.Drawing.Point(36, 157);
			chkCheckXmlFiles.Margin = new Padding(11, 11, 11, 11);
			chkCheckXmlFiles.Name = "chkCheckXmlFiles";
			chkCheckXmlFiles.Size = new System.Drawing.Size(357, 45);
			chkCheckXmlFiles.TabIndex = 1;
			chkCheckXmlFiles.Text = "Non-bundled XML files";
			chkCheckXmlFiles.UseVisualStyleBackColor = true;
			// 
			// chkCheckScripts
			// 
			chkCheckScripts.AutoSize = true;
			chkCheckScripts.Font = new System.Drawing.Font("Segoe UI", 11F);
			chkCheckScripts.Location = new System.Drawing.Point(36, 88);
			chkCheckScripts.Margin = new Padding(11, 11, 11, 11);
			chkCheckScripts.Name = "chkCheckScripts";
			chkCheckScripts.Size = new System.Drawing.Size(138, 45);
			chkCheckScripts.TabIndex = 0;
			chkCheckScripts.Text = "Scripts";
			chkCheckScripts.UseVisualStyleBackColor = true;
			// 
			// pnlConflicts
			// 
			pnlConflicts.Controls.Add(cbCheckBundles);
			pnlConflicts.Controls.Add(lblCheckBundles);
			pnlConflicts.Controls.Add(lblConflicts);
			pnlConflicts.Controls.Add(chkCheckScripts);
			pnlConflicts.Controls.Add(chkCheckXmlFiles);
			pnlConflicts.Location = new System.Drawing.Point(31, 170);
			pnlConflicts.Margin = new Padding(16, 32, 32, 8);
			pnlConflicts.Name = "pnlConflicts";
			pnlConflicts.Padding = new Padding(24, 24, 24, 24);
			pnlConflicts.Size = new System.Drawing.Size(764, 312);
			pnlConflicts.TabIndex = 19;
			pnlConflicts.Paint += PanelPaintForwarder;
			// 
			// cbCheckBundles
			// 
			cbCheckBundles.DropDownStyle = ComboBoxStyle.DropDownList;
			cbCheckBundles.Font = new System.Drawing.Font("Segoe UI", 10.8F);
			cbCheckBundles.FormattingEnabled = true;
			cbCheckBundles.Items.AddRange(new object[] { "On Every Scan", "Only on Manual Refresh", "Never" });
			cbCheckBundles.Location = new System.Drawing.Point(379, 219);
			cbCheckBundles.Margin = new Padding(11, 11, 11, 11);
			cbCheckBundles.MaxDropDownItems = 3;
			cbCheckBundles.Name = "cbCheckBundles";
			cbCheckBundles.Size = new System.Drawing.Size(336, 48);
			cbCheckBundles.TabIndex = 29;
			cbCheckBundles.SelectionChangeCommitted += CbCheckBundles_SelectionChangeCommitted;
			// 
			// lblCheckBundles
			// 
			lblCheckBundles.AutoSize = true;
			lblCheckBundles.Font = new System.Drawing.Font("Segoe UI", 11F);
			lblCheckBundles.Location = new System.Drawing.Point(68, 226);
			lblCheckBundles.Margin = new Padding(11, 11, 11, 11);
			lblCheckBundles.Name = "lblCheckBundles";
			lblCheckBundles.Size = new System.Drawing.Size(201, 41);
			lblCheckBundles.TabIndex = 29;
			lblCheckBundles.Tag = "DimmedLabel";
			lblCheckBundles.Text = "Bundled Files:";
			// 
			// lblConflicts
			// 
			lblConflicts.AutoSize = true;
			lblConflicts.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
			lblConflicts.Location = new System.Drawing.Point(32, 32);
			lblConflicts.Margin = new Padding(8, 8, 8, 8);
			lblConflicts.Name = "lblConflicts";
			lblConflicts.Size = new System.Drawing.Size(295, 37);
			lblConflicts.TabIndex = 20;
			lblConflicts.Text = "Check for Conflicts in:";
			// 
			// lblCollapse
			// 
			lblCollapse.AutoSize = true;
			lblCollapse.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
			lblCollapse.Location = new System.Drawing.Point(32, 32);
			lblCollapse.Margin = new Padding(8, 8, 8, 8);
			lblCollapse.Name = "lblCollapse";
			lblCollapse.Size = new System.Drawing.Size(399, 37);
			lblCollapse.TabIndex = 22;
			lblCollapse.Text = "Auto-collapse in Conflicts list:";
			// 
			// pnlCollapse
			// 
			pnlCollapse.Controls.Add(chkCollapseIdenticalConflicts);
			pnlCollapse.Controls.Add(chkCollapseNotMergeable);
			pnlCollapse.Controls.Add(chkCollapseCustomLoadOrder);
			pnlCollapse.Controls.Add(lblCollapse);
			pnlCollapse.Location = new System.Drawing.Point(834, 170);
			pnlCollapse.Margin = new Padding(32, 32, 16, 32);
			pnlCollapse.Name = "pnlCollapse";
			pnlCollapse.Padding = new Padding(24, 24, 24, 24);
			pnlCollapse.Size = new System.Drawing.Size(764, 312);
			pnlCollapse.TabIndex = 21;
			pnlCollapse.Paint += PanelPaintForwarder;
			// 
			// chkCollapseIdenticalConflicts
			// 
			chkCollapseIdenticalConflicts.AutoSize = true;
			chkCollapseIdenticalConflicts.Font = new System.Drawing.Font("Segoe UI", 11F);
			chkCollapseIdenticalConflicts.Location = new System.Drawing.Point(36, 88);
			chkCollapseIdenticalConflicts.Margin = new Padding(11, 11, 11, 11);
			chkCollapseIdenticalConflicts.Name = "chkCollapseIdenticalConflicts";
			chkCollapseIdenticalConflicts.Size = new System.Drawing.Size(689, 45);
			chkCollapseIdenticalConflicts.TabIndex = 23;
			chkCollapseIdenticalConflicts.Text = "Conflicts identical to previous file (bundled only)";
			chkCollapseIdenticalConflicts.UseVisualStyleBackColor = true;
			// 
			// lblMerging
			// 
			lblMerging.AutoSize = true;
			lblMerging.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
			lblMerging.Location = new System.Drawing.Point(32, 32);
			lblMerging.Margin = new Padding(8, 8, 8, 8);
			lblMerging.Name = "lblMerging";
			lblMerging.Size = new System.Drawing.Size(127, 37);
			lblMerging.TabIndex = 24;
			lblMerging.Text = "Merging";
			// 
			// pnlMerging
			// 
			pnlMerging.Controls.Add(chkCheckDuplicatePrios);
			pnlMerging.Controls.Add(chkPromptPrioritize);
			pnlMerging.Controls.Add(lblMerging);
			pnlMerging.Controls.Add(chkReviewEachMerge);
			pnlMerging.Controls.Add(chkPackReport);
			pnlMerging.Controls.Add(chkPromptOutdatedMerge);
			pnlMerging.Controls.Add(chkCompletionSounds);
			pnlMerging.Controls.Add(chkMergeReport);
			pnlMerging.Location = new System.Drawing.Point(31, 522);
			pnlMerging.Margin = new Padding(16, 32, 32, 8);
			pnlMerging.Name = "pnlMerging";
			pnlMerging.Padding = new Padding(24, 24, 24, 24);
			pnlMerging.Size = new System.Drawing.Size(764, 592);
			pnlMerging.TabIndex = 23;
			pnlMerging.Paint += PanelPaintForwarder;
			// 
			// chkCheckDuplicatePrios
			// 
			chkCheckDuplicatePrios.AutoSize = true;
			chkCheckDuplicatePrios.Font = new System.Drawing.Font("Segoe UI", 11F);
			chkCheckDuplicatePrios.Location = new System.Drawing.Point(36, 501);
			chkCheckDuplicatePrios.Margin = new Padding(11, 11, 11, 11);
			chkCheckDuplicatePrios.Name = "chkCheckDuplicatePrios";
			chkCheckDuplicatePrios.Size = new System.Drawing.Size(500, 45);
			chkCheckDuplicatePrios.TabIndex = 25;
			chkCheckDuplicatePrios.Text = "Check for duplicate mod priorities";
			chkCheckDuplicatePrios.UseVisualStyleBackColor = true;
			// 
			// lblAutomation
			// 
			lblAutomation.AutoSize = true;
			lblAutomation.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
			lblAutomation.Location = new System.Drawing.Point(41, 40);
			lblAutomation.Margin = new Padding(8, 8, 8, 8);
			lblAutomation.Name = "lblAutomation";
			lblAutomation.Size = new System.Drawing.Size(172, 37);
			lblAutomation.TabIndex = 26;
			lblAutomation.Text = "Automation";
			// 
			// pnlAutomation
			// 
			pnlAutomation.Controls.Add(chkAutoBackupLoadOrder);
			pnlAutomation.Controls.Add(chkAutoSkipKDiff3InfoDialogs);
			pnlAutomation.Controls.Add(chkAutoExit);
			pnlAutomation.Controls.Add(lblAutomation);
			pnlAutomation.Controls.Add(chkAutoCreateScriptMerges);
			pnlAutomation.Controls.Add(chkAutoOverwriteOldMerges);
			pnlAutomation.Controls.Add(chkAutoDeleteOldMerges);
			pnlAutomation.Location = new System.Drawing.Point(834, 522);
			pnlAutomation.Margin = new Padding(32, 32, 32, 8);
			pnlAutomation.Name = "pnlAutomation";
			pnlAutomation.Padding = new Padding(32, 32, 16, 32);
			pnlAutomation.Size = new System.Drawing.Size(764, 592);
			pnlAutomation.TabIndex = 25;
			pnlAutomation.Paint += PanelPaintForwarder;
			// 
			// pnlGameDirectory
			// 
			pnlGameDirectory.Anchor = AnchorStyles.Left | AnchorStyles.Right;
			pnlGameDirectory.BackColor = System.Drawing.SystemColors.ControlLight;
			pnlGameDirectory.Controls.Add(lblGameDir);
			pnlGameDirectory.Controls.Add(txtGameDir);
			pnlGameDirectory.Controls.Add(btnSelectGameDirectory);
			pnlGameDirectory.Location = new System.Drawing.Point(0, -3);
			pnlGameDirectory.Margin = new Padding(0);
			pnlGameDirectory.Name = "pnlGameDirectory";
			pnlGameDirectory.Size = new System.Drawing.Size(1628, 133);
			pnlGameDirectory.TabIndex = 27;
			pnlGameDirectory.Tag = "DialogBar";
			// 
			// pnlButtonBar
			// 
			pnlButtonBar.Anchor = AnchorStyles.Left | AnchorStyles.Right;
			pnlButtonBar.BackColor = System.Drawing.SystemColors.ControlLight;
			pnlButtonBar.Controls.Add(btnApply);
			pnlButtonBar.Controls.Add(btnCancel);
			pnlButtonBar.Controls.Add(btnOK);
			pnlButtonBar.Controls.Add(btnDefaultOptions);
			pnlButtonBar.Location = new System.Drawing.Point(0, 1451);
			pnlButtonBar.Margin = new Padding(5, 5, 5, 5);
			pnlButtonBar.Name = "pnlButtonBar";
			pnlButtonBar.Size = new System.Drawing.Size(1628, 133);
			pnlButtonBar.TabIndex = 28;
			pnlButtonBar.Tag = "DialogBar";
			// 
			// pnlTheming
			// 
			pnlTheming.Controls.Add(chkNoConflictsWitcher);
			pnlTheming.Controls.Add(lblPrioTooltips);
			pnlTheming.Controls.Add(cbPrioTooltips);
			pnlTheming.Controls.Add(lblColorTheme);
			pnlTheming.Controls.Add(cbColorTheme);
			pnlTheming.Controls.Add(lblTheming);
			pnlTheming.Controls.Add(chkAccentColor);
			pnlTheming.Location = new System.Drawing.Point(31, 1154);
			pnlTheming.Margin = new Padding(16, 32, 32, 32);
			pnlTheming.Name = "pnlTheming";
			pnlTheming.Padding = new Padding(24, 24, 24, 24);
			pnlTheming.Size = new System.Drawing.Size(1566, 264);
			pnlTheming.TabIndex = 25;
			pnlTheming.Paint += PanelPaintForwarder;
			// 
			// chkNoConflictsWitcher
			// 
			chkNoConflictsWitcher.AutoSize = true;
			chkNoConflictsWitcher.Font = new System.Drawing.Font("Segoe UI", 11F);
			chkNoConflictsWitcher.Location = new System.Drawing.Point(847, 157);
			chkNoConflictsWitcher.Margin = new Padding(11, 11, 11, 11);
			chkNoConflictsWitcher.Name = "chkNoConflictsWitcher";
			chkNoConflictsWitcher.Size = new System.Drawing.Size(601, 45);
			chkNoConflictsWitcher.TabIndex = 29;
			chkNoConflictsWitcher.Text = "Show Witcher when you have no conflicts";
			chkNoConflictsWitcher.UseVisualStyleBackColor = true;
			// 
			// lblPrioTooltips
			// 
			lblPrioTooltips.AutoSize = true;
			lblPrioTooltips.Font = new System.Drawing.Font("Segoe UI", 11F);
			lblPrioTooltips.Location = new System.Drawing.Point(36, 152);
			lblPrioTooltips.Margin = new Padding(11, 11, 11, 11);
			lblPrioTooltips.Name = "lblPrioTooltips";
			lblPrioTooltips.Size = new System.Drawing.Size(339, 82);
			lblPrioTooltips.TabIndex = 28;
			lblPrioTooltips.Tag = "DimmedLabel";
			lblPrioTooltips.Text = "Show tooltip on highest\r\npriority mod in conflicts:";
			// 
			// cbPrioTooltips
			// 
			cbPrioTooltips.DropDownStyle = ComboBoxStyle.DropDownList;
			cbPrioTooltips.DropDownWidth = 350;
			cbPrioTooltips.Font = new System.Drawing.Font("Segoe UI", 10.8F);
			cbPrioTooltips.FormattingEnabled = true;
			cbPrioTooltips.Items.AddRange(new object[] { "Never (but highest prio is still colored)", "Only for Bundled conflicts", "Only for Script conflicts", "Both Bundled and Script conflicts" });
			cbPrioTooltips.Location = new System.Drawing.Point(395, 176);
			cbPrioTooltips.Margin = new Padding(11, 11, 11, 11);
			cbPrioTooltips.MaxDropDownItems = 3;
			cbPrioTooltips.Name = "cbPrioTooltips";
			cbPrioTooltips.Size = new System.Drawing.Size(336, 48);
			cbPrioTooltips.TabIndex = 27;
			// 
			// lblColorTheme
			// 
			lblColorTheme.AutoSize = true;
			lblColorTheme.Font = new System.Drawing.Font("Segoe UI", 11F);
			lblColorTheme.Location = new System.Drawing.Point(36, 93);
			lblColorTheme.Margin = new Padding(11, 11, 11, 11);
			lblColorTheme.Name = "lblColorTheme";
			lblColorTheme.Size = new System.Drawing.Size(196, 41);
			lblColorTheme.TabIndex = 26;
			lblColorTheme.Tag = "DimmedLabel";
			lblColorTheme.Text = "Color Theme:";
			// 
			// cbColorTheme
			// 
			cbColorTheme.DropDownStyle = ComboBoxStyle.DropDownList;
			cbColorTheme.Font = new System.Drawing.Font("Segoe UI", 10.8F);
			cbColorTheme.FormattingEnabled = true;
			cbColorTheme.Items.AddRange(new object[] { "Follow Windows", "Light Theme", "Dark Theme" });
			cbColorTheme.Location = new System.Drawing.Point(395, 88);
			cbColorTheme.Margin = new Padding(11, 11, 11, 11);
			cbColorTheme.MaxDropDownItems = 3;
			cbColorTheme.Name = "cbColorTheme";
			cbColorTheme.Size = new System.Drawing.Size(336, 48);
			cbColorTheme.TabIndex = 25;
			// 
			// lblTheming
			// 
			lblTheming.AutoSize = true;
			lblTheming.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
			lblTheming.ForeColor = System.Drawing.SystemColors.ControlText;
			lblTheming.Location = new System.Drawing.Point(32, 32);
			lblTheming.Margin = new Padding(8, 8, 8, 8);
			lblTheming.Name = "lblTheming";
			lblTheming.Size = new System.Drawing.Size(793, 37);
			lblTheming.TabIndex = 24;
			lblTheming.Text = "User Interface 🡆 Restart application to apply theme change!";
			// 
			// chkAccentColor
			// 
			chkAccentColor.AutoSize = true;
			chkAccentColor.Font = new System.Drawing.Font("Segoe UI", 11F);
			chkAccentColor.Location = new System.Drawing.Point(847, 88);
			chkAccentColor.Margin = new Padding(11, 11, 11, 11);
			chkAccentColor.Name = "chkAccentColor";
			chkAccentColor.Size = new System.Drawing.Size(409, 45);
			chkAccentColor.TabIndex = 3;
			chkAccentColor.Text = "Use Windows Accent Color";
			chkAccentColor.UseVisualStyleBackColor = true;
			// 
			// pnlOptionsMain
			// 
			pnlOptionsMain.BackColor = System.Drawing.Color.Transparent;
			pnlOptionsMain.Controls.Add(pnlTheming);
			pnlOptionsMain.Controls.Add(pnlButtonBar);
			pnlOptionsMain.Controls.Add(pnlGameDirectory);
			pnlOptionsMain.Controls.Add(pnlAutomation);
			pnlOptionsMain.Controls.Add(pnlMerging);
			pnlOptionsMain.Controls.Add(pnlCollapse);
			pnlOptionsMain.Controls.Add(pnlConflicts);
			pnlOptionsMain.Location = new System.Drawing.Point(0, 0);
			pnlOptionsMain.Margin = new Padding(0);
			pnlOptionsMain.Name = "pnlOptionsMain";
			pnlOptionsMain.Size = new System.Drawing.Size(1628, 1582);
			pnlOptionsMain.TabIndex = 29;
			// 
			// chkAutoBackupLoadOrder
			// 
			chkAutoBackupLoadOrder.AutoSize = true;
			chkAutoBackupLoadOrder.Font = new System.Drawing.Font("Segoe UI", 11F);
			chkAutoBackupLoadOrder.Location = new System.Drawing.Point(44, 438);
			chkAutoBackupLoadOrder.Margin = new Padding(11);
			chkAutoBackupLoadOrder.Name = "chkAutoBackupLoadOrder";
			chkAutoBackupLoadOrder.Size = new System.Drawing.Size(668, 45);
			chkAutoBackupLoadOrder.TabIndex = 27;
			chkAutoBackupLoadOrder.Text = "Automatically make backups of Load Order file";
			chkAutoBackupLoadOrder.UseVisualStyleBackColor = true;
			// 
			// OptionsForm
			// 
			AcceptButton = btnOK;
			AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
			AutoScaleMode = AutoScaleMode.Font;
			AutoScroll = true;
			BackColor = System.Drawing.SystemColors.Control;
			CancelButton = btnCancel;
			ClientSize = new System.Drawing.Size(1628, 1582);
			Controls.Add(pnlOptionsMain);
			DoubleBuffered = true;
			FormBorderStyle = FormBorderStyle.FixedSingle;
			Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
			Margin = new Padding(6, 8, 6, 8);
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "OptionsForm";
			StartPosition = FormStartPosition.CenterParent;
			Text = "Options";
			TopMost = true;
			Load += Options_Load;
			pnlConflicts.ResumeLayout(false);
			pnlConflicts.PerformLayout();
			pnlCollapse.ResumeLayout(false);
			pnlCollapse.PerformLayout();
			pnlMerging.ResumeLayout(false);
			pnlMerging.PerformLayout();
			pnlAutomation.ResumeLayout(false);
			pnlAutomation.PerformLayout();
			pnlGameDirectory.ResumeLayout(false);
			pnlGameDirectory.PerformLayout();
			pnlButtonBar.ResumeLayout(false);
			pnlTheming.ResumeLayout(false);
			pnlTheming.PerformLayout();
			pnlOptionsMain.ResumeLayout(false);
			ResumeLayout(false);
		}

		#endregion
		private CheckBox chkPromptPrioritize;
		private CheckBox chkPromptOutdatedMerge;
		private CheckBox chkPackReport;
		private CheckBox chkMergeReport;
		private CheckBox chkCompletionSounds;
		private CheckBox chkReviewEachMerge;
		private CheckBox chkCollapseCustomLoadOrder;
		private CheckBox chkCollapseNotMergeable;
		private Button btnCancel;
		private Button btnOK;
		private Button btnApply;
		private CheckBox chkAutoDeleteOldMerges;
		private CheckBox chkAutoExit;
		private CheckBox chkAutoCreateScriptMerges;
		private CheckBox chkAutoOverwriteOldMerges;
		private CheckBox chkAutoSkipKDiff3InfoDialogs;
		private Label lblGameDir;
		private TextBox txtGameDir;
		private Button btnSelectGameDirectory;
		private Button btnDefaultOptions;
		private CheckBox chkCheckXmlFiles;
		private CheckBox chkCheckScripts;
		private Panel pnlConflicts;
		private Label lblConflicts;
		private Label lblCollapse;
		private Panel pnlCollapse;
		private Label lblMerging;
		private Panel pnlMerging;
		private Label lblAutomation;
		private Panel pnlAutomation;
		private Panel pnlGameDirectory;
		private Panel pnlButtonBar;
		private Panel pnlTheming;
		private Label lblTheming;
		private CheckBox chkAccentColor;
		private Label lblColorTheme;
		private ComboBox cbColorTheme;
		private Label lblPrioTooltips;
		private ComboBox cbPrioTooltips;
		private CheckBox chkCollapseIdenticalConflicts;
		private Panel pnlOptionsMain;
		internal ComboBox cbCheckBundles;
		private Label lblCheckBundles;
		private CheckBox chkCheckDuplicatePrios;
		private CheckBox chkNoConflictsWitcher;
		private CheckBox chkAutoBackupLoadOrder;
	}
}