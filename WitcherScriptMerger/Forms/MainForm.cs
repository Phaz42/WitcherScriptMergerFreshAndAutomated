using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using WitcherScriptMerger.Controls;
using WitcherScriptMerger.Events;
using WitcherScriptMerger.FileIndex;
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

	// The search above each tree (TreeManager applies it), and a short pause after typing before it searches.
	internal TextBox txtConflictsSearch, txtMergesSearch;

	// The status bar's link to the mod check's report (ModHealth); shown when it found problems.
	internal ToolStripStatusLabel lblStatusHealth;

	// The same, made hard to miss: the bar above the lists (UpdateProblemsBar), in a row of the main layout of its own.
	internal ProblemsBar problemsBar;
	internal const int ProblemsBarRow = 1;
	internal const int ListsRow = 2;
	internal Label lblConflictsSearchCount, lblMergesSearchCount;

	// The conflict list's filter (FilterPanel), left of its search box: green while it hides any mod.
	internal FilterButton btnConflictsFilter;
	private ToolTip _searchToolTip;
	private System.Windows.Forms.Timer _conflictsSearchDelay, _mergesSearchDelay;

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
				_searchToolTip?.Dispose();
				lblStatusHealth?.Dispose();
				problemsBar?.Dispose();
				btnConflictsFilter?.Dispose();
				txtConflictsSearch?.Dispose();
				txtMergesSearch?.Dispose();
				lblConflictsSearchCount?.Dispose();
				lblMergesSearchCount?.Dispose();
				_conflictsSearchDelay?.Dispose();
				_mergesSearchDelay?.Dispose();
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
		InitializeProblemsBar();

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
			ShowNodeToolTips = false,  // SM-FAE shows the rows' tooltips (ListToolTips)
			Size = new Size(470, 809),
			Sorted = true,
			TabIndex = 1,
			TabStop = false
		};
		tlpMain.SetColumnSpan(treConflicts, 2);
		tlpMain.Controls.Add(treConflicts, 1, ListsRow);

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
			ShowNodeToolTips = false,  // SM-FAE shows the rows' tooltips (ListToolTips)
			Size = new Size(470, 809),
			Sorted = true,
			TabIndex = 1,
			TabStop = false
		};
		tlpMain.SetColumnSpan(treMerges, 2);
		tlpMain.Controls.Add(treMerges, 4, ListsRow);

		// Initialize the NoConflicts PictureBox control
		pbNoConflicts = new();

		InitializeSearch();

		lblStatusHealth = new ToolStripStatusLabel
		{
			BorderSides = ToolStripStatusLabelBorderSides.Left,
			DisplayStyle = ToolStripItemDisplayStyle.Text,
			Font = new Font("Segoe UI", 10.2F),
			IsLink = true,
			LinkBehavior = LinkBehavior.HoverUnderline,
			Padding = new Padding(5, 0, 5, 0),
			ToolTipText = "Show the mods with problems",
			Visible = false
		};
		lblStatusHealth.Click += (sender, e) => ShowModHealthReport();
		statusStrip.Items.Insert(statusStrip.Items.IndexOf(lblStatusSpring), lblStatusHealth);
	}

	/// <summary>
	/// Adds the row for the problems bar between the headers and the lists, moving the rows below it down; while the bar is
	/// hidden, the row takes no room.
	/// </summary>
	private void InitializeProblemsBar()
	{
		foreach (Control control in tlpMain.Controls)
		{
			int row = tlpMain.GetRow(control);
			if (row >= ProblemsBarRow)
				tlpMain.SetRow(control, row + 1);
		}

		tlpMain.RowCount++;
		tlpMain.RowStyles.Insert(ProblemsBarRow, new RowStyle(SizeType.AutoSize));

		problemsBar = new ProblemsBar
		{
			Anchor = AnchorStyles.Left | AnchorStyles.Right,
			Margin = new Padding(0),  // the sides and below: PlaceProblemsBar
			Name = "problemsBar",
			Visible = false
		};
		problemsBar.ShowDetailsClicked += (sender, e) => ShowModHealthReport();
		problemsBar.DismissClicked += (sender, e) => DismissProblemsBar();
		tlpMain.Controls.Add(problemsBar, 1, ProblemsBarRow);
		tlpMain.SetColumnSpan(problemsBar, 5);
	}

	// The problems the bar has been hidden for, saved: "installed:modName" or "older:modName", separated by |, which no
	// folder name can hold.
	private const string DismissedProblemsKey = "DismissedModProblems";
	private HashSet<string> _problemsShown = [];

	// A problem is a mod and its kind: the same mod installed incorrectly later is a new problem.
	private static HashSet<string> ProblemKeys(ModHealth health) =>
		health.InstallProblems.Select(problem => "installed:" + problem.ModName)
			.Concat(health.OutdatedScripts.Select(script => "older:" + script.ModName))
			.ToHashSet(StringComparer.OrdinalIgnoreCase);

	/// <summary>
	/// After a scan: the bar shows while the mods have a problem it was not hidden for - with all of them, the hidden ones
	/// included - and stays hidden while every problem is one it was hidden for. With none left, what it was hidden for is
	/// forgotten, so a problem that comes back shows it again. Null when no scan has run yet: nothing changes.
	/// </summary>
	internal void UpdateProblemsBar(ModHealth health)
	{
		if (health == null)
			return;

		if (!health.HasProblems)
		{
			problemsBar.Visible = false;
			_problemsShown = [];
			if (!string.IsNullOrEmpty(Settings.Get(DismissedProblemsKey)))
			{
				Settings.Set(DismissedProblemsKey, string.Empty);
				Settings.Save();
			}

			return;
		}

		HashSet<string> dismissed = (Settings.Get(DismissedProblemsKey) ?? string.Empty)
			.Split('|', StringSplitOptions.RemoveEmptyEntries)
			.ToHashSet(StringComparer.OrdinalIgnoreCase);
		_problemsShown = ProblemKeys(health);
		if (_problemsShown.IsSubsetOf(dismissed))
		{
			problemsBar.Visible = false;
			return;
		}

		problemsBar.SetMessage(ProblemsMessage(health));
		PlaceProblemsBar();
		problemsBar.Visible = true;
	}

	// The bar's white space at each side is this share of the search boxes' outer white space there: the room the search
	// rows keep beside the boxes (their margins, CreateSearchRow) so that a scrollbar on one list only does not leave the
	// view unbalanced. Chosen by eye, and tuned the same way.
	private const double ProblemsBarSideShare = 0.66;

	// Measured from where the search boxes are. Below the bar, the white space there is above it, between the boxes and
	// the end of the headers: so the lists keep the distance from the boxes they have without the bar. At the sides,
	// between the window's edge and each outer box - while neither shows a search's count, which narrows its box;
	// otherwise the last measurement of the sides stands.
	private void PlaceProblemsBar()
	{
		Control left = btnConflictsFilter, right = txtMergesSearch.Parent;  // the search rows' outer ends: the filter, a SearchBox frame
		int boxesBottom = tlpMain.PointToClient(left.PointToScreen(new Point(0, left.Height))).Y;
		int below = Math.Max(0, tlpConflictsHeader.Bottom + tlpConflictsHeader.Margin.Bottom - boxesBottom);
		int sideLeft = problemsBar.Margin.Left, sideRight = problemsBar.Margin.Right;

		if (lblConflictsSearchCount.Text.Length == 0 && lblMergesSearchCount.Text.Length == 0)
		{
			int leftSpace = tlpMain.PointToClient(left.PointToScreen(Point.Empty)).X;
			int rightSpace = tlpMain.ClientSize.Width - tlpMain.PointToClient(right.PointToScreen(new Point(right.Width, 0))).X;
			int[] columns = tlpMain.GetColumnWidths();  // the bar's cell starts after the first column and ends before the last
			sideLeft = Math.Max(0, (int)Math.Round(ProblemsBarSideShare * leftSpace) - columns[0]);
			sideRight = Math.Max(0, (int)Math.Round(ProblemsBarSideShare * rightSpace) - columns[^1]);
		}

		Padding margin = new(sideLeft, 0, sideRight, below);
		if (problemsBar.Margin != margin)
			problemsBar.Margin = margin;
	}

	// The × hides the bar for the problems it shows now; the next scan that finds another brings it back.
	private void DismissProblemsBar()
	{
		problemsBar.Visible = false;
		Settings.Set(DismissedProblemsKey, string.Join('|', _problemsShown));
		Settings.Save();
	}

	// "2 mods are installed incorrectly, so the game doesn't load them." - or, with both kinds, how many mods have
	// problems and how many of each kind; a mod can have both.
	private static string ProblemsMessage(ModHealth health)
	{
		int installed = health.InstallProblems.Select(problem => problem.ModName).Distinct(StringComparer.OrdinalIgnoreCase).Count();
		int older = health.OutdatedScripts.Select(script => script.ModName).Distinct(StringComparer.OrdinalIgnoreCase).Count();
		string notLoaded = $"installed incorrectly, so the game doesn't load {(installed == 1 ? "it" : "them")}";
		const string olderVersion = "made for an older game version";

		if (older == 0)
			return $"{installed} mod{installed.GetPluralS()} {(installed == 1 ? "is" : "are")} {notLoaded}.";
		if (installed == 0)
			return $"{older} mod{older.GetPluralS()} {(older == 1 ? "was" : "were")} {olderVersion}.";

		int mods = health.ModCount;
		return $"{mods} mod{mods.GetPluralS()} {(mods == 1 ? "has" : "have")} problems: {installed} {notLoaded}; {older} {olderVersion}.";
	}

	private void ShowModHealthReport()
	{
		if (treeManager?.ModIndex?.Health is not { HasProblems: true } health)
			return;

		using ModHealthForm form = new(health);
		ThemeMngr.ApplyThemeOnForm(form);
		_ = form.ShowDialog(this);
	}

	/// <summary>
	/// Adds a search box above each tree, as a second row in its header: the header row of the main layout grows by that
	/// row's height. Row heights here are at 125% scaling, like the designer's; the theme scales them (ApplyThemeToControls).
	/// </summary>
	private void InitializeSearch()
	{
		const float searchRowHeight = 50F;
		tlpMain.RowStyles[0].Height += searchRowHeight;

		_searchToolTip = new ToolTip { AutoPopDelay = 30000, InitialDelay = 600, ReshowDelay = 200 };

		btnConflictsFilter = new FilterButton();
		_searchToolTip.SetToolTip(btnConflictsFilter, FilterToolTip(0, 0));
		txtConflictsSearch = CreateSearchRow(tlpConflictsHeader, searchRowHeight, () => treConflicts, out lblConflictsSearchCount, btnConflictsFilter);
		txtMergesSearch = CreateSearchRow(tlpMergesHeader, searchRowHeight, () => treMerges, out lblMergesSearchCount);
		_conflictsSearchDelay = CreateSearchDelay(txtConflictsSearch, () => treConflicts);
		_mergesSearchDelay = CreateSearchDelay(txtMergesSearch, () => treMerges);
	}

	private TextBox CreateSearchRow(TableLayoutPanel header, float height, Func<SMTree> tree, out Label count, FilterButton filter = null)
	{
		header.RowCount = 2;
		header.RowStyles[0] = new RowStyle(SizeType.Percent, 100F);
		header.RowStyles[1] = new RowStyle(SizeType.Absolute, height);

		// SearchBox draws the rounded frame and places the text box inside it.
		TextBox box = new()
		{
			Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point),
			Margin = new Padding(0),
			PlaceholderText = "Search mods and files  (Ctrl+F)",
			Tag = "SearchBox",
			TabStop = false
		};
		// How the search works is the "?"'s help topic, in full; the box itself has no tooltip.
		SearchBox frame = new(box) { Dock = DockStyle.Fill, Margin = new Padding(0) };
		_searchToolTip.SetToolTip(frame.Help, "How searching works");
		frame.HelpClicked += (sender, e) => HelpPanel.ShowTopic(this, HelpTopics.Search);
		// The cross does what Esc twice does: clears the search, at once rather than after the typing delay, and returns to the list.
		frame.Cleared += (sender, e) =>
		{
			treeManager?.SetSearchText(tree(), string.Empty);
			_ = tree().Focus();
		};

		count = new Label
		{
			Anchor = AnchorStyles.Right,
			AutoSize = true,
			Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point),
			Margin = new Padding(8, 0, 0, 0),
			Tag = "DimmedLabel"
		};

		TableLayoutPanel row = new()
		{
			ColumnCount = filter == null ? 2 : 3,
			Dock = DockStyle.Fill,
			// Wider than the Menu button's 25, so the box stays clear of a scrollbar on either side.
			Margin = new Padding(63, 0, 63, 6),
			RowCount = 1
		};
		// The filter takes the box's left end: the row keeps its width.
		if (filter != null)
		{
			filter.Anchor = AnchorStyles.None;
			filter.Margin = new Padding(0, 0, LogicalToDeviceUnits(6), 0);
			_ = row.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
			row.Controls.Add(filter, 0, 0);
		}

		_ = row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
		_ = row.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
		_ = row.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
		row.Controls.Add(frame, row.ColumnCount - 2, 0);
		row.Controls.Add(count, row.ColumnCount - 1, 0);

		header.Controls.Add(row, 0, 1);
		header.SetColumnSpan(row, header.ColumnCount);
		return box;
	}

	// Green while the filter hides any mod in the conflict list, and its tooltip says how much.
	private void UpdateFilterButton()
	{
		(int mods, int conflicts) = treeManager.FilterHides();
		btnConflictsFilter.IsActive = mods > 0;
		_searchToolTip.SetToolTip(btnConflictsFilter, FilterToolTip(mods, conflicts));
	}

	private static string FilterToolTip(int mods, int conflicts) => mods == 0
		? "Filter the conflict list: hide mods you don't want to see there, without changing your load order or your merges"
		: $"Filtering: {mods} {(mods == 1 ? "mod" : "mods")} hidden, {conflicts} {(conflicts == 1 ? "conflict" : "conflicts")} not shown. Click to change.";

	// Searching on every keystroke would rebuild the tree for each letter of a word.
	private System.Windows.Forms.Timer CreateSearchDelay(TextBox box, Func<SMTree> tree)
	{
		System.Windows.Forms.Timer delay = new() { Interval = 250 };
		delay.Tick += (sender, e) =>
		{
			delay.Stop();
			treeManager?.SetSearchText(tree(), box.Text);
		};
		box.TextChanged += (sender, e) =>
		{
			delay.Stop();
			delay.Start();
		};
		return delay;
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
		treeManager.SetSearchControls(txtConflictsSearch, lblConflictsSearchCount, txtMergesSearch, lblMergesSearchCount);
		btnConflictsFilter.Click += (sender, e) => FilterPanel.ShowFilter(this, treeManager);
		treeManager.FilterApplied += (sender, e) => UpdateFilterButton();
		inventoryMergeManager = new InventoryMergeManager(uiThreadManager, progressManager, treeManager);

		// Assign delegates for tree manager actions
		treeManager.CreateAllScriptMergesDelegate = inventoryMergeManager.CreateAllScriptMerges;
		treeManager.RefreshMergeInventoryDelegate = inventoryMergeManager.RefreshMergeInventory;
		treeManager.DeleteMergesDelegate = inventoryMergeManager.DeleteMerges;

		// Attach tree mouse move event handlers
		treConflicts.MouseMove += treeManager.OnTreeViewMouseMove;
		treMerges.MouseMove += treeManager.OnTreeViewMouseMove;
		treConflicts.MouseLeave += treeManager.OnTreeViewMouseLeave;
		treMerges.MouseLeave += treeManager.OnTreeViewMouseLeave;
	}

	private void OnTreeManagerInvokeRequired(object sender, InvokeRequiredEventArgs e) => BeginInvoke(e.Action);

	/// <summary>
	/// Handles the Shown event of the MainForm.
	/// </summary>
	private async void MainForm_Shown(object sender, EventArgs e)
	{
		Update();
		OptionsFrm.CheckSetGameDir();

		// Validate directories and dependencies. A game folder that fails - moved since, or picked wrongly once - is
		// asked for again here: the Options window, where the message points, cannot be reached once the program exits.
		if (!Paths.ValidateAllDirectories())
		{
			_ = uiThreadManager.ShowMessage("In the following window, please select your main Witcher 3 game folder.",
				"Select the Witcher 3 Game Folder", MessageBoxButtons.OK, MessageBoxIcon.Information);
			OptionsFrm.SelectGameDirectory();
			if (!Paths.ValidateAllDirectories())
				AutoExit(null);
		}
		if (!Paths.ValidateDependencyPaths())
			AutoExit(null, "Some dependencies are missing. Restart SM-FAE and set their paths in Dependency Locations. Or download the SM-FAE Full package, which includes all dependencies, and install that.");

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
		treeManager.RefreshConflictsTree(Settings.ScanBundlesOnEveryScan);
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
		// In a search box, keys are text: never the one-key shortcuts below, which would act on every letter typed - E
		// alone deletes all merges. Esc clears the search instead of closing SM-FAE; on an empty search it returns to the
		// list below. Enter searches at once.
		TextBox searchBox = txtConflictsSearch.Focused ? txtConflictsSearch : txtMergesSearch.Focused ? txtMergesSearch : null;
		if (searchBox != null)
		{
			if (e.KeyCode == Keys.Escape)
			{
				if (searchBox.TextLength > 0)
					searchBox.Clear();
				else
					_ = (searchBox == txtConflictsSearch ? (Control)treConflicts : treMerges).Focus();
				e.Handled = e.SuppressKeyPress = true;
			}
			else if (e.KeyCode == Keys.Enter)
			{
				treeManager.SetSearchText(searchBox == txtConflictsSearch ? treConflicts : treMerges, searchBox.Text);
				e.Handled = e.SuppressKeyPress = true;
			}

			if (e.KeyCode is not (Keys.F5 or Keys.F6))
				return;
		}

		// Ctrl+F searches the list in focus: the merges when that tree has it, otherwise the conflicts.
		if (e.Control && e.KeyCode == Keys.F)
		{
			TextBox box = treMerges.Focused ? txtMergesSearch : txtConflictsSearch;
			_ = box.Focus();
			box.SelectAll();
			e.Handled = e.SuppressKeyPress = true;
			return;
		}

		// The shortcuts are the keys alone: with Ctrl, Alt or Shift a key is the focused control's - the lists' Ctrl+A and
		// Ctrl+D select and deselect all.
		if (e.Modifiers != Keys.None)
			return;

		// Trigger button clicks based on key presses. A key used here goes no further: the list in focus would take it as
		// typing - a jump to an item starting with that letter - and beep when there is none, as it always does for Esc.
		if (e.KeyCode is Keys.F5 or Keys.F6 or Keys.C or Keys.A or Keys.D or Keys.E or Keys.Escape)
			e.Handled = e.SuppressKeyPress = true;

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
		ToolStripMenuItem mnOpenLoadOrderFile = new("Open Load Order File Folder") { Name = "mnOpenLoadOrderFile" };
		mnOpenLoadOrderFile.Click += MnOpenLoadOrderFile_Click;
		_ = openMenu.Items.Add(mnOpenLoadOrderFile);

		ToolStripMenuItem mnOpenMergedModDir = new("Open Merged Mod Folder") { Name = "mnOpenMergedModDir" };
		mnOpenMergedModDir.Click += MnOpenMergedModDir_Click;
		_ = openMenu.Items.Add(mnOpenMergedModDir);

		ToolStripMenuItem mnOpenBundleContentDir = new("Open Merged Bundle Content Folder") { Name = "mnOpenBundleContentDir" };
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
		ToolStripMenuItem mnExportConflicts = new("Export Conflicts to CSV File") { Name = "mnExportConflicts" };
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

		// KDiff3 guide
		ToolStripMenuItem mnKDiff3Guide = new("KDiff3 Guide...") { Name = "mnKDiff3Guide" };
		mnKDiff3Guide.Click += (sender, e) => KDiff3Guide.ShowFromMenu();
		_ = mainMenu.Items.Add(mnKDiff3Guide);

		ToolStripSeparator sep = new();
		_ = mainMenu.Items.Add(sep);

		// Exit DX11
		ToolStripMenuItem mnExitAndPlayDX11 = new("Exit and Launch Game (DX11)") { Name = "mnExitAndPlayDX11" };
		mnExitAndPlayDX11.Click += MnExitAndPlayDX11_Click;
		_ = mainMenu.Items.Add(mnExitAndPlayDX11);

		// Exit DX12
		ToolStripMenuItem mnExitAndPlayDX12 = new("Exit and Launch Game (DX12)") { Name = "mnExitAndPlayDX12" };
		mnExitAndPlayDX12.Click += MnExitAndPlayDX12_Click;
		_ = mainMenu.Items.Add(mnExitAndPlayDX12);

		// Exit
		ToolStripMenuItem mnExit = new("Exit") { Name = "mnExit" };
		mnExit.Click += MnExit_Click;
		_ = mainMenu.Items.Add(mnExit);

		return mainMenu;
	}

	private void MnExportConflicts_Click(object sender, EventArgs e)
	{
		// Every conflict, also those a search hides.
		using (treeManager.SearchSuspended())
			GenerateConflictsCsv(treConflicts);
	}

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

	private void MainMenu_Opening(object sender, CancelEventArgs e)
	{
		MainMenu.Items["mnRepackBundle"].Enabled = Directory.Exists(Paths.MergedBundleContent);

		// 5.x ships only the DirectX 12 executable (bin\x64_dx12); the DirectX 11 one is gone.
		MainMenu.Items["mnExitAndPlayDX11"].Enabled = !Paths.HasRemasterScripts();
	}

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
			_ = MessageBox.Show($"The Merged Mod folder ({Paths.RetrieveMergedModDir()}) doesn't exist. Have you created any merges yet?");
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
		using KeyboardShortcutsForm form = new();
		ThemeMngr.ApplyThemeOnForm(form);
		_ = form.ShowDialog(this);
	}

	private void MnExitAndPlayDX11_Click(object sender, EventArgs e)
	{
		if (TryOpenFile(Paths.GameExeDX11))
			Environment.Exit(0);
		else
			_ = MessageBox.Show($"DX11 game executable not found at the expected location:\n\n{Paths.GameExeDX11}", "Game Executable Not Found", MessageBoxButtons.OK);
	}

	private void MnExitAndPlayDX12_Click(object sender, EventArgs e)
	{
		if (TryOpenFile(Paths.GameExeDX12))
			Environment.Exit(0);
		else
			_ = MessageBox.Show($"DX12 game executable not found at the expected location:\n\n{Paths.GameExeDX12}", "Game Executable Not Found", MessageBoxButtons.OK);
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