using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using EnvDTE80;
using Microsoft.SnippetDesigner.OptionPages;
using SnippetDesignerComponents;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Microsoft.SnippetDesigner.SnippetExplorer
{
    /// <summary>
    /// The form for the snippet explorer.
    /// </summary>
    [ComVisible(true)]
    public partial class SnippetExplorerForm : UserControl, ICodeWindowHost
    {
        private SnippetIndex snippetIndex; // class which gets snippet data and how to display
        private string iconCellName = "Icon";
        private string titleCellName = "Title";
        private string descriptionCellName = "Description";
        private string codeLanguageCellName = "Language";
        private string pathCellName = "Path";
        private DTE2 dte2;
        private const int MinResultCount = 25; 

        /// <summary>
        /// Initializes a new instance of the <see cref="SnippetExplorerForm"/> class.
        /// </summary>
        public SnippetExplorerForm()
        {
            InitializeComponent();
            previewCodeWindow.CodeWindowHost = this;

            foreach (var lang in LanguageMaps.LanguageMap.DisplayLanguageToXML.Keys.Where(lang => !string.IsNullOrEmpty(lang)))
            {
                languageFilters.Items.Add(lang);
            }

            showCountComboBox.Items.Add(MinResultCount);
            showCountComboBox.Items.Add(50);
            showCountComboBox.Items.Add(100);
            showCountComboBox.Items.Add(1000);
        }

        /// <summary>
        /// Handles the PropertyChanged event of the Instance control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void SnippetIndexChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != null)
            {
                if (e.PropertyName.Equals("IsIndexLoading", StringComparison.Ordinal) ||
                    e.PropertyName.Equals("IsIndexUpdating", StringComparison.Ordinal)
                    )
                {
                    UpdateStatusLabel();
                }
            }
        }

        /// <summary>
        /// Updates the status label.
        /// </summary>
        private void UpdateStatusLabel()
        {
            if (!snippetIndex.IsIndexLoading)
            {
                Invoke(
                    (MethodInvoker) delegate { statusLabel.Text = ""; });
            }
            else
            {
                Invoke(
                    (MethodInvoker) delegate { statusLabel.Text = "Loading Snippet Index..."; });
            }

            if (!snippetIndex.IsIndexUpdating)
            {
                Invoke(
                    (MethodInvoker) delegate { statusLabel.Text = ""; });
            }
            else
            {
                Invoke(
                    (MethodInvoker) delegate { statusLabel.Text = "Updating Snippet Index..."; });
            }
        }


        /// <summary>
        /// Gets the preview code window.
        /// </summary>
        /// <value>The preview code window.</value>
        public CodeWindow PreviewCodeWindow
        {
            get { return previewCodeWindow; }
        }


        /// <summary>
        /// Service provider for codewindow to see
        /// </summary>
        public IOleServiceProvider ServiceProvider
        {
            get { return (IOleServiceProvider) SnippetDesignerPackage.Instance.GetService(typeof (IOleServiceProvider)); }
        }

        /// <summary>
        /// Part of ICOdeWindowHost interface
        /// let the code window know we  want it to be read only
        /// </summary>
        public bool ReadOnlyCodeWindow
        {
            get { return true; }
        }

        /// <summary>
        /// Set up custom context menu for codewindow
        /// by adding a command filter
        /// </summary>
        public void SetupContextMenus()
        {
            //currently not using
            return;
        }


        /// <summary> 
        /// Let this snippetExplorerForm process the mnemonics.
        /// </summary>
        protected override bool ProcessDialogChar(char charCode)
        {
            // If we're the top-level form or snippetExplorerForm, we need to do the mnemonic handling
            if (charCode != ' ' && ProcessMnemonic(charCode))
            {
                return true;
            }
            return base.ProcessDialogChar(charCode);
        }


        /// <summary>
        /// Handles the Load event of the SnippetExplorerForm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void SnippetExplorerForm_Load(object sender, EventArgs e)
        {
            dte2 = SnippetDesignerPackage.Instance.Dte;
            snippetIndex = SnippetDesignerPackage.Instance.SnippetIndex;

            UpdateStatusLabel();

            snippetIndex.PropertyChanged += SnippetIndexChanged;

            UpdateSelectedItemsFromOptions();
        }

        private void UpdateSelectedItemsFromOptions()
        {
            SnippetDesignerOptions options = SnippetDesignerPackage.Instance.Settings;

            showCountComboBox.SelectedItem = Math.Max(options.SearchResultCount, MinResultCount);

            var languageOptions = new Dictionary<string, bool>
                                      {
                                          {Resources.DisplayNameCSharp, options.HideCSharp},
                                          {Resources.DisplayNameVisualBasic, options.HideVisualBasic},
                                          {Resources.DisplayNameJavaScript, options.HideJavaScript},
                                          {Resources.DisplayNameSQL, options.HideSQL},
                                          {Resources.DisplayNameHTML, options.HideHTML},
                                          {Resources.DisplayNameXML, options.HideXML},
                                          {Resources.DisplayNameXAML, options.HideXAML},
                                          {Resources.DisplayNameCPP, options.HideCPP},
                                      };

            foreach (var pair in languageOptions)
            {
                var index = languageFilters.Items.IndexOf(pair.Key);
                if (index >= 0)
                {
                    languageFilters.SetItemCheckState(index, pair.Value ? CheckState.Unchecked : CheckState.Checked);
                }
            }
        }


        /// <summary>
        /// Perform a search on the datatable based upon the string given
        /// </summary>
        /// <param name="searchString">String to search by</param>
        public void PerformSearch(string searchString)
        {
            IEnumerable<SnippetIndexItem> foundSnippets;
            var langsToDisplay = (from string item in languageFilters.CheckedItems
                                  select LanguageMaps.LanguageMap.DisplayLanguageToXML[item]).ToList();

            //clear the grid view
            searchResultView.Rows.Clear();

            int totalFoundCount = 0;
            var maxCount = (int)showCountComboBox.SelectedItem;
            foundSnippets = snippetIndex.PerformSnippetSearch(searchString, langsToDisplay, maxCount);
            if (foundSnippets.Count() > 0)
            {
                AddItemsToGridView(foundSnippets);
                totalFoundCount += foundSnippets.Count();
            }


            if (totalFoundCount > 0)
            {
                //update the selected row since this first row will be highlighted
                //but wont fire a selection change
                searchResultView_SelectionChanged(searchResultView, null);
            }
        }


        /// <summary>
        /// for each indexitem in the collection add it to the grid view
        /// </summary>
        /// <param name="items"></param>
        public void AddItemsToGridView(IEnumerable<SnippetIndexItem> items)
        {
            if (items == null || items.Count() == 0)
            {
                return;
            }

            foreach (SnippetIndexItem item in items)
            {
                int newRowIndex = searchResultView.Rows.Add();
                DataGridViewRow newRow = searchResultView.Rows[newRowIndex];
                newRow.Cells[titleCellName].Value = item.Title;
                newRow.Cells[codeLanguageCellName].Value = item.Language;
                newRow.Cells[descriptionCellName].Value = item.Description;
                newRow.Cells[pathCellName].Value = item.File;
                newRow.Cells[iconCellName].Value = Resources.localIcon;
                newRow.Tag = item;
            }
        }


        /// <summary>
        /// When a selection of a row changes then updates the additional info section with the correct data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void searchResultView_SelectionChanged(object sender, EventArgs e)
        {
            string codeToShow = "";
            string delimiter = "";
            if (searchResultView.SelectedRows.Count == 1) //if more than once are selected dont show any additional info
            {
                var snippet = searchResultView.SelectedRows[0].Tag as SnippetIndexItem;
                //if more than one selected just take the first one
                if (snippet != null)
                {
                    codeToShow = snippet.Code;
                    delimiter = snippet.Delimiter;
                }
            }

            previewCodeWindow.TextView.Properties[SnippetReplacementTagger.ReplacementDelimiter] = delimiter;
            previewCodeWindow.CodeText = codeToShow;
        }


        /// <summary>
        /// Opens the snippet in designer.
        /// </summary>
        /// <param name="rowIndex">Index of the row.</param>
        private void OpenSnippetInDesigner(int rowIndex)
        {
            if (rowIndex >= 0)
            {
                var item = searchResultView.Rows[rowIndex].Tag as SnippetIndexItem;
                if (item != null && !String.IsNullOrEmpty(item.File) && File.Exists(item.File))
                {
                    string openFileCommand = "File.OpenFile";
                    string quotedFilePath = "\"" + item.File + "\"";
                    dte2.ExecuteCommand(openFileCommand, quotedFilePath);
                }
                else
                {
                    MessageBox.Show("Unable to open Snippet.",
                                    dte2.Application.Name,
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Asterisk);
                }
            }
        }


        /// <summary>
        /// Perform the desired double click action
        /// </summary>
        /// <param name="rowIndex"></param>
        private void OnDoubleClick(int rowIndex)
        {
            OpenSnippetInDesigner(rowIndex);
        }


        /// <summary>
        /// Run delete action on the chosen row
        /// </summary>
        /// <param name="rowIndex">row to be deleted</param>
        private bool DeleteSnippet(int rowIndex)
        {
            bool deleteHappened = false;
            if (searchResultView.SelectedRows.Count > 0)
            {
                DataGridViewRow row = searchResultView.Rows[rowIndex];
                var item = row.Tag as SnippetIndexItem;
                if (item != null)
                {
                    DialogResult result = MessageBox.Show("Are you sure you want to delete this snippet?",
                                                          dte2.Application.Name,
                                                          MessageBoxButtons.YesNo,
                                                          MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        snippetIndex.DeleteSnippetFile(item.File, item.Title);
                        deleteHappened = true;
                    }
                }
            }
            return deleteHappened;
        }

        /// <summary>
        /// When a key is pressed on a row
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void searchResultView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (searchResultView.SelectedRows.Count > 0)
                {
                    OpenSnippetInDesigner(searchResultView.SelectedRows[0].Index);
                }
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Delete)
            {
                e.SuppressKeyPress = true;
                if (searchResultView.SelectedRows.Count > 0)
                {
                    int rowIndex = searchResultView.SelectedRows[0].Index;
                    if (DeleteSnippet(rowIndex))
                    {
                        e.SuppressKeyPress = false;
                    }
                }
            }
        }


        /// <summary>
        /// Handles the MouseDown event of the searchResultView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
        private void searchResultView_MouseDown(object sender, MouseEventArgs e)
        {
            DataGridView.HitTestInfo info = searchResultView.HitTest(e.X, e.Y);
            if (e.Button == MouseButtons.Right)
            {
                if (info.RowIndex >= 0)
                {
                    searchResultView.Rows[info.RowIndex].Selected = true;
                    snippetExplorerContextMenu.Enabled = true;

                    var currentItem = searchResultView.Rows[info.RowIndex].Tag as SnippetIndexItem;
                    if (currentItem != null)
                    {
                    }
                }
                else
                {
                    snippetExplorerContextMenu.Enabled = false;
                }
            }
            else if (e.Button == MouseButtons.Left)
            {
                if (info.RowIndex >= 0)
                {
                    if (e.Clicks > 1) //this is a double click
                    {
                        OnDoubleClick(info.RowIndex);
                    }
                    else
                    {
                        var item = searchResultView.Rows[info.RowIndex].Tag as SnippetIndexItem;
                        if (item != null)
                        {
                            if (item.File != null)
                            {
                                //this will let you drag the item but seems to break the double click command
                                //searchResultView.DoDragDrop(filePath, DragDropEffects.Copy);
                            }
                        }
                    }
                }
            }
        }

        private void searchResultView_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = searchResultView.Rows[e.RowIndex];
                var item = row.Tag as SnippetIndexItem;
                if (item != null)
                {
                    row.Cells[e.ColumnIndex].ToolTipText = item.Description;
                }
            }
        }


        /// <summary>
        /// delete the snippet from the list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (searchResultView.SelectedRows.Count > 0)
            {
                int rowIndex = searchResultView.SelectedRows[0].Index;
                if (DeleteSnippet(rowIndex))
                {
                    searchResultView.Rows.Remove(searchResultView.SelectedRows[0]);
                }
            }
        }


        /// <summary>
        /// open up the snippet in the snipept designer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (searchResultView.SelectedRows.Count > 0)
            {
                int rowIndex = searchResultView.SelectedRows[0].Index;
                OpenSnippetInDesigner(rowIndex);
            }
        }

        /// <summary>
        /// Handles the Click event of the searchButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void searchButton_Click(object sender, EventArgs e)
        {
            PerformSearch(searchBox.Text);
        }

        /// <summary>
        /// Handles the KeyDown event of the searchBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing the event data.</param>
        private void searchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                PerformSearch(searchBox.Text);
            }
        }

        private void languageFilters_SelectedIndexChanged(object sender, EventArgs e)
        {
            SnippetDesignerOptions options = SnippetDesignerPackage.Instance.Settings;
            var languageSetters = new Dictionary<string, Action<bool>>
                                      {
                                          {Resources.DisplayNameCSharp, x => options.HideCSharp = x},
                                          {Resources.DisplayNameVisualBasic, x => options.HideVisualBasic = x},
                                          {Resources.DisplayNameJavaScript, x => options.HideJavaScript = x},
                                          {Resources.DisplayNameSQL, x => options.HideSQL = x},
                                          {Resources.DisplayNameHTML, x => options.HideHTML = x},
                                          {Resources.DisplayNameXML, x => options.HideXML = x},
                                          {Resources.DisplayNameXAML, x => options.HideXAML = x},
                                          {Resources.DisplayNameCPP, x => options.HideCPP = x},
                                      };

            foreach (var pair in languageSetters)
            {
                var index = languageFilters.Items.IndexOf(pair.Key);
                if (index >= 0)
                {
                    pair.Value(!languageFilters.GetItemChecked(index));
                    SnippetDesignerPackage.Instance.Settings.SaveSettingsToStorage();
                }
            }
        }

        private void showCountComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SnippetDesignerPackage.Instance.Settings.SearchResultCount = Math.Max((int)showCountComboBox.SelectedItem, MinResultCount);
            SnippetDesignerPackage.Instance.Settings.SaveSettingsToStorage();
        }
    }
}