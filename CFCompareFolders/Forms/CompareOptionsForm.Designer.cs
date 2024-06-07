namespace CFCompareFolders.Forms
{
    partial class CompareOptionsForm
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
            this.chkIncludeSubFolders = new System.Windows.Forms.CheckBox();
            this.chkIncludeHiddenFolders = new System.Windows.Forms.CheckBox();
            this.chkIncludeHiddenFiles = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.clbDifferences = new System.Windows.Forms.CheckedListBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // chkIncludeSubFolders
            // 
            this.chkIncludeSubFolders.AutoSize = true;
            this.chkIncludeSubFolders.Location = new System.Drawing.Point(12, 12);
            this.chkIncludeSubFolders.Name = "chkIncludeSubFolders";
            this.chkIncludeSubFolders.Size = new System.Drawing.Size(115, 17);
            this.chkIncludeSubFolders.TabIndex = 0;
            this.chkIncludeSubFolders.Text = "Include sub-folders";
            this.chkIncludeSubFolders.UseVisualStyleBackColor = true;
            // 
            // chkIncludeHiddenFolders
            // 
            this.chkIncludeHiddenFolders.AutoSize = true;
            this.chkIncludeHiddenFolders.Location = new System.Drawing.Point(12, 35);
            this.chkIncludeHiddenFolders.Name = "chkIncludeHiddenFolders";
            this.chkIncludeHiddenFolders.Size = new System.Drawing.Size(130, 17);
            this.chkIncludeHiddenFolders.TabIndex = 1;
            this.chkIncludeHiddenFolders.Text = "Include hidden folders";
            this.chkIncludeHiddenFolders.UseVisualStyleBackColor = true;
            // 
            // chkIncludeHiddenFiles
            // 
            this.chkIncludeHiddenFiles.AutoSize = true;
            this.chkIncludeHiddenFiles.Location = new System.Drawing.Point(12, 58);
            this.chkIncludeHiddenFiles.Name = "chkIncludeHiddenFiles";
            this.chkIncludeHiddenFiles.Size = new System.Drawing.Size(117, 17);
            this.chkIncludeHiddenFiles.TabIndex = 2;
            this.chkIncludeHiddenFiles.Text = "Include hidden files";
            this.chkIncludeHiddenFiles.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 94);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Differences:";
            // 
            // clbDifferences
            // 
            this.clbDifferences.FormattingEnabled = true;
            this.clbDifferences.Location = new System.Drawing.Point(12, 110);
            this.clbDifferences.Name = "clbDifferences";
            this.clbDifferences.Size = new System.Drawing.Size(301, 169);
            this.clbDifferences.TabIndex = 4;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(83, 298);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 5;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(164, 298);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 6;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // CompareOptionsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(323, 333);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.clbDifferences);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.chkIncludeHiddenFiles);
            this.Controls.Add(this.chkIncludeHiddenFolders);
            this.Controls.Add(this.chkIncludeSubFolders);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CompareOptionsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Options";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkIncludeSubFolders;
        private System.Windows.Forms.CheckBox chkIncludeHiddenFolders;
        private System.Windows.Forms.CheckBox chkIncludeHiddenFiles;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckedListBox clbDifferences;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
    }
}