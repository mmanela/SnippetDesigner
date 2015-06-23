using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Windows;
using EnvDTE;
using EnvDTE80;
using Microsoft.RegistryTools;
using Microsoft.SnippetDesigner.OptionPages;
using Microsoft.SnippetDesigner.SnippetExplorer;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using Window = EnvDTE.Window;

namespace Microsoft.SnippetDesigner
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the registration utility (regpkg.exe) that this class needs
    // to be registered as package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#100", "#102", "1.6.2", IconResourceID = 404)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    // This attribute registers a tool window exposed by this package.
    [ProvideToolWindow(typeof(SnippetExplorerToolWindow))]
    // Options pages
    [ProvideOptionPage(typeof(SnippetDesignerOptions), "Snippet Designer", "General Options", 14340, 17770, true)]
    [ProvideOptionPage(typeof(ResetOptions), "Snippet Designer", "Reset", 14340, 17771, true)]
    [ProvideAutoLoad(GuidList.autoLoadOnNoSolution)]
    [ProvideAutoLoad(GuidList.autoLoadOnSolutionExists)]
    [ProvideEditorExtension(typeof(EditorFactory), StringConstants.SnippetExtension, 70,
        ProjectGuid = GuidList.miscellaneousFilesProject,
        DefaultName = "Snippet Designer",
        NameResourceID = 100,
        TemplateDir = @"..\..\Templates"
        )]
    [ProvideEditorLogicalView(typeof(EditorFactory), GuidList.editorFactoryLogicalView)]
    [Guid(GuidList.SnippetDesignerPkgString)]
    [ComVisible(true)]
    public sealed class SnippetDesignerPackage : Package, IVsSelectionEvents, IDisposable, IVsInstalledProduct
    {
        internal static SnippetDesignerPackage Instance;
        private EditorFactory editorFactory;
        private OleMenuCommand snippetExportCommand;
        // Cache the Menu Command Service since we will use it multiple times
        private OleMenuCommandService menuCommandService;
        //keep track of the last window which was active
        private Window previousWindow;
        private Window currentWindow;

        //needed for the custom type descriptor provider
        private string activeSnippetTitle = String.Empty;
        private string activeSnippetLanguage = String.Empty;
        private IComponentModel componentModel;
        internal ILogger Logger { get; private set; }

        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public SnippetDesignerPackage()
        {
            Instance = this;
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", ToString()));
        }

        public IComponentModel ComponentModel
        {
            get
            {
                if (componentModel == null)
                    componentModel = (IComponentModel)GetGlobalService(typeof(SComponentModel));
                return componentModel;
            }
        }


        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <value>The settings.</value>
        public SnippetDesignerOptions Settings { get; private set; }

        /// <summary>
        /// Return the snippet index for this package
        /// </summary>
        public SnippetIndex SnippetIndex { get; private set; }

        /// <summary>
        /// Return the active snippet title so that the type desccriptor can  display it
        /// </summary>
        public string ActiveSnippetTitle
        {
            get { return activeSnippetTitle; }
            set { activeSnippetTitle = value; }
        }

        /// <summary>
        /// Return the active snippet title so that the type desccriptor can 
        /// </summary>
        public string ActiveSnippetLanguage
        {
            get { return activeSnippetLanguage; }
            set { activeSnippetLanguage = value; }
        }

        /// <summary>
        /// the one instance of the dte object created by this package
        /// </summary>
        public DTE2 Dte { get; private set; }

        public string VSVersion { get; private set; }

        public bool IsVisualStudio2010
        {
            get { return VSVersion.Equals("10.0"); }
        }

        public bool IsVisualStudio2012
        {
            get { return VSVersion.Equals("11.0"); }
        }

        public bool IsVisualStudio2013
        {
            get { return VSVersion.Equals("12.0"); }
        }

        /// <summary>
        /// Get the export snippet data
        /// contains language and code of the snippet
        /// </summary>
        public ExportToSnippetData ExportSnippetData { get; private set; }

        internal static string GetResourceString(string resourceName)
        {
            string resourceValue;
            var resourceManager = (IVsResourceManager)GetGlobalService(typeof(SVsResourceManager));
            if (resourceManager == null)
                throw new InvalidOperationException(
                    "Could not get SVsResourceManager service. Make sure the package is Sited before calling this method.");

            Guid packageGuid = typeof(SnippetDesignerPackage).GUID;
            int hr = resourceManager.LoadResourceString(ref packageGuid, -1, resourceName, out resourceValue);
            ErrorHandler.ThrowOnFailure(hr);

            return resourceValue;
        }

        internal static string GetResourceString(int resourceID)
        {
            return GetResourceString(string.Format("@{0}", resourceID));
        }

        public string GetVisualStudioResourceString(uint resourceId)
        {
            var shell = (IVsShell)GetService(typeof(SVsShell));
            string localizedResource = null;
            if (shell != null)
                shell.LoadPackageString(ref GuidList.VsEnvironmentPackage, resourceId, out localizedResource);

            return localizedResource;
        }

        /// <summary>
        /// Get the name Visual Studio is registered to
        /// </summary>
        internal static string VSRegisteredName
        {
            get
            {
                string registeredName = String.Empty;
                try
                {
                    //get the reg entry
                    RegistryKey rk = RegistryLocations.GetVSRegKey(Registry.LocalMachine, Instance.VSVersion);
                    if (rk != null)
                    {
                        rk = rk.OpenSubKey(StringConstants.VSRegistryRegistrationName);
                        registeredName = (String)rk.GetValue(StringConstants.VSRegistryRegistrationNameEntry);
                    }
                }
                catch (SecurityException)
                {
                    //The user does not have the permissions required to read the registry key.
                }
                catch (ArgumentException)
                {
                    //name is longer than the maximum length allowed (255 characters).
                }
                catch (ObjectDisposedException)
                {
                    //The Microsoft.Win32.RegistryKey is closed (closed keys cannot be accessed).
                }


                return registeredName;
            }
        }

        public new Object GetService(Type serviceType)
        {
            return base.GetService(serviceType);
        }

        /// <summary>
        /// This function is called when the user clicks the menu item that shows the 
        /// tool window. See the Initialize method to see how the menu item is associated to 
        /// this function using the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void ShowSnippetExplorer(object sender, EventArgs e)
        {
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.


            ToolWindowPane window = FindToolWindow(typeof(SnippetExplorerToolWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new COMException(Resources.CanNotCreateWindow);
            }
            Guid textEditor = GuidList.textEditorFactory;
            var windowFrame = (IVsWindowFrame)window.Frame;
            ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }


        /// <summary>
        /// Get the language of the active window
        /// </summary>
        /// <returns></returns>
        private string CurrentWindowLanguage
        {
            get
            {
                string lang = String.Empty;
                TextDocument codeDoc = CurrentTextDocument;
                if (codeDoc != null)
                {
                    lang = codeDoc.Language;
                }
                return lang;
            }
        }


        /// <summary>
        ///  get the current text document
        /// if current documents isnt a text doc return null
        /// </summary>
        internal TextDocument CurrentTextDocument
        {
            get
            {
                try
                {
                    return GetTextDocumentFromWindow(Dte.ActiveWindow);
                }
                catch (Exception e)
                {
                    Logger.Log("Error getting active window", "VsPkg", e);
                    return null;
                }
            }
        }

        internal TextDocument GetTextDocumentFromWindow(Window window)
        {
            TextDocument codeDoc = null;
            if (Dte != null)
            {
                Document doc = null;
                try
                {
                    doc = window.Document;
                }
                catch (ArgumentException)
                {
                    return null; //error occured return null
                }

                if (doc != null)
                {
                    codeDoc = doc.Object(String.Empty) as TextDocument;
                }
            }
            return codeDoc;
        }

        public void ClearSnippetExportData()
        {
            ExportSnippetData = null;
        }


        /// <summary>
        /// Called by the export command.  This fucntion will determin the exported language
        /// and the exported code and build a ExportToSnippetData object.  It then creates a new snippet
        /// which will read this export to snippet object
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExportToSnippet(object sender, EventArgs e)
        {
            // The selected item is the active window pane
            // in Visual Studio. 

            // Get the code from the Document into TextDocument codeDoc
            TextDocument codeDoc = CurrentTextDocument;
            if (codeDoc == null) //if the active window isnt a textwindow get the last active text window
            {
                codeDoc = GetTextDocumentFromWindow(previousWindow);
            }

            if (codeDoc != null)
            {
                try
                {
                    //build export object
                    var snippetText = codeDoc.Selection.Text.Normalize();
                    ExportSnippetData = new ExportToSnippetData(snippetText, codeDoc.Language.ToLower());
                    //launch new file
                    CreateNewSnippetFile();
                }
                catch (Exception ex)
                {
                    Logger.Log(ex.Message, "ExportToSnippet", ex);
                }
                return;
            }
        }


        /// <summary>
        /// The vs command line argument parser.  When you do File.NewSnippet and then args of
        /// /lang langyage and/or /code myCode it will create new snippet with those options
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateSnippet(object sender, EventArgs e)
        {
            var eventArgs = e as OleMenuCmdEventArgs;
            if (eventArgs != null && Dte != null)
            {
                var newSnippet = new NewSnippetCommand(eventArgs.InValue.ToString().Split(' '));

                //build export object
                ExportSnippetData = new ExportToSnippetData(newSnippet.Code, newSnippet.Language.ToLower());
                CreateNewSnippetFile();
            }
        }

        /// <summary>
        /// Open a new snippet file, if any export data is wanted it must be set before hand
        /// </summary>
        internal void CreateNewSnippetFile()
        {
            if (!LaunchNewFile(GetNextAvailableNewSnippetTitle()))
            {
                MessageBox.Show("Unable to create .Snippet file", "Snippet Designer Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private string GetNextAvailableNewSnippetTitle()
        {
            int i = 1;
            string newTitle = null;

            newTitle = string.Format(StringConstants.NewSnippetTitleFormat, i++);
            while (Dte.Windows.Cast<Window>().Any(window => window.Caption.Equals(newTitle)))
            {
                newTitle = string.Format(StringConstants.NewSnippetTitleFormat, i++);
            }

            return newTitle;
        }


        private IOleCommandTarget GetShellCommandDispatcher()
        {
            return GetService(typeof(SUIHostCommandDispatcher)) as IOleCommandTarget;
        }

        private bool LaunchNewFile(string fileName)
        {
            IntPtr inArgPtr = Marshal.AllocCoTaskMem(512);
            Marshal.GetNativeVariantForObject(fileName, inArgPtr);

            Guid cmdGroup = VSConstants.GUID_VSStandardCommandSet97;
            IOleCommandTarget commandTarget = GetShellCommandDispatcher();
            int hr = commandTarget.Exec(ref cmdGroup,
                                        (uint)VSConstants.VSStd97CmdID.FileNew,
                                        (uint)OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT,
                                        inArgPtr,
                                        IntPtr.Zero);
            return ErrorHandler.Succeeded(hr);
        }

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initilaization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            try
            {
                base.Initialize();
                Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", ToString()));


                //create the dte automation object so rest of package can access the automation model
                Dte = (DTE2)GetService(typeof(DTE));
                if (Dte == null)
                {
                    //if dte is null then we throw a excpetion
                    //this is a fatal error
                    throw new ArgumentNullException(Resources.ErrorDTENull);
                }

                VSVersion = Dte.Version;


                Logger = new Logger(this);
                Settings = GetDialogPage(typeof(SnippetDesignerOptions)) as SnippetDesignerOptions;

                //Create Editor Factory
                editorFactory = new EditorFactory(this);
                RegisterEditorFactory(editorFactory);


                //Set up Selection Events so that I can tell when a new window in VS has become active.
                uint cookieForSelection = 0;
                var selMonitor = GetService(typeof(SVsShellMonitorSelection)) as IVsMonitorSelection;

                if (selMonitor != null)
                    selMonitor.AdviseSelectionEvents(this, out cookieForSelection);


                // Add our command handlers for menu (commands must exist in the .vstc file)

                // Create the command for the tool window
                var snippetExplorerCommandID = new CommandID(GuidList.SnippetDesignerCmdSet,
                                                             (int)PkgCmdIDList.cmdidSnippetExplorer);
                DefineCommandHandler(ShowSnippetExplorer, snippetExplorerCommandID);


                //DefineCommandHandler not used for these since extra properties need to be set
                // Create the command for the context menu export snippet
                var contextcmdID = new CommandID(GuidList.SnippetDesignerCmdSet,
                                                 (int)PkgCmdIDList.cmdidExportToSnippet);
                snippetExportCommand = DefineCommandHandler(ExportToSnippet, contextcmdID);
                snippetExportCommand.Visible = false;

                // commandline command for exporting as snippet
                var exportCmdLineID = new CommandID(GuidList.SnippetDesignerCmdSet,
                                                    (int)PkgCmdIDList.cmdidExportToSnippetCommandLine);
                OleMenuCommand snippetExportCommandLine = DefineCommandHandler(ExportToSnippet, exportCmdLineID);
                snippetExportCommandLine.ParametersDescription = StringConstants.ArgumentStartMarker;
                //a space means arguments are coming


                // Create the command for CreateSnippet
                var createcmdID = new CommandID(GuidList.SnippetDesignerCmdSet,
                                                (int)PkgCmdIDList.cmdidCreateSnippet);
                OleMenuCommand createCommand = DefineCommandHandler(CreateSnippet, createcmdID);
                createCommand.ParametersDescription = StringConstants.ArgumentStartMarker;

                //initialize the snippet index
                SnippetIndex = new SnippetIndex();
                ThreadPool.QueueUserWorkItem(
                    delegate
                    {
                        SnippetIndex.ReadIndexFile();
                        SnippetIndex.CreateOrUpdateIndexFile();
                    }
                    );
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                throw;
            }
        }


        /// <summary>
        /// Define a command handler.
        /// When the user press the button corresponding to the CommandID
        /// the EventHandler will be called.
        /// </summary>
        /// <param name="id">The CommandID (Guid/ID pair) as defined in the .ctc file</param>
        /// <param name="handler">Method that should be called to implement the command</param>
        /// <returns>The menu command. This can be used to set parameter such as the default visibility once the package is loaded</returns>
        internal OleMenuCommand DefineCommandHandler(EventHandler handler, CommandID id)
        {
            // if the package is zombied, we don't want to add commands
            if (Zombied)
                return null;

            // Make sure we have the service
            if (menuCommandService == null)
            {
                // Get the OleCommandService object provided by the MPF; this object is the one
                // responsible for handling the collection of commands implemented by the package.
                menuCommandService = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            }
            OleMenuCommand command = null;
            if (null != menuCommandService)
            {
                // Add the command handler
                command = new OleMenuCommand(handler, id);
                menuCommandService.AddCommand(command);
            }
            return command;
        }


        /// <summary>
        /// Called when the UI Context changes.  This will be called when the user opens a new item (window) is active.
        /// This allows me to see what type of item (window) it is and decide to disable or enable the ceartin commands
        /// </summary>
        /// <param name="dwCmdUICookie"></param>
        /// <param name="fActive"></param>
        /// <returns></returns>
        public int OnCmdUIContextChanged(uint dwCmdUICookie, int fActive)
        {
            return VSConstants.S_OK;
        }

        public int OnElementValueChanged(uint elementid, object varValueOld, object varValueNew)
        {
            try
            {
                if (elementid == (uint)VSConstants.VSSELELEMID.SEID_WindowFrame)
                {

                    if (currentWindow == null)
                    {
                        currentWindow = Dte.ActiveWindow;
                    }
                    else if (currentWindow != Dte.ActiveWindow)
                    {
                        previousWindow = currentWindow;
                        currentWindow = Dte.ActiveWindow;
                    }

                    string lang = CurrentWindowLanguage;
                    if (StringConstants.ExportNameCSharp.Equals(lang, StringComparison.OrdinalIgnoreCase)
                        || StringConstants.ExportNameVisualBasic.Equals(lang, StringComparison.OrdinalIgnoreCase)
                        || StringConstants.ExportNameXML.Equals(lang, StringComparison.OrdinalIgnoreCase)
                        || StringConstants.ExportNameSQL.Equals(lang, StringComparison.OrdinalIgnoreCase)
                        || StringConstants.ExportNameSQL2.Equals(lang, StringComparison.OrdinalIgnoreCase)
                        || StringConstants.ExportNameJavaScript.Equals(lang, StringComparison.OrdinalIgnoreCase)
                        || StringConstants.ExportNameJavaScript2.Equals(lang, StringComparison.OrdinalIgnoreCase)
                        || StringConstants.ExportNameHTML.Equals(lang, StringComparison.OrdinalIgnoreCase)
                        
                        // Only allow C++ if this VS is newer than VS 2010
                        || (!IsVisualStudio2010 && StringConstants.ExportNameCPP.Equals(lang, StringComparison.OrdinalIgnoreCase))
                        
                        )
                    {
                        //make the export context menu item visible
                        snippetExportCommand.Visible = true;
                    }
                    else
                    {
                        //make the export context menu item not visible
                        snippetExportCommand.Visible = false;
                    }
                }
            }
            catch (NullReferenceException)
            {
                return VSConstants.S_FALSE;
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, "OnElementValueChanged", ex);
                return VSConstants.S_FALSE;
            }

            return VSConstants.S_OK;
        }

        public int OnSelectionChanged(IVsHierarchy pHierOld,
                                      uint itemidOld,
                                      IVsMultiItemSelect pMISOld,
                                      ISelectionContainer pSCOld,
                                      IVsHierarchy pHierNew,
                                      uint itemidNew,
                                      IVsMultiItemSelect pMISNew,
                                      ISelectionContainer pSCNew)
        {
            return VSConstants.S_OK;
        }


        public void Dispose()
        {
            if (editorFactory != null)
            {
                editorFactory.Dispose();
            }
        }

        #region IVsInstalledProduct Members

        public int IdBmpSplash(out uint pIdBmp)
        {
            pIdBmp = 500;
            return VSConstants.S_OK;
        }

        public int IdIcoLogoForAboutbox(out uint pIdIco)
        {
            pIdIco = 600;
            return VSConstants.S_OK;
        }

        public int OfficialName(out string pbstrName)
        {
            pbstrName = GetResourceString(100);
            return VSConstants.S_OK;
        }

        public int ProductDetails(out string pbstrProductDetails)
        {
            pbstrProductDetails = GetResourceString(102);
            return VSConstants.S_OK;
        }

        public int ProductID(out string pbstrPID)
        {
            pbstrPID = GuidList.SnippetDesignerPkgString;
            return VSConstants.S_OK;
        }

        #endregion
    }
}