using System.Drawing;
using System.Windows.Forms;

using WitcherScriptMerger.Controls;
using WitcherScriptMerger.Theming;

using static WitcherScriptMerger.Theming.ThemeManager;
using static WitcherScriptMerger.Program;

namespace WitcherScriptMerger.Forms
{
	partial class MainForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			btnRefreshConflicts = new Button();
			lblConflicts = new Label();
			btnCreateAllScriptMerges = new Button();
			btnDeleteAllMerges = new Button();
			btnRefreshMerged = new Button();
			lblMerges = new Label();
			btnDeleteMerges = new Button();
			pnlProgress = new Panel();
			btnDontExit = new Button();
			btnExitNow = new Button();
			lblBundleScanInfo = new Label();
			lblProgressCurrentPhase = new Label();
			lblProgressCurrentAction = new Label();
			progressBar = new ProgressBar();
			statusStrip = new StatusStrip();
			lblStatusLeft1 = new ToolStripStatusLabel();
			lblStatusLeft2 = new ToolStripStatusLabel();
			lblStatusLeft3 = new ToolStripStatusLabel();
			lblStatusSpring = new ToolStripStatusLabel();
			lblStatusRight = new ToolStripStatusLabel();
			toolTip1 = new ToolTip(components);
			btnCreateMerges = new Button();
			treConflictsToolTip = new ToolTip(components);
			tlpMain = new TableLayoutPanel();
			tlpConflictsHeader = new TableLayoutPanel();
			btnMainMenu = new Button();
			tlpMergesHeader = new TableLayoutPanel();
			tableLayoutPanel2 = new TableLayoutPanel();
			button1 = new Button();
			contextMenuStrip1 = new ContextMenuStrip(components);
			pnlProgress.SuspendLayout();
			statusStrip.SuspendLayout();
			tlpMain.SuspendLayout();
			tlpConflictsHeader.SuspendLayout();
			tlpMergesHeader.SuspendLayout();
			SuspendLayout();
			// 
			// btnRefreshConflicts
			// 
			btnRefreshConflicts.Anchor = AnchorStyles.Right;
			btnRefreshConflicts.AutoSize = true;
			btnRefreshConflicts.Font = new Font("Segoe UI", 10F);
			btnRefreshConflicts.Location = new Point(372, 19);
			btnRefreshConflicts.Margin = new Padding(0, 0, 25, 0);
			btnRefreshConflicts.MinimumSize = new Size(75, 35);
			btnRefreshConflicts.Name = "btnRefreshConflicts";
			btnRefreshConflicts.Size = new Size(75, 35);
			btnRefreshConflicts.TabIndex = 0;
			btnRefreshConflicts.Text = "&Refresh";
			toolTip1.SetToolTip(btnRefreshConflicts, "Refresh Conflicts (Keyboard: F5)");
			btnRefreshConflicts.UseVisualStyleBackColor = false;
			btnRefreshConflicts.Click += BtnRefreshConflicts_Click;
			// 
			// lblConflicts
			// 
			lblConflicts.Anchor = AnchorStyles.None;
			lblConflicts.AutoSize = true;
			lblConflicts.Font = new Font("Segoe UI", 14F);
			lblConflicts.Location = new Point(193, 24);
			lblConflicts.Margin = new Padding(2);
			lblConflicts.Name = "lblConflicts";
			lblConflicts.Size = new Size(85, 25);
			lblConflicts.TabIndex = 16;
			lblConflicts.Text = "Conflicts";
			// 
			// btnCreateAllScriptMerges
			// 
			btnCreateAllScriptMerges.Anchor = AnchorStyles.Left;
			btnCreateAllScriptMerges.AutoSize = true;
			btnCreateAllScriptMerges.Font = new Font("Segoe UI", 11F);
			btnCreateAllScriptMerges.Location = new Point(245, 908);
			btnCreateAllScriptMerges.Margin = new Padding(5, 0, 0, 0);
			btnCreateAllScriptMerges.Name = "btnCreateAllScriptMerges";
			btnCreateAllScriptMerges.Size = new Size(179, 50);
			btnCreateAllScriptMerges.TabIndex = 17;
			btnCreateAllScriptMerges.Text = "Create &All Script Merges";
			toolTip1.SetToolTip(btnCreateAllScriptMerges, "Create All Script Merges (Keyboard: A)");
			btnCreateAllScriptMerges.UseVisualStyleBackColor = false;
			btnCreateAllScriptMerges.Click += BtnCreateAllMerges_Click;
			// 
			// btnDeleteAllMerges
			// 
			btnDeleteAllMerges.Anchor = AnchorStyles.Left;
			btnDeleteAllMerges.AutoSize = true;
			btnDeleteAllMerges.Font = new Font("Segoe UI", 11F);
			btnDeleteAllMerges.Location = new Point(725, 908);
			btnDeleteAllMerges.Margin = new Padding(5, 0, 0, 0);
			btnDeleteAllMerges.Name = "btnDeleteAllMerges";
			btnDeleteAllMerges.Size = new Size(138, 50);
			btnDeleteAllMerges.TabIndex = 8;
			btnDeleteAllMerges.Text = "D&elete All Merges";
			toolTip1.SetToolTip(btnDeleteAllMerges, "Delete All Merges (Keyboard: E)");
			btnDeleteAllMerges.UseVisualStyleBackColor = false;
			btnDeleteAllMerges.Click += BtnDeleteAllMerges_Click;
			// 
			// btnRefreshMerged
			// 
			btnRefreshMerged.Anchor = AnchorStyles.Right;
			btnRefreshMerged.AutoSize = true;
			btnRefreshMerged.Font = new Font("Segoe UI", 10F);
			btnRefreshMerged.Location = new Point(372, 19);
			btnRefreshMerged.Margin = new Padding(0, 0, 25, 0);
			btnRefreshMerged.MinimumSize = new Size(75, 35);
			btnRefreshMerged.Name = "btnRefreshMerged";
			btnRefreshMerged.Size = new Size(75, 35);
			btnRefreshMerged.TabIndex = 0;
			btnRefreshMerged.Text = "Re&fresh";
			toolTip1.SetToolTip(btnRefreshMerged, "Refresh Merges (Keyboard: F6)");
			btnRefreshMerged.UseVisualStyleBackColor = false;
			btnRefreshMerged.Click += BtnRefreshMerged_Click;
			// 
			// lblMerges
			// 
			lblMerges.Anchor = AnchorStyles.None;
			lblMerges.AutoSize = true;
			lblMerges.Font = new Font("Segoe UI", 14F);
			lblMerges.Location = new Point(198, 24);
			lblMerges.Margin = new Padding(2);
			lblMerges.Name = "lblMerges";
			lblMerges.Size = new Size(75, 25);
			lblMerges.TabIndex = 7;
			lblMerges.Text = "Merges";
			// 
			// btnDeleteMerges
			// 
			btnDeleteMerges.Anchor = AnchorStyles.Right;
			btnDeleteMerges.AutoSize = true;
			btnDeleteMerges.Font = new Font("Segoe UI", 11F);
			btnDeleteMerges.Location = new Point(538, 908);
			btnDeleteMerges.Margin = new Padding(0, 0, 5, 0);
			btnDeleteMerges.Name = "btnDeleteMerges";
			btnDeleteMerges.Size = new Size(177, 50);
			btnDeleteMerges.TabIndex = 2;
			btnDeleteMerges.Text = "&Delete Selected Merges";
			toolTip1.SetToolTip(btnDeleteMerges, "Delete Selected Merges (Keyboard: D)");
			btnDeleteMerges.UseVisualStyleBackColor = false;
			btnDeleteMerges.Click += BtnDeleteMerges_Click;
			// 
			// pnlProgress
			// 
			pnlProgress.Controls.Add(btnDontExit);
			pnlProgress.Controls.Add(btnExitNow);
			pnlProgress.Controls.Add(lblBundleScanInfo);
			pnlProgress.Controls.Add(lblProgressCurrentPhase);
			pnlProgress.Controls.Add(lblProgressCurrentAction);
			pnlProgress.Controls.Add(progressBar);
			pnlProgress.Dock = DockStyle.Fill;
			pnlProgress.Font = new Font("Segoe UI", 10.8F);
			pnlProgress.Location = new Point(0, 0);
			pnlProgress.Margin = new Padding(4, 6, 4, 6);
			pnlProgress.Name = "pnlProgress";
			pnlProgress.Padding = new Padding(12, 14, 12, 14);
			pnlProgress.Size = new Size(960, 1002);
			pnlProgress.TabIndex = 8;
			pnlProgress.Visible = false;
			// 
			// btnDontExit
			// 
			btnDontExit.Anchor = AnchorStyles.None;
			btnDontExit.BackColor = Color.LightSteelBlue;
			btnDontExit.FlatAppearance.BorderColor = SystemColors.ButtonShadow;
			btnDontExit.FlatStyle = FlatStyle.Flat;
			btnDontExit.Font = new Font("Segoe UI", 16F);
			btnDontExit.ForeColor = SystemColors.ControlText;
			btnDontExit.Location = new Point(268, 573);
			btnDontExit.Margin = new Padding(8);
			btnDontExit.Name = "btnDontExit";
			btnDontExit.Size = new Size(204, 80);
			btnDontExit.TabIndex = 3;
			btnDontExit.Text = "Don't Exit";
			btnDontExit.UseVisualStyleBackColor = false;
			btnDontExit.Visible = false;
			btnDontExit.Click += BtnDontExit_Click;
			// 
			// btnExitNow
			// 
			btnExitNow.Anchor = AnchorStyles.None;
			btnExitNow.BackColor = Color.DarkSeaGreen;
			btnExitNow.FlatAppearance.BorderColor = SystemColors.ControlDarkDark;
			btnExitNow.FlatStyle = FlatStyle.Flat;
			btnExitNow.Font = new Font("Segoe UI", 16F);
			btnExitNow.Location = new Point(488, 573);
			btnExitNow.Margin = new Padding(8);
			btnExitNow.Name = "btnExitNow";
			btnExitNow.Size = new Size(204, 80);
			btnExitNow.TabIndex = 4;
			btnExitNow.Text = "Exit Now";
			btnExitNow.UseVisualStyleBackColor = false;
			btnExitNow.Visible = false;
			btnExitNow.Click += BtnExitNow_Click;
			// 
			// lblBundleScanInfo
			// 
			lblBundleScanInfo.Anchor = AnchorStyles.Left | AnchorStyles.Right;
			lblBundleScanInfo.Font = new Font("Segoe UI", 10F);
			lblBundleScanInfo.Location = new Point(55, 570);
			lblBundleScanInfo.Margin = new Padding(16);
			lblBundleScanInfo.Name = "lblBundleScanInfo";
			lblBundleScanInfo.Size = new Size(850, 112);
			lblBundleScanInfo.TabIndex = 5;
			lblBundleScanInfo.Tag = "DimmedLabel";
			lblBundleScanInfo.Text = resources.GetString("lblBundleScanInfo.Text");
			lblBundleScanInfo.Visible = false;
			// 
			// lblProgressCurrentPhase
			// 
			lblProgressCurrentPhase.Anchor = AnchorStyles.Left | AnchorStyles.Right;
			lblProgressCurrentPhase.Font = new Font("Segoe UI", 16F);
			lblProgressCurrentPhase.Location = new Point(25, 287);
			lblProgressCurrentPhase.Margin = new Padding(16);
			lblProgressCurrentPhase.Name = "lblProgressCurrentPhase";
			lblProgressCurrentPhase.Size = new Size(910, 124);
			lblProgressCurrentPhase.TabIndex = 2;
			lblProgressCurrentPhase.Text = "Initializing";
			lblProgressCurrentPhase.TextAlign = ContentAlignment.BottomCenter;
			// 
			// lblProgressCurrentAction
			// 
			lblProgressCurrentAction.Anchor = AnchorStyles.Left | AnchorStyles.Right;
			lblProgressCurrentAction.Font = new Font("Segoe UI", 12F);
			lblProgressCurrentAction.Location = new Point(28, 487);
			lblProgressCurrentAction.Margin = new Padding(16);
			lblProgressCurrentAction.Name = "lblProgressCurrentAction";
			lblProgressCurrentAction.Size = new Size(905, 75);
			lblProgressCurrentAction.TabIndex = 1;
			lblProgressCurrentAction.Text = "...";
			lblProgressCurrentAction.TextAlign = ContentAlignment.TopCenter;
			// 
			// progressBar
			// 
			progressBar.Anchor = AnchorStyles.Left | AnchorStyles.Right;
			progressBar.Location = new Point(32, 431);
			progressBar.Margin = new Padding(20, 4, 20, 4);
			progressBar.Name = "progressBar";
			progressBar.Size = new Size(896, 36);
			progressBar.TabIndex = 0;
			// 
			// statusStrip
			// 
			statusStrip.Font = new Font("Segoe UI", 10F);
			statusStrip.ImageScalingSize = new Size(20, 20);
			statusStrip.Items.AddRange(new ToolStripItem[] { lblStatusLeft1, lblStatusLeft2, lblStatusLeft3, lblStatusSpring, lblStatusRight });
			statusStrip.Location = new Point(0, 978);
			statusStrip.Name = "statusStrip";
			statusStrip.Padding = new Padding(1, 0, 21, 0);
			statusStrip.Size = new Size(960, 24);
			statusStrip.SizingGrip = false;
			statusStrip.TabIndex = 3;
			statusStrip.Text = "statusStrip1";
			// 
			// lblStatusLeft1
			// 
			lblStatusLeft1.DisplayStyle = ToolStripItemDisplayStyle.Text;
			lblStatusLeft1.Font = new Font("Segoe UI", 10.2F);
			lblStatusLeft1.Name = "lblStatusLeft1";
			lblStatusLeft1.Padding = new Padding(0, 0, 5, 0);
			lblStatusLeft1.Size = new Size(72, 19);
			lblStatusLeft1.Text = "Loading...";
			// 
			// lblStatusLeft2
			// 
			lblStatusLeft2.BorderSides = ToolStripStatusLabelBorderSides.Left;
			lblStatusLeft2.DisplayStyle = ToolStripItemDisplayStyle.Text;
			lblStatusLeft2.Font = new Font("Segoe UI", 10.2F);
			lblStatusLeft2.Name = "lblStatusLeft2";
			lblStatusLeft2.Padding = new Padding(5, 0, 5, 0);
			lblStatusLeft2.Size = new Size(35, 23);
			lblStatusLeft2.Text = "   ";
			lblStatusLeft2.Visible = false;
			// 
			// lblStatusLeft3
			// 
			lblStatusLeft3.BorderSides = ToolStripStatusLabelBorderSides.Left;
			lblStatusLeft3.DisplayStyle = ToolStripItemDisplayStyle.Text;
			lblStatusLeft3.Font = new Font("Segoe UI", 10.2F);
			lblStatusLeft3.Name = "lblStatusLeft3";
			lblStatusLeft3.Padding = new Padding(5, 0, 5, 0);
			lblStatusLeft3.Size = new Size(35, 23);
			lblStatusLeft3.Text = "   ";
			lblStatusLeft3.Visible = false;
			// 
			// lblStatusSpring
			// 
			lblStatusSpring.DisplayStyle = ToolStripItemDisplayStyle.Text;
			lblStatusSpring.Font = new Font("Segoe UI", 10F);
			lblStatusSpring.Name = "lblStatusSpring";
			lblStatusSpring.Size = new Size(852, 19);
			lblStatusSpring.Spring = true;
			// 
			// lblStatusRight
			// 
			lblStatusRight.BorderSides = ToolStripStatusLabelBorderSides.Left;
			lblStatusRight.DisplayStyle = ToolStripItemDisplayStyle.Text;
			lblStatusRight.Name = "lblStatusRight";
			lblStatusRight.Padding = new Padding(10, 0, 0, 0);
			lblStatusRight.Size = new Size(14, 19);
			// 
			// btnCreateMerges
			// 
			btnCreateMerges.Anchor = AnchorStyles.Right;
			btnCreateMerges.AutoSize = true;
			btnCreateMerges.Font = new Font("Segoe UI", 11F);
			btnCreateMerges.Location = new Point(59, 908);
			btnCreateMerges.Margin = new Padding(0, 0, 5, 0);
			btnCreateMerges.Name = "btnCreateMerges";
			btnCreateMerges.Size = new Size(176, 50);
			btnCreateMerges.TabIndex = 6;
			btnCreateMerges.Text = "&Create Selected Merges";
			toolTip1.SetToolTip(btnCreateMerges, "Create Selected Merges (Keyboard: C)");
			btnCreateMerges.UseVisualStyleBackColor = false;
			btnCreateMerges.Click += BtnMergeFiles_Click;
			// 
			// tlpMain
			// 
			tlpMain.ColumnCount = 7;
			tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 4F));
			tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
			tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
			tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 8F));
			tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
			tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
			tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 4F));
			tlpMain.Controls.Add(btnDeleteAllMerges, 5, 2);
			tlpMain.Controls.Add(btnCreateAllScriptMerges, 2, 2);
			tlpMain.Controls.Add(btnCreateMerges, 1, 2);
			tlpMain.Controls.Add(btnDeleteMerges, 4, 2);
			tlpMain.Controls.Add(tlpConflictsHeader, 1, 0);
			tlpMain.Controls.Add(tlpMergesHeader, 4, 0);
			tlpMain.Dock = DockStyle.Fill;
			tlpMain.Location = new Point(0, 0);
			tlpMain.Margin = new Padding(0);
			tlpMain.Name = "tlpMain";
			tlpMain.RowCount = 3;
			tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 74F));
			tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 90F));
			tlpMain.Size = new Size(960, 978);
			tlpMain.TabIndex = 4;
			// 
			// tlpConflictsHeader
			// 
			tlpConflictsHeader.ColumnCount = 3;
			tlpMain.SetColumnSpan(tlpConflictsHeader, 2);
			tlpConflictsHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333321F));
			tlpConflictsHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333359F));
			tlpConflictsHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333359F));
			tlpConflictsHeader.Controls.Add(btnMainMenu, 0, 0);
			tlpConflictsHeader.Controls.Add(lblConflicts, 1, 0);
			tlpConflictsHeader.Controls.Add(btnRefreshConflicts, 2, 0);
			tlpConflictsHeader.Dock = DockStyle.Fill;
			tlpConflictsHeader.Location = new Point(4, 0);
			tlpConflictsHeader.Margin = new Padding(0);
			tlpConflictsHeader.Name = "tlpConflictsHeader";
			tlpConflictsHeader.RowCount = 1;
			tlpConflictsHeader.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			tlpConflictsHeader.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
			tlpConflictsHeader.Size = new Size(472, 74);
			tlpConflictsHeader.TabIndex = 18;
			// 
			// btnMainMenu
			// 
			btnMainMenu.Anchor = AnchorStyles.Left;
			btnMainMenu.AutoSize = true;
			btnMainMenu.Font = new Font("Segoe UI", 14F);
			btnMainMenu.Location = new Point(25, 16);
			btnMainMenu.Margin = new Padding(25, 0, 0, 0);
			btnMainMenu.MinimumSize = new Size(75, 35);
			btnMainMenu.Name = "btnMainMenu";
			btnMainMenu.Padding = new Padding(5, 0, 5, 0);
			btnMainMenu.Size = new Size(81, 42);
			btnMainMenu.TabIndex = 17;
			btnMainMenu.Text = "Menu";
			btnMainMenu.UseVisualStyleBackColor = false;
			btnMainMenu.Click += BtnMainMenu_Click;
			// 
			// tlpMergesHeader
			// 
			tlpMergesHeader.ColumnCount = 3;
			tlpMain.SetColumnSpan(tlpMergesHeader, 2);
			tlpMergesHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333321F));
			tlpMergesHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333359F));
			tlpMergesHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333359F));
			tlpMergesHeader.Controls.Add(lblMerges, 1, 0);
			tlpMergesHeader.Controls.Add(btnRefreshMerged, 2, 0);
			tlpMergesHeader.Dock = DockStyle.Fill;
			tlpMergesHeader.Location = new Point(484, 0);
			tlpMergesHeader.Margin = new Padding(0);
			tlpMergesHeader.Name = "tlpMergesHeader";
			tlpMergesHeader.RowCount = 1;
			tlpMergesHeader.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			tlpMergesHeader.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
			tlpMergesHeader.Size = new Size(472, 74);
			tlpMergesHeader.TabIndex = 19;
			// 
			// tableLayoutPanel2
			// 
			tableLayoutPanel2.ColumnCount = 3;
			tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333321F));
			tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333359F));
			tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333359F));
			tableLayoutPanel2.Dock = DockStyle.Fill;
			tableLayoutPanel2.Location = new Point(0, 0);
			tableLayoutPanel2.Name = "tableLayoutPanel2";
			tableLayoutPanel2.RowCount = 1;
			tableLayoutPanel2.Size = new Size(200, 100);
			tableLayoutPanel2.TabIndex = 0;
			// 
			// button1
			// 
			button1.Anchor = AnchorStyles.Left;
			button1.Font = new Font("Segoe UI", 10F);
			button1.Location = new Point(25, 32);
			button1.Margin = new Padding(25, 0, 0, 0);
			button1.MinimumSize = new Size(75, 35);
			button1.Name = "button1";
			button1.Size = new Size(75, 35);
			button1.TabIndex = 17;
			button1.Text = "Menu";
			button1.UseVisualStyleBackColor = false;
			// 
			// contextMenuStrip1
			// 
			contextMenuStrip1.ImageScalingSize = new Size(20, 20);
			contextMenuStrip1.Name = "contextMenuStrip1";
			contextMenuStrip1.Size = new Size(61, 4);
			// 
			// MainForm
			// 
			AutoScaleDimensions = new SizeF(8F, 19F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(960, 1002);
			Controls.Add(tlpMain);
			Controls.Add(statusStrip);
			Controls.Add(pnlProgress);
			Font = new Font("Segoe UI", 10.2F);
			Icon = (Icon)resources.GetObject("$this.Icon");
			KeyPreview = true;
			Margin = new Padding(4, 6, 4, 6);
			MinimumSize = new Size(970, 600);
			Name = "MainForm";
			StartPosition = FormStartPosition.CenterScreen;
			Text = "Script Merger: Fresh & Automated Edition";
			FormClosing += MainForm_FormClosing;
			Load += MainForm_Load;
			Shown += MainForm_Shown;
			KeyDown += MainForm_KeyDown;
			pnlProgress.ResumeLayout(false);
			statusStrip.ResumeLayout(false);
			statusStrip.PerformLayout();
			tlpMain.ResumeLayout(false);
			tlpMain.PerformLayout();
			tlpConflictsHeader.ResumeLayout(false);
			tlpConflictsHeader.PerformLayout();
			tlpMergesHeader.ResumeLayout(false);
			tlpMergesHeader.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion
		internal Button btnCreateAllScriptMerges;
		internal Button btnCreateMerges;
		internal Button btnDeleteAllMerges;
		internal Button btnDeleteMerges;
		internal Button btnDontExit;
		internal Button btnExitNow;
		internal Button btnMainMenu;
		internal Button btnRefreshConflicts;
		internal Button btnRefreshMerged;
		internal Button button1;
		internal Label lblConflicts;
		internal Label lblMerges;
		internal Label lblProgressCurrentAction;
		internal Label lblProgressCurrentPhase;
		internal Panel pnlProgress;
		internal ProgressBar progressBar;
		internal StatusStrip statusStrip;
		internal TableLayoutPanel tableLayoutPanel2;
		internal TableLayoutPanel tlpMergesHeader;
		internal TableLayoutPanel tlpConflictsHeader;
		internal TableLayoutPanel tlpMain;
		internal ToolStripStatusLabel lblStatusLeft1;
		internal ToolStripStatusLabel lblStatusLeft2;
		internal ToolStripStatusLabel lblStatusLeft3;
		internal ToolStripStatusLabel lblStatusRight;
		internal ToolStripStatusLabel lblStatusSpring;
		internal ToolTip toolTip1;
		internal ToolTip treConflictsToolTip;
		internal Label lblBundleScanInfo;
		internal ContextMenuStrip contextMenuStrip1;
	}
}

