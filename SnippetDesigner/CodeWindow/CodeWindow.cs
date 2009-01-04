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
                Guid langGuid = defaultLanguage;

                if (SnippetDesignerPackage.Instance.Settings.EnableColorization)
                {
                    //is this language in the hash
                    if (langServ.ContainsKey(lang))
                    {
                        langGuid = langServ[lang];
                    }
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

        public void HighlightSpan(TextSpan span, KindOfMarker markerType)
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

        public string GetSpanText(TextSpan span)
        {
            string word;
            this.TextLines.GetLineText(span.iStartLine, span.iStartIndex, span.iEndLine, span.iEndIndex, out word);
            return word;
        }

        public string GetLineText(int line)
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

        public string GetCharacterAtPosition(TextPoint positon)
        {
            IVsTextLines textLines = this.TextLines;
            IVsTextView textView = this.TextView;
            string charAtPos;
            textLines.GetLineText(positon.Line, positon.Index, positon.Line, positon.Index + 1, out charAtPos);
            return charAtPos;
        }

        public TextSpan GetWordTextSpanFromCurrentPosition()
        {
            int line;
            int column;
            this.TextView.GetCaretPos(out line, out column);
            return GetWordTextSpanFromPosition(new TextPoint(line, column));
        }

        public string GetWordFromCurrentPosition()
        {
            int line;
            int column;
            this.TextView.GetCaretPos(out line, out column);
            return GetWordFromPosition(new TextPoint(line, column));
        }

        public string GetWordFromPosition(TextPoint positon)
        {
            IVsTextLines textLines = this.TextLines;
            IVsTextView textView = this.TextView;

            TextSpan[] span = new TextSpan[1];
            textView.GetWordExtent(positon.Line, positon.Index, 0, span);
            string wordAtPos;
            textLines.GetLineText(span[0].iStartLine, span[0].iStartIndex, span[0].iEndLine, span[0].iEndIndex, out wordAtPos);
            return wordAtPos;
        }


        public TextSpan GetWordTextSpanFromPosition(TextPoint positon)
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
