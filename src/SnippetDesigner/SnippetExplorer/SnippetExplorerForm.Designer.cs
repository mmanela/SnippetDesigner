namespace Microsoft.SnippetDesigner.SnippetExplorer
{
    partial class SnippetExplorerForm
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            if( disposing )
            {
                if(components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );
        }


        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code codeWindowHost.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SnippetExplorerForm));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.snippetExplorerSplitter = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.languageFilters = new System.Windows.Forms.CheckedListBox();
            this.languageLabel = new System.Windows.Forms.Label();
            this.searchOptionBar = new System.Windows.Forms.ToolStrip();
            this.showCountLabel = new System.Windows.Forms.ToolStripLabel();
            this.showCountComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.statusLabel = new System.Windows.Forms.ToolStripLabel();
            this.searchResultView = new System.Windows.Forms.DataGridView();
            this.Icon = new System.Windows.Forms.DataGridViewImageColumn();
            this.Title = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Description = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Language = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Path = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.snippetExplorerContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.panel2 = new System.Windows.Forms.Panel();
            this.searchBox = new System.Windows.Forms.TextBox();
            this.searchButton = new System.Windows.Forms.Button();
            this.previewCodeWindow = new Microsoft.SnippetDesigner.CodeWindow();
            ((System.ComponentModel.ISupportInitialize)(this.snippetExplorerSplitter)).BeginInit();
            this.snippetExplorerSplitter.Panel1.SuspendLayout();
            this.snippetExplorerSplitter.Panel2.SuspendLayout();
            this.snippetExplorerSplitter.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.searchOptionBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.searchResultView)).BeginInit();
            this.snippetExplorerContextMenu.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // snippetExplorerSplitter
            // 
            resources.ApplyResources(this.snippetExplorerSplitter, "snippetExplorerSplitter");
            this.snippetExplorerSplitter.Name = "snippetExplorerSplitter";
            // 
            // snippetExplorerSplitter.Panel1
            // 
            this.snippetExplorerSplitter.Panel1.BackColor = System.Drawing.SystemColors.ControlDark;
            this.snippetExplorerSplitter.Panel1.Controls.Add(this.tableLayoutPanel1);
            // 
            // snippetExplorerSplitter.Panel2
            // 
            this.snippetExplorerSplitter.Panel2.Controls.Add(this.previewCodeWindow);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.searchOptionBar, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.searchResultView, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.panel2, 0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // panel1
            // 
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.languageFilters);
            this.panel1.Controls.Add(this.languageLabel);
            this.panel1.Name = "panel1";
            // 
            // languageFilters
            // 
            this.languageFilters.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.languageFilters.CheckOnClick = true;
            resources.ApplyResources(this.languageFilters, "languageFilters");
            this.languageFilters.FormattingEnabled = true;
            this.languageFilters.MultiColumn = true;
            this.languageFilters.Name = "languageFilters";
            this.languageFilters.SelectedIndexChanged += new System.EventHandler(this.languageFilters_SelectedIndexChanged);
            // 
            // languageLabel
            // 
            resources.ApplyResources(this.languageLabel, "languageLabel");
            this.languageLabel.Name = "languageLabel";
            // 
            // searchOptionBar
            // 
            resources.ApplyResources(this.searchOptionBar, "searchOptionBar");
            this.searchOptionBar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.searchOptionBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showCountLabel,
            this.showCountComboBox,
            this.statusLabel});
            this.searchOptionBar.Name = "searchOptionBar";
            this.searchOptionBar.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.searchOptionBar.Stretch = true;
            // 
            // showCountLabel
            // 
            this.showCountLabel.Name = "showCountLabel";
            resources.ApplyResources(this.showCountLabel, "showCountLabel");
            // 
            // showCountComboBox
            // 
            this.showCountComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.showCountComboBox.Name = "showCountComboBox";
            resources.ApplyResources(this.showCountComboBox, "showCountComboBox");
            this.showCountComboBox.SelectedIndexChanged += new System.EventHandler(this.showCountComboBox_SelectedIndexChanged);
            // 
            // statusLabel
            // 
            resources.ApplyResources(this.statusLabel, "statusLabel");
            this.statusLabel.Name = "statusLabel";
            // 
            // searchResultView
            // 
            this.searchResultView.AllowUserToAddRows = false;
            this.searchResultView.AllowUserToDeleteRows = false;
            this.searchResultView.AllowUserToOrderColumns = true;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.AliceBlue;
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            this.searchResultView.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.searchResultView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.searchResultView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.searchResultView.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.searchResultView.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.searchResultView.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.searchResultView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.searchResultView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Icon,
            this.Title,
            this.Description,
            this.Language,
            this.Path});
            this.searchResultView.ContextMenuStrip = this.snippetExplorerContextMenu;
            resources.ApplyResources(this.searchResultView, "searchResultView");
            this.searchResultView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.searchResultView.GridColor = System.Drawing.Color.White;
            this.searchResultView.MultiSelect = false;
            this.searchResultView.Name = "searchResultView";
            this.searchResultView.ReadOnly = true;
            this.searchResultView.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.searchResultView.RowHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.searchResultView.RowHeadersVisible = false;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.White;
            this.searchResultView.RowsDefaultCellStyle = dataGridViewCellStyle3;
            this.searchResultView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.searchResultView.ShowCellErrors = false;
            this.searchResultView.ShowEditingIcon = false;
            this.searchResultView.ShowRowErrors = false;
            this.searchResultView.CellMouseEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.searchResultView_CellMouseEnter);
            this.searchResultView.SelectionChanged += new System.EventHandler(this.searchResultView_SelectionChanged);
            this.searchResultView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.searchResultView_KeyDown);
            this.searchResultView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.searchResultView_MouseDown);
            // 
            // Icon
            // 
            this.Icon.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            resources.ApplyResources(this.Icon, "Icon");
            this.Icon.Name = "Icon";
            this.Icon.ReadOnly = true;
            this.Icon.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Icon.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // Title
            // 
            this.Title.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.Title.DividerWidth = 1;
            resources.ApplyResources(this.Title, "Title");
            this.Title.Name = "Title";
            this.Title.ReadOnly = true;
            // 
            // Description
            // 
            resources.ApplyResources(this.Description, "Description");
            this.Description.Name = "Description";
            this.Description.ReadOnly = true;
            // 
            // Language
            // 
            this.Language.DividerWidth = 1;
            resources.ApplyResources(this.Language, "Language");
            this.Language.Name = "Language";
            this.Language.ReadOnly = true;
            // 
            // Path
            // 
            this.Path.DividerWidth = 1;
            resources.ApplyResources(this.Path, "Path");
            this.Path.Name = "Path";
            this.Path.ReadOnly = true;
            // 
            // snippetExplorerContextMenu
            // 
            this.snippetExplorerContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.toolStripSeparator1});
            this.snippetExplorerContextMenu.Name = "snippetExplorerContextMenu";
            resources.ApplyResources(this.snippetExplorerContextMenu, "snippetExplorerContextMenu");
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            resources.ApplyResources(this.openToolStripMenuItem, "openToolStripMenuItem");
            this.openToolStripMenuItem.Click += new System.EventHandler(this.editToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            resources.ApplyResources(this.deleteToolStripMenuItem, "deleteToolStripMenuItem");
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // panel2
            // 
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Controls.Add(this.searchBox);
            this.panel2.Controls.Add(this.searchButton);
            this.panel2.Name = "panel2";
            // 
            // searchBox
            // 
            resources.ApplyResources(this.searchBox, "searchBox");
            this.searchBox.Name = "searchBox";
            this.searchBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.searchBox_KeyDown);
            // 
            // searchButton
            // 
            resources.ApplyResources(this.searchButton, "searchButton");
            this.searchButton.Name = "searchButton";
            this.searchButton.UseVisualStyleBackColor = true;
            this.searchButton.Click += new System.EventHandler(this.searchButton_Click);
            // 
            // previewCodeWindow
            // 
            this.previewCodeWindow.CodeText = "";
            resources.ApplyResources(this.previewCodeWindow, "previewCodeWindow");
            this.previewCodeWindow.Name = "previewCodeWindow";
            // 
            // SnippetExplorerForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.Controls.Add(this.snippetExplorerSplitter);
            this.Name = "SnippetExplorerForm";
            this.Load += new System.EventHandler(this.SnippetExplorerForm_Load);
            this.snippetExplorerSplitter.Panel1.ResumeLayout(false);
            this.snippetExplorerSplitter.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.snippetExplorerSplitter)).EndInit();
            this.snippetExplorerSplitter.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.searchOptionBar.ResumeLayout(false);
            this.searchOptionBar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.searchResultView)).EndInit();
            this.snippetExplorerContextMenu.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

        }
        #endregion

        private System.Windows.Forms.SplitContainer snippetExplorerSplitter;
        private CodeWindow previewCodeWindow;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.DataGridView searchResultView;
        private System.Windows.Forms.ToolStrip searchOptionBar;
        private System.Windows.Forms.ToolStripLabel statusLabel;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label languageLabel;
        private System.Windows.Forms.ContextMenuStrip snippetExplorerContextMenu;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TextBox searchBox;
        private System.Windows.Forms.Button searchButton;
        private System.Windows.Forms.DataGridViewImageColumn Icon;
        private System.Windows.Forms.DataGridViewTextBoxColumn Title;
        private System.Windows.Forms.DataGridViewTextBoxColumn Description;
        private System.Windows.Forms.DataGridViewTextBoxColumn Language;
        private System.Windows.Forms.DataGridViewTextBoxColumn Path;
        private System.Windows.Forms.CheckedListBox languageFilters;
        private System.Windows.Forms.ToolStripLabel showCountLabel;
        private System.Windows.Forms.ToolStripComboBox showCountComboBox;

    }
}
