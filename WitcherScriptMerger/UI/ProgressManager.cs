using System;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

using WitcherScriptMerger.FileIndex;
using WitcherScriptMerger.Forms;

namespace WitcherScriptMerger.UI;

/// <summary>
/// Manages the progress UI elements and updates in the MainForm.
/// </summary>
internal class ProgressManager
{
	private readonly MainForm mainForm;
	internal Label lblBundleScanInfo;
	internal ToolStripStatusLabel lblStatusLeft1;
	internal ToolStripStatusLabel lblStatusLeft2;
	internal ToolStripStatusLabel lblStatusLeft3;
	internal ToolStripStatusLabel lblStatusRight;
	internal Label lblProgressCurrentAction;
	internal Label lblProgressCurrentPhase;
	internal ProgressBar progressBar;
	internal readonly IntPtr Handle;
	internal Panel pnlProgress;

	/// <summary>
	/// Initializes a new instance of the <see cref="ProgressManager"/> class.
	/// </summary>
	internal ProgressManager(MainForm mainForm)
	{
		this.mainForm = mainForm;
		lblBundleScanInfo = mainForm.lblBundleScanInfo;
		lblStatusLeft1 = mainForm.lblStatusLeft1;
		lblStatusLeft2 = mainForm.lblStatusLeft2;
		lblStatusLeft3 = mainForm.lblStatusLeft3;
		lblStatusRight = mainForm.lblStatusRight;
		lblProgressCurrentAction = mainForm.lblProgressCurrentAction;
		lblProgressCurrentPhase = mainForm.lblProgressCurrentPhase;
		progressBar = mainForm.progressBar;
		Handle = mainForm.Handle;
		pnlProgress = mainForm.pnlProgress;
	}

	/// <summary>
	/// Initializes the progress screen with the specified progress text and style.
	/// </summary>
	internal void InitializeProgressScreen(string progressOf, ProgressBarStyle style = ProgressBarStyle.Marquee)
	{
		progressBar.Value = 0;
		lblProgressCurrentPhase.Text = progressOf;
		lblProgressCurrentAction.Text = string.Empty;
		progressBar.Style = style;

		switch (style)
		{
			case ProgressBarStyle.Continuous:
				TaskbarProgress.SetValue(Handle, 0, 100);
				TaskbarProgress.SetState(Handle, TaskbarProgress.TaskbarStates.Normal);
				break;
			case ProgressBarStyle.Marquee:
				TaskbarProgress.SetState(Handle, TaskbarProgress.TaskbarStates.Indeterminate);
				break;
		}

		pnlProgress.Visible = true;
		mainForm.Update();
	}

	/// <summary>
	/// Hides the progress screen.
	/// </summary>
	internal void HideProgressScreen()
	{
		pnlProgress.Visible = false;
		mainForm.treMerges.Select();

		TaskbarProgress.SetState(Handle, TaskbarProgress.TaskbarStates.NoProgress);
	}

	/// <summary>
	/// Updates the status text labels with information from the TreeView and ModFileIndex.
	/// </summary>
	internal void UpdateStatusText(ModFileIndex modFileIndex)
	{
		int solvableCount = mainForm.treConflicts.FileNodes.Count(node => ModFile.IsTextFile(node.Text));

		// Update conflict status
		if (mainForm.treConflicts.IsEmpty())
		{
			lblStatusLeft1.Text = "0 conflicts";
		}
		else
		{
			lblStatusLeft1.Text = $"{solvableCount} mergeable";
			if (solvableCount < mainForm.treConflicts.FileNodes.Count)
			{
				lblStatusLeft2.Text = $"{mainForm.treConflicts.FileNodes.Count - solvableCount} not mergeable";
				lblStatusLeft2.Visible = true;
			}
		}

		// Update merge status
		lblStatusLeft3.Text = string.Format(
			CultureInfo.InvariantCulture,
			"{0} merge{1}",
			mainForm.treMerges.FileNodes.Count,
			mainForm.treMerges.FileNodes.Count.GetPluralS()
		);
		lblStatusLeft3.Visible = true;

		// Update mod file index status
		if (modFileIndex != null)
		{
			lblStatusRight.Text = string.Format(
				CultureInfo.InvariantCulture,
				"Found {0} mod{1}, {2} script{3}, {4} XML{5}, {6} bundle{7}",
				modFileIndex.ModCount,
				modFileIndex.ModCount.GetPluralS(),
				modFileIndex.ScriptCount,
				modFileIndex.ScriptCount.GetPluralS(),
				modFileIndex.XmlCount,
				modFileIndex.XmlCount.GetPluralS(),
				modFileIndex.BundleCount,
				modFileIndex.BundleCount.GetPluralS());
		}
	}

	/// <summary>
	/// Cancels the automatic exit timer and updates the UI accordingly.
	/// </summary>
	internal void DontExit()
	{
		mainForm._autoExitTimer.Dispose();
		progressBar.Show();
		mainForm.btnDontExit.Hide();
		mainForm.btnExitNow.Hide();
		_ = mainForm.btnCreateMerges.Focus();
		HideProgressScreen();
		mainForm.uiThreadManager.SetMergeButtonTextIfValidSelection();
	}
}