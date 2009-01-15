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

        //if the user typed a single character then store it here so we know what it is
        //otherwise its null
        protected string lastCharacterEntered = null;

        //hash which maps the dispaly names of the languages to the path to their user snippet directory
        internal Dictionary<string, string> snippetDirectories = SnippetDirectories.Instance.UserSnippetDirectories;

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
        private Regex validReplacement = new Regex(StringConstants.ValidReplacementString, RegexOptions.Compiled);




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
                    string currId = (string)row.Cells[StringConstants.ColumnID].EditedFormattedValue;
                    currId = currId.Trim();
                    if (String.IsNullOrEmpty(currId)) continue;

                    bool isObj = false;
                    if ((((DataGridViewComboBoxCell)row.Cells[StringConstants.ColumnReplacementKind]).EditedFormattedValue as string).ToLower() == Resources.ReplacementObjectName.ToLower())
                    {
                        isObj = true;
                    }

                    bool isEditable = (bool)((DataGridViewCheckBoxCell)row.Cells[StringConstants.ColumnEditable]).EditedFormattedValue;
                    replacements.Add(new Literal((string)row.Cells[StringConstants.ColumnID].EditedFormattedValue,
                        (string)row.Cells[StringConstants.ColumnTooltip].EditedFormattedValue,
                        (string)row.Cells[StringConstants.ColumnDefault].EditedFormattedValue,
                        (string)row.Cells[StringConstants.ColumnFunction].EditedFormattedValue,
                        isObj,
                        isEditable,
                        (string)row.Cells[StringConstants.ColumnType].EditedFormattedValue
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

        public SnippetEditorForm()
        {
        }


        #region Snippet Save and Load Methods

        /// <summary>
        /// save snippet and update the snippets in memory
        /// </summary>
        /// <returns></returns>
        public bool SaveSnippet()
        {
            PushFieldsIntoActiveSnippet();
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
            PushFieldsIntoActiveSnippet();
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
                PullFieldsFromActiveSnippet();
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
        public void PullFieldsFromActiveSnippet()
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
                snippetTypes.Add(new SnippetType(StringConstants.SnippetTypeExpansion));
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
        public void PushFieldsIntoActiveSnippet()
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

        private List<String> GetSnippetTitles()
        {
            List<string> snippetTitles = new List<string>();
            foreach (Snippet s in snippetFile.Snippets)
            {
                snippetTitles.Add(s.Title);
            }
            return snippetTitles;
        }

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

        private void toolStripSnippetsTitles_SelectedIndexChanged(object sender, EventArgs e)
        {
            ToolStripComboBox snippetsBox = sender as ToolStripComboBox;
            if (snippetsBox != null)
            {
                string newTitle = snippetsBox.SelectedItem as string;
                if (!String.IsNullOrEmpty(newTitle) && newTitle != activeSnippet.Title)
                {
                    PushFieldsIntoActiveSnippet();

                    //foreach (Snippet sn in snippetFile.Snippets)
                    for (int i = 0; i < snippetFile.Snippets.Count; i++)
                    {
                        if (snippetFile.Snippets[i].Title.Equals(newTitle, StringComparison.InvariantCulture))
                        {
                            snippetIndex = i;
                            activeSnippet = snippetFile.Snippets[i];
                        }
                    }

                    PullFieldsFromActiveSnippet();

                    //clear and show all markers
                    //RefreshReplacementMarkers(false);

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
                    PushFieldsIntoActiveSnippet();
                    isFormDirty = true;
                }

            }
        }

        private void SetOrDisableTypeField(bool isObject, int rowIndex)
        {
            if (isObject) //if this is an object than enable the type field
            {

                replacementGridView.Rows[rowIndex].Cells[StringConstants.ColumnType].Value = String.Empty;
                replacementGridView.Rows[rowIndex].Cells[StringConstants.ColumnType].ReadOnly = false;
            }
            else //this is not a object so disable type field
            {

                replacementGridView.Rows[rowIndex].Cells[StringConstants.ColumnType].Value = Resources.TypeInvalidForLiteralSymbol;
                replacementGridView.Rows[rowIndex].Cells[StringConstants.ColumnType].ReadOnly = true;
            }


        }

        #region Replacement Grid Events

        private void replacementGridView_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            DataGridView grid = sender as DataGridView;
            if (grid != null)
            {
                if (grid.Columns[e.ColumnIndex].Name == StringConstants.ColumnID)
                {
                    previousIDValue = (string)grid.Rows[e.RowIndex].Cells[e.ColumnIndex].EditedFormattedValue;
                    return;
                }
            }
            previousIDValue = null;
        }

        private void replacementGridView_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView grid = sender as DataGridView;
            if (grid != null)
            {
                currentlySelectedId = grid.Rows[e.RowIndex].Cells[StringConstants.ColumnID].Value as string;
                RefreshReplacementMarkers();
            }
        }

        private void replacementGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {

            DataGridView grid = sender as DataGridView;
            if (grid != null)
            {
                isFormDirty = true;
                if (grid.Columns[e.ColumnIndex].Name == StringConstants.ColumnID)
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
                        if (IsValidReplaceableText(newIdValue))
                        {
                            //build new replacement text
                            string newReplacement = TurnTextIntoReplacementSymbol(newIdValue);
                            string oldReplacement = TurnTextIntoReplacementSymbol(previousIDValue);


                            //replace all occurances of the oldReplacement with newReplacement
                            //set false so it allows us to overdie existing replacements
                            ReplaceAll(oldReplacement, newReplacement, false);
                            ReplaceAll(newIdValue, newReplacement, true);

                            //add any existing instances of this vairable as new replacement
                            //ReplacementMake(newIdValue);

                            //a update was made so refresh marker
                            //RefreshReplacementMarkers(false);
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
                else if (grid.Columns[e.ColumnIndex].Name == StringConstants.ColumnReplacementKind)
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

        private void replacementGridView_RowsRemoved(object sender, DataGridViewRowCancelEventArgs e)
        {
            UpdateMarkersAfterDeletedGridViewRow(e.Row);
        }

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

        #region Replacement Functions

        private string TurnTextIntoReplacementSymbol(string text)
        {
            return StringConstants.SymbolReplacement + text + StringConstants.SymbolReplacement;
        }

        public void RefreshReplacementMarkers(int lineToMark)
        {
            ClearAllMarkers(lineToMark);
            List<string> allReplacements = new List<string>();
            foreach (DataGridViewRow row in replacementGridView.Rows)
            {
                string idValue = ((string)row.Cells[StringConstants.ColumnID].EditedFormattedValue).Trim();
                if (idValue.Length > 0)
                {
                    allReplacements.Add(idValue);

                }

            }
            //search through the code window and update all replcement highlight martkers
            MarkReplacements(allReplacements, lineToMark);
        }

        public void RefreshReplacementMarkers() { RefreshReplacementMarkers(-1); }

        public void ClearAllMarkers(int lineToClear)
        {
            //clear all yellow markers
            ClearMarkersOfType(GuidList.yellowMarker, lineToClear);
            //clear all yellow markers with borders
            ClearMarkersOfType(GuidList.yellowMarkerWithBorder, lineToClear);
        }

        public void ClearMarkersOfType(Guid markerGuid, int lineToClear)
        {
            if (SnippetDesignerPackage.Instance == null)
            {
                return;
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
                if (span[0].iStartLine == lineToClear || lineToClear < 0)
                {
                    //clear and tell the code window to stop keeping track of this marker
                    marker.Invalidate();
                    marker.UnadviseClient();
                }
            }
        }

        public void MakeClickedReplacementActive()
        {

            TextSpan currentWordSpan;
            //see if the person clicked inside of a replacement and return its span
            if (GetClickedOnReplacementSpan(out currentWordSpan))
            {
                string currentWord = this.CodeWindow.GetSpanText(currentWordSpan);

                foreach (DataGridViewRow row in replacementGridView.Rows)
                {
                    if ((string)row.Cells[StringConstants.ColumnID].Value == currentWord)
                    {
                        replacementGridView.ClearSelection();
                        row.Selected = true;
                        currentlySelectedId = row.Cells[StringConstants.ColumnID].Value as string;
                        RefreshReplacementMarkers();
                        break;
                    }
                }
            }



        }

        public void ReplacementRemove()
        {
            TextSpan currentWordSpan;
            if (GetClickedOnReplacementSpan(out currentWordSpan))
            {
                string currentWord = this.CodeWindow.GetSpanText(currentWordSpan);
                ReplacementRemove(currentWord, currentWordSpan);
            }
        }

        public void ReplacementRemove(string textToChange, TextSpan replaceSpan)
        {

            DataGridViewRow rowToDelete = null;
            foreach (DataGridViewRow row in replacementGridView.Rows)
            {
                if ((string)row.Cells[StringConstants.ColumnID].Value == textToChange)
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

        public void CreateReplacementFromSelection()
        {
            string selectedText = String.Empty;
            var selection = this.CodeWindow.Selection;
            var selectionLength = this.CodeWindow.SelectionLength;
            if (selectionLength != 0)
            {
                //trim any replacement symbols or spaces
                selectedText = this.CodeWindow.SelectedText.Trim();

            }
            else
            {
                selectedText = this.CodeWindow.GetWordFromCurrentPosition();
            }


            //make replacement with the desired text
            if (IsValidReplaceableText(selectedText) &&
                CreateReplacement(selectedText) &&
                selectionLength > 0)
            {
                selection.iEndIndex += StringConstants.SymbolReplacement.Length * 2;
                this.CodeWindow.Selection = selection;
            }
        }

        public bool IsValidReplaceableText(string text)
        {
            return validReplacement.IsMatch(text);
        }

        public bool CreateReplacement(string textToChange)
        {
            if (!IsValidReplaceableText(textToChange))
                return false;


            //check if replacement exists already
            bool existsAlready = false;
            foreach (DataGridViewRow row in this.replacementGridView.Rows)
            {
                if ((string)row.Cells[StringConstants.ColumnID].EditedFormattedValue == textToChange ||
                    textToChange.Trim() == String.Empty)
                {
                    //this replacement already exists or is nothing don't add it to the replacement list
                    existsAlready = true;
                }

            }

            //build new replacement text
            string newText = TurnTextIntoReplacementSymbol(textToChange);
            if (!existsAlready)
            {
                object[] newRow = { textToChange, textToChange, textToChange, Resources.ReplacementLiteralName, String.Empty, String.Empty, true };
                int rowIndex = this.replacementGridView.Rows.Add(newRow);
                SetOrDisableTypeField(false, rowIndex);
            }

            //replace all occurances of the textToFind with $textToFind$
            int numFoundAndReplaced = ReplaceAll(textToChange, newText, true);

            return numFoundAndReplaced > 0;
        }

        private void UpdateMarkersAfterDeletedGridViewRow(DataGridViewRow deletedRow)
        {
            if (deletedRow != null)
            {
                string deletedID = deletedRow.Cells[StringConstants.ColumnID].EditedFormattedValue as string;
                if (deletedID != null)
                {
                    //build new replacement text 
                    string currentText = TurnTextIntoReplacementSymbol(deletedID);
                    ReplaceAll(currentText, deletedID, false);
                    //RefreshReplacementMarkers(false);
                }
            }

        }

        public void MarkReplacements(List<string> replaceIDs, int lineToMark)
        {

            if (replaceIDs == null)
            {
                return;
            }

            int lineLength;
            int startLine = 0;
            int endLine = CodeWindow.LineCount;

            if (lineToMark > -1)//are we just replacing markers on the given line
            {
                startLine = lineToMark;
                endLine = startLine + 1;

            }

            //loop through all the lines we are searching
            for (int line = startLine; line < endLine; line++)
            {
                //get the length of this line
                lineLength = CodeWindow.LineLength(line);

                //loop over the line looking for SnippetDesigner.StringConstants.SymbolReplacement and find the next matching one
                for (int index = 0; index < lineLength; index++)
                {
                    //find the character at this position
                    string character = CodeWindow.GetCharacterAtPosition(new TextPoint(line, index));
                    //check if this character is the replacement symbol
                    if (character == SnippetDesigner.StringConstants.SymbolReplacement)
                    {
                        int nextIndex = index + 1;
                        while (nextIndex < lineLength && CodeWindow.GetCharacterAtPosition(new TextPoint(line, nextIndex)) != SnippetDesigner.StringConstants.SymbolReplacement)
                        {
                            nextIndex++;
                        }
                        if (nextIndex < lineLength) //we found another SymbolReplacement
                        {
                            //create text span for the space between the two SnippetDesigner.StringConstants.ReplacementSymbols
                            string textBetween;

                            //make sure text between SnippetDesigner.StringConstants.SymbolReplacement signs matches replaceID
                            CodeWindow.TextLines.GetLineText(line, index + 1, line, nextIndex, out textBetween);
                            if (replaceIDs.Contains(textBetween))
                            {//this replacement exists already so mark

                                //create span that we will mark
                                TextSpan replacementMarkerSpan;
                                replacementMarkerSpan.iStartLine = replacementMarkerSpan.iEndLine = line;
                                replacementMarkerSpan.iStartIndex = index;
                                //make the span 2*length of SymbolReplacement longer since we are marker the replacement symbol also
                                replacementMarkerSpan.iEndIndex =
                                    nextIndex +
                                    (SnippetDesigner.StringConstants.SymbolReplacement.Length +
                                     SnippetDesigner.StringConstants.SymbolReplacement.Length - 1);

                                KindOfMarker markerType;
                                //determine if this is the adctive replacement
                                //and chosoe the right highlight marker
                                if (CurrentlySelectedId != null && CurrentlySelectedId == textBetween)
                                {
                                    markerType = KindOfMarker.YellowWithBorder;
                                }
                                else
                                {
                                    markerType = KindOfMarker.Yellow;
                                }
                                CodeWindow.HighlightSpan(replacementMarkerSpan, markerType);//mark this span with the desired color marker
                                index = nextIndex; //skip the ending SnippetDesigner.StringConstants.SymbolReplacement, it will be incremented the one extra in the next loop iteration
                            }
                            else
                            {
                                string trimedText = textBetween.Trim();
                                //this replacement does not exist yet so create it only if the last character entered was the replacement symbol
                                if (lastCharacterEntered != null //make sure a single character was just entered
                                    && lastCharacterEntered == SnippetDesigner.StringConstants.SymbolReplacement //make sure the last charcter is a $
                                    && trimedText == textBetween //make sure this replacement doesnt have whitespace in it
                                    && trimedText != String.Empty //and make sure its not empty
                                    && trimedText != StringConstants.SymbolEndWord //the word cant be end
                                    && trimedText != StringConstants.SymbolSelectedWord // and the word cant be selected they have special meaning
                                    )
                                {
                                    //make the text into a replacement but dont add the replacement symbols since the user is doing it
                                    CreateReplacement(textBetween);
                                    RefreshReplacementMarkers(lineToMark);
                                    //clear last character 
                                    lastCharacterEntered = null;
                                }
                                else
                                {
                                    index = nextIndex - 1;//subtract one since it will be incrememented in the next loop iteration
                                }
                            }
                        }
                    }


                }

            }
        }

        public int ReplaceAll(string currentWord, string newWord, bool replacementAware)
        {
            TextSpan span;
            TextPoint nextPoint = new TextPoint();
            int numberReplaced = 0;
            //search through every string we can replace
            while (FindNextReplaceableString(currentWord, nextPoint, out span))
            {
                //calculate the difference in word length
                int differenceInWordLength = newWord.Length - currentWord.Length;

                nextPoint.Line = span.iEndLine;
                nextPoint.Index = span.iEndIndex + differenceInWordLength;

                //replace the span with the given text
                if (ReplaceSpanWithText(newWord, span, replacementAware))
                {
                    numberReplaced++;
                    nextPoint.Index++;
                }

            }
            return numberReplaced;
        }

        public bool ReplaceSpanWithText(string newWord, TextSpan replaceSpan, bool replacementAware)
        {
            IVsTextView textView = CodeWindow.TextView;
            try
            {
                //either we arent marking this as a replacement so just replace the text
                //or we are marking it as a replacement so we need to make sure its not a reaplcement marker already
                if ((replacementAware && !IsSpanReplacement(replaceSpan)) || !replacementAware) //are we creating a replacement marker
                {
                    textView.ReplaceTextOnLine(replaceSpan.iStartLine,
                           replaceSpan.iStartIndex,
                           (replaceSpan.iEndIndex - replaceSpan.iStartIndex),
                           newWord,
                           newWord.Length);

                    return true;
                }

            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        public bool FindEnclosingReplacementQuoteSpan(TextSpan span, out TextSpan quoteSpan)
        {
            int line = span.iStartLine;
            int lineLength = CodeWindow.LineLength(line);
            int left = span.iStartIndex - 1;
            int right = span.iEndIndex;
            quoteSpan = new TextSpan();
            while (left >= 0 && CodeWindow.GetCharacterAtPosition(new TextPoint(line, left)) != StringConstants.DoubleQuoteString)
            {
                left--;
            }
            while (right < lineLength && CodeWindow.GetCharacterAtPosition(new TextPoint(line, right)) != StringConstants.DoubleQuoteString)
            {
                right++;
            }
            if (right >= lineLength || left < 0)
            {
                return false;//we didnt find a quoted replacement string
            }
            quoteSpan.iStartLine = quoteSpan.iEndLine = line;
            quoteSpan.iStartIndex = left;
            quoteSpan.iEndIndex = right + 1;//the end character should be exclusive not inclusive

            //is this span surrounded by the replcement markers
            if (!IsSpanReplacement(quoteSpan))
            {
                return false;
            }

            //we have a correct quotes string replcement
            return true;
        }

        public bool GetClickedOnReplacementSpan(out TextSpan replacementSpan)
        {

            replacementSpan = CodeWindow.GetWordTextSpanFromCurrentPosition();
            TextSpan currentWordSpan = replacementSpan;

            string currentWord = CodeWindow.GetSpanText(replacementSpan);

            if (String.IsNullOrEmpty(currentWord) == true)//you might have selected more than a word, so use what you selected
            {
                replacementSpan = CodeWindow.Selection;
            }

            //make sure this is infact a replacement
            if (!this.IsSpanReplacement(currentWordSpan))
            {
                //this span doesnt seem to be a replacement but maybe its a string and the user just
                //clicked in the middle of it so lets intelligently see if thats true
                if (!this.FindEnclosingReplacementQuoteSpan(currentWordSpan, out replacementSpan))
                {
                    return false;
                }
            }

            //we have found a replacement span
            return true;

        }

        public bool IsSpanReplacement(TextSpan replaceSpan)
        {
            int length = CodeWindow.LineLength(replaceSpan.iEndLine);
            //make sure there is room for this replacement
            if (replaceSpan.iStartIndex >= 0 && replaceSpan.iEndIndex <= length && (replaceSpan.iEndIndex - replaceSpan.iStartIndex) >= 1)
            {
                //see if replacement symbols surround this span
                if (CodeWindow.GetCharacterAtPosition(new TextPoint(replaceSpan.iStartLine, replaceSpan.iStartIndex - 1)) == SnippetDesigner.StringConstants.SymbolReplacement &&
                    CodeWindow.GetCharacterAtPosition(new TextPoint(replaceSpan.iEndLine, replaceSpan.iEndIndex)) == SnippetDesigner.StringConstants.SymbolReplacement
                    )
                {
                    return true;
                }
                //check the first and last characters of the span to see if they are the replacement symbols
                if (CodeWindow.GetCharacterAtPosition(new TextPoint(replaceSpan.iStartLine, replaceSpan.iStartIndex)) == SnippetDesigner.StringConstants.SymbolReplacement &&
                    CodeWindow.GetCharacterAtPosition(new TextPoint(replaceSpan.iEndLine, replaceSpan.iEndIndex - 1)) == SnippetDesigner.StringConstants.SymbolReplacement
                    )
                {
                    return true;
                }
            }
            return false;

        }

        public bool FindNextReplaceableString(string textToFind, TextPoint startPositon, out TextSpan returnSpan)
        {
            int lastLine = CodeWindow.LineCount - 1;
            TextPoint endPositon = new TextPoint(lastLine, CodeWindow.LineLength(lastLine));

            if (startPositon == null || textToFind == null)
            {
                returnSpan = new TextSpan();
                return false;
            }

            IVsTextLines textLines = CodeWindow.TextLines;

            int lineLength;
            string lineText;
            returnSpan = new TextSpan();
            //loop through all the lines we are searching
            for (int line = startPositon.Line; line <= endPositon.Line; line++)
            {
                if (line == endPositon.Line)
                {//if this is the last line then get the correct end index
                    lineLength = endPositon.Index;
                }
                else
                { //if this isnt the last line then the line length is the end index
                    lineLength = CodeWindow.LineLength(line);
                }
                //retrieve all the text on this line as a string
                textLines.GetLineText(line, 0, line, lineLength, out lineText);

                int position = startPositon.Index;//initialize the start position
                int index = -1;
                //find the next index where textToFind appears starting from position
                while (position < lineLength && ((index = lineText.IndexOf(textToFind, position)) > -1))
                {

                    //only three items are valid replaceable strings
                    //1. a word which is [A-Za-z0-9_]+
                    //2. if the string begins and ends with the replacement symbol then we are replacing a replacement
                    //3. if the string beings and ends with quotes then this is a quote string we are replacing
                    if (CodeWindow.GetWordFromPosition(new TextPoint(line, index)) == textToFind ||
                        //or is this the text we are looking for 
                        textToFind[0] == SnippetDesigner.StringConstants.SymbolReplacement[0] && textToFind[textToFind.Length - 1] == SnippetDesigner.StringConstants.SymbolReplacement[0] ||
                        textToFind[0] == SnippetDesigner.StringConstants.DoubleQuoteString[0] && textToFind[textToFind.Length - 1] == SnippetDesigner.StringConstants.DoubleQuoteString[0]
                        )
                    {
                        //update the span to reflect what text we found
                        returnSpan.iStartIndex = index;
                        returnSpan.iStartLine = line;
                        returnSpan.iEndIndex = index + textToFind.Length;
                        returnSpan.iEndLine = line;
                        return true;
                    }
                    position += textToFind.Length + index;//move to the next position and repeat the loop
                }
                startPositon.Index = 0;//only offset from first line
            }
            return false;

        }


        #endregion

        private void mainObjectsRepaiont_Paint(object sender, PaintEventArgs e)
        {
            SnippetDesignerPackage.Instance.ActiveSnippetLanguage = this.SnippetLanguage;
            SnippetDesignerPackage.Instance.ActiveSnippetTitle = this.SnippetTitle;
        }

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
