namespace Microsoft.SnippetDesigner.OptionPages
{
    partial class ResetOptionsControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rebuildIndexLabel = new System.Windows.Forms.Label();
            this.resetIndexDirectoriesButton = new System.Windows.Forms.Button();
            this.rebuildIndexButton = new System.Windows.Forms.Button();
            this.resetIndexedDirectoriesLabel = new System.Windows.Forms.Label();
            this.resetSnippetIndexFilePath = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.resetSnippetIndexFilePath);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.rebuildIndexLabel);
            this.groupBox1.Controls.Add(this.resetIndexDirectoriesButton);
            this.groupBox1.Controls.Add(this.rebuildIndexButton);
            this.groupBox1.Controls.Add(this.resetIndexedDirectoriesLabel);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(560, 174);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Snippet Designer Resets";
            // 
            // rebuildIndexLabel
            // 
            this.rebuildIndexLabel.AutoSize = true;
            this.rebuildIndexLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rebuildIndexLabel.Location = new System.Drawing.Point(189, 111);
            this.rebuildIndexLabel.Name = "rebuildIndexLabel";
            this.rebuildIndexLabel.Size = new System.Drawing.Size(207, 18);
            this.rebuildIndexLabel.TabIndex = 4;
            this.rebuildIndexLabel.Text = "Rebuild the index from scratch";
            // 
            // resetIndexDirectoriesButton
            // 
            this.resetIndexDirectoriesButton.Location = new System.Drawing.Point(6, 33);
            this.resetIndexDirectoriesButton.Name = "resetIndexDirectoriesButton";
            this.resetIndexDirectoriesButton.Size = new System.Drawing.Size(149, 23);
            this.resetIndexDirectoriesButton.TabIndex = 2;
            this.resetIndexDirectoriesButton.Text = "Reset Indexed Directories";
            this.resetIndexDirectoriesButton.UseVisualStyleBackColor = true;
            this.resetIndexDirectoriesButton.Click += new System.EventHandler(this.resetIndexDirectoriesButton_Click);
            // 
            // rebuildIndexButton
            // 
            this.rebuildIndexButton.Location = new System.Drawing.Point(6, 111);
            this.rebuildIndexButton.Name = "rebuildIndexButton";
            this.rebuildIndexButton.Size = new System.Drawing.Size(149, 23);
            this.rebuildIndexButton.TabIndex = 3;
            this.rebuildIndexButton.Text = "Rebuild Snippet Index";
            this.rebuildIndexButton.UseVisualStyleBackColor = true;
            this.rebuildIndexButton.Click += new System.EventHandler(this.rebuildIndexButton_Click);
            // 
            // resetIndexedDirectoriesLabel
            // 
            this.resetIndexedDirectoriesLabel.AutoEllipsis = true;
            this.resetIndexedDirectoriesLabel.AutoSize = true;
            this.resetIndexedDirectoriesLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.resetIndexedDirectoriesLabel.Location = new System.Drawing.Point(189, 38);
            this.resetIndexedDirectoriesLabel.Name = "resetIndexedDirectoriesLabel";
            this.resetIndexedDirectoriesLabel.Size = new System.Drawing.Size(351, 18);
            this.resetIndexedDirectoriesLabel.TabIndex = 1;
            this.resetIndexedDirectoriesLabel.Text = "Clear the list of additional snippet directories to index";
            // 
            // resetSnippetIndexFilePath
            // 
            this.resetSnippetIndexFilePath.Location = new System.Drawing.Point(6, 72);
            this.resetSnippetIndexFilePath.Name = "resetSnippetIndexFilePath";
            this.resetSnippetIndexFilePath.Size = new System.Drawing.Size(149, 23);
            this.resetSnippetIndexFilePath.TabIndex = 6;
            this.resetSnippetIndexFilePath.Text = "Reset Index File Path";
            this.resetSnippetIndexFilePath.UseVisualStyleBackColor = true;
            this.resetSnippetIndexFilePath.Click += new System.EventHandler(this.resetSnippetIndexFilePath_Click);
            // 
            // label1
            // 
            this.label1.AutoEllipsis = true;
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(189, 77);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(231, 18);
            this.label1.TabIndex = 5;
            this.label1.Text = "Reset path to the snippet index file";
            // 
            // ResetOptionsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Name = "ResetOptionsControl";
            this.Size = new System.Drawing.Size(560, 174);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label resetIndexedDirectoriesLabel;
        private System.Windows.Forms.Button resetIndexDirectoriesButton;
        private System.Windows.Forms.Button rebuildIndexButton;
        private System.Windows.Forms.Label rebuildIndexLabel;
        private System.Windows.Forms.Button resetSnippetIndexFilePath;
        private System.Windows.Forms.Label label1;
    }
}
