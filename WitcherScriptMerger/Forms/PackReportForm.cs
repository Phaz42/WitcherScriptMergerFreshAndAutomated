using System;
using System.IO;
using System.Windows.Forms;

namespace WitcherScriptMerger.Forms;

internal partial class PackReportForm : BaseForm
{
	#region Initialization

	internal PackReportForm(string bundlePath)
	{
		InitializeComponent();

		if (bundlePath == null)
		{ return; }

		txtBundlePath.Text = bundlePath;

		string[] contentPaths = Directory.GetFiles(Paths.MergedBundleContent, "*", SearchOption.AllDirectories);
		txtContent.Text = string.Join(Environment.NewLine, contentPaths);

		chkShowAfterPack.Checked = Program.Settings.Get<bool>("ReportAfterPack");

		btnOK.Select();
	}

	private void PackReportForm_FormClosing(object sender, FormClosingEventArgs e) => Program.Settings.Set("ReportAfterPack", chkShowAfterPack.Checked);

	#endregion

	#region Button Clicks

	private void BtnOpenBundleDir_Click(object sender, EventArgs e) => Program.TryOpenFileLocation(txtBundlePath.Text);

	private void BtnOpenContentDir_Click(object sender, EventArgs e) => Program.TryOpenDirectory(Paths.MergedBundleContent);

	private void BtnOK_Click(object sender, EventArgs e) => DialogResult = DialogResult.OK;

	#endregion

	private void Txt_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.Control && e.KeyCode == Keys.A)
			(sender as TextBox).SelectAll();
	}
}
