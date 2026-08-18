using System;
using System.Linq;
using System.Windows.Forms;

using WitcherScriptMerger.FileIndex;
using WitcherScriptMerger.Forms;
using WitcherScriptMerger.Theming;

using static WitcherScriptMerger.Program;

namespace WitcherScriptMerger.UI;

internal class UIThreadManager
{
	private readonly MainForm mainForm;

	internal UIThreadManager(MainForm mainForm) => this.mainForm = mainForm;

	/// <summary>
	/// Shows a message box on the main form, ensuring it's invoked on the UI thread if necessary.
	/// </summary>
	internal DialogResult ShowMessage(
		string text,
		string title = "",
		MessageBoxButtons buttons = MessageBoxButtons.OK,
		MessageBoxIcon icon = MessageBoxIcon.None,
		MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1)
	{
		ActivateSafely();

		return mainForm.InvokeRequired
			? mainForm.Invoke(new Func<DialogResult>(() => MessageBox.Show(mainForm, text, title, buttons, icon, defaultButton)))
			: MessageBox.Show(mainForm, text, title, buttons, icon, defaultButton);
	}

	internal DialogResult ShowError(string text, string title = "Error") => ShowMessage(text, title, MessageBoxButtons.OK, MessageBoxIcon.Error);

	/// <summary>
	/// Shows a modal form on the main form, ensuring it's invoked on the UI thread if necessary.
	/// Applies theming to specific form types.
	/// </summary>
	internal DialogResult ShowModal(Form form)
	{
		ActivateSafely();

		if (mainForm.InvokeRequired)
		{
			return mainForm.Invoke(new Func<DialogResult>(() =>
			{
				if (form is MergeReportForm or PackReportForm)
					ThemeMngr.ApplyThemeOnForm(form);
				return form.ShowDialog(mainForm);
			}));
		}
		else
		{
			if (form is MergeReportForm or PackReportForm)
				ThemeMngr.ApplyThemeOnForm(form);
			return form.ShowDialog(mainForm);
		}
	}

	/// <summary>
	/// Activates the main form, ensuring it's invoked on the UI thread if necessary.
	/// </summary>
	internal void ActivateSafely()
	{
		if (mainForm.InvokeRequired)
		{
			_ = mainForm.Invoke((MethodInvoker)delegate ()
			{
				mainForm.Activate();
			});
		}
		else
		{
			mainForm.Activate();
		}
	}

	/// <summary>
	/// Initiates the auto-exit procedure if no script conflicts are found.
	/// This shows a progress screen with a countdown and buttons to cancel or exit immediately.
	/// </summary>
	internal void ExitIfNoMoreConflicts()
	{
		if (mainForm.treConflicts.GetCategoryNode(Categories.Script) != null)
			return;

		mainForm.progressManager.InitializeProgressScreen("Merging complete, auto closing in 2 seconds...");
		mainForm.progressBar.Hide();
		mainForm.btnDontExit.Show();
		mainForm.btnExitNow.Show();
		_ = mainForm.btnExitNow.Focus();
		mainForm._autoExitTimer = new System.Threading.Timer(AutoExit, null, 2000, 0);
	}

	/// <summary>
	/// Shows or hides the "no conflicts" image based on the presence of conflicts and user settings.
	/// </summary>
	internal void ShowHideNoConflictsWitcher()
	{
		if (mainForm.treConflicts.IsEmpty() && Settings.Get<bool>("NoConflictsWitcher"))
		{
			// Show "no conflicts" image
			mainForm.tlpMain.Controls.Remove(mainForm.treConflicts);
			mainForm.pbNoConflicts.BackColor = ThemeManager.CurrentTheme.StandardButtonBackColor;
			mainForm.pbNoConflicts.Image = ThemeManager.CurrentTheme.NoConflictsImage;
			mainForm.pbNoConflicts.Dock = DockStyle.Fill;
			mainForm.pbNoConflicts.SizeMode = PictureBoxSizeMode.Zoom;
			mainForm.pbNoConflicts.Margin = new Padding(0);
			mainForm.tlpMain.SetColumnSpan(mainForm.pbNoConflicts, 2);
			mainForm.tlpMain.Controls.Add(mainForm.pbNoConflicts, 1, 1);
			mainForm.lblConflicts.Text = "No Conflicts!";
		}
		else
		{
			// Show conflicts tree
			mainForm.tlpMain.Controls.Remove(mainForm.pbNoConflicts);
			mainForm.tlpMain.SetColumnSpan(mainForm.treConflicts, 2);
			mainForm.tlpMain.Controls.Add(mainForm.treConflicts, 1, 1);
			mainForm.lblConflicts.Text = "Conflicts";
		}
	}

	/// <summary>
	/// Sets the text of the "Create Merges" button based on the number of valid file nodes selected in the conflicts tree.
	/// </summary>
	internal void SetMergeButtonTextIfValidSelection()
	{
		int validFileNodeCount = mainForm.treConflicts.FileNodes.Count(node => node.GetTreeNodes().Count(modNode => modNode.Checked) > 1);
		mainForm.btnCreateMerges.Text = validFileNodeCount > 1
			? "&Create " + validFileNodeCount + " Merges"
			: "&Create Selected Merges";
	}

	/// <summary>
	/// Sets the text of the "Delete Merges" button based on the number of file nodes selected in the merges tree.
	/// </summary>
	internal void SetUnmergeButtonTextIfValidSelection()
	{
		int selectedCount = mainForm.treMerges.FileNodes.Count(node => node.Checked);
		mainForm.btnDeleteMerges.Text = selectedCount > 1
			? "&Delete " + selectedCount + " Merges"
			: "&Delete Selected Merges";
	}
}
