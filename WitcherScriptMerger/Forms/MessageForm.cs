using System;
using System.Windows.Forms;

namespace WitcherScriptMerger.Forms;
internal partial class MessageForm : Form
{
	internal string Message
	{
		get => tbMessage.Text;
		set => tbMessage.Text = value;
	}

	internal MessageForm() => InitializeComponent();

	private void MessageForm_Load(object sender, EventArgs e) => ActiveControl = btnYes;
}
