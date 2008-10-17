// Copyright (C) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Microsoft.SnippetLibrary;
using System.Globalization;
using Microsoft.VisualStudio.TextManager.Interop;
using System.IO;
using System.Text.RegularExpressions;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.Win32;

using IServiceProvider = System.IServiceProvider;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using Microsoft.RegistryTools;
using System.Diagnostics;

namespace Microsoft.SnippetDesigner
{
    /// <summary>
    /// The form which represents the GUI of the snippet editor.
    /// It implements ISnippetEditor which defines what properties and functions a snippet editor
    /// must use.
    /// </summary>
    [ComVisible(true)]
    public partial class SnippetEditorForm :
        UserControl,
        ISnippetEditor
    {

        //Snippy Library Access Code
        private SnippetFile snippetFile; //represents an instance of this snippet application
        private int snippetIndex; // index of the snippet in the snippetFile
        private Snippet activeSnippet; //represents the current snippet in memory

        //hash which maps the dispaly names of the languages to the path to their user snippet directory
        internal Dictionary<string, string> snippetDirectories = new Dictionary<string, string>();

        //hash of the header field to its data
        private Dictionary<string, DataGridViewCell> headerFields = new Dictionary<string, DataGridViewCell>();

        private bool isFormDirty = false;//is this form in a dirty state

        //the value of the id cell you entered before edit
        //the purpose of this is so if you modify a replcement id in the gridview we can know which ids in the codewindow
        //to update
        private string previousIDValue;


        //this is the id column value of the row you have currently selected
        //this value is use so that we know which ids to give the different higlighting
        private string currentlySelectedId;


        //store the last language selected so we know when to remove adornments
        private string previousLanguageSelected = String.Empty;


        //header snippet data
        private string snippetTitle = String.Empty;
        private string snippetDescription = String.Empty;
        private string snippetAuthor = String.Empty;
        private string snippetShortcut = String.Empty;
        private string snippetHelpUrl = String.Empty;
        private string snippetKind = String.Empty;
        private List<string> snippetKeywords = new List<string>();
        private List<string> snippetImports = new List<string>();
        private List<string> snippetReferences = new List<string>();
        private List<SnippetType> snippetTypes = new List<SnippetType>();
        private Regex validReplacement;




        /// <summary>
        /// the snippet application which represents all the snippets in the file
        /// </summary>
        public SnippetFile SnippetFile
        {
            get
            {
                return snippetFile;
            }

        }

        /// <summary>
        /// the current snippet we are working with in the snippet file
        /// </summary>
        public Snippet ActiveSnippet
        {
            get
            {
                return activeSnippet;
            }

            set
            {
                activeSnippet = value;
            }

        }

        /// <summary>
        /// Is this form in a diry state
        /// </summary>
        public bool IsFormDirty
        {
            get
            {
                return isFormDirty;
            }
            set
            {
                isFormDirty = value;
            }
        }

        /// <summary>
        /// This is the id column value of the row you have currently selected
        /// this is used for showing different markers for the active replcement
        /// </summary>
        public string CurrentlySelectedId
        {
            get
            {
                return currentlySelectedId;
            }
        }

        #region Properties which interact with fields in the gui

        /// <summary>
        /// File name of the snippet
        /// </summary>
        public string SnippetFileName
        {
            get
            {
                return snippetFile.FileName;
            }
        }
        /// <summary>
        /// Get the list of snippet titles form the codeWindowHost
        /// Set the list of items in the codeWindowHost
        /// </summary>
        public List<string> SnippetTitles
        {

            get
            {
                string[] titleArray = new string[this.toolStripSnippetTitles.Items.Count];
                this.toolStripSnippetTitles.Items.CopyTo(titleArray, 0);
                return new List<string>(titleArray);
            }
            set
            {
                this.toolStripSnippetTitles.Items.Clear();
                foreach (string title in value)
                {
                    this.toolStripSnippetTitles.Items.Add(title);
                }
                this.toolStripSnippetTitles.SelectedIndex = this.toolStripSnippetTitles.Items.IndexOf(activeSnippet.Title);
            }

        }


        /// <summary>
        /// The active snippet title
        /// </summary>
        public string SnippetTitle
        {
            get
            {
                return snippetTitle;
            }

            set
            {
                snippetTitle = value;

            }
        }



        public string SnippetDescription
        {
            get
            {
                return snippetDescription;
            }

            set
            {
                if (snippetDescription != value)
                {
                    isFormDirty = true;
                }
                snippetDescription = value;
            }
        }

        public string SnippetAuthor
        {
            get
            {
                return snippetAuthor;
            }

            set
            {
                if (snippetAuthor != value)
                {
                    isFormDirty = true;
                }
                snippetAuthor = value;

            }
        }

        public string SnippetHelpUrl
        {
            get
            {
                return snippetHelpUrl;
            }

            set
            {
                if (snippetHelpUrl != value)
                {
                    isFormDirty = true;
                }
                snippetHelpUrl = value;
            }
        }

        public string SnippetShortcut
        {
            get
            {
                return snippetShortcut;
            }

            set
            {
                if (snippetShortcut != value)
                {
                    isFormDirty = true;
                }
                snippetShortcut = value;
            }
        }

        public List<string> SnippetKeywords
        {
            get
            {
                return snippetKeywords;
            }

            set
            {

                //TODO: check if the keywords have changed
                isFormDirty = true;


                snippetKeywords.Clear();
                snippetKeywords.AddRange(value);
            }
        }

        public string SnippetCode
        {
            get
            {
                return snippetCodeWindow.CodeText;
            }

            set
            {
                snippetCodeWindow.CodeText = value;
            }
        }



        public List<SnippetType> SnippetTypes
        {
            get
            {
                return snippetTypes;
            }

            set
            {
                //TODO: check if the snippettype have changed
                isFormDirty = true;


                snippetTypes.Clear();
                snippetTypes.AddRange(value);
            }

        }

        public string SnippetKind
        {
            get
            {
                return snippetKind;
            }

            set
            {
                snippetKind = value;
            }
        }


        /// <summary>
        /// Get: Converts the snippet language from display to xml form and returns it
        /// Set: Take a string snippet language in xml form and ocnverts to display form then adds it to GUI
        /// </summary>
        public string SnippetLanguage
        {
            get
            {
                if (this.toolStripLanguageBox.SelectedIndex > -1)
                {
                    string langString = this.toolStripLanguageBox.SelectedItem.ToString();
                    if (LanguageMaps.LanguageMap.DisplayLanguageToXML.ContainsKey(langString))
                    {
                        return LanguageMaps.LanguageMap.DisplayLanguageToXML[langString];
                    }
                    else
                    {
                        return String.Empty;
                    }
                }
                else
                {
                    return String.Empty;
                }
            }

            set
            {
                string language = String.Empty;
                if (!String.IsNullOrEmpty(value) && LanguageMaps.LanguageMap.SnippetSchemaLanguageToDisplay.ContainsKey(value.ToLower()))
                {
                    language = LanguageMaps.LanguageMap.SnippetSchemaLanguageToDisplay[value.ToLower()];
                }
                else
                {
                    language = LanguageMaps.LanguageMap.ToDisplayForm(SnippetDesignerPackage.Instance.Settings.DefaultLanguage);
                }

                int index = this.toolStripLanguageBox.Items.IndexOf(language);
                if (index >= 0)
                {
                    this.toolStripLanguageBox.SelectedIndex = index;
                }
                else
                {
                    this.toolStripLanguageBox.SelectedIndex = 0;//select first
                }
            }
        }

        public List<string> SnippetImports
        {
            get
            {
                return snippetImports;
            }

            set
            {
                //This wont be called from the editor properties window 
                //so we need to figure out a different way to tell if it is in a dirty state

                snippetImports.Clear();
                snippetImports.AddRange(value);
            }
        }

        public List<string> SnippetReferences
        {
            get
            {
                return snippetReferences;
            }

            set
            {
                //This wont be called from the editor properties window 
                //so we need to figure out a different way to tell if it is in a dirty state

                snippetReferences.Clear();
                snippetReferences.AddRange(value);
            }

        }

        public List<Literal> SnippetReplacements
        {
            get
            {
                List<Literal> replacements = new List<Literal>();
                foreach (DataGridViewRow row in this.replacementGridView.Rows)
                {
                    if (row.IsNewRow) continue;
                    string currId = (string)row.Cells[ConstantStrings.ColumnID].EditedFormattedValue;
                    currId = currId.Trim();
                    if (String.IsNullOrEmpty(currId)) continue;

                    bool isObj = false;
                    if ((((DataGridViewComboBoxCell)row.Cells[ConstantStrings.ColumnReplacementKind]).EditedFormattedValue as string).ToLower() == Resources.ReplacementObjectName.ToLower())
                    {
                        isObj = true;
                    }

                    bool isEditable = (bool)((DataGridViewCheckBoxCell)row.Cells[ConstantStrings.ColumnEditable]).EditedFormattedValue;
                    replacements.Add(new Literal((string)row.Cells[ConstantStrings.ColumnID].EditedFormattedValue,
                        (string)row.Cells[ConstantStrings.ColumnTooltip].EditedFormattedValue,
                        (string)row.Cells[ConstantStrings.ColumnDefault].EditedFormattedValue,
                        (string)row.Cells[ConstantStrings.ColumnFunction].EditedFormattedValue,
                        isObj,
                        isEditable,
                        (string)row.Cells[ConstantStrings.ColumnType].EditedFormattedValue
                        ));


                }
                return replacements;
            }

            set
            {
                //literals and objects
                this.replacementGridView.Rows.Clear();
                foreach (Literal literal in value)
                {
                    string objOrLiteral = Resources.ReplacementLiteralName;
                    if (literal.Object == true)
                    {
                        objOrLiteral = Resources.ReplacementObjectName;
                    }

                    object[] row = { literal.ID, literal.ToolTip, literal.DefaultValue, objOrLiteral, literal.Type, literal.Function, literal.Editable };
                    int rowIndex = this.replacementGridView.Rows.Add(row);
                    if (!literal.Object)
                    {
                        SetOrDisableTypeField(false, rowIndex);
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Get the codewindow snippetExplorerForm
        /// </summary>
        public CodeWindow CodeWindow
        {
            get
            {
                return snippetCodeWindow;
            }

        }


        /// <summary>
        /// Initialize the hashes and values in the form
        /// </summary>
        public SnippetEditorForm()
        {
            snippetDirectories = SnippetDirectories.Instance.UserSnippetDirectories;

            validReplacement = new Regex(ConstantStrings.ValidReplacementString, RegexOptions.Compiled);
        }


        #region Snippet Save and Load Methods

        /// <summary>
        /// save snippet and update the snippets in memory
        /// </summary>
        /// <returns></returns>
        public bool SaveSnippet()
        {
            UpdateSnippetInMemory();
            if (SnippetDesignerPackage.Instance != null)
            {
                SnippetDesignerPackage.Instance.SnippetIndex.UpdateSnippetFile(snippetFile);
            }
            snippetFile.Save();
            return true;
        }

        /// <summary>
        /// Save snippet as
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public bool SaveSnippetAs(string fileName)
        {
            UpdateSnippetInMemory();
            foreach (Snippet snippetItem in snippetFile.Snippets)
            {
                if (SnippetDesignerPackage.Instance != null)
                {
                    SnippetDesignerPackage.Instance.SnippetIndex.CreateIndexItemDataFromSnippet(snippetItem, fileName);
                }
            }
            snippetFile.SaveAs(fileName);
            return true;
        }

        /// <summary>
        /// Load the snippet
        /// 
        /// throws IOExcpetion if load failure
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public bool LoadSnippet(string fileName)
        {
            try
            {
                //load the snipept into memory
                snippetFile = new SnippetFile(fileName);

                snippetIndex = 0;

                //set this snippet as the active snippet
                activeSnippet = snippetFile.Snippets[snippetIndex]; ;
                //populate the gui with this snippets information
                PopulateFieldsFromActiveSnippet();
                //indicate that this snippet is not dirty
                isFormDirty = false;
            }
            catch (IOException) //abort loading snippet, fail program
            { //since an io error occured
                throw;
            }

            return true;
        }

        #endregion

        /// <summary>
        /// Takes data from in memory snippet file and populates the gui form
        /// </summary>
        public void PopulateFieldsFromActiveSnippet()
        {

            //snippet information
            this.SnippetTitle = activeSnippet.Title;
            this.SnippetAuthor = activeSnippet.Author;
            this.SnippetDescription = activeSnippet.Description;
            this.SnippetHelpUrl = activeSnippet.HelpUrl;
            this.SnippetShortcut = activeSnippet.Shortcut;
            this.SnippetKeywords = activeSnippet.Keywords;


            this.SnippetTitles = GetSnippetTitles();

            if (activeSnippet.SnippetTypes.Count <= 0)
            { //if no type specified then make it expansion by default
                snippetTypes.Add(new SnippetType(ConstantStrings.SnippetTypeExpansion));
            }
            else
            {
                this.SnippetTypes = activeSnippet.SnippetTypes;
            }



            //code - for some unknown reason this must be done before language is set to stop some inconsitency
            //including highlighting and color coding 
            this.SnippetCode = activeSnippet.Code;

            //kind and language values
            this.SnippetKind = activeSnippet.CodeKindAttribute;

            this.SnippetLanguage = activeSnippet.CodeLanguageAttribute;

            //imports and references
            this.SnippetImports = activeSnippet.Imports;

            this.SnippetReferences = activeSnippet.References;

            //literals and objects
            this.SnippetReplacements = activeSnippet.Literals;


        }//end PopulateFieldsFromSnippet


        /// <summary>
        /// Takes the data from the form and adds it to the in memory xml document
        /// </summary>
        public void UpdateSnippetInMemory()
        {

            //add header info
            activeSnippet.Title = this.SnippetTitle;
            activeSnippet.Author = this.SnippetAuthor;
            activeSnippet.Description = this.SnippetDescription;
            activeSnippet.HelpUrl = this.SnippetHelpUrl;
            activeSnippet.Shortcut = this.SnippetShortcut;

            //update keywords
            activeSnippet.Keywords = this.SnippetKeywords;


            //add snippet types
            activeSnippet.SnippetTypes = this.SnippetTypes;


            //add code
            activeSnippet.Code = this.SnippetCode;


            //must be after code node is declared
            //kind and language values
            activeSnippet.CodeKindAttribute = this.SnippetKind;


            activeSnippet.CodeLanguageAttribute = this.SnippetLanguage;


            //imports and references
            activeSnippet.Imports = this.SnippetImports;

            activeSnippet.References = this.SnippetReferences;

            //literals and objects
            activeSnippet.Literals = this.SnippetReplacements;

        } //end UpdateSnippetInMemory



        /// <summary>
        /// Gets the snippet titles.
        /// </summary>
        /// <returns></returns>
        private List<String> GetSnippetTitles()
        {
            List<string> snippetTitles = new List<string>();
            foreach (Snippet s in snippetFile.Snippets)
            {
                snippetTitles.Add(s.Title);
            }
            return snippetTitles;
        }

        /// <summary>
        /// Change language service when change in langauge combo box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void languageComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            ToolStripComboBox langCombo = sender as ToolStripComboBox;
            if (langCombo != null)
            {
                string languageText = langCombo.SelectedItem.ToString();
                if (previousLanguageSelected != languageText) //make sure this is actually a change
                {
                    if (!this.snippetCodeWindow.LangServices.ContainsKey(languageText))
                    {
                        languageText = String.Empty;
                    }

                    this.snippetCodeWindow.SetLanguageService(languageText);
                    isFormDirty = true;

                    if (languageText == Resources.DisplayNameXML)
                    {
                        //The XML Editor defines its own properties window and by removing adornments it will stop it
                        // from showing and allow ours to show
                        IOleServiceProvider sp = this.snippetCodeWindow.VsCodeWindow as IOleServiceProvider;
                        if (sp != null)
                        {
                            ServiceProvider site = new ServiceProvider(sp);
                            IVsCodeWindowManager cMan = site.GetService(typeof(SVsCodeWindowManager)) as IVsCodeWindowManager;
                            if (cMan != null)
                            {
                                cMan.RemoveAdornments();
                            }
                        }

                    }

                    //store the last language
                    previousLanguageSelected = languageText;

                    //refresh the properties window
                    SnippetEditor sEditor = (this as SnippetEditor);
                    if (sEditor != null)
                    {
                        sEditor.RefreshPropertiesWindow();
                    }
                }


            }
        }

        /// <summary>
        /// New snippet has been chosen so updated form
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void toolStripSnippetsTitles_SelectedIndexChanged(object sender, EventArgs e)
        {
            ToolStripComboBox snippetsBox = sender as ToolStripComboBox;
            if (snippetsBox != null)
            {
                string newTitle = snippetsBox.SelectedItem as string;
                if (!String.IsNullOrEmpty(newTitle) && newTitle != activeSnippet.Title)
                {
                    UpdateSnippetInMemory();

                    //foreach (Snippet sn in snippetFile.Snippets)
                    for (int i = 0; i < snippetFile.Snippets.Count; i++)
                    {
                        if (snippetFile.Snippets[i].Title.Equals(newTitle, StringComparison.InvariantCulture))
                        {
                            snippetIndex = i;
                            activeSnippet = snippetFile.Snippets[i];
                        }
                    }

                    PopulateFieldsFromActiveSnippet();

                    //clear and show all markers
                    ClearAllMarkers(false);
                    UpdateReplacementMarkers(false);

                    //not the best way to do this but since I dont know if we want to move the change current snippet to the 
                    // porperties window this will have to do for now
                    // I am assuming this object is actually an instance of snippeteditor
                    SnippetEditor theEditor = this as SnippetEditor;
                    if (theEditor != null)
                    {
                        theEditor.RefreshPropertiesWindow();
                    }
                }
            }
        }


        /// <summary>
        /// Handles the TextUpdate event of the toolStripSnippetTitles control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void toolStripSnippetTitles_TextUpdate(object sender, EventArgs e)
        {
            ToolStripComboBox snippetsBox = sender as ToolStripComboBox;
            Debug.WriteLine("Text Update " + snippetsBox.Text);

            if (snippetsBox != null)
            {
                string newTitle = snippetsBox.Text;
                if (!String.IsNullOrEmpty(newTitle) && newTitle != snippetTitle)
                {
                    snippetsBox.Items.Remove(snippetTitle);
                    snippetTitle = newTitle;
                    UpdateSnippetInMemory();
                    isFormDirty = true;
                }

            }
        }

        /// <summary>
        /// Set or disable the type field since its only valid for a replacement that is an object
        /// </summary>
        /// <param name="isObject"></param>
        /// <param name="rowIndex"></param>
        private void SetOrDisableTypeField(bool isObject, int rowIndex)
        {
            if (isObject) //if this is an object than enable the type field
            {

                replacementGridView.Rows[rowIndex].Cells[ConstantStrings.ColumnType].Value = String.Empty;
                replacementGridView.Rows[rowIndex].Cells[ConstantStrings.ColumnType].ReadOnly = false;
            }
            else //this is not a object so disable type field
            {

                replacementGridView.Rows[rowIndex].Cells[ConstantStrings.ColumnType].Value = Resources.TypeInvalidForLiteralSymbol;
                replacementGridView.Rows[rowIndex].Cells[ConstantStrings.ColumnType].ReadOnly = true;
            }


        }

        //these are event handler functions that help synchronize replacement grid view events
        //with the replacements in the codewindow
        #region Replacement Grid Events

        /// <summary>
        /// When a cell begins being edited see if it is an id cell and store its value for use later
        /// when the person commits a change in the cell
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void replacementGridView_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            DataGridView grid = sender as DataGridView;
            if (grid != null)
            {


                if (grid.Columns[e.ColumnIndex].Name == ConstantStrings.ColumnID)
                {
                    previousIDValue = (string)grid.Rows[e.RowIndex].Cells[e.ColumnIndex].EditedFormattedValue;
                    return;
                }
            }
            previousIDValue = null;
        }

        /// <summary>
        /// when the user enter a row give foucs to its id repalcements
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void replacementGridView_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView grid = sender as DataGridView;
            if (grid != null)
            {
                currentlySelectedId = grid.Rows[e.RowIndex].Cells[ConstantStrings.ColumnID].Value as string;
                ClearAllMarkers(false);
                UpdateReplacementMarkers(false);
            }
        }


        /// <summary>
        /// When the id value in the replacement grid is changed update the highlighting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void replacementGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {

            DataGridView grid = sender as DataGridView;
            if (grid != null)
            {
                isFormDirty = true;
                if (grid.Columns[e.ColumnIndex].Name == ConstantStrings.ColumnID)
                {
                    string newIdValue = (string)grid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                    if (newIdValue == null)
                    {   //if null make it empty
                        newIdValue = String.Empty;
                    }

                    //make sure a change is being made
                    if (previousIDValue != null && newIdValue != previousIDValue)
                    {
                        //check if the change you made is valid if not tell the user and return to previous value
                        if (validReplacement.IsMatch(newIdValue))
                        {
                            //build new replacement text
                            string newReplacement = TurnTextIntoReplacementSymbol(newIdValue);
                            string oldReplacement = TurnTextIntoReplacementSymbol(previousIDValue);


                            //replace all occurances of the oldReplacement with newReplacement
                            //set false so it allows us to overdie existing replacements
                            this.CodeWindow.ReplaceAll(oldReplacement, newReplacement, false);

                            //add any existing instances of this vairable as new replacement
                            ReplacementMake(newIdValue);

                            //a update was made so refresh markers
                            ClearAllMarkers(false);
                            UpdateReplacementMarkers(false);
                            isFormDirty = true;//form is now dirty
                        }
                        else
                        {

                            string message = String.Format(Resources.ErrorInvalidReplacementID, newIdValue);
                            string title = String.Empty;
                            SnippetDesignerPackage.Instance.DisplayOKMessageBox(title, message, OLEMSGICON.OLEMSGICON_WARNING);
                            //set id cell back to the old value
                            grid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = previousIDValue;
                        }
                    }


                }
                else if (grid.Columns[e.ColumnIndex].Name == ConstantStrings.ColumnReplacementKind)
                {
                    if ((string)grid.Rows[e.RowIndex].Cells[e.ColumnIndex].EditedFormattedValue == Resources.ReplacementLiteralName)
                    {
                        SetOrDisableTypeField(false, e.RowIndex);
                    }
                    else
                    {
                        SetOrDisableTypeField(true, e.RowIndex);
                    }

                }

            }
        }



        /// <summary>
        /// When a row is deleted remove all replcamement symbols around the ID removed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void replacementGridView_RowsRemoved(object sender, DataGridViewRowCancelEventArgs e)
        {
            UpdateMarkersAfterDeletedGridViewRow(e.Row);
        }

        /// <summary>
        /// Delete the selected row
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void removeReplacementToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataGridViewRow rowToDelete = null;
            //if there is only one row then jsut clear it
            if (this.replacementGridView.Rows.Count == 1)
            {
                this.replacementGridView.Rows[0].Cells.Clear();
            }
            //if a row has a cell selected the remove that row from the datagrid
            else if (this.replacementGridView.SelectedCells.Count > 0)
            {
                rowToDelete = this.replacementGridView.Rows[this.replacementGridView.SelectedCells[0].RowIndex];
            }
            else
            {
                //nothing to delete
                return;
            }
            //update markers and remove the row
            UpdateMarkersAfterDeletedGridViewRow(rowToDelete);
            this.replacementGridView.Rows.Remove(rowToDelete);
        }


        #endregion



        //These are functions that deal with updated and mofiying the replacments in the code window
        // such as removing, make and making a replacement active
        #region Replacement Functions



        /// <summary>
        /// Given text it returns the text with the replacement symbols surrounding it
        /// for example if you input MATT into this function it will retur $MATT$ if $ is your replacement symbol
        /// </summary>
        /// <param name="text">text to turn into replacement symbol</param>
        /// <returns></returns>
        private string TurnTextIntoReplacementSymbol(string text)
        {
            return ConstantStrings.SymbolReplacement + text + ConstantStrings.SymbolReplacement;
        }


        /// <summary>
        /// Mark All replacements
        /// </summary>
        /// <param name="currentLineOnly">Whether or not to update the current lines markers only</param>
        public void UpdateReplacementMarkers(bool currentLineOnly)
        {
            List<string> allReplacements = new List<string>();
            foreach (DataGridViewRow row in replacementGridView.Rows)
            {
                string idValue = ((string)row.Cells[ConstantStrings.ColumnID].EditedFormattedValue).Trim();
                if (idValue.Length > 0)
                {
                    allReplacements.Add(idValue);

                }

            }
            //search through the code window and update all replcement highlight martkers
            CodeWindow.MarkReplacements(allReplacements, currentLineOnly);
        }




        /// <summary>
        /// Clear every replacement marker
        /// </summary>
        /// <param name="currentLineOnly">Whether to only clear the current lines markers</param>
        public void ClearAllMarkers(bool currentLineOnly)
        {
            //clear all yellow markers
            ClearMarkersOfType(GuidList.yellowMarker, currentLineOnly);
            //clear all yellow markers with borders
            ClearMarkersOfType(GuidList.yellowMarkerWithBorder, currentLineOnly);
        }

        /// <summary>
        /// Clear every replacement marker of a given type
        /// </summary>
        /// <param name="markerGuid">The guid of the type of the marker you want to clear</param>
        /// <param name="currentLineOnly">Whether to only clear the current lines markers</param>
        public void ClearMarkersOfType(Guid markerGuid, bool currentLineOnly)
        {
            if (SnippetDesignerPackage.Instance == null)
            {
                return;
            }
            int lineToClear = -1;
            if (currentLineOnly)
            {
                int col;
                this.CodeWindow.TextView.GetCaretPos(out lineToClear, out col);
            }
            int lastLine = CodeWindow.LineCount - 1;


            int markerTypeID;

            //get text manager service
            IVsTextManager textManager = (IVsTextManager)SnippetDesignerPackage.Instance.GetService(typeof(SVsTextManager));
            //get marker ID of the yellow marker
            textManager.GetRegisteredMarkerTypeID(ref markerGuid, out markerTypeID);

            IVsEnumLineMarkers markerEnum;
            //get enum of all yellow markers
            CodeWindow.TextLines.EnumMarkers(0, 0, lastLine, CodeWindow.LineLength(lastLine), markerTypeID, 0, out markerEnum);
            int markerCount = 0;
            markerEnum.GetCount(out markerCount);
            IVsTextLineMarker marker;
            //loop over each marker
            for (int i = 0; i < markerCount; i++)
            {
                markerEnum.Next(out marker);
                TextSpan[] span = new TextSpan[1];
                marker.GetCurrentSpan(span);
                //if this is the right line to clear or if we are clearing all lines
                if (span[0].iStartLine == lineToClear || !currentLineOnly)
                {
                    //clear and tell the code window to stop keeping track of this marker
                    marker.Invalidate();
                    marker.UnadviseClient();
                }
            }
        }



        /// <summary>
        ///  Determine if the location the user clicked it in a replcement and if so
        /// make that the active replacement and highlight that replcement in the replcementGrid
        /// </summary>
        public void MakeClickedReplacementActive()
        {

            TextSpan currentWordSpan;
            //see if the person clicked inside of a replacement and return its span
            if (this.CodeWindow.GetClickedOnReplacementSpan(out currentWordSpan))
            {
                string currentWord = this.CodeWindow.GetSpanText(currentWordSpan);

                foreach (DataGridViewRow row in replacementGridView.Rows)
                {
                    if ((string)row.Cells[ConstantStrings.ColumnID].Value == currentWord)
                    {
                        replacementGridView.ClearSelection();
                        row.Selected = true;
                        currentlySelectedId = row.Cells[ConstantStrings.ColumnID].Value as string;
                        ClearAllMarkers(false);
                        UpdateReplacementMarkers(false);
                        break;
                    }
                }
            }



        }

        /// <summary>
        /// Take the textToFind at the current cursor position and and turn all insatnces into non replacements
        /// </summary>
        /// <param name="textToChange">text to stop from being a replcement</param>
        public void ReplacementRemove()
        {
            TextSpan currentWordSpan;
            if (this.CodeWindow.GetClickedOnReplacementSpan(out currentWordSpan))
            {
                string currentWord = this.CodeWindow.GetSpanText(currentWordSpan);
                ReplacementRemove(currentWord, currentWordSpan);
            }
        }



        /// <summary>
        /// Find all replacements with the given ID and turn them into non replacements
        /// </summary>
        /// <param name="textToChange">text to stop from being a replcement</param>
        /// <param name="replaceSpan">the span this replacement ovccupies</param>
        public void ReplacementRemove(string textToChange, TextSpan replaceSpan)
        {

            DataGridViewRow rowToDelete = null;
            foreach (DataGridViewRow row in replacementGridView.Rows)
            {
                if ((string)row.Cells[ConstantStrings.ColumnID].Value == textToChange)
                {
                    rowToDelete = row;
                    break;
                }
            }

            if (rowToDelete != null)
            {
                UpdateMarkersAfterDeletedGridViewRow(rowToDelete);
                replacementGridView.Rows.Remove(rowToDelete);
            }
        }



        /// <summary>
        /// Take the textToFind at the current cursor position and make into a replacement
        /// 
        /// Step one check if the user higlihgted something
        /// if not
        /// step two the word ar current cursor position
        /// </summary>
        public void ReplacementMake()
        {
            string selectedText = String.Empty;
            if (this.CodeWindow.SelectionLength != 0)
            {
                //trim any replacement symbols or spaces
                selectedText = this.CodeWindow.SelectedText.Trim(ConstantStrings.SymbolReplacement[0], ' ');
            }
            else
            {
                selectedText = this.CodeWindow.GetWordFromCurrentPosition();
            }
            //make replacement with the desired text
            ReplacementMake(selectedText);
        }

        /// <summary>
        /// Take the textToFind at the current cursor position and make into a replacement
        /// </summary>
        /// <param name="textToChange">The text that we want to make into a replacement</param>
        /// 
        public void ReplacementMake(string textToChange)
        {

            //if invalid text to make into a replacement return
            if (!validReplacement.IsMatch(textToChange))
            {
                //not a valid replacement
                return;
            }

            //build new replacement text
            string newText = TurnTextIntoReplacementSymbol(textToChange);



            //replace all occurances of the textToFind with $textToFind$
            int numFoundAndReplaced = this.CodeWindow.ReplaceAll(textToChange, newText, true);

            if (numFoundAndReplaced > 0)
            {
                //check if replacement exists already
                foreach (DataGridViewRow row in this.replacementGridView.Rows)
                {
                    if ((string)row.Cells[ConstantStrings.ColumnID].EditedFormattedValue == textToChange || textToChange.Trim() == String.Empty)
                    {
                        //this replacement already exists or is nothing don't add it to the replacement list
                        return;
                    }

                }

                object[] newRow = { textToChange, textToChange, textToChange, Resources.ReplacementLiteralName, String.Empty, String.Empty, true };
                int rowIndex = this.replacementGridView.Rows.Add(newRow);
                SetOrDisableTypeField(false, rowIndex);
            }

            UpdateReplacementMarkers(false);//refresh all replacements
        }


        /// <summary>
        /// Given a row that has been deleted update the code window
        /// </summary>
        /// <param name="deletedRow">row that is going ot be delted</param>
        private void UpdateMarkersAfterDeletedGridViewRow(DataGridViewRow deletedRow)
        {
            if (deletedRow != null)
            {
                string deletedID = deletedRow.Cells[ConstantStrings.ColumnID].EditedFormattedValue as string;
                if (deletedID != null)
                {
                    //build new replacement text 
                    string currentText = TurnTextIntoReplacementSymbol(deletedID);
                    this.CodeWindow.ReplaceAll(currentText, deletedID, false);
                    ClearAllMarkers(false);
                    UpdateReplacementMarkers(false);
                }
            }

        }

        #endregion

        /// <summary>
        /// When any object is repainted make sure that the global snippet language
        /// and snippet title are set
        /// 
        /// This is needed since our custom typedescription provider doesn't get updated otherwise since
        /// we can only have one of them active
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mainObjectsRepaiont_Paint(object sender, PaintEventArgs e)
        {
            SnippetDesignerPackage.Instance.ActiveSnippetLanguage = this.SnippetLanguage;
            SnippetDesignerPackage.Instance.ActiveSnippetTitle = this.SnippetTitle;
        }

        /// <summary>
        /// When the user right clicks update the current selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void snippetReplacementGrid_MouseDown(object sender, MouseEventArgs e)
        {

            DataGridView.HitTestInfo info = replacementGridView.HitTest(e.X, e.Y);
            if (info.RowIndex >= 0)
            {
                replacementGridView.Rows[info.RowIndex].Selected = true;
            }
        }









    }
}
