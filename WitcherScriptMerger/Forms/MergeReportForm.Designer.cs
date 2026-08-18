namespace WitcherScriptMerger.Forms
{
    partial class MergeReportForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MergeReportForm));
            lblMergedFiles = new System.Windows.Forms.Label();
            btnOpenMergedDir = new System.Windows.Forms.Button();
            btnOpenMergedFile = new System.Windows.Forms.Button();
            txtMergedPath = new System.Windows.Forms.TextBox();
            btnOpenDir2 = new System.Windows.Forms.Button();
            btnOpenFile2 = new System.Windows.Forms.Button();
            txtFilePath2 = new System.Windows.Forms.TextBox();
            btnMergeReportOK = new System.Windows.Forms.Button();
            chkShowAfterMerge = new System.Windows.Forms.CheckBox();
            lblTempContentFiles = new System.Windows.Forms.Label();
            txtFilePath1 = new System.Windows.Forms.TextBox();
            btnOpenFile1 = new System.Windows.Forms.Button();
            btnOpenDir1 = new System.Windows.Forms.Button();
            lblArrowDown = new System.Windows.Forms.Label();
            lblMod1 = new System.Windows.Forms.Label();
            lblMod2 = new System.Windows.Forms.Label();
            lblMergedFile = new System.Windows.Forms.Label();
            lblNewPlus = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // lblMergedFiles
            // 
            lblMergedFiles.AutoSize = true;
            lblMergedFiles.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lblMergedFiles.Location = new System.Drawing.Point(16, 23);
            lblMergedFiles.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblMergedFiles.Name = "lblMergedFiles";
            lblMergedFiles.Size = new System.Drawing.Size(230, 28);
            lblMergedFiles.TabIndex = 5;
            lblMergedFiles.Text = "Created new merged file!";
            // 
            // btnOpenMergedDir
            // 
            btnOpenMergedDir.Anchor = System.Windows.Forms.AnchorStyles.Top;
            btnOpenMergedDir.BackColor = System.Drawing.Color.LightSteelBlue;
            btnOpenMergedDir.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlDark;
            btnOpenMergedDir.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnOpenMergedDir.Font = new System.Drawing.Font("Segoe UI", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            btnOpenMergedDir.ForeColor = System.Drawing.SystemColors.ControlText;
            btnOpenMergedDir.Location = new System.Drawing.Point(545, 460);
            btnOpenMergedDir.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            btnOpenMergedDir.Name = "btnOpenMergedDir";
            btnOpenMergedDir.Size = new System.Drawing.Size(388, 35);
            btnOpenMergedDir.TabIndex = 2;
            btnOpenMergedDir.Text = "Open Directory";
            btnOpenMergedDir.UseVisualStyleBackColor = false;
            btnOpenMergedDir.Click += BtnOpenOutputDir_Click;
            // 
            // btnOpenMergedFile
            // 
            btnOpenMergedFile.Anchor = System.Windows.Forms.AnchorStyles.Top;
            btnOpenMergedFile.BackColor = System.Drawing.Color.LightSteelBlue;
            btnOpenMergedFile.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlDark;
            btnOpenMergedFile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnOpenMergedFile.Font = new System.Drawing.Font("Segoe UI", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            btnOpenMergedFile.ForeColor = System.Drawing.SystemColors.ControlText;
            btnOpenMergedFile.Location = new System.Drawing.Point(150, 460);
            btnOpenMergedFile.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            btnOpenMergedFile.Name = "btnOpenMergedFile";
            btnOpenMergedFile.Size = new System.Drawing.Size(387, 35);
            btnOpenMergedFile.TabIndex = 1;
            btnOpenMergedFile.Text = "Open File";
            btnOpenMergedFile.UseVisualStyleBackColor = false;
            btnOpenMergedFile.Click += BtnOpenOutputFile_Click;
            // 
            // txtMergedPath
            // 
            txtMergedPath.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            txtMergedPath.BorderStyle = System.Windows.Forms.BorderStyle.None;
            txtMergedPath.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            txtMergedPath.ForeColor = System.Drawing.SystemColors.ControlText;
            txtMergedPath.Location = new System.Drawing.Point(24, 420);
            txtMergedPath.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            txtMergedPath.Name = "txtMergedPath";
            txtMergedPath.ReadOnly = true;
            txtMergedPath.Size = new System.Drawing.Size(1045, 23);
            txtMergedPath.TabIndex = 0;
            txtMergedPath.KeyDown += Txt_KeyDown;
            // 
            // btnOpenDir2
            // 
            btnOpenDir2.Anchor = System.Windows.Forms.AnchorStyles.Top;
            btnOpenDir2.BackColor = System.Drawing.Color.LightSteelBlue;
            btnOpenDir2.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlDark;
            btnOpenDir2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnOpenDir2.Font = new System.Drawing.Font("Segoe UI", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            btnOpenDir2.ForeColor = System.Drawing.SystemColors.ControlText;
            btnOpenDir2.Location = new System.Drawing.Point(545, 301);
            btnOpenDir2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            btnOpenDir2.Name = "btnOpenDir2";
            btnOpenDir2.Size = new System.Drawing.Size(388, 35);
            btnOpenDir2.TabIndex = 2;
            btnOpenDir2.Text = "Open Directory";
            btnOpenDir2.UseVisualStyleBackColor = false;
            btnOpenDir2.Click += BtnOpenDir2_Click;
            // 
            // btnOpenFile2
            // 
            btnOpenFile2.Anchor = System.Windows.Forms.AnchorStyles.Top;
            btnOpenFile2.BackColor = System.Drawing.Color.LightSteelBlue;
            btnOpenFile2.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlDark;
            btnOpenFile2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnOpenFile2.Font = new System.Drawing.Font("Segoe UI", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            btnOpenFile2.ForeColor = System.Drawing.SystemColors.ControlText;
            btnOpenFile2.Location = new System.Drawing.Point(150, 301);
            btnOpenFile2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            btnOpenFile2.Name = "btnOpenFile2";
            btnOpenFile2.Size = new System.Drawing.Size(387, 35);
            btnOpenFile2.TabIndex = 1;
            btnOpenFile2.Text = "Open File";
            btnOpenFile2.UseVisualStyleBackColor = false;
            btnOpenFile2.Click += BtnOpenFile2_Click;
            // 
            // txtFilePath2
            // 
            txtFilePath2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            txtFilePath2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            txtFilePath2.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            txtFilePath2.ForeColor = System.Drawing.SystemColors.ControlText;
            txtFilePath2.Location = new System.Drawing.Point(24, 261);
            txtFilePath2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            txtFilePath2.Name = "txtFilePath2";
            txtFilePath2.ReadOnly = true;
            txtFilePath2.Size = new System.Drawing.Size(1045, 23);
            txtFilePath2.TabIndex = 0;
            txtFilePath2.KeyDown += Txt_KeyDown;
            // 
            // btnMergeReportOK
            // 
            btnMergeReportOK.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnMergeReportOK.BackColor = System.Drawing.Color.DarkSeaGreen;
            btnMergeReportOK.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btnMergeReportOK.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlDarkDark;
            btnMergeReportOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnMergeReportOK.Font = new System.Drawing.Font("Segoe UI", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            btnMergeReportOK.Location = new System.Drawing.Point(922, 551);
            btnMergeReportOK.Margin = new System.Windows.Forms.Padding(8);
            btnMergeReportOK.Name = "btnMergeReportOK";
            btnMergeReportOK.Size = new System.Drawing.Size(143, 35);
            btnMergeReportOK.TabIndex = 4;
            btnMergeReportOK.Text = "OK";
            btnMergeReportOK.UseVisualStyleBackColor = false;
            btnMergeReportOK.Click += BtnOK_Click;
            // 
            // chkShowAfterMerge
            // 
            chkShowAfterMerge.AutoSize = true;
            chkShowAfterMerge.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            chkShowAfterMerge.Location = new System.Drawing.Point(17, 557);
            chkShowAfterMerge.Margin = new System.Windows.Forms.Padding(8);
            chkShowAfterMerge.Name = "chkShowAfterMerge";
            chkShowAfterMerge.Size = new System.Drawing.Size(292, 27);
            chkShowAfterMerge.TabIndex = 3;
            chkShowAfterMerge.Text = "&Show this report after each merge";
            chkShowAfterMerge.UseVisualStyleBackColor = true;
            // 
            // lblTempContentFiles
            // 
            lblTempContentFiles.AutoSize = true;
            lblTempContentFiles.Location = new System.Drawing.Point(336, 18);
            lblTempContentFiles.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblTempContentFiles.Name = "lblTempContentFiles";
            lblTempContentFiles.Size = new System.Drawing.Size(538, 40);
            lblTempContentFiles.TabIndex = 6;
            lblTempContentFiles.Text = "Note: The first 2 files listed below were temporarily unpacked from .bundle files.\r\nThey will be deleted when all merges are finished.";
            // 
            // txtFilePath1
            // 
            txtFilePath1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            txtFilePath1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            txtFilePath1.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            txtFilePath1.ForeColor = System.Drawing.SystemColors.ControlText;
            txtFilePath1.Location = new System.Drawing.Point(24, 125);
            txtFilePath1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            txtFilePath1.Name = "txtFilePath1";
            txtFilePath1.ReadOnly = true;
            txtFilePath1.Size = new System.Drawing.Size(1045, 23);
            txtFilePath1.TabIndex = 0;
            txtFilePath1.KeyDown += Txt_KeyDown;
            // 
            // btnOpenFile1
            // 
            btnOpenFile1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            btnOpenFile1.BackColor = System.Drawing.Color.LightSteelBlue;
            btnOpenFile1.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlDark;
            btnOpenFile1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnOpenFile1.Font = new System.Drawing.Font("Segoe UI", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            btnOpenFile1.ForeColor = System.Drawing.SystemColors.ControlText;
            btnOpenFile1.Location = new System.Drawing.Point(150, 165);
            btnOpenFile1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            btnOpenFile1.Name = "btnOpenFile1";
            btnOpenFile1.Size = new System.Drawing.Size(387, 35);
            btnOpenFile1.TabIndex = 1;
            btnOpenFile1.Text = "Open File";
            btnOpenFile1.UseVisualStyleBackColor = false;
            btnOpenFile1.Click += BtnOpenFile1_Click;
            // 
            // btnOpenDir1
            // 
            btnOpenDir1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            btnOpenDir1.BackColor = System.Drawing.Color.LightSteelBlue;
            btnOpenDir1.FlatAppearance.BorderColor = System.Drawing.SystemColors.ControlDark;
            btnOpenDir1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnOpenDir1.Font = new System.Drawing.Font("Segoe UI", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            btnOpenDir1.ForeColor = System.Drawing.SystemColors.ControlText;
            btnOpenDir1.Location = new System.Drawing.Point(545, 165);
            btnOpenDir1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            btnOpenDir1.Name = "btnOpenDir1";
            btnOpenDir1.Size = new System.Drawing.Size(388, 35);
            btnOpenDir1.TabIndex = 2;
            btnOpenDir1.Text = "Open Directory";
            btnOpenDir1.UseVisualStyleBackColor = false;
            btnOpenDir1.Click += BtnOpenDir1_Click;
            // 
            // lblArrowDown
            // 
            lblArrowDown.Anchor = System.Windows.Forms.AnchorStyles.Top;
            lblArrowDown.AutoSize = true;
            lblArrowDown.Font = new System.Drawing.Font("Segoe UI", 30F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblArrowDown.Location = new System.Drawing.Point(507, 345);
            lblArrowDown.Margin = new System.Windows.Forms.Padding(0);
            lblArrowDown.Name = "lblArrowDown";
            lblArrowDown.Size = new System.Drawing.Size(69, 67);
            lblArrowDown.TabIndex = 8;
            lblArrowDown.Text = "🠟";
            lblArrowDown.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblMod1
            // 
            lblMod1.AutoSize = true;
            lblMod1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lblMod1.Location = new System.Drawing.Point(51, 87);
            lblMod1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 5);
            lblMod1.Name = "lblMod1";
            lblMod1.Size = new System.Drawing.Size(70, 28);
            lblMod1.TabIndex = 9;
            lblMod1.Text = "Mod 1";
            // 
            // lblMod2
            // 
            lblMod2.AutoSize = true;
            lblMod2.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lblMod2.Location = new System.Drawing.Point(51, 223);
            lblMod2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 5);
            lblMod2.Name = "lblMod2";
            lblMod2.Size = new System.Drawing.Size(70, 28);
            lblMod2.TabIndex = 10;
            lblMod2.Text = "Mod 2";
            // 
            // lblMergedFile
            // 
            lblMergedFile.AutoSize = true;
            lblMergedFile.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lblMergedFile.Location = new System.Drawing.Point(51, 382);
            lblMergedFile.Margin = new System.Windows.Forms.Padding(4, 0, 4, 5);
            lblMergedFile.Name = "lblMergedFile";
            lblMergedFile.Size = new System.Drawing.Size(116, 28);
            lblMergedFile.TabIndex = 11;
            lblMergedFile.Text = "Merged File";
            // 
            // lblNewPlus
            // 
            lblNewPlus.Anchor = System.Windows.Forms.AnchorStyles.Top;
            lblNewPlus.AutoSize = true;
            lblNewPlus.Font = new System.Drawing.Font("Segoe UI", 45F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblNewPlus.Location = new System.Drawing.Point(494, 178);
            lblNewPlus.Margin = new System.Windows.Forms.Padding(0);
            lblNewPlus.Name = "lblNewPlus";
            lblNewPlus.Size = new System.Drawing.Size(95, 100);
            lblNewPlus.TabIndex = 12;
            lblNewPlus.Text = "+";
            lblNewPlus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // MergeReportForm
            // 
            AcceptButton = btnMergeReportOK;
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = btnMergeReportOK;
            ClientSize = new System.Drawing.Size(1082, 618);
            Controls.Add(lblMergedFile);
            Controls.Add(lblMod2);
            Controls.Add(lblMod1);
            Controls.Add(lblArrowDown);
            Controls.Add(btnOpenMergedDir);
            Controls.Add(btnOpenDir2);
            Controls.Add(btnOpenMergedFile);
            Controls.Add(btnOpenDir1);
            Controls.Add(txtMergedPath);
            Controls.Add(btnOpenFile2);
            Controls.Add(lblTempContentFiles);
            Controls.Add(txtFilePath2);
            Controls.Add(btnOpenFile1);
            Controls.Add(chkShowAfterMerge);
            Controls.Add(txtFilePath1);
            Controls.Add(btnMergeReportOK);
            Controls.Add(lblMergedFiles);
            Controls.Add(lblNewPlus);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            MaximumSize = new System.Drawing.Size(1892, 665);
            MinimumSize = new System.Drawing.Size(823, 665);
            Name = "MergeReportForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Merge Finished";
            TopMost = true;
            FormClosing += MergeReportForm_FormClosing;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.Label lblMergedFiles;
        private System.Windows.Forms.Button btnOpenMergedDir;
        private System.Windows.Forms.Button btnOpenMergedFile;
        private System.Windows.Forms.TextBox txtMergedPath;
        private System.Windows.Forms.Button btnOpenDir2;
        private System.Windows.Forms.Button btnOpenFile2;
        private System.Windows.Forms.TextBox txtFilePath2;
        private System.Windows.Forms.Button btnMergeReportOK;
        private System.Windows.Forms.CheckBox chkShowAfterMerge;
        private System.Windows.Forms.Label lblTempContentFiles;
        private System.Windows.Forms.TextBox txtFilePath1;
        private System.Windows.Forms.Button btnOpenFile1;
        private System.Windows.Forms.Button btnOpenDir1;
        private System.Windows.Forms.Label lblArrowDown;
        private System.Windows.Forms.Label lblMod1;
        private System.Windows.Forms.Label lblMod2;
        private System.Windows.Forms.Label lblMergedFile;
        private System.Windows.Forms.Label lblNewPlus;
    }
}