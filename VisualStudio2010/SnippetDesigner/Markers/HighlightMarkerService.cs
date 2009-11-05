using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.SnippetDesigner
{
    /// <summary>
    /// Service which proffers the markers and also has fucntions to isnert markers
    /// </summary>
    [Guid(GuidList.markerServiceString)]
    public class HighlightMarkerService : IVsTextMarkerTypeProvider
    {
        private SnippetDesignerPackage package;
       
        private SnippetReplacementMarker snippetReplacementMarker;

        internal HighlightMarkerService(SnippetDesignerPackage package)
        {
            this.package = package;
            snippetReplacementMarker = new SnippetReplacementMarker();
        }


        /// <summary>
        /// Insert a mark over the given span
        /// </summary>
        /// <param name="cmdID">id of desired marker</param>
        /// <param name="ts">a span of text</param>
        /// <returns>true if success</returns>
        public bool InsertMarker(uint cmdID, TextSpan ts)
        {
            Guid guidMarker;
            IVsTextMarkerClient textMarkerClient = null;
            int markerTypeID;

            switch (cmdID)
            {
                case PkgCmdIDList.cmdidSnippetReplacementMarker:
                    guidMarker = typeof (SnippetReplacementMarker).GUID;
                    textMarkerClient = snippetReplacementMarker;
                    break;
                default:
                    Debug.Assert(false, Resources.ErrorInvalidMarkerID);
                    return false;
            }

            IVsTextManager textManager = (IVsTextManager) package.GetService(typeof (SVsTextManager));
            if (textManager == null)
            {
                return false;
            }
            int hr = textManager.GetRegisteredMarkerTypeID(ref guidMarker, out markerTypeID);

            IVsTextView textView;
            hr = textManager.GetActiveView(0, null, out textView);
            if (textView == null)
            {
                return false;
            }
            IVsTextLines textLines;
            hr = textView.GetBuffer(out textLines);
            if (textLines == null)
            {
                return false;
            }

            hr = textLines.CreateLineMarker(markerTypeID,
                                            ts.iStartLine,
                                            ts.iStartIndex,
                                            ts.iEndLine,
                                            ts.iEndIndex,
                                            textMarkerClient,
                                            null);

            return true;
        }

        #region IVsTextMarkerTypeProvider Members

        public int GetTextMarkerType(ref Guid pguidMarker, out IVsPackageDefinedTextMarkerType ppMarkerType)
        {
            if (pguidMarker == typeof (SnippetReplacementMarker).GUID)
            {
                ppMarkerType = snippetReplacementMarker;
                return VSConstants.S_OK;
            }


            ppMarkerType = null;
            return VSConstants.E_FAIL;
        }

        #endregion
    }
}