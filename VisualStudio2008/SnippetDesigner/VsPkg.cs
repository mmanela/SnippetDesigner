using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using EnvDTE;
using Microsoft.RegistryTools;
using Microsoft.SnippetDesigner.ContentTypes;
using Microsoft.SnippetDesigner.OptionPages;
using Microsoft.SnippetDesigner.SnippetExplorer;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using MsOle = Microsoft.VisualStudio.OLE.Interop;


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
    // A Visual Studio component can be registered under different regitry roots; for instance
    // when you debug your package you want to register it in the experimental hive. This
    // attribute specifies the registry root to use if no one is provided to regpkg.exe with
    // the /root switch.
    [DefaultRegistryRoot(@"Microsoft\VisualStudio\9.0")]
    // This attribute is used to register the informations needed to show the this package
    // in the Help/About dialog of Visual Studio.
    //[InstalledProductRegistration(false, "#100", "#102", "1.0",IconResourceID=400)]
    [InstalledProductRegistration(true, null, null, null)]
    // In order be loaded inside Visual Studio in a machine that has not the VS SDK installed, 
    // package needs to have a valid load key (it can be requested at 
    // http://msdn.microsoft.com/vstudio/extend/). This attributes tells the shell that this 
    // package has a load key embedded in its resources.
    [ProvideLoadKey("Standard", "1.1", "Snippet Designer", "Microsoft", 1)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource(1000, 1)]
    // This attribute registers a tool window exposed by this package.
    [ProvideToolWindow(typeof (SnippetExplorerToolWindow))]
    // Options pages
    [ProvideOptionPage(typeof (SnippetDesignerOptions), "Snippet Designer", "General Options", 14340, 17770, true)]
    [ProvideOptionPage(typeof (ResetOptions), "Snippet Designer", "Reset", 14340, 17771, true)]
    // Language Service
    // This attribute is needed to indicate that the we offer this language service
    [ProvideService(typeof (CSharpSnippetLanguageService))]
    [ProvideLanguageService(typeof (CSharpSnippetLanguageService), "CSharp Snippets", 202)]
    [ProvideService(typeof (VBSnippetLanguageService))]
    [ProvideLanguageService(typeof (VBSnippetLanguageService), "VB Snippets", 203)]
    [ProvideService(typeof (XMLSnippetLanguageService))]
    [ProvideLanguageService(typeof (XMLSnippetLanguageService), "XML Snippets", 204)]
    // These attributes registers the HighLightMarker service and two custom markers 
    [ProvideService(typeof (HighlightMarkerService), ServiceName = StringConstants.MarkerServiceName)]
    [ProvideCustomMarker(StringConstants.SnippetReplacementMarker, 200, typeof (SnippetReplacementMarker), typeof (SnippetDesignerPackage), typeof (HighlightMarkerService))]
    [ProvideCustomMarker(StringConstants.ActiveSnippetReplacementMarker, 201, typeof (ActiveSnippetReplacementMarker), typeof (SnippetDesignerPackage), typeof (HighlightMarkerService))]
    //cause the package to autoload - Only when a solution exists
    [ProvideAutoLoad(GuidList.autoLoadOnSolutionExists)]
    [ProvideEditorExtension(typeof (EditorFactory), StringConstants.SnippetExtension, 32,
        ProjectGuid = GuidList.provideEditorExtensionProject,
        DefaultName = "Snippet Designer"
        )]
    [ProvideEditorLogicalView(typeof (EditorFactory), GuidList.editorFactoryLogicalView)]
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

        private CSharpSnippetLanguageService csharpSnippetLangService;
        private VBSnippetLanguageService vbSnippetLangService;
        private XMLSnippetLanguageService xmlSnippetLangService;

        //needed for the custom type descriptor provider
        private string activeSnippetTitle = String.Empty;
        private string activeSnippetLanguage = String.Empty;

        internal ILogger Logger { get; private set; }

        //index of snippets

        // options pages

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
        /// Get the service which you can aquire highlight markers from
        /// </summary>
        public HighlightMarkerService MarkerService { get; private set; }

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
        public DTE DTE { get; private set; }

        /// <summary>
        /// Get the export snippet data
        /// contains language and code of the snippet
        /// </summary>
        public ExportToSnippetData ExportSnippetData { get; private set; }

        public void ClearSnippetExportData()
        {
            ExportSnippetData = null;
        }

        internal static string GetResourceString(string resourceName)
        {
            string resourceValue;
            IVsResourceManager resourceManager = (IVsResourceManager) GetGlobalService(typeof (SVsResourceManager));
            if (resourceManager == null)
                throw new InvalidOperationException("Could not get SVsResourceManager service. Make sure the package is Sited before calling this method.");

            Guid packageGuid = typeof (SnippetDesignerPackage).GUID;
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
            IVsShell shell = (IVsShell)GetService(typeof(SVsShell));
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
                    RegistryKey rk = RegistryLocations.GetVSRegKey(Registry.LocalMachine);
                    if (rk != null)
                    {
                        rk = rk.OpenSubKey(StringConstants.VSRegistryRegistrationName);
                        registeredName = (String) rk.GetValue(StringConstants.VSRegistryRegistrationNameEntry);
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


            ToolWindowPane window = FindToolWindow(typeof (SnippetExplorerToolWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new COMException(Resources.CanNotCreateWindow);
            }
            Guid textEditor = GuidList.textEditorFactory;
            IVsWindowFrame windowFrame = (IVsWindowFrame) window.Frame;
            ErrorHandler.ThrowOnFailure(windowFrame.Show());
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
                    ExportSnippetData = new ExportToSnippetData(codeDoc.Selection.Text.Normalize(), codeDoc.Language.ToLower());
                    //launch new file
                    CreateNewSnippetFile();
                }
                catch (NullReferenceException)
                {
                    //if this happens then just exit, export failed
                }
                return;
            }
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
            get { return GetTextDocumentFromWindow(DTE.ActiveWindow); }
        }

        internal TextDocument GetTextDocumentFromWindow(Window window)
        {
            TextDocument codeDoc = null;
            if (DTE != null)
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

        /// <summary>
        /// The vs command line argument parser.  When you do File.NewSnippet and then args of
        /// /lang langyage and/or /code myCode it will create new snippet with those options
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateSnippet(object sender, EventArgs e)
        {
            OleMenuCmdEventArgs eventArgs = e as OleMenuCmdEventArgs;
            if (eventArgs != null && DTE != null)
            {
                NewSnippetCommand newSnippet = new NewSnippetCommand(eventArgs.InValue.ToString().Split(' '));

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
            if (DTE != null)
            {
                int TemplateNameResourceID = 106;
                uint EnvironmentTemplateCategoryResourceID = 13565;
                var commandArgs = string.Format(
                    StringConstants.MakeSnippetDTEFormat,
                    GetVisualStudioResourceString(EnvironmentTemplateCategoryResourceID),
                    GetResourceString(TemplateNameResourceID));
                DTE.ExecuteCommand(StringConstants.NewFileDTECommand, commandArgs);
            }
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
                DTE = (DTE) GetService(typeof (DTE));
                if (DTE == null)
                {
                    //if dte is null then we throw a excpetion
                    //this is a fatal error
                    throw new ArgumentNullException(Resources.ErrorDTENull);
                }


                Logger = new Logger(this);

                Settings = GetDialogPage(typeof (SnippetDesignerOptions)) as SnippetDesignerOptions;


                // Create instance of RegularExpressionLanguageService type
                csharpSnippetLangService = new CSharpSnippetLanguageService();
                csharpSnippetLangService.SetSite(this);

                vbSnippetLangService = new VBSnippetLanguageService();
                vbSnippetLangService.SetSite(this);

                xmlSnippetLangService = new XMLSnippetLanguageService();
                xmlSnippetLangService.SetSite(this);

                // Add our language service objects to packages services container
                ((IServiceContainer) this).AddService(typeof (CSharpSnippetLanguageService), csharpSnippetLangService, true);
                ((IServiceContainer) this).AddService(typeof (VBSnippetLanguageService), vbSnippetLangService, true);
                ((IServiceContainer) this).AddService(typeof (XMLSnippetLanguageService), xmlSnippetLangService, true);

                //Create Editor Factory
                editorFactory = new EditorFactory(this);
                RegisterEditorFactory(editorFactory);


                //Set up Selection Events so that I can tell when a new window in VS has become active.
                uint cookieForSelection = 0;
                IVsMonitorSelection selMonitor = GetService(typeof (SVsShellMonitorSelection)) as IVsMonitorSelection;

                if (selMonitor != null)
                    selMonitor.AdviseSelectionEvents(this, out cookieForSelection);


                // Add our command handlers for menu (commands must exist in the .vstc file)

                // Create the command for the tool window
                CommandID snippetExplorerCommandID = new CommandID(GuidList.SnippetDesignerCmdSet, (int) PkgCmdIDList.cmdidSnippetExplorer);
                DefineCommandHandler(ShowSnippetExplorer, snippetExplorerCommandID);


                //DefineCommandHandler not used for these since extra properties need to be set
                // Create the command for the context menu export snippet
                CommandID contextcmdID = new CommandID(GuidList.SnippetDesignerCmdSet, (int) PkgCmdIDList.cmdidExportToSnippet);
                snippetExportCommand = DefineCommandHandler(ExportToSnippet, contextcmdID);
                snippetExportCommand.Visible = false;

                // commandline command for exporting as snippet
                CommandID exportCmdLineID = new CommandID(GuidList.SnippetDesignerCmdSet, (int) PkgCmdIDList.cmdidExportToSnippetCommandLine);
                OleMenuCommand snippetExportCommandLine = DefineCommandHandler(ExportToSnippet, exportCmdLineID);
                snippetExportCommandLine.ParametersDescription = StringConstants.ArgumentStartMarker; //a space means arguments are coming


                // Create the command for CreateSnippet
                CommandID createcmdID = new CommandID(GuidList.SnippetDesignerCmdSet, (int) PkgCmdIDList.cmdidCreateSnippet);
                OleMenuCommand createCommand = DefineCommandHandler(CreateSnippet, createcmdID);
                createCommand.ParametersDescription = StringConstants.ArgumentStartMarker; //a space means arguments are coming

                // Create and proffer the marker service
                MarkerService = new HighlightMarkerService(this);
                ((IServiceContainer) this).AddService(MarkerService.GetType(), MarkerService, true);

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
                menuCommandService = GetService(typeof (IMenuCommandService)) as OleMenuCommandService;
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
            TextDocument textDoc = CurrentTextDocument;
            if (currentWindow == null)
            {
                currentWindow = DTE.ActiveWindow;
            }
            else if (currentWindow != DTE.ActiveWindow)
            {
                previousWindow = currentWindow;
                currentWindow = DTE.ActiveWindow;
            }

            string lang = CurrentWindowLanguage.ToLower(); //turn to lower case for comparisons
            //TODO: move these into a config file
            if (lang == StringConstants.ExportNameCSharp
                || lang == StringConstants.ExportNameVisualBasic
                || lang == StringConstants.ExportNameXML
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
            return VSConstants.S_OK;
        }

        //not used - needed to satify IVsSelectionEvents interface.  Only OnCmdUIContextChanged used
        public int OnElementValueChanged(uint elementid, object varValueOld, object varValueNew)
        {
            return VSConstants.S_OK;
        }

        //not used - needed to satify IVsSelectionEvents interface.  Only OnCmdUIContextChanged used
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