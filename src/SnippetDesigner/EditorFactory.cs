using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using IServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Microsoft.SnippetDesigner
{
    /// <summary>
    /// Factory for creating our editors
    /// </summary>
    [ComVisible(true)]
    [Guid(GuidList.editorFactoryString)]
    public class EditorFactory : IVsEditorFactory, IDisposable
    {
        private ServiceProvider vsServiceProvider;
        private bool disposed;

        private readonly SnippetDesignerPackage editorPackage;


        public EditorFactory(SnippetDesignerPackage package)
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering {0} constructor", ToString()));
            editorPackage = package;
        }

        #region IVsEditorFactory Members

        public int SetSite(IServiceProvider psp)
        {
            vsServiceProvider = new ServiceProvider(psp, false);
            return VSConstants.S_OK;
        }

        public object GetService(Type serviceType)
        {
            return vsServiceProvider.GetService(serviceType);
        }

        // This method is called by the Environment (inside IVsUIShellOpenDocument::
        // OpenStandardEditor and OpenSpecificEditor) to map a LOGICAL view to a 
        // PHYSICAL view. A LOGICAL view identifies the purpose of the view that is
        // desired (e.g. a view appropriate for Debugging [LOGVIEWID_Debugging], or a 
        // view appropriate for text view manipulation as by navigating to a find
        // result [LOGVIEWID_TextView]). A PHYSICAL view identifies an actual type 
        // of view implementation that an IVsEditorFactory can create. 
        //
        // NOTE: Physical views are identified by a string of your choice with the 
        // one constraint that the default/primary physical view for an codeWindowHost  
        // *MUST* use a NULL string as its physical view name (*pbstrPhysicalView = NULL).
        //
        // NOTE: It is essential that the implementation of MapLogicalView properly
        // validates that the LogicalView desired is actually supported by the codeWindowHost.
        // If an unsupported LogicalView is requested then E_NOTIMPL must be returned.
        //
        // NOTE: The special Logical Views supported by an Editor Factory must also 
        // be registered in the local registry hive. LOGVIEWID_Primary is implicitly 
        // supported by all code window types and does not need to be registered.
        // For example, an code window that supports a ViewCode/ViewDesigner scenario
        // might register something like the following:
        //      HKLM\Software\Microsoft\VisualStudio\8.0\Editors\
        //          {...guidEditor...}\
        //              LogicalViews\
        //                  {...LOGVIEWID_TextView...} = s ''
        //                  {...LOGVIEWID_Code...} = s ''
        //                  {...LOGVIEWID_Debugging...} = s ''
        //                  {...LOGVIEWID_Designer...} = s 'Form'
        //
        public int MapLogicalView(ref Guid rguidLogicalView, out string pbstrPhysicalView)
        {
            pbstrPhysicalView = null; // initialize out parameter

            // we support only a single physical view
            if (VSConstants.LOGVIEWID_Primary == rguidLogicalView)
                return VSConstants.S_OK; // primary view uses NULL as pbstrPhysicalView
            else
                return VSConstants.E_NOTIMPL; // you must return E_NOTIMPL for any unrecognized rguidLogicalView values
        }

        public int Close()
        {
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Create an instance of the codeWindowHost
        /// </summary>
        /// <param name="grfCreateDoc"></param>
        /// <param name="fileToLoad"></param>
        /// <param name="pszPhysicalView"></param>
        /// <param name="pvHier"></param>
        /// <param name="itemid"></param>
        /// <param name="punkDocDataExisting"></param>
        /// <param name="ppunkDocView"></param>
        /// <param name="ppunkDocData"></param>
        /// <param name="pbstrEditorCaption"></param>
        /// <param name="pguidCmdUI"></param>
        /// <param name="pgrfCDW"></param>
        /// <returns></returns>
        public int CreateEditorInstance(
            uint grfCreateDoc,
            string pszMkDocument,
            string pszPhysicalView,
            IVsHierarchy pvHier,
            uint itemid,
            IntPtr punkDocDataExisting,
            out IntPtr ppunkDocView,
            out IntPtr ppunkDocData,
            out string pbstrEditorCaption,
            out Guid pguidCmdUI,
            out int pgrfCDW)
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering {0} CreateEditorInstance()", ToString()));

            // Initialize to null
            ppunkDocView = IntPtr.Zero;
            ppunkDocData = IntPtr.Zero;
            pguidCmdUI = GuidList.snippetEditorFactory;
            pgrfCDW = 0;
            pbstrEditorCaption = null;

            // Validate inputs
            if ((grfCreateDoc & (VSConstants.CEF_OPENFILE | VSConstants.CEF_SILENT)) == 0)
            {
                Debug.Assert(false, "Only Open or Silent is valid");
                return VSConstants.E_INVALIDARG;
            }
            if (punkDocDataExisting != IntPtr.Zero)
            {
                return VSConstants.VS_E_INCOMPATIBLEDOCDATA;
            }

            // Create the Document (codeWindowHost)
            SnippetEditor snipEditor = new SnippetEditor(editorPackage);
            ppunkDocView = Marshal.GetIUnknownForObject(snipEditor);
            ppunkDocData = Marshal.GetIUnknownForObject(snipEditor);
            pbstrEditorCaption = String.Empty;

            return VSConstants.S_OK;
        }

        #endregion

        #region IDisposable

        // Implement IDisposable.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue 
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the 
        // runtime from inside the finalizer and you should not reference 
        // other objects. Only unmanaged resources can be disposed.
        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!disposed)
            {
                // If disposing equals true, dispose all managed 
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                }

                // Call the appropriate methods to clean up 
                // unmanaged resources here.
                // If disposing is false, 
                // only the following code is executed.
                if (vsServiceProvider != null)
                {
                    vsServiceProvider.Dispose();
                }
            }
            disposed = true;
        }


        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method 
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        ~EditorFactory()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion
    }
}