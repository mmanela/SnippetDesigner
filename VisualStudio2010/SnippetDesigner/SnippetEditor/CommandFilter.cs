using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;

namespace Microsoft.SnippetDesigner
{
    /// <summary>
    /// Command Filter which is assigned to the text view
    /// this allows us to intercept commands directly targeting the text view and perform our own operations
    /// In this case we are displaying our own context menu
    /// </summary>
    [ComVisible(true)]
    public class CommandFilter : IOleCommandTarget
    {
        private readonly SnippetEditor snippetEditor;
        private IOleCommandTarget oldFilter;


        /// <summary>
        /// Create the command filter with a reference to the snippet codeWindowHost so that it can
        /// cause the appropriate actions to occur like showing the context menu or running a command
        /// </summary>
        /// <param name="codeWindowHost"></param>
        public CommandFilter(SnippetEditor editor)
        {
            snippetEditor = editor;
        }

        /// <summary>
        /// Initialize the command filter
        /// </summary>
        /// <param name="currentFilter">The filter currently beng used that we want to be the fall back filter</param>
        public void Init(IOleCommandTarget currentFilter)
        {
            oldFilter = currentFilter;
        }

        #region IOleCommandTarget Members

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            int hr = (int) Constants.OLECMDERR_E_NOTSUPPORTED;
            if (pguidCmdGroup == VSConstants.VSStd2K)
            {
                switch (nCmdID)
                {
                        //Catch the command for showing a context menu and display our context menu 
                    case (uint) VSConstants.VSStd2KCmdID.SHOWCONTEXTMENU:
                        {
                            snippetEditor.ShowContextMenu();
                            hr = VSConstants.S_OK;
                            break;
                        }
                    case (uint) VSConstants.VSStd2KCmdID.ECMD_LEFTCLICK:
                        {
                            snippetEditor.MakeClickedReplacementActive();
                            break;
                        }
                    default:
                        {
                            //this is a command we arent intercepting so forward it
                            hr = oldFilter.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
                            break;
                        }
                }
            }
            else if (pguidCmdGroup == GuidList.SnippetDesignerCmdSet)
            {
                switch (nCmdID)
                {
                        //our replace command ahs been pressed to act on it
                    case PkgCmdIDList.cmdidSnippetMakeReplacement:
                        {
                            snippetEditor.CreateReplacementFromSelection(); //make the current cursor position a replacement
                            hr = VSConstants.S_OK;
                            break;
                        }
                    case PkgCmdIDList.cmdidSnippetRemoveReplacement:
                        {
                            snippetEditor.ReplacementRemove(); //remove the replcement at the current cusor position
                            hr = VSConstants.S_OK;
                            break;
                        }
                    case PkgCmdIDList.cmdidSnippetInsertEnd:
                        {
                            snippetEditor.InsertEndMarker();
                            hr = VSConstants.S_OK;
                            break;
                        }
                    case PkgCmdIDList.cmdidSnippetInsertSelected:
                        {
                            snippetEditor.InsertSelectedMarker();
                            hr = VSConstants.S_OK;
                            break;
                        }
                    default:
                        {
                            //this is a command we arent intercepting so forward it
                            hr = oldFilter.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
                            break;
                        }
                }
            }
            else
            {
                hr = oldFilter.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
            }

            return hr;
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            if (pguidCmdGroup == VSConstants.VSStd2K && prgCmds != null)
            {
                switch (prgCmds[0].cmdID)
                {
                        //indicate that we support the context menu command
                    case (uint) VSConstants.VSStd2KCmdID.SHOWCONTEXTMENU:
                        {
                            prgCmds[0].cmdf = (uint) OLECMDF.OLECMDF_SUPPORTED | (uint) OLECMDF.OLECMDF_ENABLED;
                            return VSConstants.S_OK;
                        }
                    case (uint)VSConstants.VSStd2KCmdID.ECMD_LEFTCLICK:
                        {
                            prgCmds[0].cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED | (uint)OLECMDF.OLECMDF_ENABLED;
                            return VSConstants.S_OK;
                        }
                    default:
                        {
                            return oldFilter.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
                        }
                }
            }

            else
            {
                return oldFilter.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
            }
        }

        #endregion
    }
}