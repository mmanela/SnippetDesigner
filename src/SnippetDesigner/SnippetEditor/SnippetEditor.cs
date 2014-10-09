using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Constants = EnvDTE.Constants;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Microsoft.SnippetDesigner
{
    /// <summary>
    /// The snippet codeWindowHost class.  This inherits the SnippetEditorForm which controls all the aestectics of how it looks
    /// and the rest of the interface are needed by VSIP to allow this to be both the DocData and DocView
    /// </summary>
    [ComVisible(true)]
    public sealed class SnippetEditor :
        SnippetEditorForm, // SnippetEditor GUI form
        ICodeWindowHost,
        IVsWindowPane, // To make this a doc window
        IOleCommandTarget, // To handle any commands that are passed by VS
        IOleServiceProvider,
        IVsPersistDocData,
        IVsTextViewEvents,
        IVsTextLinesEvents,
        IPersistFileFormat,
        IVsFileChangeEvents,
        IVsDocDataFileChangeControl,
        IVsFileBackup //to support backup of files. Visual Studio File Recovery 
    {
        private readonly SnippetDesignerPackage snippetDesignerPackage;
        private IVsTextView activeTextView;

        private SelectionContainer selContainer;
        private ITrackSelection trackSel;


        // this is the snippet format - a codeWindowHost can support multiple formats which would have
        // different values however we only supports one
        private const uint snippetFormat = 0;

        //the current file name
        private string fileName = string.Empty;

        //the name this file previous had.  This is needed for the manual rename in the running doc table
        private string previousFileName = string.Empty;

        private bool isDirty;
        private IVsFileChangeEx vsFileChangeEx;
        private bool backupObsolete = true;
        private bool fileChangedTimerSet;
        private Timer reloadTimer = new Timer();

        // Counter of the file system changes to ignore.
        private int changesToIgnore;

        // Cookie for the subscription to the file system notification events.
        private uint vsFileChangeCookie;

        private bool isFileNew; //this flag is used to see if this file is new or a previously saved file

        private bool loadDone; //set to true when whole laoding process is done
        //this is needed so we know when to start moinitroing text changes

        /// <summary>
        /// Service provider for codeiwndow to se
        /// </summary>
        public IOleServiceProvider ServiceProvider { get; private set; }

        /// <summary>
        /// Part of ICOdeWindowHost interface
        /// let the code window know we dont want it to be read only
        /// </summary>
        public bool ReadOnlyCodeWindow
        {
            get { return false; }
        }

        /// <summary>
        /// Return the object which keeps track of what the currently slected item is
        /// </summary>
        private ITrackSelection TrackSelection
        {
            get
            {
                if (trackSel == null)
                {
                    //get the trackselection service and return its interface
                    trackSel = (ITrackSelection) GetVsService(typeof (ITrackSelection));
                }
                return trackSel;
            }
        }

        /// <summary>
        /// Get the frame that contains ou codeWindowHost
        /// </summary>
        public IVsWindowFrame EditorFrame
        {
            get
            {
                //get service on the window frame for this codeWindowHost
                return GetVsService(typeof (SVsWindowFrame)) as IVsWindowFrame;
            }
        }


        /// <summary>
        /// Initialize the snippet codeWindowHost
        /// initialize private variables
        /// tell the codewindow that we are its parent so he can site us
        /// </summary>
        /// <param name="sacPackage"></param>
        public SnippetEditor(SnippetDesignerPackage package)
        {
            snippetDesignerPackage = package;
            vsFileChangeCookie = VSConstants.VSCOOKIE_NIL; //initialize the file change cookie to null

            changesToIgnore = 0; //start with no changes to ignore

            InitializePropertiesWindow(); //set up the properties window

            InitializeComponent(); //initialize gui components
            snippetCodeWindow.CodeWindowHost = this; //tell the code window this is its parent codeWindowHost

            logger = package.Logger;
        }


        /// <summary>
        /// Retrieves the requested service from the Shell.
        /// </summary>
        /// <param name="serviceType">Service that is being requested</param>
        /// <returns>An object which type is as requested</returns>
        public object GetVsService(Type serviceType)
        {
            if (ServiceProvider == null)
            {
                return null;
            }
            //create a generic service provider from the OleServiceProvider of visual studio
            ServiceProvider sp = new ServiceProvider(ServiceProvider, false);
            if (sp != null)
            {
                return sp.GetService(serviceType); //get the requested service
            }
            else
            {
                return null;
            }
        }


        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (snippetCodeWindow != null)
                    {
                        //dispose of code window
                        snippetCodeWindow.Dispose();
                        snippetCodeWindow = null;
                    }
                    if (reloadTimer != null)
                    {
                        reloadTimer.Dispose();
                        reloadTimer = null;
                    }
                    GC.SuppressFinalize(this);
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        /// <summary>
        /// Initialize the propery window by createing the codeWindowHost properties class and a selection container
        /// </summary>
        private void InitializePropertiesWindow()
        {
            trackSel = null;
            // Create an ArrayList to store the objects that can be selected
            ArrayList listObjects = new ArrayList();


            //Add a custom type provider to filter the properties
            TypeDescriptor.AddProvider(new FilteredPropertiesTypeDescriptorProvider(typeof (EditorProperties)), typeof (EditorProperties));

            // Create the object that will show the document's properties
            // on the properties window.
            EditorProperties prop = new EditorProperties(this);
            listObjects.Add(prop);

            // Create the SelectionContainer object.
            selContainer = new SelectionContainer(true, false);
            selContainer.SelectableObjects = listObjects;
            selContainer.SelectedObjects = listObjects;
        }

        /// <summary>
        /// Display the properties window since it's part of the snippet codeWindowHost
        /// </summary>
        private void ShowPropertiesWindow()
        {
            //show the properties window
            IVsUIShell vsShell = GetVsService(typeof (SVsUIShell)) as IVsUIShell;
            Guid propWinGuid = new Guid(Constants.vsWindowKindProperties);
            IVsWindowFrame propFrame = null;
            vsShell.FindToolWindow((uint) __VSFINDTOOLWIN.FTW_fForceCreate, ref propWinGuid, out propFrame);
            if (propFrame != null)
            {
                propFrame.Show();
            }
        }

        /// <summary>
        ///Force the properties window the refresh itself
        /// </summary>
        internal void RefreshPropertiesWindow()
        {
            if (TrackSelection != null && selContainer != null)
            {
                TrackSelection.OnSelectChange(selContainer);
            }
        }


        /// <summary>
        /// Sets the codeWindowHost up for a new blank snippet file and then
        /// checks if any data is being exported from another codeWindowHost
        /// </summary>
        private void InitializeNewSnippet()
        {
            // until someone change the file, we can consider it not dirty as
            // the user would be annoyed if we prompt him to save an empty file
            isDirty = false;
            isFileNew = true;
            //load from the export object
            LoadDataFromExport();

            object captionValue;
            //get caption and make it title without its extension
            EditorFrame.GetProperty((int) __VSFPROPID.VSFPROPID_OwnerCaption, out captionValue);
            ActiveSnippet.Title = SnippetTitle = Path.GetFileNameWithoutExtension(captionValue.ToString());

            //add titles to snippet titles property
            var titles = new CollectionWithEvents<string>();
            titles.Add(SnippetTitle);
            SnippetTitles = titles;
            SnippetAuthor = SnippetDesignerPackage.VSRegisteredName;

            //make sure title is in snippets data memory
            PushFieldsIntoActiveSnippet();
        }

        /// <summary>
        /// load the snippet from the exported code
        /// </summary>
        private void LoadDataFromExport()
        {
            //get the current export data object
            ExportToSnippetData exportData = snippetDesignerPackage.ExportSnippetData;
            if (exportData != null) //if this object isnt null
            {
                SnippetCode = exportData.Code; //read the code
                SnippetLanguage = exportData.Language; //read the language
                snippetDesignerPackage.ClearSnippetExportData(); //clear the export data
            }
        }


        /// <summary>
        /// Initialize the command filter needed for the context menus.
        /// </summary>
        public void SetupContextMenus()
        {
            CommandFilter filter = new CommandFilter(this);
            IOleCommandTarget originalFilter;
            ErrorHandler.ThrowOnFailure(CodeWindow.TextViewAdapter.AddCommandFilter(filter, out originalFilter));
            filter.Init(originalFilter);
        }


        /// <summary>
        /// Display the context menu for the snippet codeWindowHost where the user clicks
        /// </summary>
        public void ShowContextMenu()
        {
            // Get a reference to the UIShell.
            IVsUIShell uiShell = Package.GetGlobalService(typeof (SVsUIShell)) as IVsUIShell;
            if (null == uiShell)
            {
                return;
            }

            // Get the position of the cursor.
            Point currentCursorPosition = Cursor.Position;
            POINTS[] pnts = new POINTS[1];
            pnts[0].x = (short) currentCursorPosition.X;
            pnts[0].y = (short) currentCursorPosition.Y;

            // Show the menu.
            Guid menuGuid = GuidList.SnippetDesignerCmdSet;
            //tell the ui shell to show the context menu
            uiShell.ShowContextMenu(0, ref menuGuid, (int) PkgCmdIDList.SnippetContextMenu, pnts, snippetCodeWindow.TextViewAdapter as IOleCommandTarget);
        }

        /// <summary>
        /// Create a dialog box to use the save the snippet
        /// </summary>
        private void CreateSaveAsDialog()
        {
            string currLang = String.Empty;
            if (toolStripLanguageBox.SelectedIndex > -1)
            {
                currLang = toolStripLanguageBox.SelectedItem.ToString();
            }

            currLang = currLang.Trim();
            string initialFileName = string.Empty;
            if (snippetDirectories.ContainsKey(currLang))
            {
                initialFileName = snippetDirectories[currLang];
            }
            else
            {
                initialFileName = snippetDirectories[String.Empty];
            }

            if (isFileNew)
            {
                initialFileName = Path.Combine(initialFileName, SnippetTitle);
            }
            else
            {
                initialFileName += Path.Combine(initialFileName, Path.GetFileName(fileName));
            }


            int can;
            string fileaNameNew;
            IVsUIShell uiShell = Package.GetGlobalService(typeof (SVsUIShell)) as IVsUIShell;
            int hr = uiShell.SaveDocDataToFile(VSSAVEFLAGS.VSSAVE_SaveAs, this, initialFileName, out fileaNameNew, out can);
        }

        /// <summary>
        /// Save the snippet by passing the correct parameters into the generalsave function
        /// </summary>
        private void Save()
        {
            GeneralSave(fileName, true, 0);
        }

        /// <summary>
        /// Save the contents of the textbox into the specified file. If doing the save on the same file, we need to
        /// suspend notifications for file changes during the save operation.
        /// </summary>
        /// <param name="fileToLoad">Pointer to the file name. If the fileToLoad parameter is a null reference 
        /// we need to save using the current file
        /// </param>
        /// <param name="doSaveAs">Boolean value that indicates whether the fileNameToNotify parameter is to be used 
        /// as the current working file.
        /// If doSaveAs == true, fileNameToNotify needs to be made the current file and the dirty flag needs to be cleared after the save.
        ///                   Also, file notifications need to be enabled for the new file and disabled for the old file 
        /// If doSaveAs == false, this save operation is a Save a Copy As operation. In this case, 
        ///                   the current file is unchanged and dirty flag is not cleared
        /// </param>
        /// <param name="formatIndex">Zero based index into the list of formats that indicates the format in which 
        /// the file will be saved</param>
        /// <returns>S_OK if the method succeeds</returns>
        private int GeneralSave(string fileNameToSave, bool doSaveAs, uint formatIndex)
        {
            int hr = VSConstants.S_OK;
            bool doingSaveOnSameFile = false;
            // If file is null or same --> SAVE
            if (fileNameToSave == null || fileNameToSave == fileName)
            {
                doSaveAs = true;
                doingSaveOnSameFile = true;
            }

            //Suspend file change notifications for only Save since we don't have notifications setup
            //for SaveAs and SaveCopyAs (as they are different files)
            if (doingSaveOnSameFile)
            {
                SetFileChangeNotification(fileName, false);
            }

            try
            {
                if (doingSaveOnSameFile)
                {
                    SaveSnippet();
                }
                else
                {
                    SaveSnippetAs(fileNameToSave);
                }
            }
            catch (ArgumentException)
            {
                hr = VSConstants.E_FAIL;
            }
            catch (IOException)
            {
                hr = VSConstants.E_FAIL;
            }
            catch (UnauthorizedAccessException)
            {
                logger.MessageBox("Unable to access file", "Unable to write to " + fileName + "  Do you not have rights to access this file?  You might need to run VS as admin",
                                  LogType.Error);
                hr = VSConstants.E_FAIL;
            }
            finally
            {
                //restore the file change notifications
                if (doingSaveOnSameFile)
                {
                    SetFileChangeNotification(fileNameToSave, true);
                }
            }

            if (VSConstants.E_FAIL == hr)
            {
                return hr;
            }

            //Save and Save as
            if (doSaveAs)
            {
                //Save as
                if (null != fileNameToSave && !fileName.Equals(fileNameToSave))
                {
                    SetFileChangeNotification(fileName, false); //remove notification from old file
                    SetFileChangeNotification(fileNameToSave, true); //add notification for new file
                    previousFileName = fileName; //save previous filename
                    fileName = fileNameToSave; //store the new file name
                }
                isDirty = false;
                IsFormDirty = false;
                IVsTextBuffer buffer = snippetCodeWindow.TextBufferAdapter;
                buffer.SetStateFlags(0);
            }

            // Since all changes are now saved properly to disk, there's no need for a backup.
            backupObsolete = false;
            return hr;
        }

        #region IVsWindowPane Members

        public int ClosePane()
        {
            return VSConstants.S_OK;
        }

        public int CreatePaneWindow(IntPtr hwndParent, int x, int y, int cx, int cy, out IntPtr hwnd)
        {
            NativeMethods.SetParent(Handle, hwndParent);
            hwnd = Handle;
            Size = new Size(cx - x, cy - y);

            return VSConstants.S_OK;
        }

        public int GetDefaultSize(SIZE[] defaultSize)
        {
            if (defaultSize.Length >= 1)
            {
                defaultSize[0].cx = 300;
                defaultSize[0].cy = 200;
            }
            return VSConstants.S_OK;
        }

        public int LoadViewState(IStream loadStream)
        {
            return VSConstants.S_OK;
        }

        public int SaveViewState(IStream saveStream)
        {
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Called by the enviorment to provide us with our site
        /// This lets us call services that are important to the codeWindowHost
        /// </summary>
        /// <param name="psp"></param>
        /// <returns></returns>
        public int SetSite(IOleServiceProvider psp)
        {
            ServiceProvider = psp;
            //Guid to be used in SetGuidProperty as a ref parameter to tell frame that we want texteditor key bindings
            Guid cmdUI_TextEditor = GuidList.textEditorFactory;
            int hr = EditorFrame.SetGuidProperty((int) __VSFPROPID.VSFPROPID_InheritKeyBindings, ref cmdUI_TextEditor);

            return hr;
        }

        /// <summary>
        /// Send key commands to text view if it has focus
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public int TranslateAccelerator(MSG[] messagesToTranslate)
        {
            int hr = VSConstants.S_FALSE;
            if (messagesToTranslate == null)
            {
                return hr;
            }

            // defer to active code window
            if (activeTextView != null)
            {
                IVsWindowPane vsWindowPane = (IVsWindowPane) activeTextView;
                hr = vsWindowPane.TranslateAccelerator(messagesToTranslate);
            }
            else
            {
                switch (messagesToTranslate[0].message)
                {
                    case NativeMethods.WM_KEYDOWN:
                    case NativeMethods.WM_SYSKEYDOWN:
                    case NativeMethods.WM_CHAR:
                    case NativeMethods.WM_SYSCHAR:
                        {
                            Message msg = new Message();
                            msg.HWnd = messagesToTranslate[0].hwnd;
                            msg.Msg = (int) messagesToTranslate[0].message;
                            msg.LParam = messagesToTranslate[0].lParam;
                            msg.WParam = messagesToTranslate[0].wParam;

                            Control ctrl = FromChildHandle(msg.HWnd);
                            if (ctrl != null && ctrl.PreProcessMessage(ref msg))
                                hr = VSConstants.S_OK;
                        }
                        break;

                    default:
                        break;
                }
            }


            return hr;
        }

        #endregion

        #region File Change Notification Helpers

        /// <summary>
        /// In this function we inform the shell when we wish to receive 
        /// events when our file is changed or we inform the shell when 
        /// we wish not to receive events anymore.
        /// </summary>
        /// <param name="fileNameToNotify">File name string</param>
        /// <param name="startNotify">TRUE indicates advise, FALSE indicates unadvise.</param>
        /// <returns>Result of the operation</returns>
        private int SetFileChangeNotification(string fileNameToNotify, bool startNotify)
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "\t **** Inside SetFileChangeNotification ****"));

            int result = VSConstants.E_FAIL;

            //Get the File Change service
            if (null == vsFileChangeEx)
                vsFileChangeEx = (IVsFileChangeEx) GetVsService(typeof (SVsFileChangeEx));
            if (null == vsFileChangeEx)
                return VSConstants.E_UNEXPECTED;

            // Setup Notification if startNotify is TRUE, Remove if startNotify is FALSE.
            if (startNotify)
            {
                if (vsFileChangeCookie == VSConstants.VSCOOKIE_NIL)
                {
                    //Receive notifications if either the attributes of the file change or 
                    //if the size of the file changes or if the last modified time of the file changes
                    result = vsFileChangeEx.AdviseFileChange(fileNameToNotify,
                                                             (uint) (_VSFILECHANGEFLAGS.VSFILECHG_Attr | _VSFILECHANGEFLAGS.VSFILECHG_Size | _VSFILECHANGEFLAGS.VSFILECHG_Time),
                                                             this,
                                                             out vsFileChangeCookie);
                    if (vsFileChangeCookie == VSConstants.VSCOOKIE_NIL)
                    {
                        return VSConstants.E_FAIL;
                    }
                }
                result = VSConstants.S_OK;
            }
            else
            {
                if (vsFileChangeCookie != VSConstants.VSCOOKIE_NIL)
                {
                    //if we want to unadvise and the cookieTextViewEvents isnt null then unadvise changes
                    result = vsFileChangeEx.UnadviseFileChange(vsFileChangeCookie);
                    vsFileChangeCookie = VSConstants.VSCOOKIE_NIL;
                    result = VSConstants.S_OK;
                }
            }
            return result;
        }

        /// <summary>
        /// Notify the codeWindowHost of the changes made to a directory
        /// </summary>
        /// <param name="pszDirectory">Name of the directory that has changed</param>
        /// <returns></returns>
        int IVsFileChangeEvents.DirectoryChanged(string pszDirectory)
        {
            //Nothing to do here
            return VSConstants.S_OK;
        }

        #endregion

        #region IOleCommandTarget Members

        /// <summary>
        /// Exec is called by the shell whenever a command is issued to the codeWindowHost.
        /// In this function we need to either handle the command and do the appropriate action
        /// or return OLECMDERR_E_NOTSUPPORTED to tell the shell to find someone above us to handle it
        /// </summary>
        /// <param name="commandGroup">group of the command</param>
        /// <param name="commandID">id of the command</param>
        /// <param name="commandOption"></param>
        /// <param name="pvaIn"></param>
        /// <param name="pvaOut"></param>
        /// <returns></returns>
        public int Exec(ref Guid commandGroup, uint commandID, uint commandOption, IntPtr pvaIn, IntPtr pvaOut)
        {
            int hr = (int) VisualStudio.OLE.Interop.Constants.OLECMDERR_E_NOTSUPPORTED;
            if (commandGroup == VSConstants.GUID_VSStandardCommandSet97)
            {
                switch (commandID)
                {
                    case (uint) VSConstants.VSStd97CmdID.Cut:
                    case (uint) VSConstants.VSStd97CmdID.Copy:
                    case (uint) VSConstants.VSStd97CmdID.Paste:
                        {
                            if (activeTextView == null)
                            {
                                //catch the cut copy and paste messages sent to grid view
                                return VSConstants.S_OK;
                            }
                            break;
                        }

                    case (uint) VSConstants.VSStd97CmdID.SaveProjectItem:
                    case (uint) VSConstants.VSStd97CmdID.Save:
                        {
                            //is this a new file
                            if (isFileNew)
                            {
                                //show a save dialog
                                CreateSaveAsDialog();
                            }
                            else
                            {
                                //no save dialog needed just save
                                Save();
                            }

                            return VSConstants.S_OK;
                        }
                    case (uint) VSConstants.VSStd97CmdID.SaveProjectItemAs:
                    case (uint) VSConstants.VSStd97CmdID.SaveAs:
                        {
                            //show a save dialog
                            CreateSaveAsDialog();
                            return VSConstants.S_OK;
                        }
                    default:
                        break;
                }
            }

            //Check if the activeTextView is not null, if it isnt then the codewindow has focus
            //when the code window has focus we want to pass commands we get to it
            //this lets the codewindow get keystrokes
            if (activeTextView != null)
            {
                IOleCommandTarget cmdTarget = (IOleCommandTarget) activeTextView;
                hr = cmdTarget.Exec(ref commandGroup, commandID, commandOption, pvaIn, pvaOut);
            }


            return hr;
        }

        /// <summary>
        /// QueryStatus is called by the shell to see which commands we support
        /// </summary>
        /// <param name="commandGroup">Group the command is in</param>
        /// <param name="commandCount"></param>
        /// <param name="prgCmds">array of command information</param>
        /// <param name="cmdText"></param>
        /// <returns></returns>
        public int QueryStatus(ref Guid commandGroup, uint commandCount, OLECMD[] prgCmds, IntPtr cmdText)
        {
            int hr = (int) VisualStudio.OLE.Interop.Constants.OLECMDERR_E_NOTSUPPORTED;


            //Check if the activeTextView is not null, if it isnt then the codewindow has focus
            //when the code window has focus we want to query its status of the current command
            if (activeTextView != null)
            {
                IOleCommandTarget cmdTarget = (IOleCommandTarget) activeTextView;
                hr = cmdTarget.QueryStatus(ref commandGroup, commandCount, prgCmds, cmdText);
            }

            return hr;
        }

        #endregion

        #region IOleServiceProvider Members

        /// <summary>
        /// If we are asked for a service just pass the request onto our service provider vsServiceProvider
        /// </summary>
        /// <param name="guidService"></param>
        /// <param name="riid"></param>
        /// <param name="ppvObject"></param>
        /// <returns></returns>
        public int QueryService(ref Guid guidService, ref Guid riid, out IntPtr ppvObject)
        {
            if (ServiceProvider != null)
                return ServiceProvider.QueryService(ref guidService, ref riid, out ppvObject);
            else
            {
                ppvObject = IntPtr.Zero;
                Debug.WriteLine(guidService.ToString());
                return VSConstants.E_NOINTERFACE;
            }
        }

        #endregion

        #region IPersistFileFormat Members

        /// <summary>
        /// Get the ID of this codeWindowHost which is the editorFactory guid
        /// </summary>
        /// <param name="classID"></param>
        /// <returns></returns>
        int IPersist.GetClassID(out Guid classID)
        {
            classID = GuidList.snippetEditorFactory;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Notifies the object that it has concluded the Save transaction
        /// In this method we compare the new filename with the previous one
        /// if they have changed then we need to update the running document table with this change
        /// </summary>
        /// <param name="fileToLoad">Pointer to the file name</param>
        /// <returns>S_OK if the funtion succeeds</returns>
        int IPersistFileFormat.SaveCompleted(string fileSaved)
        {
            //Make sure we just did a save as or a save n a new file otherwise the following isnt needed
            if (previousFileName.Length > 0 && fileSaved.Length > 0 && previousFileName != fileSaved)
            {
                // Get a reference to the Running Document Table
                IVsRunningDocumentTable runningDocTable = (IVsRunningDocumentTable) GetVsService(typeof (SVsRunningDocumentTable));
                int hr = VSConstants.S_OK;

                // Lock the document and get the documents information
                uint docCookie;
                IVsHierarchy hierarchy;
                uint itemID;
                IntPtr docData;
                hr = runningDocTable.FindAndLockDocument(
                    (uint) _VSRDTFLAGS.RDT_ReadLock,
                    previousFileName,
                    out hierarchy,
                    out itemID,
                    out docData,
                    out docCookie
                    );

                IntPtr hier = Marshal.GetComInterfaceForObject(hierarchy, typeof (IVsHierarchy));

                //Because we are handling the save ourselves we break some of the things auotmatically handled by the RDT
                //for example when we save as the file name in the RDT wont be updated to the new file
                // to fix this we just rename the file in the RDT as seen below
                hr = runningDocTable.RenameDocument(previousFileName, fileSaved, hier, itemID);
                if (isFileNew) //is this a files first save
                {
                    //if this is a new file and its first save then we have to do something special here
                    //since we are handling the save manually some thing in the rdts so get propagated correctly
                    //so we need to tell the RDT that this file is not a temp file otherwise it will think
                    // the recently renamed file is still a temporay file that is bad
                    hierarchy.SetProperty(itemID, (int) __VSHPROPID.VSHPROPID_IsNewUnsavedItem, false);
                    isFileNew = false; //this is no longer the files first save
                }

                //the reason we need this is not clear right now but when we have a new file in its tab caption
                //isnt updated so make sure it is updated
                EditorFrame.SetProperty((int) __VSFPROPID.VSFPROPID_OwnerCaption, Path.GetFileName(fileName));

                // Unlock the document.
                // Note that we have to unlock the document even if the previous call failed.
                runningDocTable.UnlockDocument((uint) _VSRDTFLAGS.RDT_ReadLock, docCookie);
                //release reference to IVsHierarchy intptr
                Marshal.Release(hier);
                // Check ff the call to NotifyDocChanged failed.
                ErrorHandler.ThrowOnFailure(hr);
            }
            return VSConstants.S_OK;
        }


        /// <summary>
        /// Returns the path to the object's current working file 
        /// </summary>
        /// <param name="currentFileName">Pointer to the file name</param>
        /// <param name="formatIndex">Value that indicates the current format of the file as a zero based index
        /// into the list of formats. Since we support only a single format, we need to return zero. 
        /// Subsequently, we will return a single element in the format list through a call to GetFormatList.</param>
        /// <returns></returns>
        int IPersistFileFormat.GetCurFile(out string currentFileName, out uint formatIndex)
        {
            // We only support 1 format so return its index
            formatIndex = snippetFormat;
            currentFileName = fileName;
            return VSConstants.S_OK;
        }


        /// <summary>
        /// Initialization for the object 
        /// </summary>
        /// <param name="formatIndex">Zero based index into the list of formats that indicates the current format 
        /// of the file</param>
        int IPersistFileFormat.InitNew(uint formatIndex)
        {
            if (formatIndex != snippetFormat)
            {
                throw new ArgumentException(Resources.UnknownFileFormat);
            }

            //initialize the new snippet
            InitializeNewSnippet();


            return VSConstants.S_OK;
        }


        /// <summary>
        /// Returns the class identifier of the codeWindowHost type
        /// </summary>
        /// <param name="classID">pointer to the class identifier</param>
        /// <returns>S_OK if the method succeeds</returns>
        int IPersistFileFormat.GetClassID(out Guid classID)
        {
            ((IPersist) this).GetClassID(out classID);
            return VSConstants.S_OK;
        }


        /// <summary>
        /// Provides the caller with the information necessary to open the standard common "Save As" dialog box. 
        /// This returns an enumeration of supported formats, from which the caller selects the appropriate format. 
        /// Each string for the format is terminated with a newline (\n) character. 
        /// The last string in the buffer must be terminated with the newline character as well. 
        /// The first string in each pair is a display string that describes the filter, such as "Text Only 
        /// (*.txt)". The second string specifies the filter pattern, such as "*.txt". To specify multiple filter 
        /// patterns for a single display string, use a semicolon to separate the patterns: "*.htm;*.html;*.asp". 
        /// A pattern string can be a combination of valid file name characters and the asterisk (*) wildcard character. 
        /// Do not include spaces in the pattern string. The following string is an example of a file pattern string: 
        /// "HTML File (*.htm; *.html; *.asp)\n*.htm;*.html;*.asp\nText File (*.txt)\n*.txt\n."
        /// </summary>
        /// <param name="formatList">Pointer to a string that contains pairs of format filter strings</param>
        /// <returns>S_OK if the method succeeds</returns>
        int IPersistFileFormat.GetFormatList(out string formatList)
        {
            formatList = Resources.EditorFormatString;
            return VSConstants.S_OK;
        }


        /// <summary>
        /// Loads the file content
        /// </summary>
        /// <param name="fileToLoad">the full path name of the file to load</param>
        /// <param name="formatMode">file format mode</param>
        /// <param name="isReadOnly">determines if the file should be opened as read only</param>
        /// <returns>S_OK if the method succeeds</returns>
        int IPersistFileFormat.Load(string fileToLoad, uint formatMode, int isReadOnly)
        {
            if (fileToLoad == null)
            {
                return VSConstants.E_INVALIDARG;
            }

            int hr = VSConstants.S_OK;
            try
            {
                // Show the wait cursor while loading the file
                IVsUIShell VsUiShell = (IVsUIShell) GetService(typeof (SVsUIShell));
                if (VsUiShell != null)
                {
                    // Note: we don't want to throw or exit if this call fails, so
                    // don't check the return code.
                    hr = VsUiShell.SetWaitCursor();
                }

                // Load the file
                try
                {
                    LoadSnippet(fileToLoad);
                }
                catch (IOException)
                {
                    Trace.WriteLine(Resources.ErrorFileIO);
                    throw; //throw the excpetion so vs handles it
                }

                RefreshReplacementMarkers();
                //clear and show all markers

                isDirty = false; //the file is not dirty since we just loaded it
                //clear the buffer dirty flag, this stops the * from appearing after we load
                //it doesnt make sense to call a file dirty when you first load it 
                IVsTextBuffer buffer = snippetCodeWindow.TextBufferAdapter;
                buffer.SetStateFlags(0);


                // Hook up to file change notifications
                if (String.IsNullOrEmpty(fileName) || 0 != String.Compare(fileName, fileToLoad, true, CultureInfo.CurrentCulture))
                {
                    fileName = fileToLoad;
                    SetFileChangeNotification(fileToLoad, true);

                    // Notify the load or reload
                    NotifyDocChanged();
                }
            }
            finally
            {
                RefreshPropertiesWindow();
            }
            //create the properties
            ShowPropertiesWindow();
            loadDone = true;

            return VSConstants.S_OK;
        }


        /// <summary>
        /// Determines whether an object has changed since being saved to its current file
        /// </summary>
        /// <param name="dirty">true if the document has changed</param>
        /// <returns>S_OK if the method succeeds</returns>
        int IPersistFileFormat.IsDirty(out int dirty)
        {
            IVsPersistDocData bufferDoc = (IVsPersistDocData) snippetCodeWindow.TextBufferAdapter;
            if(bufferDoc == null)
            {
                dirty = 0;
                return VSConstants.S_OK;
            }
            int codeWindowDirty = 0;
            bufferDoc.IsDocDataDirty(out codeWindowDirty);

            if (isDirty || codeWindowDirty == 1 || IsFormDirty)
            {
                dirty = 1;
            }
            else
            {
                dirty = 0;
            }
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Save the contents of the textbox into the specified file. If doing the save on the same file, we need to
        /// suspend notifications for file changes during the save operation.
        /// </summary>
        /// <param name="fileNameToSave">Pointer to the file name. If the fileNameToSave parameter is a null reference 
        /// we need to save using the current file
        /// </param>
        /// <param name="savaType">Boolean value that indicates whether the fileNameToSave parameter is to be used 
        /// as the current working file.
        /// If savaType != 0, fileNameToSave needs to be made the current file and the dirty flag needs to be cleared after the save.
        ///                   Also, file notifications need to be enabled for the new file and disabled for the old file 
        /// If savaType == 0, this save operation is a Save a Copy As operation. In this case, 
        ///                   the current file is unchanged and dirty flag is not cleared
        /// </param>
        /// <param name="formatIndex">Zero based index into the list of formats that indicates the format in which 
        /// the file will be saved</param>
        /// <returns>S_OK if the method succeeds</returns>
        int IPersistFileFormat.Save(string fileNameToSave, int savaType, uint formatIndex)
        {
            bool doSaveAs = true;
            if (savaType == 0)
            {
                doSaveAs = false;
            }

            //convert savaType into a bool
            return GeneralSave(fileNameToSave, doSaveAs, formatIndex);
        }

        #endregion

        #region IVsPersistDocData Members

        /// <summary>
        /// Used to determine if the document data has changed since the last time it was saved
        /// </summary>
        /// <param name="dirty">Will be set to 1 if the data has changed</param>
        /// <returns>S_OK if the function succeeds</returns>
        int IVsPersistDocData.IsDocDataDirty(out int dirty)
        {
            return ((IPersistFileFormat) this).IsDirty(out dirty);
        }


        /// <summary>
        /// Saves the document data. Before actually saving the file, we first need to indicate to the environment
        /// that a file is about to be saved. This is done through the "SVsQueryEditQuerySave" service. We call the
        /// "QuerySaveFile" function on the service instance and then proceed depending on the result returned as follows:
        /// If result is QSR_SaveOK - We go ahead and save the file and the file is not read only at this point.
        /// If result is QSR_ForceSaveAs - We invoke the "Save As" functionality which will bring up the Save file name 
        ///                                dialog 
        /// If result is QSR_NoSave_Cancel - We cancel the save operation and indicate that the document could not be saved
        ///                                by setting the "saveCanceled" flag
        /// If result is QSR_NoSave_Continue - Nothing to do here as the file need not be saved
        /// </summary>
        /// <param name="saveFlag">Flags which specify the file save options:
        /// VSSAVE_Save        - Saves the current file to itself.
        /// VSSAVE_SaveAs      - Prompts the User for a filename and saves the file to the file specified.
        /// VSSAVE_SaveCopyAs  - Prompts the user for a filename and saves a copy of the file with a name specified.
        /// VSSAVE_SilentSave  - Saves the file without prompting for a name or confirmation.  
        /// </param>
        /// <param name="newFilePath">The path to the new document</param>
        /// <param name="saveCanceled">value 1 if the document could not be saved</param>
        /// <returns></returns>
        int IVsPersistDocData.SaveDocData(VSSAVEFLAGS saveFlag, out string newFilePath, out int saveCanceled)
        {
            newFilePath = null;
            saveCanceled = 0;
            int hr = VSConstants.S_OK;

            switch (saveFlag)
            {
                case VSSAVEFLAGS.VSSAVE_Save:
                case VSSAVEFLAGS.VSSAVE_SilentSave:
                    {
                        IVsQueryEditQuerySave2 queryEditQuerySave = (IVsQueryEditQuerySave2) GetVsService(typeof (SVsQueryEditQuerySave));

                        // Call QueryEditQuerySave
                        uint result = 0;
                        hr = queryEditQuerySave.QuerySaveFile(
                            fileName,
                            // filename
                            0,
                            // flags
                            null,
                            // file attributes
                            out result); // result
                        if (ErrorHandler.Failed(hr))
                            return hr;

                        // Process according to result from QuerySave
                        switch ((tagVSQuerySaveResult) result)
                        {
                            case tagVSQuerySaveResult.QSR_NoSave_Cancel:
                                // Note that this is also case tagVSQuerySaveResult.QSR_NoSave_UserCanceled because these
                                // two tags have the same value.
                                saveCanceled = ~0;
                                break;

                            case tagVSQuerySaveResult.QSR_SaveOK:
                                {
                                    // Call the shell to do the save for us
                                    IVsUIShell uiShell = (IVsUIShell) GetVsService(typeof (SVsUIShell));
                                    hr = uiShell.SaveDocDataToFile(saveFlag, this, fileName, out newFilePath, out saveCanceled);
                                    if (ErrorHandler.Failed(hr))
                                        return hr;
                                }
                                break;

                            case tagVSQuerySaveResult.QSR_ForceSaveAs:
                                {
                                    // Call the shell to do the SaveAS for us
                                    IVsUIShell uiShell = (IVsUIShell) GetVsService(typeof (SVsUIShell));
                                    hr = uiShell.SaveDocDataToFile(VSSAVEFLAGS.VSSAVE_SaveAs, this, fileName, out newFilePath, out saveCanceled);
                                    if (ErrorHandler.Failed(hr))
                                        return hr;
                                }
                                break;

                            case tagVSQuerySaveResult.QSR_NoSave_Continue:
                                // In this case there is nothing to do.
                                break;

                            default:
                                throw new COMException(Resources.SCCError);
                        }
                        break;
                    }
                case VSSAVEFLAGS.VSSAVE_SaveAs:
                case VSSAVEFLAGS.VSSAVE_SaveCopyAs:
                    {
                        // Make sure the file name as the right extension
                        if (string.Compare(StringConstants.SnippetExtension, Path.GetExtension(fileName), true, CultureInfo.InvariantCulture) != 0)
                        {
                            fileName += StringConstants.SnippetExtension;
                        }
                        // Call the shell to do the save for us
                        IVsUIShell uiShell = (IVsUIShell) GetVsService(typeof (SVsUIShell));
                        hr = uiShell.SaveDocDataToFile(saveFlag, this, fileName, out newFilePath, out saveCanceled);
                        if (ErrorHandler.Failed(hr))
                            return hr;
                        break;
                    }
                default:
                    throw new ArgumentException(Resources.BadSaveFlags);
            }
            ;

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Loads the document data from the file specified
        /// Set its buffer moniker to a random guid to make sure that the language services
        /// can tell the difference between multiple code windows.  The c# and J# language services
        /// compare bufffer monikers and since our filename isnt the monkiker since the buffer is in memory
        /// we must add unique random one
        /// </summary>
        /// <param name="fileToLoad">Path to the document file which needs to be loaded</param>
        /// <returns>S_Ok if the method succeeds</returns>
        int IVsPersistDocData.LoadDocData(string fileToLoad)
        {
            //set the buffer moniker
            IVsUserData udata = (IVsUserData) CodeWindow.TextBufferAdapter;
            //generate random gui
            string uniqueMoniker = Guid.NewGuid().ToString();
            //guid for buffer moniker property
            Guid bufferMonikerGuid = typeof (IVsUserData).GUID;
            //set the moniker
            udata.SetData(ref bufferMonikerGuid, uniqueMoniker);

            //continue with load of the document
            return ((IPersistFileFormat) this).Load(fileToLoad, 0, 0);
        }

        /// <summary>
        /// Used to set the initial name for unsaved, newly created document data
        /// </summary>
        /// <param name="pszDocDataPath">String containing the path to the document. We need to ignore this parameter
        /// </param>
        /// <returns>S_OK if the mthod succeeds</returns>
        int IVsPersistDocData.SetUntitledDocPath(string pszDocDataPath)
        {
            return ((IPersistFileFormat) this).InitNew(snippetFormat);
        }


        /// <summary>
        /// Returns the Guid of the codeWindowHost factory that created the IVsPersistDocData object
        /// </summary>
        /// <param name="classID">Pointer to the class identifier of the codeWindowHost type</param>
        /// <returns>S_OK if the method succeeds</returns>
        int IVsPersistDocData.GetGuidEditorType(out Guid classID)
        {
            return ((IPersistFileFormat) this).GetClassID(out classID);
        }


        /// <summary>
        /// Close the IVsPersistDocData object
        /// </summary>
        /// <returns>S_OK if the function succeeds</returns>
        int IVsPersistDocData.Close()
        {
            //we are closing this file so we dont want to be notified about it anymore
            SetFileChangeNotification(fileName, false);
            if (snippetCodeWindow != null)
            {
                snippetCodeWindow.Dispose();
            }
            return VSConstants.S_OK;
        }


        /// <summary>
        /// Determines if it is possible to reload the document data
        /// </summary>
        /// <param name="isReloadable">set to 1 if the document can be reloaded</param>
        /// <returns>S_OK if the method succeeds</returns>
        int IVsPersistDocData.IsDocDataReloadable(out int isReloadable)
        {
            // Allow file to be reloaded
            isReloadable = 1;
            return VSConstants.S_OK;
        }


        /// <summary>
        /// Renames the document data
        /// </summary>
        /// <param name="attributes"></param>
        /// <param name="hierNew"></param>
        /// <param name="itemidNew"></param>
        /// <param name="newFileName"></param>
        /// <returns></returns>
        int IVsPersistDocData.RenameDocData(uint attributes, IVsHierarchy hierNew, uint itemidNew, string newFileName)
        {
            // TODO:  Add EditorPane.RenameDocData implementation
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Reloads the document data
        /// </summary>
        /// <param name="ignoreNextChange">Flag indicating whether to ignore the next file change when reloading the document data.
        /// This flag should not be set for us since we implement the "IVsDocDataFileChangeControl" interface in order to 
        /// indicate ignoring of file changes
        /// </param>
        /// <returns>S_OK if the mthod succeeds</returns>
        int IVsPersistDocData.ReloadDocData(uint ignoreNextChange)
        {
            return ((IPersistFileFormat) this).Load(fileName, ignoreNextChange, 0);
        }

        /// <summary>
        /// Called by the Running Document Table when it registers the document data. 
        /// </summary>
        /// <param name="docCookie">Handle for the document to be registered</param>
        /// <param name="hierNew">Pointer to the IVsHierarchy interface</param>
        /// <param name="itemidNew">Item identifier of the document to be registered from VSITEM</param>
        /// <returns></returns>
        int IVsPersistDocData.OnRegisterDocData(uint docCookie, IVsHierarchy hierNew, uint itemidNew)
        {
            return VSConstants.S_OK;
        }

        #endregion

        #region IVsFileChangeEvents

        /// <summary>
        /// Gets an instance of the RunningDocumentTable (RDT) service which manages the set of currently open 
        /// documents in the environment and then notifies the client that an open document has changed
        /// </summary>
        private void NotifyDocChanged()
        {
            // Make sure that we have a file name
            if (fileName.Length == 0)
                return;

            // Get a reference to the Running Document Table
            IVsRunningDocumentTable runningDocTable = (IVsRunningDocumentTable) GetVsService(typeof (SVsRunningDocumentTable));
            // Lock the document
            uint docCookie;
            IVsHierarchy hierarchy;
            uint itemID;
            IntPtr docData;
            int hr = runningDocTable.FindAndLockDocument(
                (uint) _VSRDTFLAGS.RDT_ReadLock,
                fileName,
                out hierarchy,
                out itemID,
                out docData,
                out docCookie
                );

            ErrorHandler.ThrowOnFailure(hr);

            // Send the notification
            hr = runningDocTable.NotifyDocumentChanged(docCookie, (uint) __VSRDTATTRIB.RDTA_DocDataReloaded);

            // Unlock the document.
            // Note that we have to unlock the document even if the previous call failed.
            runningDocTable.UnlockDocument((uint) _VSRDTFLAGS.RDT_ReadLock, docCookie);

            // Check ff the call to NotifyDocChanged failed.
            ErrorHandler.ThrowOnFailure(hr);
        }

        /// <summary>
        /// Notify the codeWindowHost of the changes made to one or more files
        /// </summary>
        /// <param name="numberOfChanges">Number of files that have changed</param>
        /// <param name="filesChanged">array of the files names that have changed</param>
        /// <param name="typesOfChanges">Array of the flags indicating the type of changes</param>
        /// <returns></returns>
        int IVsFileChangeEvents.FilesChanged(uint numberOfChanges, string[] filesChanged, uint[] typesOfChanges)
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "\t**** Inside FilesChanged ****"));

            //check the different parameters
            if (0 == numberOfChanges || null == filesChanged || null == typesOfChanges)
                return VSConstants.E_INVALIDARG;

            //ignore file changes if we are in that mode
            if (changesToIgnore != 0)
                return VSConstants.S_OK;

            for (uint i = 0; i < numberOfChanges; i++)
            {
                if (!String.IsNullOrEmpty(filesChanged[i]) && String.Compare(filesChanged[i], fileName, true, CultureInfo.CurrentCulture) == 0)
                {
                    // if it looks like the file contents have changed (either the size or the modified
                    // time has changed) then we need to prompt the user to see if we should reload the
                    // file. it is important to not syncronisly reload the file inside of this FilesChanged
                    // notification. first it is possible that there will be more than one FilesChanged 
                    // notification being sent (sometimes you get separate notifications for file attribute
                    // changing and file size/time changing). also it is the preferred UI style to not
                    // prompt the user until the user re-activates the environment application window.
                    // this is why we use a timer to delay prompting the user.
                    if (0 != (typesOfChanges[i] & (int) (_VSFILECHANGEFLAGS.VSFILECHG_Time | _VSFILECHANGEFLAGS.VSFILECHG_Size)))
                    {
                        if (!fileChangedTimerSet)
                        {
                            reloadTimer = new Timer();
                            fileChangedTimerSet = true;
                            reloadTimer.Interval = 1000;
                            reloadTimer.Tick += OnFileChangeEvent;
                            reloadTimer.Enabled = true;
                        }
                    }
                }
            }

            return VSConstants.S_OK;
        }

        /// <summary>
        /// This event is triggered when one of the files loaded into the environment has changed outside of the
        /// codeWindowHost
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFileChangeEvent(object sender, EventArgs e)
        {
            //Disable the timer
            reloadTimer.Enabled = false;
            // string message = this.GetResourceString("@101");    //get the message string from the resource
            string message = fileName + Environment.NewLine + Environment.NewLine + Resources.OutsideEditorFileChange;

            string title = String.Empty;
            IVsUIShell VsUiShell = (IVsUIShell) GetVsService(typeof (SVsUIShell));
            int result = 0;
            Guid tempGuid = Guid.Empty;
            if (VsUiShell != null)
            {
                //Show up a message box indicating that the file has changed outside of VS environment
                VsUiShell.ShowMessageBox(0,
                                         ref tempGuid,
                                         title,
                                         message,
                                         null,
                                         0,
                                         OLEMSGBUTTON.OLEMSGBUTTON_YESNOCANCEL,
                                         OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                                         OLEMSGICON.OLEMSGICON_QUERY,
                                         0,
                                         out result);
            }
            //if the user selects "Yes", reload the current file
            if (result == (int) DialogResult.Yes)
            {
                ((IVsPersistDocData) this).ReloadDocData(0);
            }

            fileChangedTimerSet = false;
        }

        #endregion

        #region IVsTextViewEvents Members

        public void OnChangeCaretLine(IVsTextView view, int newLine, int oldLine)
        {
        }

        public void OnChangeScrollInfo(IVsTextView view, int bar, int minUnit, int maxUnits, int visibleUnits, int firstVisibleUnit)
        {
        }

        public void OnKillFocus(IVsTextView viewFocusLost)
        {
            activeTextView = null;
        }

        public void OnSetBuffer(IVsTextView setView, IVsTextLines setBuffer)
        {
        }

        /// <summary>
        /// When the textview we are monitoring gets focus this is called
        /// </summary>
        /// <param name="focusedView"></param>
        public void OnSetFocus(IVsTextView focusedView)
        {
            //make sure codewindow is told it has focus
            //it doesnt always know it has focus cause the IVTextView will gobble up the info
            CodeWindow.Select();
            activeTextView = focusedView;
            RefreshReplacementMarkers();
        }

        #endregion

        #region IVsTextLinesEvents

        void IVsTextLinesEvents.OnChangeLineAttributes(int firstLine, int lastLine)
        {
        }

        void IVsTextLinesEvents.OnChangeLineText(TextLineChange[] textLineChanges, int last)
        {
            if (!loadDone)
            {
                return;
            }


            int startIndex = textLineChanges[0].iStartIndex;
            int endIndex = textLineChanges[0].iNewEndIndex;
            if (endIndex - startIndex == SnippetDelimiter.Length)
            {
                lastCharacterEntered = CodeWindow.GetCharacterAtPosition(new TextPoint(textLineChanges[0].iStartLine, startIndex));
            }
            else
            {
                lastCharacterEntered = null;
            }

            RefreshReplacementMarkers();
        }

        #endregion

        #region IVsDocDataFileChangeControl

        /// <summary>
        /// Called by the shell to notify if a file change must be ignored.
        /// </summary>
        /// <param name="ignoreFlag">Flag not zero if the file change must be ignored.</param>
        int IVsDocDataFileChangeControl.IgnoreFileChanges(int ignoreFlag)
        {
            if (0 != ignoreFlag)
            {
                // The changes must be ignored, so increase the counter of changes to ignore
                ++changesToIgnore;
            }
            else
            {
                if (changesToIgnore > 0)
                {
                    --changesToIgnore;
                }
            }

            return VSConstants.S_OK;
        }

        #endregion

        #region IVsFileBackup Members

        /// <summary>
        /// This method is used to Persist the data to a single file. On a successful backup this 
        /// should clear up the backup dirty bit
        /// </summary>
        /// <param name="pszBackupFileName">Name of the file to persist</param>
        /// <returns>S_OK if the data can be successfully persisted.
        /// This should return STG_S_DATALOSS or STG_E_INVALIDCODEPAGE if there is no way to 
        /// persist to a file without data loss
        /// </returns>
        int IVsFileBackup.BackupFile(string pszBackupFileName)
        {
            try
            {
                SaveSnippetAs(pszBackupFileName);
                backupObsolete = false;
            }
            catch (ArgumentException)
            {
                return VSConstants.E_FAIL;
            }
            catch (IOException)
            {
                return VSConstants.E_FAIL;
            }
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Used to set the backup dirty bit. This bit should be set when the object is modified 
        /// and cleared on calls to BackupFile and any Save method
        /// </summary>
        /// <param name="backUpDirtyBitSet">the dirty bit to be set</param>
        /// <returns>returns 1 if the backup dirty bit is set, 0 otherwise</returns>
        int IVsFileBackup.IsBackupFileObsolete(out int backUpDirtyBitSet)
        {
            if (backupObsolete)
                backUpDirtyBitSet = 1;
            else
                backUpDirtyBitSet = 0;
            return VSConstants.S_OK;
        }

        #endregion
    }
}