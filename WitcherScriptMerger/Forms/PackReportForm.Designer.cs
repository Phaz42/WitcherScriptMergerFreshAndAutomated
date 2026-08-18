namespace WitcherScriptMerger.Forms
{
    partial class PackReportForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PackReportForm));
            txtBundlePath = new System.Windows.Forms.TextBox();
            btnOpenContentDir = new System.Windows.Forms.Button();
            lblContent = new System.Windows.Forms.Label();
            txtContent = new System.Windows.Forms.TextBox();
            btnOpenBundleDir = new System.Windows.Forms.Button();
            lblPackedBundle = new System.Windows.Forms.Label();
            btnOK = new System.Windows.Forms.Button();
            chkShowAfterPack = new System.Windows.Forms.CheckBox();
            label1 = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // txtBundlePath
            // 
            txtBundlePath.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            txtBundlePath.BorderStyle = System.Windows.Forms.BorderStyle.None;
            txtBundlePath.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            txtBundlePath.Location = new System.Drawing.Point(16, 118);
            txtBundlePath.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            txtBundlePath.Name = "txtBundlePath";
            txtBundlePath.ReadOnly = true;
            txtBundlePath.Size = new System.Drawing.Size(716, 23);
            txtBundlePath.TabIndex = 1;
            txtBundlePath.KeyDown += Txt_KeyDown;
            // 
            // btnOpenContentDir
            // 
            btnOpenContentDir.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnOpenContentDir.BackColor = System.Drawing.Color.LightSteelBlue;
            btnOpenContentDir.FlatAppearance.BorderColor = System.Drawing.SystemColors.ButtonShadow;
            btnOpenContentDir.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnOpenContentDir.Font = new System.Drawing.Font("Segoe UI", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            btnOpenContentDir.Location = new System.Drawing.Point(611, 175);
            btnOpenContentDir.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            btnOpenContentDir.Name = "btnOpenContentDir";
            btnOpenContentDir.Size = new System.Drawing.Size(123, 32);
            btnOpenContentDir.TabIndex = 2;
            btnOpenContentDir.Text = "Open Directory";
            btnOpenContentDir.UseVisualStyleBackColor = false;
            btnOpenContentDir.Click += BtnOpenContentDir_Click;
            // 
            // lblContent
            // 
            lblContent.AutoSize = true;
            lblContent.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblContent.Location = new System.Drawing.Point(16, 182);
            lblContent.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblContent.Name = "lblContent";
            lblContent.Size = new System.Drawing.Size(74, 23);
            lblContent.TabIndex = 12;
            lblContent.Text = "Content";
            // 
            // txtContent
            // 
            txtContent.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            txtContent.BorderStyle = System.Windows.Forms.BorderStyle.None;
            txtContent.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            txtContent.Location = new System.Drawing.Point(16, 214);
            txtContent.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            txtContent.Multiline = true;
            txtContent.Name = "txtContent";
            txtContent.ReadOnly = true;
            txtContent.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            txtContent.Size = new System.Drawing.Size(716, 290);
            txtContent.TabIndex = 3;
            txtContent.KeyDown += Txt_KeyDown;
            // 
            // btnOpenBundleDir
            // 
            btnOpenBundleDir.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnOpenBundleDir.BackColor = System.Drawing.Color.LightSteelBlue;
            btnOpenBundleDir.FlatAppearance.BorderColor = System.Drawing.SystemColors.ButtonShadow;
            btnOpenBundleDir.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnOpenBundleDir.Font = new System.Drawing.Font("Segoe UI", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            btnOpenBundleDir.Location = new System.Drawing.Point(611, 80);
            btnOpenBundleDir.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            btnOpenBundleDir.Name = "btnOpenBundleDir";
            btnOpenBundleDir.Size = new System.Drawing.Size(123, 32);
            btnOpenBundleDir.TabIndex = 0;
            btnOpenBundleDir.Text = "Open Directory";
            btnOpenBundleDir.UseVisualStyleBackColor = false;
            btnOpenBundleDir.Click += BtnOpenBundleDir_Click;
            // 
            // lblPackedBundle
            // 
            lblPackedBundle.AutoSize = true;
            lblPackedBundle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lblPackedBundle.Location = new System.Drawing.Point(16, 17);
            lblPackedBundle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblPackedBundle.Name = "lblPackedBundle";
            lblPackedBundle.Size = new System.Drawing.Size(216, 28);
            lblPackedBundle.TabIndex = 8;
            lblPackedBundle.Text = "Packed new bundle file!";
            // 
            // btnOK
            // 
            btnOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnOK.BackColor = System.Drawing.Color.DarkSeaGreen;
            btnOK.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btnOK.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlDarkDark;
            btnOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnOK.Font = new System.Drawing.Font("Segoe UI", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            btnOK.Location = new System.Drawing.Point(591, 515);
            btnOK.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            btnOK.Name = "btnOK";
            btnOK.Size = new System.Drawing.Size(143, 35);
            btnOK.TabIndex = 5;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = false;
            btnOK.Click += BtnOK_Click;
            // 
            // chkShowAfterPack
            // 
            chkShowAfterPack.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            chkShowAfterPack.AutoSize = true;
            chkShowAfterPack.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            chkShowAfterPack.Location = new System.Drawing.Point(21, 517);
            chkShowAfterPack.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            chkShowAfterPack.Name = "chkShowAfterPack";
            chkShowAfterPack.Size = new System.Drawing.Size(319, 27);
            chkShowAfterPack.TabIndex = 4;
            chkShowAfterPack.Text = "&Show this report after packing bundle";
            chkShowAfterPack.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            label1.Location = new System.Drawing.Point(16, 86);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(167, 23);
            label1.TabIndex = 14;
            label1.Text = "Merged Bundle File";
            // 
            // PackReportForm
            // 
            AcceptButton = btnOK;
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = btnOK;
            ClientSize = new System.Drawing.Size(749, 566);
            Controls.Add(label1);
            Controls.Add(txtBundlePath);
            Controls.Add(btnOpenContentDir);
            Controls.Add(btnOpenBundleDir);
            Controls.Add(lblContent);
            Controls.Add(chkShowAfterPack);
            Controls.Add(txtContent);
            Controls.Add(btnOK);
            Controls.Add(lblPackedBundle);
            ForeColor = System.Drawing.SystemColors.ControlText;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(518, 400);
            Name = "PackReportForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Pack Finished";
            TopMost = true;
            FormClosing += PackReportForm_FormClosing;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TextBox txtBundlePath;
        private System.Windows.Forms.Button btnOpenBundleDir;
        private System.Windows.Forms.Label lblPackedBundle;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.CheckBox chkShowAfterPack;
        private System.Windows.Forms.Label lblContent;
        private System.Windows.Forms.TextBox txtContent;
        private System.Windows.Forms.Button btnOpenContentDir;
        private System.Windows.Forms.Label label1;
    }
}