namespace WitcherScriptMerger.Forms;

partial class MessageForm
{
	/// <summary>
	/// Required designer variable.
	/// </summary>
	private System.ComponentModel.IContainer components = null;

	/// <summary>
	/// Clean up any resources being used.
	/// </summary>
	/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
	protected override void Dispose(bool disposing)
	{
		if (disposing && (components != null))
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	#region Windows Form Designer generated code

	/// <summary>
	/// Required method for Designer support - do not modify
	/// the contents of this method with the code editor.
	/// </summary>
	private void InitializeComponent()
	{
		tlpMessageForm = new System.Windows.Forms.TableLayoutPanel();
		tbMessage = new System.Windows.Forms.TextBox();
		panel1 = new System.Windows.Forms.Panel();
		btnNo = new System.Windows.Forms.Button();
		btnYes = new System.Windows.Forms.Button();
		tlpMessageForm.SuspendLayout();
		panel1.SuspendLayout();
		SuspendLayout();
		// 
		// tlpMessageForm
		// 
		tlpMessageForm.ColumnCount = 1;
		tlpMessageForm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
		tlpMessageForm.Controls.Add(tbMessage, 0, 0);
		tlpMessageForm.Controls.Add(panel1, 0, 1);
		tlpMessageForm.Dock = System.Windows.Forms.DockStyle.Fill;
		tlpMessageForm.Location = new System.Drawing.Point(0, 0);
		tlpMessageForm.Name = "tlpMessageForm";
		tlpMessageForm.RowCount = 2;
		tlpMessageForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
		tlpMessageForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 90F));
		tlpMessageForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
		tlpMessageForm.Size = new System.Drawing.Size(979, 673);
		tlpMessageForm.TabIndex = 0;
		// 
		// tbMessage
		// 
		tbMessage.Dock = System.Windows.Forms.DockStyle.Fill;
		tbMessage.Location = new System.Drawing.Point(3, 3);
		tbMessage.Multiline = true;
		tbMessage.Name = "tbMessage";
		tbMessage.ReadOnly = true;
		tbMessage.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
		tbMessage.Size = new System.Drawing.Size(973, 577);
		tbMessage.TabIndex = 0;
		// 
		// panel1
		// 
		panel1.Controls.Add(btnNo);
		panel1.Controls.Add(btnYes);
		panel1.Dock = System.Windows.Forms.DockStyle.Fill;
		panel1.Location = new System.Drawing.Point(3, 586);
		panel1.Name = "panel1";
		panel1.Size = new System.Drawing.Size(973, 84);
		panel1.TabIndex = 1;
		// 
		// btnNo
		// 
		btnNo.Anchor = System.Windows.Forms.AnchorStyles.Right;
		btnNo.AutoSize = true;
		btnNo.DialogResult = System.Windows.Forms.DialogResult.No;
		btnNo.Font = new System.Drawing.Font("Segoe UI", 11F);
		btnNo.Location = new System.Drawing.Point(741, 17);
		btnNo.Margin = new System.Windows.Forms.Padding(0, 0, 5, 0);
		btnNo.Name = "btnNo";
		btnNo.Size = new System.Drawing.Size(221, 50);
		btnNo.TabIndex = 8;
		btnNo.Text = "&No";
		btnNo.UseVisualStyleBackColor = false;
		// 
		// btnYes
		// 
		btnYes.Anchor = System.Windows.Forms.AnchorStyles.Right;
		btnYes.AutoSize = true;
		btnYes.DialogResult = System.Windows.Forms.DialogResult.Yes;
		btnYes.Font = new System.Drawing.Font("Segoe UI", 11F);
		btnYes.Location = new System.Drawing.Point(515, 17);
		btnYes.Margin = new System.Windows.Forms.Padding(0, 0, 5, 0);
		btnYes.Name = "btnYes";
		btnYes.Size = new System.Drawing.Size(221, 50);
		btnYes.TabIndex = 7;
		btnYes.Text = "&Yes";
		btnYes.UseVisualStyleBackColor = false;
		// 
		// MessageForm
		// 
		AcceptButton = btnYes;
		AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
		AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		CancelButton = btnNo;
		ClientSize = new System.Drawing.Size(979, 673);
		Controls.Add(tlpMessageForm);
		FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
		Name = "MessageForm";
		SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
		StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		Text = "MessageForm";
		Load += MessageForm_Load;
		tlpMessageForm.ResumeLayout(false);
		tlpMessageForm.PerformLayout();
		panel1.ResumeLayout(false);
		panel1.PerformLayout();
		ResumeLayout(false);
	}

	#endregion

	private System.Windows.Forms.TableLayoutPanel tlpMessageForm;
	private System.Windows.Forms.TextBox tbMessage;
	private System.Windows.Forms.Panel panel1;
	internal System.Windows.Forms.Button btnNo;
	internal System.Windows.Forms.Button btnYes;
}