using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;

using WitcherScriptMerger.Controls;
using WitcherScriptMerger.Events;
using WitcherScriptMerger.Inventory;
using WitcherScriptMerger.Theming;
using WitcherScriptMerger.UI;

using static WitcherScriptMerger.Program;

using Timer = System.Threading.Timer;


namespace WitcherScriptMerger.Forms;

internal partial class MainForm : BaseForm
{
	#region Members

	internal Timer _autoExitTimer;
	internal ContextMenuStrip MainMenu;
	internal TreeManager treeManager;
	internal ConflictTree treConflicts;
	internal MergeTree treMerges;
	internal PictureBox pbNoConflicts;
	internal UIThreadManager uiThreadManager;
	internal ProgressManager progressManager;
	internal InventoryMergeManager inventoryMergeManager;

	#endregion


	#region Dispose

	private bool _disposed;

	protected override void Dispose(bool disposing)
	{
		if (!_disposed)
		{
			if (disposing)
			{
				// Dispose managed resources
				_autoExitTimer?.Dispose();
				MainMenu?.Dispose();
				treConflicts?.Dispose();
				treMerges?.Dispose();
				pbNoConflicts?.Dispose();
			}

			// Dispose unmanaged resources (if any)

			_disposed = true;
		}

		base.Dispose(disposing);
	}

	#endregion


	#region Form Operations

	private void InitializeCustomComponent()
	{
		// Initialize the ConflictTree control
		treConflicts = new ConflictTree(ThemeManager.CurrentTheme)
		{
			Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
			BorderStyle = BorderStyle.None,
			CheckBoxes = true,
			Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point),
			ItemHeight = 30,
			Location = new Point(4, 74),
			Margin = new Padding(0),
			Name = "treConflicts",
			ShowLines = false,
			Size = new Size(470, 809),
			Sorted = true,
			TabIndex = 1,
			TabStop = false
		};
		tlpMain.SetColumnSpan(treConflicts, 2);
		tlpMain.Controls.Add(treConflicts, 1, 1);

		// Initialize the MergeTree control
		treMerges = new MergeTree(ThemeManager.CurrentTheme)
		{
			Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
			BorderStyle = BorderStyle.None,
			CheckBoxes = true,
			Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point),
			ItemHeight = 30,
			Location = new Point(482, 74),
			Margin = new Padding(0),
			Name = "treMerges",
			ShowLines = false,
			ShowNodeToolTips = true,
			Size = new Size(470, 809),
			Sorted = true,
			TabIndex = 1,
			TabStop = false
		};
		tlpMain.SetColumnSpan(treMerges, 2);
		tlpMain.Controls.Add(treMerges, 4, 1);

		// Initialize the NoConflicts PictureBox control
		pbNoConflicts = new();
	}

	internal MainForm()
	{
		InitializeComponent();
		InitializeCustomComponent();
	}

	/// <summary>
	/// Handles the Load event of the MainForm.
	/// </summary>
	private void MainForm_Load(object sender, EventArgs e)
	{
		Text += $" v{Application.ProductVersion}";
		pnlProgress.BringToFront();
		MainMenu = CreateMainMenu();
		LoadLastWindowConfiguration();
		ThemeMngr.ApplyThemeOnForm(this);

		uiThreadManager = new UIThreadManager(this);
		progressManager = new ProgressManager(this);
		treeManager = new TreeManager(treConflicts, treMerges, uiThreadManager, progressManager, treConflictsToolTip);
		treeManager.InvokeRequired += OnTreeManagerInvokeRequired;
		inventoryMergeManager = new InventoryMergeManager(uiThreadManager, progressManager, treeManager);

		// Assign delegates for tree manager actions
		treeManager.CreateAllScriptMergesDelegate = inventoryMergeManager.CreateAllScriptMerges;
		treeManager.RefreshMergeInventoryDelegate = inventoryMergeManager.RefreshMergeInventory;
		treeManager.DeleteMergesDelegate = inventoryMergeManager.DeleteMerges;

		// Attach tree mouse move event handlers
		treConflicts.MouseMove += treeManager.OnTreeViewMouseMove;
		treMerges.MouseMove += treeManager.OnTreeViewMouseMove;
	}

	private void OnTreeManagerInvokeRequired(object sender, InvokeRequiredEventArgs e) => BeginInvoke(e.Action);

	/// <summary>
	/// Handles the Shown event of the MainForm.
	/// </summary>
	private async void MainForm_Shown(object sender, EventArgs e)
	{
		Update();
		OptionsFrm.CheckSetGameDir();

		// Validate directories and dependencies
		if (!Paths.ValidateAllDirectories())
			AutoExit(null);
		if (!Paths.ValidateDependencyPaths())
			AutoExit(null, "(Some) dependencies are missing. Restart the application and set the right dependency paths. Alternatively, download the SM-FAE \"FULL\" package, which includes all dependencies, and install that.");

		// Check inventory order
		if (!inventoryMergeManager.GetIsInventoryInOrder())
			AutoExit(null);

		// Refresh merge inventory
		bool refreshRunning = await inventoryMergeManager.RefreshMergeInventory().ConfigureAwait(true);
		if (refreshRunning)
			// Don't continue (and don't call RefreshConflictsTree) if RefreshMergeInventory() is still running.
			// RefreshConflictsTree will be called through RefreshMergeInventory -> RefreshMergeTree ->
			// DeleteMergesDelegate -> DeleteMerges -> RefreshTrees if needed.
			return;

		// Refresh conflicts tree based on settings
		treeManager.RefreshConflictsTree(Settings.Get<int>("CheckBundles") == 0);
	}

	/// <summary>
	/// Handles the FormClosing event of the MainForm.
	/// </summary>
	private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
	{
		// Prevent closing if progress is visible
		if (pnlProgress.Visible)
		{
			e.Cancel = true;
			return;
		}

		// Save game directory and window state settings
		Settings.Set("GameDirectory", Paths.GameDirectory);

		if (WindowState == FormWindowState.Maximized)
		{
			Settings.Set("StartMaximized", true);
		}
		else
		{
			Settings.Set("StartMaximized", false);
			Settings.Set("StartWidth", Width);
			Settings.Set("StartHeight", Height);
			Settings.Set("StartPosTop", Top);
			Settings.Set("StartPosLeft", Left);
		}

		Settings.Save();

		StopListening();
	}

	/// <summary>
	/// Loads the last window configuration from settings.
	/// </summary>
	private void LoadLastWindowConfiguration()
	{
		// Load window position
		int top = Settings.Get<int>("StartPosTop");
		int left = Settings.Get<int>("StartPosLeft");
		if (top > 0)
			Top = top;
		if (left > 0)
			Left = left;
		if (Top > 0 || Left > 0)
			StartPosition = FormStartPosition.Manual;

		// Load window size
		int startWidth = Settings.Get<int>("StartWidth");
		int startHeight = Settings.Get<int>("StartHeight");
		if (startWidth > 0)
			Width = startWidth;
		if (startHeight > 0)
			Height = startHeight;

		// Load window state
		if (Settings.Get<bool>("StartMaximized"))
			WindowState = FormWindowState.Maximized;
	}

	#endregion


	#region Key Input

	/// <summary>
	/// Handles the KeyDown event of the MainForm.
	/// </summary>
	private void MainForm_KeyDown(object sender, KeyEventArgs e)
	{
		// Trigger button clicks based on key presses
		if (e.KeyCode == Keys.F5) BtnRefreshConflicts_Click(null, null);
		if (e.KeyCode == Keys.F6) BtnRefreshMerged_Click(null, null);
		if (e.KeyCode == Keys.C) BtnMergeFiles_Click(null, null);
		if (e.KeyCode == Keys.A) BtnCreateAllMerges_Click(null, null);
		if (e.KeyCode == Keys.D) BtnDeleteMerges_Click(null, null);
		if (e.KeyCode == Keys.E) BtnDeleteAllMerges_Click(null, null);
		if (e.KeyCode == Keys.Escape) Application.Exit();
	}

	#endregion


	#region File/Dir Operations

	/// <summary>
	/// Generates a CSV file containing conflict information from the specified TreeView.
	/// </summary>
	internal static void GenerateConflictsCsv(TreeView treeView)
	{
		// Configure save file dialog
		using SaveFileDialog saveFileDialog = new()
		{
			FileName = "Conflicts.csv",
			InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
			Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
			DefaultExt = "csv",
			AddExtension = true
		};

		if (saveFileDialog.ShowDialog() == DialogResult.OK)
		{
			string filePath = saveFileDialog.FileName;

			StringBuilder csvContent = new();
			string sep = CultureInfo.CurrentCulture.TextInfo.ListSeparator;
			_ = csvContent.AppendLine(CultureInfo.InvariantCulture, $"\"Category\"{sep}\"FilePath\"{sep}\"ModName\"{sep}\"ModPrio\"");

			// Iterate through tree nodes to build CSV content
			foreach (TreeNode rootNode in treeView.Nodes)
				foreach (TreeNode scriptNode in rootNode.Nodes)
					foreach (TreeNode modNode in scriptNode.Nodes)
					{
						string line = $"\"{rootNode.Text}\"{sep}\"{scriptNode.Text}\"{sep}\"{modNode.Text}\"{sep}\"{Program.LoadOrder.GetPriorityByName(modNode.Text)}\"";
						_ = csvContent.AppendLine(line);
					}

			File.WriteAllText(filePath, csvContent.ToString());
		}
	}

	#endregion


	#region Menus

	internal ContextMenuStrip CreateMainMenu()
	{
		ContextMenuStrip mainMenu = new()
		{
			ShowImageMargin = false,
			Renderer = new MenuRenderer(ThemeManager.CurrentTheme, 12),
		};
		mainMenu.Opening += MainMenu_Opening;
		mainMenu.Closing += MainMenu_Closing;

		ContextMenuStrip openMenu = new()
		{
			ShowImageMargin = false,
			Renderer = new MenuRenderer(ThemeManager.CurrentTheme, 12)
		};

		// Open
		ToolStripMenuItem mnOpen = new("Open...") { Name = "mnOpen" };
		mnOpen.DropDownOpening += MnOpen_DropDownOpening;
		_ = mainMenu.Items.Add(mnOpen);


		// Open Submenu
		ToolStripMenuItem mnOpenLoadOrderFile = new("Open Load Order File Directory") { Name = "mnOpenLoadOrderFile" };
		mnOpenLoadOrderFile.Click += MnOpenLoadOrderFile_Click;
		_ = openMenu.Items.Add(mnOpenLoadOrderFile);

		ToolStripMenuItem mnOpenMergedModDir = new("Open Merged Mod Directory") { Name = "mnOpenMergedModDir" };
		mnOpenMergedModDir.Click += MnOpenMergedModDir_Click;
		_ = openMenu.Items.Add(mnOpenMergedModDir);

		ToolStripMenuItem mnOpenBundleContentDir = new("Open Merged Bundle Content Directory") { Name = "mnOpenBundleContentDir" };
		mnOpenBundleContentDir.Click += MnOpenBundleContentDir_Click;
		_ = openMenu.Items.Add(mnOpenBundleContentDir);

		mnOpen.DropDown = openMenu;


		// Options
		ToolStripMenuItem mnOptions = new("Options...") { Name = "mnOptions" };
		mnOptions.Click += MnOptions_Click;
		_ = mainMenu.Items.Add(mnOptions);

		// Dependency
		ToolStripMenuItem mnDependencies = new("Dependency Locations...") { Name = "mnDependencies" };
		mnDependencies.Click += MnDependencies_Click;
		_ = mainMenu.Items.Add(mnDependencies);

		// Repack
		ToolStripMenuItem mnExportConflicts = new("Export conflicts to CSV file") { Name = "mnExportConflicts" };
		mnExportConflicts.Click += MnExportConflicts_Click;
		_ = mainMenu.Items.Add(mnExportConflicts);

		// Repack
		ToolStripMenuItem mnRepackBundle = new("Repack Merged Bundle") { Name = "mnRepackBundle" };
		mnRepackBundle.Click += MnRepackBundle_Click;
		_ = mainMenu.Items.Add(mnRepackBundle);

		// Keyboard
		ToolStripMenuItem mnKeyboardShortcuts = new("Keyboard Shortcuts...") { Name = "mnKeyboardShortcuts" };
		mnKeyboardShortcuts.Click += MnKeyboardShortcuts_Click;
		_ = mainMenu.Items.Add(mnKeyboardShortcuts);

		ToolStripSeparator sep = new();
		_ = mainMenu.Items.Add(sep);

		// Exit DX11
		ToolStripMenuItem mnExitAndPlayDX11 = new("Exit and launch game (DX11)") { Name = "mnExitAndPlayDX11" };
		mnExitAndPlayDX11.Click += MnExitAndPlayDX11_Click;
		_ = mainMenu.Items.Add(mnExitAndPlayDX11);

		// Exit DX12
		ToolStripMenuItem mnExitAndPlayDX12 = new("Exit and launch game (DX12)") { Name = "mnExitAndPlayDX12" };
		mnExitAndPlayDX12.Click += MnExitAndPlayDX12_Click;
		_ = mainMenu.Items.Add(mnExitAndPlayDX12);

		// Exit
		ToolStripMenuItem mnExit = new("Exit") { Name = "mnExit" };
		mnExit.Click += MnExit_Click;
		_ = mainMenu.Items.Add(mnExit);

		return mainMenu;
	}

	private void MnExportConflicts_Click(object sender, EventArgs e) => GenerateConflictsCsv(treConflicts);

	private void MainMenu_Closing(object sender, ToolStripDropDownClosingEventArgs e)
	{
		Point mousePosition = MousePosition;
		if (btnMainMenu.Bounds.Contains(btnMainMenu.PointToClient(mousePosition)) &&
			e.CloseReason != ToolStripDropDownCloseReason.CloseCalled)
		{
			e.Cancel = true;
		}
	}

	private void BtnMainMenu_Click(object sender, EventArgs e)
	{
		Button btn = (Button)sender;
		if (MainMenu.Visible)
			MainMenu.Close(ToolStripDropDownCloseReason.CloseCalled);
		else
			MainMenu.Show(btn, new Point(0, btn.Height + 2));
	}

	private void MainMenu_Opening(object sender, CancelEventArgs e) => MainMenu.Items["mnRepackBundle"].Enabled = Directory.Exists(Paths.MergedBundleContent);

	private void MnOpen_DropDownOpening(object sender, EventArgs e)
	{
		ToolStripMenuItem mnOpen = (ToolStripMenuItem)MainMenu.Items["mnOpen"];

		ToolStripMenuItem mnOpenLoadOrderFile = (ToolStripMenuItem)mnOpen.DropDownItems["mnOpenLoadOrderFile"];
		mnOpenLoadOrderFile.Enabled = File.Exists(Program.LoadOrder?.FilePath ?? "");

		ToolStripMenuItem mnOpenMergedModDir = (ToolStripMenuItem)mnOpen.DropDownItems["mnOpenMergedModDir"];
		mnOpenMergedModDir.Enabled = Directory.Exists(Paths.RetrieveMergedModDir());

		ToolStripMenuItem mnOpenBundleContentDir = (ToolStripMenuItem)mnOpen.DropDownItems["mnOpenBundleContentDir"];
		mnOpenBundleContentDir.Enabled = Directory.Exists(Paths.MergedBundleContent);
	}

	private void MnOpenLoadOrderFile_Click(object sender, EventArgs e) => TryOpenFile(Program.LoadOrder.FilePath);

	private void MnOpenMergedModDir_Click(object sender, EventArgs e)
	{
		if (!Directory.Exists(Paths.RetrieveMergedModDir()))
		{
			_ = MessageBox.Show($"The Merged Mods directory ({Paths.RetrieveMergedModDir()}) does not exist. Did you create any merges yet?");
			return;
		}

		_ = TryOpenDirectory(Paths.RetrieveMergedModDir());
	}

	private void MnOpenBundleContentDir_Click(object sender, EventArgs e) => TryOpenDirectory(Paths.MergedBundleContent);

	private void MnOptions_Click(object sender, EventArgs e) => uiThreadManager.ShowModal(OptionsFrm);

	private void MnDependencies_Click(object sender, EventArgs e)
	{
		using DependencyForm dependencyForm = new();
		_ = uiThreadManager.ShowModal(dependencyForm);
	}

	private void MnRepackBundle_Click(object sender, EventArgs e) => inventoryMergeManager.RepackBundle();

	private void MnKeyboardShortcuts_Click(object sender, EventArgs e)
	{
		string caption = $"{AppName} Keyboard Shortcuts";
		string msg =
			$"These keyboard shortcuts are enabled in the main window:\n\n" +
			$"F5: Refresh Conflicts Panel\n\n" +
			$"F6: Refresh Merges Panel\n\n" +
			$"C: Create Selected Merges\n\n" +
			$"A: Create ALL Script (only) Merges\n\n" +
			$"D: Delete Selected Merges\n\n" +
			$"E: Delete ALL Merges\n\n" +
			$"Esc: Exit Application";
		_ = uiThreadManager.ShowMessage(msg, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
	}

	private void MnExitAndPlayDX11_Click(object sender, EventArgs e)
	{
		if (TryOpenFile(Paths.GameExeDX11))
			Environment.Exit(0);
		else
			_ = MessageBox.Show($"DX11 game executable not found at expected location:\n\n{Paths.GameExeDX11}", "Game executable not found", MessageBoxButtons.OK);
	}

	private void MnExitAndPlayDX12_Click(object sender, EventArgs e)
	{
		if (TryOpenFile(Paths.GameExeDX12))
			Environment.Exit(0);
		else
			_ = MessageBox.Show($"DX12 game executable not found at expected location:\n\n{Paths.GameExeDX12}", "Game executable not found", MessageBoxButtons.OK);
	}

	private void MnExit_Click(object sender, EventArgs e) => AutoExit();

	#endregion


	#region Button Clicks

	private async void BtnRefreshMerged_Click(object sender, EventArgs e) =>
		await inventoryMergeManager.ManualMergesRefresh().ConfigureAwait(true);

	private void BtnRefreshConflicts_Click(object sender, EventArgs e) => treeManager.ManualConflictsRefresh();

	private void BtnMergeFiles_Click(object sender, EventArgs e) => inventoryMergeManager.MergeFiles();

	private void BtnDeleteMerges_Click(object sender, EventArgs e) => inventoryMergeManager.ManualDeleteMerges();

	private void BtnDeleteAllMerges_Click(object sender, EventArgs e) => inventoryMergeManager.ManualDeleteAllMerges();

	private void BtnCreateAllMerges_Click(object sender, EventArgs e) => inventoryMergeManager.CreateAllScriptMerges(true);

	private void BtnDontExit_Click(object sender, EventArgs e) => progressManager.DontExit();

	private void BtnExitNow_Click(object sender, EventArgs e) => AutoExit();

	#endregion
}