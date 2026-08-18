using System;
using System.Windows.Forms;

namespace WitcherScriptMerger.Forms;

internal partial class MergeReportForm : BaseForm
{
	#region Initialization

	internal MergeReportForm(
		int mergeNum, int mergeTotal,
		string file1, string file2, string outputFile,
		string modName1, string modName2)
	{
		InitializeComponent();

		if (file1 is null)
		{ return; }

		if (mergeTotal > 1)
		{
			Text += $" ({mergeNum} of {mergeTotal})";  // Window title
			if (mergeNum < mergeTotal)
				btnMergeReportOK.Text = "Continue";
		}

		lblTempContentFiles.Visible = outputFile.StartsWithIgnoreCase(Paths.MergedBundleContent);

		lblMod1.Text = modName1;
		lblMod2.Text = modName2;

		txtFilePath1.Text = file1;
		txtFilePath2.Text = file2;
		txtMergedPath.Text = outputFile;

		chkShowAfterMerge.Checked = Program.Settings.Get<bool>("ReportAfterMerge");

		btnMergeReportOK.Select();
	}

	private void MergeReportForm_FormClosing(object sender, FormClosingEventArgs e) => Program.Settings.Set("ReportAfterMerge", chkShowAfterMerge.Checked);

	#endregion

	#region Button Clicks

	private void BtnOpenFile1_Click(object sender, EventArgs e) => Program.TryOpenFile(txtFilePath1.Text);

	private void BtnOpenFile2_Click(object sender, EventArgs e) => Program.TryOpenFile(txtFilePath2.Text);

	private void BtnOpenOutputFile_Click(object sender, EventArgs e) => Program.TryOpenFile(txtMergedPath.Text);

	private void BtnOpenDir1_Click(object sender, EventArgs e) => Program.TryOpenFileLocation(txtFilePath1.Text);

	private void BtnOpenDir2_Click(object sender, EventArgs e) => Program.TryOpenFileLocation(txtFilePath2.Text);

	private void BtnOpenOutputDir_Click(object sender, EventArgs e) => Program.TryOpenFileLocation(txtMergedPath.Text);

	private void BtnOK_Click(object sender, EventArgs e) => DialogResult = DialogResult.OK;

	#endregion

	private void Txt_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.Control && e.KeyCode == Keys.A)
			(sender as TextBox).SelectAll();
	}
}
