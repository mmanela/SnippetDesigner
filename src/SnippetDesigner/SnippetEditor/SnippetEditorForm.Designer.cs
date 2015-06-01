namespace Microsoft.SnippetDesigner
{
    public partial class SnippetEditorForm
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resoursces being used.
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
        /// the contents of this method with the code codeWindowHost.
        /// </summary>
        public void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SnippetEditorForm));
            this.topCommandBar = new System.Windows.Forms.ToolStrip();
            this.snippetsLabel = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSnippetTitles = new System.Windows.Forms.ToolStripComboBox();
            this.languageLabel = new System.Windows.Forms.ToolStripLabel();
            this.toolStripLanguageBox = new System.Windows.Forms.ToolStripComboBox();
            this.shortcutLabel = new System.Windows.Forms.ToolStripLabel();
            this.shortcutTextBox = new System.Windows.Forms.ToolStripTextBox();
            this.codeReplacementsSplitter = new System.Windows.Forms.SplitContainer();
            this.snippetCodeWindow = new Microsoft.SnippetDesigner.CodeWindow();
            this.replacementTable = new System.Windows.Forms.TableLayoutPanel();
            this.replacementGridView = new System.Windows.Forms.DataGridView();
            this.ID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Tooltip = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DefaultsTo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ReplacementKind = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.Type = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Function = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Editable = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.replacementGridMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.removeReplacementToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.replacementLabel = new System.Windows.Forms.Label();
            this.topCommandBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.codeReplacementsSplitter)).BeginInit();
            this.codeReplacementsSplitter.Panel1.SuspendLayout();
            this.codeReplacementsSplitter.Panel2.SuspendLayout();
            this.codeReplacementsSplitter.SuspendLayout();
            this.replacementTable.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.replacementGridView)).BeginInit();
            this.replacementGridMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // topCommandBar
            // 
            this.topCommandBar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.topCommandBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.snippetsLabel,
            this.toolStripSnippetTitles,
            this.languageLabel,
            this.toolStripLanguageBox,
            this.shortcutLabel,
            this.shortcutTextBox});
            resources.ApplyResources(this.topCommandBar, "topCommandBar");
            this.topCommandBar.Name = "topCommandBar";
            this.topCommandBar.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.topCommandBar.Paint += new System.Windows.Forms.PaintEventHandler(this.mainObjectsRepaiont_Paint);
            // 
            // snippetsLabel
            // 
            this.snippetsLabel.Name = "snippetsLabel";
            resources.ApplyResources(this.snippetsLabel, "snippetsLabel");
            this.snippetsLabel.Text = global::Microsoft.SnippetDesigner.Resources.SnippetFormTitlesLabelText;
            // 
            // toolStripSnippetTitles
            // 
            resources.ApplyResources(this.toolStripSnippetTitles, "toolStripSnippetTitles");
            this.toolStripSnippetTitles.Name = "toolStripSnippetTitles";
            this.toolStripSnippetTitles.SelectedIndexChanged += new System.EventHandler(this.toolStripSnippetsTitles_SelectedIndexChanged);
            this.toolStripSnippetTitles.TextUpdate += new System.EventHandler(this.toolStripSnippetTitles_TextUpdate);
            // 
            // languageLabel
            // 
            this.languageLabel.Name = "languageLabel";
            resources.ApplyResources(this.languageLabel, "languageLabel");
            this.languageLabel.Text = global::Microsoft.SnippetDesigner.Resources.SnippetFormLanguageLabelText;
            // 
            // toolStripLanguageBox
            // 
            this.toolStripLanguageBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripLanguageBox.Name = "toolStripLanguageBox";
            resources.ApplyResources(this.toolStripLanguageBox, "toolStripLanguageBox");
            this.toolStripLanguageBox.SelectedIndexChanged += new System.EventHandler(this.languageComboBox_SelectedIndexChanged);
            // 
            // shortcutLabel
            // 
            this.shortcutLabel.Name = "shortcutLabel";
            resources.ApplyResources(this.shortcutLabel, "shortcutLabel");
            // 
            // shortcutTextBox
            // 
            this.shortcutTextBox.Name = "shortcutTextBox";
            resources.ApplyResources(this.shortcutTextBox, "shortcutTextBox");
            this.shortcutTextBox.TextChanged += new System.EventHandler(this.shortcutTextBox_TextChanged);
            // 
            // codeReplacementsSplitter
            // 
            resources.ApplyResources(this.codeReplacementsSplitter, "codeReplacementsSplitter");
            this.codeReplacementsSplitter.Name = "codeReplacementsSplitter";
            // 
            // codeReplacementsSplitter.Panel1
            // 
            this.codeReplacementsSplitter.Panel1.Controls.Add(this.snippetCodeWindow);
            // 
            // codeReplacementsSplitter.Panel2
            // 
            this.codeReplacementsSplitter.Panel2.Controls.Add(this.replacementTable);
            // 
            // snippetCodeWindow
            // 
            resources.ApplyResources(this.snippetCodeWindow, "snippetCodeWindow");
            this.snippetCodeWindow.CodeText = "";
            this.snippetCodeWindow.Name = "snippetCodeWindow";
            this.snippetCodeWindow.Paint += new System.Windows.Forms.PaintEventHandler(this.mainObjectsRepaiont_Paint);
            // 
            // replacementTable
            // 
            resources.ApplyResources(this.replacementTable, "replacementTable");
            this.replacementTable.Controls.Add(this.replacementGridView, 0, 1);
            this.replacementTable.Controls.Add(this.replacementLabel, 0, 0);
            this.replacementTable.Name = "replacementTable";
            // 
            // replacementGridView
            // 
            this.replacementGridView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.replacementGridView.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this.replacementGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.replacementGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ID,
            this.Tooltip,
            this.DefaultsTo,
            this.ReplacementKind,
            this.Type,
            this.Function,
            this.Editable});
            this.replacementGridView.ContextMenuStrip = this.replacementGridMenu;
            resources.ApplyResources(this.replacementGridView, "replacementGridView");
            this.replacementGridView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnKeystroke;
            this.replacementGridView.MultiSelect = false;
            this.replacementGridView.Name = "replacementGridView";
            this.replacementGridView.CellBeginEdit += new System.Windows.Forms.DataGridViewCellCancelEventHandler(this.replacementGridView_CellBeginEdit);
            this.replacementGridView.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.replacementGridView_CellValueChanged);
            this.replacementGridView.RowEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.replacementGridView_RowEnter);
            this.replacementGridView.UserDeletingRow += new System.Windows.Forms.DataGridViewRowCancelEventHandler(this.replacementGridView_RowsRemoved);
            this.replacementGridView.Paint += new System.Windows.Forms.PaintEventHandler(this.mainObjectsRepaiont_Paint);
            this.replacementGridView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.snippetReplacementGrid_MouseDown);
            // 
            // ID
            // 
            this.ID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.ID.HeaderText = global::Microsoft.SnippetDesigner.Resources.SnippetFormColumnHeaderID;
            this.ID.Name = "ID";
            this.ID.ToolTipText = global::Microsoft.SnippetDesigner.Resources.SnippetFormTipID;
            // 
            // Tooltip
            // 
            this.Tooltip.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Tooltip.HeaderText = global::Microsoft.SnippetDesigner.Resources.SnippetFormColumnHeaderTooltip;
            this.Tooltip.Name = "Tooltip";
            this.Tooltip.ToolTipText = global::Microsoft.SnippetDesigner.Resources.SnippetFormTipToolTip;
            // 
            // DefaultsTo
            // 
            this.DefaultsTo.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.DefaultsTo.HeaderText = global::Microsoft.SnippetDesigner.Resources.SnippetFormColumnHeaderDefault;
            this.DefaultsTo.Name = "DefaultsTo";
            this.DefaultsTo.ToolTipText = global::Microsoft.SnippetDesigner.Resources.SnippetFormTipDefault;
            // 
            // ReplacementKind
            // 
            this.ReplacementKind.HeaderText = global::Microsoft.SnippetDesigner.Resources.SnippetFormColumnHeaderReplacementKind;
            this.ReplacementKind.Items.AddRange(new object[] {
            global::Microsoft.SnippetDesigner.Resources.ReplacementLiteralName,
            global::Microsoft.SnippetDesigner.Resources.ReplacementObjectName});
            this.ReplacementKind.Name = "ReplacementKind";
            this.ReplacementKind.ToolTipText = global::Microsoft.SnippetDesigner.Resources.SnippetFormTipReplacementKind;
            // 
            // Type
            // 
            this.Type.HeaderText = global::Microsoft.SnippetDesigner.Resources.SnippetFormColumnHeaderType;
            this.Type.Name = "Type";
            this.Type.ToolTipText = global::Microsoft.SnippetDesigner.Resources.SnippetFormTipReplacementType;
            // 
            // Function
            // 
            this.Function.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Function.HeaderText = global::Microsoft.SnippetDesigner.Resources.SnippetFormColumnHeaderFunction;
            this.Function.Name = "Function";
            this.Function.ToolTipText = global::Microsoft.SnippetDesigner.Resources.SnippetFormTipFunction;
            // 
            // Editable
            // 
            this.Editable.HeaderText = global::Microsoft.SnippetDesigner.Resources.SnippetFormColumnHeaderEditable;
            this.Editable.Name = "Editable";
            this.Editable.ToolTipText = global::Microsoft.SnippetDesigner.Resources.SnippetFormTipEditable;
            // 
            // replacementGridMenu
            // 
            this.replacementGridMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.removeReplacementToolStripMenuItem});
            this.replacementGridMenu.Name = "replacementGridMenu";
            resources.ApplyResources(this.replacementGridMenu, "replacementGridMenu");
            // 
            // removeReplacementToolStripMenuItem
            // 
            this.removeReplacementToolStripMenuItem.Name = "removeReplacementToolStripMenuItem";
            resources.ApplyResources(this.removeReplacementToolStripMenuItem, "removeReplacementToolStripMenuItem");
            this.removeReplacementToolStripMenuItem.Click += new System.EventHandler(this.removeReplacementToolStripMenuItem_Click);
            // 
            // replacementLabel
            // 
            resources.ApplyResources(this.replacementLabel, "replacementLabel");
            this.replacementLabel.Name = "replacementLabel";
            // 
            // SnippetEditorForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.codeReplacementsSplitter);
            this.Controls.Add(this.topCommandBar);
            this.Name = "SnippetEditorForm";
            this.topCommandBar.ResumeLayout(false);
            this.topCommandBar.PerformLayout();
            this.codeReplacementsSplitter.Panel1.ResumeLayout(false);
            this.codeReplacementsSplitter.Panel1.PerformLayout();
            this.codeReplacementsSplitter.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.codeReplacementsSplitter)).EndInit();
            this.codeReplacementsSplitter.ResumeLayout(false);
            this.replacementTable.ResumeLayout(false);
            this.replacementTable.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.replacementGridView)).EndInit();
            this.replacementGridMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }



        #endregion

        internal System.Windows.Forms.ToolStrip topCommandBar;
        internal System.Windows.Forms.ToolStripLabel snippetsLabel;
        internal System.Windows.Forms.Label replacementLabel;
        internal System.Windows.Forms.ToolStripLabel languageLabel;
        internal System.Windows.Forms.ToolStripComboBox toolStripLanguageBox;
        internal CodeWindow snippetCodeWindow;
        private System.Windows.Forms.SplitContainer codeReplacementsSplitter;
        private System.Windows.Forms.TableLayoutPanel replacementTable;
        internal System.Windows.Forms.DataGridView replacementGridView;
        private System.Windows.Forms.ToolStripComboBox toolStripSnippetTitles;
        private System.Windows.Forms.ContextMenuStrip replacementGridMenu;
        private System.Windows.Forms.ToolStripMenuItem removeReplacementToolStripMenuItem;
        private System.Windows.Forms.DataGridViewTextBoxColumn ID;
        private System.Windows.Forms.DataGridViewTextBoxColumn Tooltip;
        private System.Windows.Forms.DataGridViewTextBoxColumn DefaultsTo;
        private System.Windows.Forms.DataGridViewComboBoxColumn ReplacementKind;
        private System.Windows.Forms.DataGridViewTextBoxColumn Type;
        private System.Windows.Forms.DataGridViewTextBoxColumn Function;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Editable;
        private System.Windows.Forms.ToolStripLabel shortcutLabel;
        private System.Windows.Forms.ToolStripTextBox shortcutTextBox;

    }
}
