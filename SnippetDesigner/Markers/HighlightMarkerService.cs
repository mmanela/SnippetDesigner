// Copyright (C) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.CommandBars;
using System.ComponentModel.Design;

namespace Microsoft.SnippetDesigner
{
    /// <summary>
    /// different type of markers available
    /// </summary>
    public enum KindOfMarker
    {
        Yellow,
        YellowWithBorder
    }

    /// <summary>
    /// Service which proffers the markers and also has fucntions to isnert markers
    /// </summary>
    [Guid(GuidList.markerServiceString)]
    internal class HighlightMarkerService : IVsTextMarkerTypeProvider
    {

        private SnippetDesignerPackage package;

        private YellowHighlightMarker yellowHighlightMarker;
        private YellowHighlightMarkerWithBorder yellowHighlightMarkerWithBorder;
        internal HighlightMarkerService(SnippetDesignerPackage package)
		{
			this.package = package;
            yellowHighlightMarker = new YellowHighlightMarker();
            yellowHighlightMarkerWithBorder = new YellowHighlightMarkerWithBorder();
		}

        ~HighlightMarkerService()
		{
		}



        /// <summary>
        /// Insert a mark over the current Selection
        /// </summary>
        /// <param name="cmdID">the cmdId of the desired marker</param>
        /// <returns>true if success</returns>
        public bool InsertMarker(uint cmdID)
        {
            IVsTextManager textManager = (IVsTextManager)package.GetService(typeof(SVsTextManager));

            IVsTextView textView;
            int hr = textManager.GetActiveView(0, null, out textView);
            TextSpan[] ts = new TextSpan[1];
            hr = textView.GetSelectionSpan(ts);

            return InsertMarker(cmdID, ts[0]);
            
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
                case PkgCmdIDList.cmdidYellowHighlightMarker:
                    guidMarker = typeof(YellowHighlightMarker).GUID;
                    textMarkerClient = (IVsTextMarkerClient)yellowHighlightMarker;
                    break;
                case PkgCmdIDList.cmdidYellowHighlightMarkerWithBorder:
                    guidMarker = typeof(YellowHighlightMarkerWithBorder).GUID;
                    textMarkerClient = (IVsTextMarkerClient)yellowHighlightMarkerWithBorder;
                    break;
                default:
                    Debug.Assert(false, Resources.ErrorInvalidMarkerID);
                    return false;
            }

            IVsTextManager textManager = (IVsTextManager)package.GetService(typeof(SVsTextManager));
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

            hr = textLines.CreateLineMarker(markerTypeID, ts.iStartLine, ts.iStartIndex,
                ts.iEndLine, ts.iEndIndex, textMarkerClient, null);

            return true;
        }

        #region IVsTextMarkerTypeProvider Members

        public int GetTextMarkerType(ref Guid pguidMarker, out IVsPackageDefinedTextMarkerType ppMarkerType)
        {
            if (pguidMarker == typeof(YellowHighlightMarker).GUID)
            {
                ppMarkerType = yellowHighlightMarker;
                return VSConstants.S_OK;
            }
            else if (pguidMarker == typeof(YellowHighlightMarkerWithBorder).GUID)
            {
                ppMarkerType = yellowHighlightMarkerWithBorder;
                return VSConstants.S_OK;
            }
            

            ppMarkerType = null;
            return VSConstants.E_FAIL;
        }
        #endregion
    }
}
        