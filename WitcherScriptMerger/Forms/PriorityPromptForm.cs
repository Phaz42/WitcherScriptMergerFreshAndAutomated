using System.Drawing;
using System.Windows.Forms;

using WitcherScriptMerger.LoadOrder;

using static WitcherScriptMerger.Program;

namespace WitcherScriptMerger.Forms;

internal partial class PriorityPromptForm : BaseForm
{
	private TableLayoutPanel tableLayoutPanel1;
	private Label lblTitle;
	private NumericUpDown upDownPrio;
	private Button btnOK;
	private Panel pnlTitle;
	private Label lblModName;
	private Label lblLoadOrderInfo;

	internal PriorityPromptForm()
	{
		InitializeComponent();
		ThemeMngr.ApplyThemeOnForm(this);
	}

	internal int? ShowDialog(string modName, int value = 0)
	{
		Icon = MainFrm.Icon;
		lblModName.Text = modName;
		upDownPrio.Minimum = CustomLoadOrder.TopPriority + 1;
		upDownPrio.Maximum = CustomLoadOrder.BottomPriority;
		btnOK.DialogResult = DialogResult.OK;
		if (value >= upDownPrio.Minimum && value <= upDownPrio.Maximum)
			upDownPrio.Value = value;
		upDownPrio.Select(0, upDownPrio.Text.Length);

		return ShowDialog() == DialogResult.OK ? System.Convert.ToInt32(upDownPrio.Value) : null;
	}

	private void PriorityPromptForm_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Escape)
			Close();
	}

	private void InitializeComponent()
	{
		tableLayoutPanel1 = new TableLayoutPanel();
		lblLoadOrderInfo = new Label();
		upDownPrio = new NumericUpDown();
		btnOK = new Button();
		pnlTitle = new Panel();
		lblModName = new Label();
		lblTitle = new Label();
		tableLayoutPanel1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)upDownPrio).BeginInit();
		pnlTitle.SuspendLayout();
		SuspendLayout();
		// 
		// tableLayoutPanel1
		// 
		tableLayoutPanel1.ColumnCount = 2;
		_ = tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
		_ = tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
		tableLayoutPanel1.Controls.Add(lblLoadOrderInfo, 0, 1);
		tableLayoutPanel1.Controls.Add(upDownPrio, 0, 2);
		tableLayoutPanel1.Controls.Add(btnOK, 1, 2);
		tableLayoutPanel1.Controls.Add(pnlTitle, 0, 0);
		tableLayoutPanel1.Dock = DockStyle.Fill;
		tableLayoutPanel1.Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
		tableLayoutPanel1.Location = new Point(0, 0);
		tableLayoutPanel1.Margin = new Padding(0);
		tableLayoutPanel1.Name = "tableLayoutPanel1";
		tableLayoutPanel1.RowCount = 3;
		_ = tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
		_ = tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 35F));
		_ = tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
		tableLayoutPanel1.Size = new Size(646, 295);
		tableLayoutPanel1.TabIndex = 0;
		// 
		// lblLoadOrderInfo
		// 
		lblLoadOrderInfo.AutoSize = true;
		lblLoadOrderInfo.BackColor = Color.Transparent;
		tableLayoutPanel1.SetColumnSpan(lblLoadOrderInfo, 2);
		lblLoadOrderInfo.Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
		lblLoadOrderInfo.Location = new Point(23, 93);
		lblLoadOrderInfo.Margin = new Padding(23, 20, 0, 0);
		lblLoadOrderInfo.Name = "lblLoadOrderInfo";
		lblLoadOrderInfo.Size = new Size(603, 50);
		lblLoadOrderInfo.TabIndex = 5;
		lblLoadOrderInfo.Tag = "";
		lblLoadOrderInfo.Text = "You can use the up/down arrow keys, or type a number. Press Enter to confirm.";
		// 
		// upDownPrio
		// 
		upDownPrio.Anchor = AnchorStyles.Right;
		upDownPrio.Font = new Font("Segoe UI", 25F, FontStyle.Regular, GraphicsUnit.Point);
		upDownPrio.Location = new Point(163, 204);
		upDownPrio.Margin = new Padding(0, 20, 23, 20);
		upDownPrio.Name = "upDownPrio";
		upDownPrio.Size = new Size(137, 63);
		upDownPrio.TabIndex = 0;
		// 
		// btnOK
		// 
		btnOK.Anchor = AnchorStyles.Left;
		btnOK.Font = new Font("Segoe UI", 15F, FontStyle.Regular, GraphicsUnit.Point);
		btnOK.Location = new Point(346, 201);
		btnOK.Margin = new Padding(23, 20, 0, 20);
		btnOK.Name = "btnOK";
		btnOK.Size = new Size(137, 69);
		btnOK.TabIndex = 1;
		btnOK.Text = "OK";
		btnOK.UseVisualStyleBackColor = true;
		// 
		// pnlTitle
		// 
		pnlTitle.BackColor = Color.Transparent;
		tableLayoutPanel1.SetColumnSpan(pnlTitle, 2);
		pnlTitle.Controls.Add(lblModName);
		pnlTitle.Controls.Add(lblTitle);
		pnlTitle.Dock = DockStyle.Fill;
		pnlTitle.Location = new Point(3, 3);
		pnlTitle.Name = "pnlTitle";
		pnlTitle.Size = new Size(640, 67);
		pnlTitle.TabIndex = 6;
		// 
		// lblModName
		// 
		lblModName.AutoSize = true;
		lblModName.Font = new Font("Segoe UI", 13F, FontStyle.Bold, GraphicsUnit.Point);
		lblModName.Location = new Point(219, 18);
		lblModName.Margin = new Padding(0);
		lblModName.Name = "lblModName";
		lblModName.Size = new Size(122, 30);
		lblModName.TabIndex = 1;
		lblModName.Tag = "AccentLabel";
		lblModName.Text = "ModName";
		// 
		// lblTitle
		// 
		lblTitle.AutoSize = true;
		lblTitle.Font = new Font("Segoe UI", 13F, FontStyle.Bold, GraphicsUnit.Point);
		lblTitle.Location = new Point(20, 18);
		lblTitle.Margin = new Padding(23, 0, 0, 0);
		lblTitle.Name = "lblTitle";
		lblTitle.Size = new Size(205, 30);
		lblTitle.TabIndex = 0;
		lblTitle.Text = "Set Load Order for";
		// 
		// PriorityPromptForm
		// 
		AcceptButton = btnOK;
		AutoScaleDimensions = new SizeF(8F, 20F);
		ClientSize = new Size(646, 295);
		Controls.Add(tableLayoutPanel1);
		FormBorderStyle = FormBorderStyle.FixedSingle;
		KeyPreview = true;
		Margin = new Padding(3, 4, 3, 4);
		MaximizeBox = false;
		MinimizeBox = false;
		Name = "PriorityPromptForm";
		StartPosition = FormStartPosition.CenterParent;
		Text = "Set Load Order";
		KeyDown += PriorityPromptForm_KeyDown;
		tableLayoutPanel1.ResumeLayout(false);
		tableLayoutPanel1.PerformLayout();
		((System.ComponentModel.ISupportInitialize)upDownPrio).EndInit();
		pnlTitle.ResumeLayout(false);
		pnlTitle.PerformLayout();
		ResumeLayout(false);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			tableLayoutPanel1.Dispose();
			lblTitle.Dispose();
			upDownPrio.Dispose();
			btnOK.Dispose();
			pnlTitle.Dispose();
			lblModName.Dispose();
			lblLoadOrderInfo.Dispose();
		}

		base.Dispose(disposing);
	}
}