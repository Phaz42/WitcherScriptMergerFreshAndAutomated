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
	/// Shows a task dialog on the main form, on the UI thread, and returns the button chosen. Where the native task dialog
	/// is unavailable (comctl32 6, which a compatibility layer may lack), <paramref name="fallback"/> asks instead.
	/// </summary>
	internal TaskDialogButton ShowTaskDialog(TaskDialogPage page, Func<TaskDialogButton> fallback)
	{
		ActivateSafely();

		TaskDialogButton Show()
		{
			try
			{
				return TaskDialog.ShowDialog(mainForm, page);
			}
			catch (Exception e) when (e is InvalidOperationException or System.ComponentModel.Win32Exception or EntryPointNotFoundException)
			{
				return fallback();
			}
		}

		return mainForm.InvokeRequired ? (TaskDialogButton)mainForm.Invoke(new Func<TaskDialogButton>(Show)) : Show();
	}

	/// <summary>
	/// Shows an error whose technical details are collapsed under "Show technical details", so the user first reads what
	/// happened and what to do; "Copy details" puts all of it on the clipboard. Where the native task dialog is
	/// unavailable, a plain message box shows everything, the details last (Ctrl+C copies a message box's text).
	/// </summary>
	internal void ShowErrorWithDetails(string heading, string text, string details, string title = "Error")
	{
		ActivateSafely();

		void Show()
		{
			// Copies the whole error - what a user pastes into a forum post or bug report - and leaves the dialog open.
			TaskDialogButton copyButton = new("Copy details") { AllowCloseDialog = false };
			copyButton.Click += (sender, e) =>
			{
				string all = $"{title}\n\n{heading}\n\n{text}\n\n{details}".Replace("\r\n", "\n", StringComparison.Ordinal);
				try
				{
					Clipboard.SetText(all.Replace("\n", "\r\n", StringComparison.Ordinal));
				}
				catch (System.Runtime.InteropServices.ExternalException)
				{
					// Another program holds the clipboard; the details stay readable in the dialog.
				}
			};

			TaskDialogPage page = new()
			{
				Caption = title,
				Heading = heading,
				Text = text,
				Icon = TaskDialogIcon.Error,
				Buttons = { copyButton, TaskDialogButton.OK },
				DefaultButton = TaskDialogButton.OK,
				Expander = new TaskDialogExpander(details)
				{
					CollapsedButtonText = "Show technical details",
					ExpandedButtonText = "Hide technical details",
					Position = TaskDialogExpanderPosition.AfterFootnote
				}
			};

			try
			{
				_ = TaskDialog.ShowDialog(mainForm, page);
			}
			catch (Exception e) when (e is InvalidOperationException or System.ComponentModel.Win32Exception or EntryPointNotFoundException)
			{
				// The task dialog needs comctl32 6 (visual styles); a compatibility layer may lack it or part of it.
				_ = MessageBox.Show(mainForm, $"{heading}\n\n{text}\n\nTechnical details:\n{details}", title, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		if (mainForm.InvokeRequired)
			_ = mainForm.Invoke((MethodInvoker)Show);
		else
			Show();
	}

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
		if (mainForm.treConflicts.HasCategory(Categories.Script))
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
		// The image is a file the user may replace or remove (NoConflictsImage); with none to show, the empty tree stays.
		// NoConflictsImage owns the image and disposes the previous one when it loads another, so the picture box lets go of
		// it first - an animated GIF is still playing there - and never keeps one it is not given now, even when hidden.
		mainForm.pbNoConflicts.Image = null;
		System.Drawing.Image image = null;
		// No conflicts at all - a search that shows none is not the same.
		if (mainForm.treConflicts.HasNoFileNodes && Settings.Get<bool>("NoConflictsWitcher"))
			image = NoConflictsImage.Get(ThemeManager.CurrentTheme.IsDark);

		mainForm.pbNoConflicts.Image = image;

		if (image != null)
		{
			// Show "no conflicts" image
			mainForm.tlpMain.Controls.Remove(mainForm.treConflicts);
			mainForm.pbNoConflicts.BackColor = ThemeManager.CurrentTheme.StandardButtonBackColor;
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
		// Selected conflicts a search hides are merged too, so they count.
		int validFileNodeCount = mainForm.treConflicts.AllFileNodes.Count(node => node.GetTreeNodes().Count(modNode => modNode.Checked) > 1);
		mainForm.btnCreateMerges.Text = validFileNodeCount > 1
			? "&Create " + validFileNodeCount + " Merges"
			: "&Create Selected Merges";
	}

	/// <summary>
	/// Sets the text of the "Delete Merges" button based on the number of file nodes selected in the merges tree.
	/// </summary>
	internal void SetUnmergeButtonTextIfValidSelection()
	{
		int selectedCount = mainForm.treMerges.AllFileNodes.Count(node => node.Checked);
		mainForm.btnDeleteMerges.Text = selectedCount > 1
			? "&Delete " + selectedCount + " Merges"
			: "&Delete Selected Merges";
	}
}
