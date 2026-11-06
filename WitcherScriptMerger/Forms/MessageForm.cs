using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace WitcherScriptMerger.Forms;
internal partial class MessageForm : Form
{
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	internal string Message
	{
		get => tbMessage.Text;
		set => tbMessage.Text = value;
	}

	internal MessageForm() => InitializeComponent();

	private void MessageForm_Load(object sender, EventArgs e) => ActiveControl = btnYes;
}
