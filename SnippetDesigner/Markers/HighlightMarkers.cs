// Copyright (C) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.SnippetDesigner
{
    /// <summary>
    /// The yellow highlight marker
    /// </summary>
    [Guid(GuidList.yellowMarkerString)]
    public class YellowHighlightMarker : 
        IVsPackageDefinedTextMarkerType, 
        IVsMergeableUIItem,
        IVsTextMarkerClient
    {
        
        #region IVsPackageDefinedTextMarkerType Members

        /// <summary>
        /// Should never be called we arent using glyphs
        /// </summary>
        /// <param name="hdc"></param>
        /// <param name="pRect"></param>
        /// <param name="iMarkerType"></param>
        /// <param name="pMarkerColors"></param>
        /// <param name="dwGlyphDrawFlags"></param>
        /// <param name="iLineHeight"></param>
        /// <returns></returns>
        public int DrawGlyphWithColors(System.IntPtr hdc, Microsoft.VisualStudio.OLE.Interop.RECT[] pRect, int iMarkerType, IVsTextMarkerColorSet pMarkerColors, uint dwGlyphDrawFlags, int iLineHeight)
        {
            return 0;
        }

        public int GetDefaultFontFlags(out uint pdwFontFlags)
        {
            pdwFontFlags = 0;
            return 0;
        }
        
        public int GetPriorityIndex(out int piPriorityIndex)
        {
            piPriorityIndex = 10000;
            return 0;
        }

        public int GetVisualStyle(out uint pdwVisualFlags)
        {
            pdwVisualFlags = (uint)MARKERVISUAL.MV_COLOR_ALWAYS | (uint)MARKERVISUAL.MV_TIP_FOR_BODY;
           
            return 0;
        }

        public int GetDefaultLineStyle(COLORINDEX[] piLineColor, LINESTYLE[] piLineIndex)
        {
            piLineColor[0] = COLORINDEX.CI_DARKBLUE;
            piLineIndex[0] = LINESTYLE.LI_SOLID;
            return 0;
        }

        public int GetBehaviorFlags(out uint pdwFlags)
        {
            // snap to current line
            pdwFlags = (uint)MARKERBEHAVIORFLAGS.MB_DEFAULT;
            return 0;
        }

        public int GetDefaultColors(COLORINDEX[] piForeground, COLORINDEX[] piBackground)
        {
            piForeground[0] = COLORINDEX.CI_BLACK;
            piBackground[0] = COLORINDEX.CI_YELLOW;
            return 0;
        }

        #endregion

        #region IVsMergeableUIItem Members

        public int GetDisplayName(out string pbstrDisplayName)
        {
            pbstrDisplayName = Resources.YellowHighlightMarkerName;
            return 0;
        }

        public int GetMergingPriority(out int piMergingPriority)
        {
            piMergingPriority = 0x100;
            return 0;
        }

        public int GetCanonicalName(out string pbstrNonLocalizeName)
        {
            pbstrNonLocalizeName = Resources.YellowHighlightMarkerName;
            return 0;
        }

        public int GetDescription(out string pbstrDesc)
        {
            pbstrDesc = Resources.YellowHighlightMarkerDescription;
            return 0;
        }

        #endregion

        /// <summary>
        /// These methods should be called when modfications happen to the text marker
        /// however currently I am not sure why they arent called.
        /// </summary>
        /// <param name="pMarker"></param>
        /// <param name="iItem"></param>
        /// <param name="pbstrText"></param>
        /// <param name="pcmdf"></param>
        /// <returns></returns>
        #region IVsTextMarkerClient Members

        public int GetMarkerCommandInfo(IVsTextMarker pMarker, int iItem, string[] pbstrText, uint[] pcmdf)
        {
            // This is basically like a QueryStatus implementation.
            // TODO:  Add GetMarkerCommandInfo implementation
            return 0;
        }

        public void MarkerInvalidated()
        {
            // TODO:  Add MarkerInvalidated implementation
        }

        public void OnAfterSpanReload()
        {
            // TODO:  Add OnAfterSpanReload implementation
        }

        public void OnBufferSave(string pszFileName)
        {
            // TODO:  Add OnBufferSave implementation
        }

        public void OnBeforeBufferClose()
        {
            // TODO:  Add OnBeforeBufferClose implementation
        }

        public int ExecMarkerCommand(IVsTextMarker pMarker, int iItem)
        {
           
            // TODO:  Add ExecMarkerCommand implementation
            return 0;
        }

        public int OnAfterMarkerChange(IVsTextMarker pMarker)
        {
            // TODO:  Add OnAfterMarkerChange implementation
            return 0;
        }

        public int GetTipText(IVsTextMarker pMarker, string[] pbstrText)
        {
            pbstrText[0] = Resources.YellowHighlightMarkerText;
            return 0;
        }

        #endregion

    }



    /// <summary>
    /// The yellow highlight marker that has a black border
    /// </summary>
    [Guid(GuidList.yellowMarkerWithBorderString)]
    public class YellowHighlightMarkerWithBorder : IVsPackageDefinedTextMarkerType,
        IVsMergeableUIItem,
        IVsTextMarkerClient
    {



        #region IVsPackageDefinedTextMarkerType Members

        /// <summary>
        /// Should never be called we arent using glyphs
        /// </summary>
        /// <param name="hdc"></param>
        /// <param name="pRect"></param>
        /// <param name="iMarkerType"></param>
        /// <param name="pMarkerColors"></param>
        /// <param name="dwGlyphDrawFlags"></param>
        /// <param name="iLineHeight"></param>
        /// <returns></returns>
        public int DrawGlyphWithColors(System.IntPtr hdc, Microsoft.VisualStudio.OLE.Interop.RECT[] pRect, int iMarkerType, IVsTextMarkerColorSet pMarkerColors, uint dwGlyphDrawFlags, int iLineHeight)
        {
            return 0;
        }

        public int GetDefaultFontFlags(out uint pdwFontFlags)
        {
            pdwFontFlags = 0;
            return 0;
        }

        public int GetPriorityIndex(out int piPriorityIndex)
        {
            piPriorityIndex = 10000;
            return 0;
        }

        public int GetVisualStyle(out uint pdwVisualFlags)
        {
            pdwVisualFlags = (uint)MARKERVISUAL.MV_COLOR_ALWAYS | (uint)MARKERVISUAL.MV_BORDER | (uint)MARKERVISUAL.MV_TIP_FOR_BODY;

            return 0;
        }

        public int GetDefaultLineStyle(COLORINDEX[] piLineColor, LINESTYLE[] piLineIndex)
        {
            piLineColor[0] = COLORINDEX.CI_DARKBLUE;
            piLineIndex[0] = LINESTYLE.LI_SOLID;
            return 0;
        }

        public int GetBehaviorFlags(out uint pdwFlags)
        {
            // snap to current line
            pdwFlags = (uint)MARKERBEHAVIORFLAGS.MB_DEFAULT;
            return 0;
        }

        public int GetDefaultColors(COLORINDEX[] piForeground, COLORINDEX[] piBackground)
        {
            piForeground[0] = COLORINDEX.CI_BLACK;
            piBackground[0] = COLORINDEX.CI_YELLOW;
            return 0;
        }

        #endregion

        #region IVsMergeableUIItem Members

        public int GetDisplayName(out string pbstrDisplayName)
        {
            pbstrDisplayName = Resources.YellowHighlightMarkerName;
            return 0;
        }

        public int GetMergingPriority(out int piMergingPriority)
        {
            piMergingPriority = 0x100;
            return 0;
        }

        public int GetCanonicalName(out string pbstrNonLocalizeName)
        {
            pbstrNonLocalizeName = Resources.YellowHighlightMarkerName;
            return 0;
        }

        public int GetDescription(out string pbstrDesc)
        {
            pbstrDesc = Resources.YellowHighlightMarkerDescription;
            return 0;
        }

        #endregion

        /// <summary>
        /// These methods should be called when modfications happen to the text marker
        /// however currently I am not sure why they arent called.
        /// </summary>
        /// <param name="pMarker"></param>
        /// <param name="iItem"></param>
        /// <param name="pbstrText"></param>
        /// <param name="pcmdf"></param>
        /// <returns></returns>
        #region IVsTextMarkerClient Members

        public int GetMarkerCommandInfo(IVsTextMarker pMarker, int iItem, string[] pbstrText, uint[] pcmdf)
        {
            // This is basically like a QueryStatus implementation.
            // TODO:  Add GetMarkerCommandInfo implementation
            return 0;
        }

        public void MarkerInvalidated()
        {

            // TODO:  Add MarkerInvalidated implementation
        }

        public void OnAfterSpanReload()
        {
            // TODO:  Add OnAfterSpanReload implementation
        }

        public void OnBufferSave(string pszFileName)
        {
            // TODO:  Add OnBufferSave implementation
        }

        public void OnBeforeBufferClose()
        {
            // TODO:  Add OnBeforeBufferClose implementation
        }

        public int ExecMarkerCommand(IVsTextMarker pMarker, int iItem)
        {

            // TODO:  Add ExecMarkerCommand implementation
            return 0;
        }

        public int OnAfterMarkerChange(IVsTextMarker pMarker)
        {
            // TODO:  Add OnAfterMarkerChange implementation
            return 0;
        }

        public int GetTipText(IVsTextMarker pMarker, string[] pbstrText)
        {
            pbstrText[0] = Resources.YellowHighlightMarkerText;
            return 0;
        }

        #endregion

    }
}
