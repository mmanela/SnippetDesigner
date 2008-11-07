// Copyright (C) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Package;

using IServiceProvider = System.IServiceProvider;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using VSStd97CmdID = Microsoft.VisualStudio.VSConstants.VSStd97CmdID;

namespace Microsoft.SnippetDesigner
{
    /// <summary>
    /// The code window which is held inside the snippet codeWindowHost form
    /// </summary>
    public partial class CodeWindow : UserControl, ISnippetCodeWindow
    {
        internal ICodeWindowHost codeWindowHost;
        internal SnippetEditor snippetEditor;
        private IVsCodeWindow vsCodeWindow;
        private IntPtr hWndCodeWindow;
        private uint cookieTextViewEvents;
        private uint cookieTextLineEvents;
        private bool isHandleCreated;
        private Dictionary<string, Guid> langServ = new Dictionary<string, Guid>();
        private IVsTextLines vsTextBuffer;
        private Guid defaultLanguage;
        IConnectionPoint textViewEventsConnectionPoint; 
        IConnectionPoint textLinesEventsConnectionPoint;
        #region properties

        public IVsCodeWindow VsCodeWindow
        {
            get
            {
                return vsCodeWindow;
            }
        }

        public Dictionary<string, Guid> LangServices
        {
            get
            {
                return langServ;
            }

        }


        //get and set the text in the code window
        public string CodeText
        {

            get
            {
                IVsTextLines vsTextLines = this.TextLines;
                if (vsTextLines != null)
                {
                    string codeText = String.Empty;
                    int numLines;
                    int lastLineIndex;
                    ErrorHandler.ThrowOnFailure(vsTextLines.GetLastLineIndex(out numLines, out lastLineIndex));
                    ErrorHandler.ThrowOnFailure(vsTextLines.GetLineText(0, 0, numLines, lastLineIndex, out codeText));
                    return codeText;
                }
                else
                {
                    return String.Empty;
                }
            }
            set
            {

                IVsTextLines vsTextLines = this.TextLines;
                if (vsTextLines != null)
                {
                    SetText(value);
                }




            }
        }

        /// <summary>

        /// This is a helper routine to replace the contents of the Text Buffer with "newText".
        /// This function handles the interop of passing the string as a block of memory 
        /// via an IntPtr. It uses a CoTaskMemAlloc to allocate a block of memory at a fixed 
        /// location.
        /// </summary>
        /// <param name="newText"></param>
        private void SetText(string newText)
        {
            IVsTextLines textLines = this.TextLines;
            int endLine, endCol;

            ErrorHandler.ThrowOnFailure(textLines.GetLastLineIndex(out endLine, out endCol));

            int len = (newText == null) ? 0 : newText.Length;

            //the pointer to the text must not move during the replacelines operation
            IntPtr pText = Marshal.StringToCoTaskMemAuto(newText);

            try
            {
                ErrorHandler.ThrowOnFailure(textLines.ReplaceLines(0, 0, endLine, endCol, pText, len, null));
            }

            finally
            {
                //free the text ptr
                Marshal.FreeCoTaskMem(pText);
            }
        }


        /// <summary>
        /// The TextLines interface of the codewindows textbuffer
        /// </summary>
        internal IVsTextLines TextLines
        {
            get
            {
                if (vsCodeWindow != null)
                {
                    IVsTextLines vsTextLines;
                    ErrorHandler.ThrowOnFailure(vsCodeWindow.GetBuffer(out vsTextLines));
                    return vsTextLines;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                vsTextBuffer = value;
            }
        }

        /// <summary>
        /// the primary view for the code window
        /// </summary>
        internal IVsTextView TextView
        {
            get
            {
                if (vsCodeWindow != null)
                {
                    IVsTextView textView;
                    ErrorHandler.ThrowOnFailure(vsCodeWindow.GetPrimaryView(out textView));
                    return textView;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// the window pane for this code window
        /// </summary>
        internal IVsWindowPane VsWindowPane
        {
            get
            {
                return vsCodeWindow as IVsWindowPane;
            }
        }

        /// <summary>
        /// The command target of this codewindow
        /// </summary>
        internal IOleCommandTarget VsCommandTarget
        {
            get
            {
                return vsCodeWindow as IOleCommandTarget;
            }
        }
        #endregion


        /// <summary>
        /// The code windows parent CodeWindowHost.
        /// This is the Snippet CodeWindowHost that this code window needs a reference to.
        /// 
        /// The Set method is very important.  Our codewindow needs two things to be created.
        /// A reference to the parent codeWindowHost and the window handle.  Since we have both of these we can create
        /// the code window.  This code window can be created during the set here if we have the handle already 
        /// or after this set in the OnHandleCreated event since by that point 
        /// we will have both the handle and the parent codeWindowHost set
        /// </summary>
        internal ICodeWindowHost CodeWindowHost
        {
            get
            {
                return codeWindowHost;
            }
            //this may only be called once per snippet instance of the codewindow
            set
            {
                if (codeWindowHost == null)
                {
                    codeWindowHost = value;
                    if (isHandleCreated)
                    {
                        CreateVsCodeWindow();
                        codeWindowHost.SetupContextMenus();
                    }

                    if (codeWindowHost is SnippetEditor)
                    {
                        snippetEditor = value as SnippetEditor;
                    }
                }

            }

        }

        /// <summary>
        /// Constructor for the code window which is a user snippetExplorerForm that hosts a vscodewindow
        /// </summary>
        public CodeWindow()
        {
            InitializeComponent();

            //add the languages we support to a has that maps a display name of a language to its guid
            langServ.Add(SnippetDesigner.Resources.DisplayNameVisualBasic, GuidList.vbSnippetLanguageService);
            langServ.Add(SnippetDesigner.Resources.DisplayNameCSharp, GuidList.csharpSnippetLanguageService);
            langServ.Add(SnippetDesigner.Resources.DisplayNameXML, GuidList.xmlSnippetLanguageService);

            //default to text
            defaultLanguage = GuidList.textLangSvc;
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
                if (cookieTextLineEvents != 0)
                {
                    textLinesEventsConnectionPoint.Unadvise(cookieTextLineEvents);
                }
                if (cookieTextViewEvents != 0)
                {
                    textViewEventsConnectionPoint.Unadvise(cookieTextViewEvents);
                }

                if (vsCodeWindow != null)
                {
                    vsCodeWindow.Close();
                    vsCodeWindow = null;
                }

            }
            base.Dispose(disposing);
        }


        /// <summary>
        /// This gets called once this snippetExplorerForm has recieved its windows handle
        /// This is important since we need this inorder to create the vs code window
        /// since we create the code window pane ourselves
        /// 
        /// If this gets called before codeWindowHost is set then we must not create the code window yet
        /// it will be created when codeWindowHost gets set
        /// </summary>
        /// <param name="e"></param>
        protected override void OnHandleCreated(EventArgs e)
        {

            base.OnHandleCreated(e);
            if (codeWindowHost != null) //do we know our parent window?
            { //if so create code window and set up the context menus for this code window
                CreateVsCodeWindow();
                codeWindowHost.SetupContextMenus();
            }

            //mark that the handle is created
            isHandleCreated = true;
        }


        /// <summary>
        /// Set the language service give a language string that is in the form of one of the 
        /// SnippetDesigner.Resources.DisplayName languages
        /// </summary>
        /// <param name="lang"></param>
        public void SetLanguageService(string lang)
        {
            if (vsCodeWindow != null)
            {
                IVsTextLines vsTextLines = this.TextLines;
                Guid langGuid;
                //is this language in the hash
                if (langServ.ContainsKey(lang))
                {
                    langGuid = langServ[lang];
                }
                else
                {
                    //if we dont recognize the language then set it to the deafult
                    langGuid = defaultLanguage;
                }

                ErrorHandler.ThrowOnFailure(vsTextLines.SetLanguageServiceID(ref langGuid));

            }
        }

        // Defined functions for searching and replacing and marking words
        #region Searching and Replacing and Marking and Replcements

        /// <summary>
        /// get the length of the selection
        /// </summary>
        /// <returns>length of selection</returns>
        internal int SelectionLength
        {
            get
            {
                return SelectedText.Length;
            }
        }

        /// <summary>
        /// The number of lines
        /// </summary>
        internal int LineCount
        {
            get
            {
                if (this.TextLines != null)
                {
                    int lineCount;
                    this.TextLines.GetLineCount(out lineCount);
                    return lineCount;
                }
                else
                {
                    return -1;
                }
            }
        }
        /// <summary>
        /// gets or sets the selection text span
        /// </summary>
        /// <returns>selected text</returns>
        internal TextSpan Selection
        {
            get
            {
                if (this.TextView != null)
                {
                    TextSpan[] selSpan = new TextSpan[1];
                    this.TextView.GetSelectionSpan(selSpan);
                    return selSpan[0];
                }
                else
                {
                    TextSpan empty = new TextSpan();
                    return empty;
                }
            }
            set
            {
                this.TextView.SetSelection(value.iStartLine, value.iStartIndex, value.iEndLine, value.iEndIndex);
            }
        }

        /// <summary>
        /// get the selected text
        /// </summary>
        /// <returns>selected text</returns>
        internal string SelectedText
        {
            get
            {
                if (this.TextView != null)
                {
                    string selectedText = String.Empty;
                    this.TextView.GetSelectedText(out selectedText);
                    return selectedText;
                }
                else
                {
                    return String.Empty;
                }
            }
        }

        /// <summary>
        /// Get start of the selection
        /// </summary>
        /// <returns>start point of selection</returns>
        internal TextPoint SelectionStart
        {
            get
            {
                if (this.TextView != null)
                {
                    string selectedText = String.Empty;
                    TextSpan[] selSpan = new TextSpan[1];
                    this.TextView.GetSelectionSpan(selSpan);
                    return new TextPoint(selSpan[0].iStartLine, selSpan[0].iStartIndex);
                }
                else
                {
                    return null;
                }
            }
        }



        /// <summary>
        /// Get start of the selection
        /// </summary>
        /// <returns>start point of selection</returns>
        internal TextPoint SelectionEnd
        {
            get
            {
                if (this.TextView != null)
                {
                    string selectedText = String.Empty;
                    TextSpan[] selSpan = new TextSpan[1];
                    this.TextView.GetSelectionSpan(selSpan);
                    return new TextPoint(selSpan[0].iEndLine, selSpan[0].iEndIndex);
                }
                else
                {
                    return null;
                }
            }
        }


        /// <summary>
        /// Length of a line in the buffer
        /// </summary>
        /// <param name="line">the line</param>
        /// <returns>the length of the line</returns>
        internal int LineLength(int line)
        {
            if (this.TextLines != null)
            {
                int lineLength;
                this.TextLines.GetLengthOfLine(line, out lineLength);
                return lineLength;
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Mark a span of text with a specfic color marker and the $ sign symbols
        /// </summary>
        /// <param name="span">a textspan representing a amount of text</param>
        /// <param name="color">a color</param>
        internal void MarkSpan(TextSpan span, KindOfMarker markerType)
        {
            if (SnippetDesignerPackage.Instance == null)
            {
                return; //we need an instance of share and collaborate
            }

            if (markerType == KindOfMarker.Yellow)
            {
                SnippetDesignerPackage.Instance.MarkerService.InsertMarker(PkgCmdIDList.cmdidYellowHighlightMarker, span);
            }
            else if (markerType == KindOfMarker.YellowWithBorder)
            {
                SnippetDesignerPackage.Instance.MarkerService.InsertMarker(PkgCmdIDList.cmdidYellowHighlightMarkerWithBorder, span);
            }

        }

        /// <summary>
        /// Mark all replacement items given the id of the replacement
        /// If text surrounded by the replacement symbol is found then turn it into a repalcement
        /// </summary>
        /// <param name="replaceID">ther id/name of the replacement</param>
        /// <param name="color">color to mark the replacement</param>
        internal void MarkReplacements(List<string> replaceIDs, bool currentLineOnly)
        {

            if (replaceIDs == null || snippetEditor == null)
            {
                return;
            }

            int lineLength;
            int startLine = 0;
            int endLine = LineCount;

            if (currentLineOnly)//are we just replacing markers on the current line
            {
                int col;
                //get the current line
                this.TextView.GetCaretPos(out startLine, out col);
                endLine = startLine + 1;

            }

            //loop through all the lines we are searching
            for (int line = startLine; line < endLine; line++)
            {
                //get the length of this line
                lineLength = LineLength(line);

                //loop over the line looking for SnippetDesigner.ConstantStrings.SymbolReplacement and find the next matching one
                for (int index = 0; index < lineLength; index++)
                {
                    //find the character at this position
                    string character = GetCharacterAtPosition(new TextPoint(line, index));
                    //check if this character is the repalcement symbol
                    if (character == SnippetDesigner.ConstantStrings.SymbolReplacement)
                    {
                        int nextIndex = index + 1;
                        while (nextIndex < lineLength && GetCharacterAtPosition(new TextPoint(line, nextIndex)) != SnippetDesigner.ConstantStrings.SymbolReplacement)
                        {
                            nextIndex++;
                        }
                        if (nextIndex < lineLength) //we found another SnippetDesigner.ConstantStrings.SymbolReplacement
                        {
                            //create text span for the space between the two SnippetDesigner.ConstantStrings.ReplacementSymbols
                            string textBetween;

                            //make sure text between SnippetDesigner.ConstantStrings.SymbolReplacement signs matches replaceID
                            TextLines.GetLineText(line, index + 1, line, nextIndex, out textBetween);
                            if (replaceIDs.Contains(textBetween))
                            {//this replacement exists already so mark

                                //create span that we will mark
                                TextSpan replacementMarkerSpan;
                                replacementMarkerSpan.iStartLine = replacementMarkerSpan.iEndLine = line;
                                replacementMarkerSpan.iStartIndex = index;
                                //make the span 2*length of SymbolReplacement longer since we are marker the replacement symbol also
                                replacementMarkerSpan.iEndIndex = nextIndex + (SnippetDesigner.ConstantStrings.SymbolReplacement.Length + SnippetDesigner.ConstantStrings.SymbolReplacement.Length - 1);

                                KindOfMarker markerType;
                                //determine if this is the adctive replacement
                                //and chosoe the right highlight marker
                                if (this.snippetEditor.CurrentlySelectedId != null && this.snippetEditor.CurrentlySelectedId == textBetween)
                                {
                                    markerType = KindOfMarker.YellowWithBorder;
                                }
                                else
                                {
                                    markerType = KindOfMarker.Yellow;
                                }
                                MarkSpan(replacementMarkerSpan, markerType);//mark this span with the desired color marker
                                index = nextIndex; //skip the ending SnippetDesigner.ConstantStrings.SymbolReplacement, it will be incremented the one extra in the next loop iteration
                            }
                            else
                            {
                                string trimedText = textBetween.Trim();
                                //this replacement does not exist yet so create it only if the last character entered was the replacement symbol
                                if (this.snippetEditor.LastCharacterEntered != null //make sure a single character was just entered
                                    && this.snippetEditor.LastCharacterEntered == SnippetDesigner.ConstantStrings.SymbolReplacement //make sure the last charcter is a $
                                    && trimedText == textBetween //make sure this replacement doesnt have whitespace in it
                                    && trimedText != String.Empty //and make sure its not empty
                                    && trimedText != ConstantStrings.SymbolEndWord //the word cant be end
                                    && trimedText != ConstantStrings.SymbolSelectedWord // and the word cant be selected they have special meaning
                                    )
                                {
                                    //make the text into a replacement but dont add the replacement symbols since the user is doing it
                                    this.snippetEditor.ReplacementMake(textBetween);

                                    //clear last character 
                                    this.snippetEditor.LastCharacterEntered = null;
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



        /// <summary>
        /// Replaces all occurances in the code buffer of one textToFind with another
        /// </summary>
        /// <param name="currentWord">currentWord to be replaced</param>
        /// <param name="newWord">replace the currentWord with newWord</param>
        /// <param name="markReplacements">Highlight the words that over been replaced</param>
        /// <returns>returns the number of replacements</returns>
        internal int ReplaceAll(string currentWord, string newWord, bool markReplacements)
        {
            TextSpan span;
            TextPoint nextPoint = new TextPoint();
            int numberReplaced = 0;
            //search through every string we can replace
            while (FindReplaceableString(currentWord, nextPoint, out span))
            {
                //calculate the difference in word length
                int differenceInWordLength = newWord.Length - currentWord.Length;

                //advance to next point, which depends on the difference in the size of the words
                //this is very important since as we replace words their indicies change
                nextPoint.Index = span.iEndIndex + 1 + differenceInWordLength;
                nextPoint.Line = span.iEndLine;
                //incremembt number of replacements made
                numberReplaced++;
                //replace the span witht eh given text
                ReplaceSpanWithText(newWord, span, markReplacements);

            }
            return numberReplaced;
        }



        /// <summary>
        /// Replace a span in a line with a string
        /// </summary>
        /// <param name="newWord">textToFind to reaplce span with</param>
        /// <param name="replaceSpan">span to be replaced</param>
        /// <param name="markReplacement">we are creating a marked replacement textToFind</param>
        /// <returns>returns true if succesfull</returns>
        internal bool ReplaceSpanWithText(string newWord, TextSpan replaceSpan, bool markReplacement)
        {
            IVsTextView textView = this.TextView;
            try
            {
                //either we arent marking this as a replacement so just replace the text
                //or we are marking it as a replacement so we need to make sure its not a reaplcement marker already
                if ((markReplacement && !IsSpanReplacement(replaceSpan)) || !markReplacement) //are we creating a replacement marker
                {
                    textView.ReplaceTextOnLine(replaceSpan.iStartLine,
                           replaceSpan.iStartIndex,
                           (replaceSpan.iEndIndex - replaceSpan.iStartIndex),
                           newWord,
                           newWord.Length);
                }

            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
        internal bool FindEnclosingReplacementQuoteSpan(TextSpan span, out TextSpan quoteSpan)
        {
            int line = span.iStartLine;
            int lineLength = LineLength(line);
            int left = span.iStartIndex - 1;
            int right = span.iEndIndex;
            quoteSpan = new TextSpan();
            while (left >= 0 && GetCharacterAtPosition(new TextPoint(line, left)) != ConstantStrings.DoubleQuoteString)
            {
                left--;
            }
            while (right < lineLength && GetCharacterAtPosition(new TextPoint(line, right)) != ConstantStrings.DoubleQuoteString)
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


        /// <summary>
        /// Based on where the user clicks or selects get the span of the chosen replacement
        /// This doesn't check if this span is in our replacement list just that it meets the criteria for a replacement
        /// </summary>
        /// <param name="replacementSpan">The span of the click on replcement</param>
        /// <returns>True if a replacement is found</returns>
        internal bool GetClickedOnReplacementSpan(out TextSpan replacementSpan)
        {

            replacementSpan = this.GetWordTextSpanFromCurrentPosition();
            TextSpan currentWordSpan = replacementSpan;

            string currentWord = this.GetSpanText(replacementSpan);

            if (String.IsNullOrEmpty(currentWord) == true)//you might have selected more than a word, so use what you selected
            {
                replacementSpan = this.Selection;
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



        /// <summary>
        /// Examines the span and the characters around it to see if it has the repalcement symbols around it arleady
        /// </summary>
        /// <param name="span">span to test</param>
        /// <returns>true or false</returns>
        internal bool IsSpanReplacement(TextSpan replaceSpan)
        {
            int length = LineLength(replaceSpan.iEndLine);
            //make sure there is room for this replacement
            if (replaceSpan.iStartIndex >= 0 && replaceSpan.iEndIndex <= length && (replaceSpan.iEndIndex - replaceSpan.iStartIndex) >= 1)
            {
                //see if replacement symbols surround this span
                if (GetCharacterAtPosition(new TextPoint(replaceSpan.iStartLine, replaceSpan.iStartIndex - 1)) == SnippetDesigner.ConstantStrings.SymbolReplacement &&
                    GetCharacterAtPosition(new TextPoint(replaceSpan.iEndLine, replaceSpan.iEndIndex)) == SnippetDesigner.ConstantStrings.SymbolReplacement
                    )
                {
                    return true;
                }
                //check the first and last characters of the span to see if they are the replacement symbols
                if (GetCharacterAtPosition(new TextPoint(replaceSpan.iStartLine, replaceSpan.iStartIndex)) == SnippetDesigner.ConstantStrings.SymbolReplacement &&
                    GetCharacterAtPosition(new TextPoint(replaceSpan.iEndLine, replaceSpan.iEndIndex - 1)) == SnippetDesigner.ConstantStrings.SymbolReplacement
                    )
                {
                    return true;
                }
            }
            return false;

        }

        /// <summary>
        ///Find a word starting at a specifc point in the buffer
        /// </summary>
        /// <param name="textToFind">textToFind to find</param>
        /// <param name="startPositon">the point to start the search at</param>
        /// <returns>The span of the textToFind</returns>
        internal bool FindReplaceableString(string word, TextPoint startPositon, out TextSpan returnSpan)
        {

            int lastLine = LineCount - 1;
            return FindReplaceableString(word, startPositon, new TextPoint(lastLine, LineLength(lastLine)), out returnSpan);

        }

        /// <summary>
        /// Find a word starting at a specifc point in the buffer and ending at a specifc point
        /// </summary>
        /// <param name="textToFind">textToFind</param>
        /// <param name="startPositon">the point to start the search at</param>
        /// <returns>The span of the textToFind</returns>
        internal bool FindReplaceableString(string textToFind, TextPoint startPositon, TextPoint endPositon, out TextSpan returnSpan)
        {
            if (startPositon == null || endPositon == null || textToFind == null)
            {
                returnSpan = new TextSpan();
                return false;
            }

            IVsTextLines textLines = this.TextLines;

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
                    lineLength = this.LineLength(line);
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
                    if (GetWordFromPosition(new TextPoint(line, index)) == textToFind ||
                        //or is this the text we are looking for 
                        textToFind[0] == SnippetDesigner.ConstantStrings.SymbolReplacement[0] && textToFind[textToFind.Length - 1] == SnippetDesigner.ConstantStrings.SymbolReplacement[0] ||
                        textToFind[0] == SnippetDesigner.ConstantStrings.DoubleQuoteString[0] && textToFind[textToFind.Length - 1] == SnippetDesigner.ConstantStrings.DoubleQuoteString[0]
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

        /// <summary>
        /// Gets the text contained in a span
        /// </summary>
        /// <param name="span">the span to get the text from</param>
        /// <returns>the text in teh span</returns>
        internal string GetSpanText(TextSpan span)
        {
            string word;
            this.TextLines.GetLineText(span.iStartLine, span.iStartIndex, span.iEndLine, span.iEndIndex, out word);
            return word;
        }

        /// <summary>
        /// Given a line number get the text on that line
        /// </summary>
        /// <param name="line">The line to get the text from</param>
        internal string GetLineText(int line)
        {

            if (line >= 0 && line < LineCount)
            {
                TextSpan span = new TextSpan();
                span.iStartIndex = 0;
                span.iEndIndex = LineLength(line);
                span.iEndLine = span.iStartLine = line;
                return GetSpanText(span);
            }
            else
            {
                return string.Empty;
            }

        }



        /// <summary>
        /// Gets the character found at a specifc cursor poistion
        /// </summary>
        /// <param name="positon">The position to find the character</param>
        /// <returns>The character found</returns>
        internal string GetCharacterAtPosition(TextPoint positon)
        {
            IVsTextLines textLines = this.TextLines;
            IVsTextView textView = this.TextView;
            string charAtPos;
            textLines.GetLineText(positon.Line, positon.Index, positon.Line, positon.Index + 1, out charAtPos);
            return charAtPos;
        }

        /// <summary>
        /// Gets the textToFind found at the current cursor poistion
        /// </summary>
        /// <returns>The word span found found</returns>
        internal TextSpan GetWordTextSpanFromCurrentPosition()
        {
            int line;
            int column;
            this.TextView.GetCaretPos(out line, out column);
            return GetWordTextSpanFromPosition(new TextPoint(line, column));
        }

        /// <summary>
        /// Gets the textToFind found at the current cursor poistion
        /// </summary>
        /// <returns>The word found</returns>
        internal string GetWordFromCurrentPosition()
        {
            int line;
            int column;
            this.TextView.GetCaretPos(out line, out column);
            return GetWordFromPosition(new TextPoint(line, column));
        }

        /// <summary>
        /// Gets the text found at a specifc cursor poistion
        /// </summary>
        /// <param name="positon">The position to find the textToFind</param>
        /// <returns>The word found at the position</returns>
        internal string GetWordFromPosition(TextPoint positon)
        {
            IVsTextLines textLines = this.TextLines;
            IVsTextView textView = this.TextView;

            TextSpan[] span = new TextSpan[1];
            textView.GetWordExtent(positon.Line, positon.Index, 0, span);
            string wordAtPos;
            textLines.GetLineText(span[0].iStartLine, span[0].iStartIndex, span[0].iEndLine, span[0].iEndIndex, out wordAtPos);
            return wordAtPos;
        }

        /// <summary>
        /// Gets the words text span found at a specifc cursor poistion
        /// </summary>
        /// <param name="positon">The position to find the textToFind</param>
        /// <returns>The word span for that position</returns>
        internal TextSpan GetWordTextSpanFromPosition(TextPoint positon)
        {
            if (positon == null)
                return new TextSpan();

            IVsTextView textView = this.TextView;
            TextSpan[] span = new TextSpan[1];
            textView.GetWordExtent(positon.Line, positon.Index, 0, span);
            return span[0];
        }


        #endregion



        /// <summary>
        /// Constructs the IVsCodeWindow and attaches an IVsTextBuffer
        /// </summary>
        /// <returns>S_OK if success</returns>
        private int CreateVsCodeWindow()
        {

            int hr = VSConstants.S_OK;
            Guid clsidVsCodeWindow = typeof(VsCodeWindowClass).GUID;
            Guid iidVsCodeWindow = typeof(IVsCodeWindow).GUID;
            Guid clsidVsTextBuffer = typeof(VsTextBufferClass).GUID;
            Guid iidVsTextLines = typeof(IVsTextLines).GUID;

            //create/site a VsTextBuffer object
            vsTextBuffer = (IVsTextLines)SnippetDesignerPackage.Instance.CreateInstance(ref clsidVsTextBuffer, ref iidVsTextLines, typeof(IVsTextLines));
            IObjectWithSite ows = (IObjectWithSite)vsTextBuffer;

            //set the site of the buffer to the parent editor
            ows.SetSite(codeWindowHost.ServiceProvider);

            // tell the text buffer to not attempt to try to figure out the language service on its own
            // we only want it to use the language service we explicitly tell it to use
            Guid VsBufferDetectLangSID = Microsoft.VisualStudio.Package.EditorFactory.GuidVSBufferDetectLangSid;
            IVsUserData vsUserData = (IVsUserData)vsTextBuffer;
            vsUserData.SetData(ref VsBufferDetectLangSID, false);


            // create/initialize a VsCodeWindow object
            vsCodeWindow = (IVsCodeWindow)SnippetDesignerPackage.Instance.CreateInstance(ref clsidVsCodeWindow, ref iidVsCodeWindow, typeof(IVsCodeWindow));

            //set readonly value based codewindowhosts readonly value
            uint readOnlyValue = 0;
            if (codeWindowHost.ReadOnlyCodeWindow)
            {
                readOnlyValue = (uint)TextViewInitFlags2.VIF_READONLY;
            }
   
            //set the inital view properties of the code window
            INITVIEW[] initView = new INITVIEW[1];
            initView[0].fSelectionMargin = 1;//use selection margin
            initView[0].fWidgetMargin = 0;//no widget margin
            initView[0].fDragDropMove = 1;//allow drag and drop of text
            initView[0].fVirtualSpace = 0;//no virtual space
            initView[0].IndentStyle = Microsoft.VisualStudio.TextManager.Interop.vsIndentStyle.vsIndentStyleDefault;

            IVsCodeWindowEx vsCodeWindowEx = (IVsCodeWindowEx)vsCodeWindow;
            hr = vsCodeWindowEx.Initialize((uint)_codewindowbehaviorflags.CWB_DISABLEDROPDOWNBAR | (uint)_codewindowbehaviorflags.CWB_DISABLESPLITTER,
                0, null, null,
                //tell codewindow which flags to use
                (uint)TextViewInitFlags.VIF_SET_WIDGET_MARGIN |
                (uint)TextViewInitFlags.VIF_SET_SELECTION_MARGIN |
                (uint)TextViewInitFlags.VIF_SET_VIRTUAL_SPACE |
                (uint)TextViewInitFlags.VIF_SET_DRAGDROPMOVE |
                (uint)TextViewInitFlags2.VIF_SUPPRESS_STATUS_BAR_UPDATE |
                (uint)TextViewInitFlags2.VIF_SUPPRESSBORDER |
                (uint)TextViewInitFlags2.VIF_SUPPRESSTRACKCHANGES |
                readOnlyValue |
                (uint)TextViewInitFlags2.VIF_SUPPRESSTRACKGOBACK,
                initView);
            //set the codewindows text buffer
            hr = vsCodeWindow.SetBuffer((IVsTextLines)vsTextBuffer);
            IVsWindowPane vsWindowPane = (IVsWindowPane)vsCodeWindow;
            //set the site of the codewindow at the parent edtiors service provider
            hr = vsWindowPane.SetSite(codeWindowHost.ServiceProvider);
            //create the codewindow as the size of the snippetExplorerForm its in
            hr = vsWindowPane.CreatePaneWindow(this.Handle, 0, 0, this.Parent.Size.Width, this.Parent.Size.Height, out hWndCodeWindow);

            //get the textview object
            IVsTextView vsTextView = this.TextView;


            //we are only getting events if the codewindowhost is the snippet editor
            if (snippetEditor != null)
            {
                // sink IVsTextViewEvents, so we can determine when a VsCodeWindow object actually has the focus.
                IConnectionPointContainer connptCntr = (IConnectionPointContainer)vsTextView;
                Guid riid = typeof(IVsTextViewEvents).GUID;
                
                //find the desired connection point
                connptCntr.FindConnectionPoint(ref riid, out textViewEventsConnectionPoint);
                //connect to this connection point to be advised of changes
                textViewEventsConnectionPoint.Advise(snippetEditor, out cookieTextViewEvents);


                // sink IVsTextLineEvents, so we can determine when the buffer is changed
                connptCntr = (IConnectionPointContainer)TextLines;
                riid = typeof(IVsTextLinesEvents).GUID;
                
                //find the desired connection point
                connptCntr.FindConnectionPoint(ref riid, out textLinesEventsConnectionPoint);
                //connect to this connection point to be advised of changes
                textLinesEventsConnectionPoint.Advise(snippetEditor, out cookieTextLineEvents);
                
            }

            return hr;
        }

        /// <summary>
        /// Keep the code window we created the right size when the window gets resized.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CodeWindow_SizeChanged(object sender, EventArgs e)
        {
            NativeMethods.SetWindowPos(hWndCodeWindow, IntPtr.Zero,
                0, 0,
                this.Width,
                this.Height, 0);
        }


    }
}
