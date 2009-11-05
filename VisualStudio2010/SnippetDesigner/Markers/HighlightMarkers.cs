using System;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.SnippetDesigner
{

    public class  SnippetReplacementMarker : 
        IVsTextMarkerClientAdvanced,        
        IVsPackageDefinedTextMarkerType,
        IVsMergeableUIItem,
        IVsTextMarkerClient
    {
        #region IVsPackageDefinedTextMarkerType Members

        public int DrawGlyphWithColors(IntPtr hdc, RECT[] pRect, int iMarkerType, IVsTextMarkerColorSet pMarkerColors, uint dwGlyphDrawFlags, int iLineHeight)
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
            pbstrDisplayName = Resources.SnippetReplacementMarker;
            return 0;
        }

        public int GetMergingPriority(out int piMergingPriority)
        {
            piMergingPriority = 0x100;
            return 0;
        }

        public int GetCanonicalName(out string pbstrNonLocalizeName)
        {
            pbstrNonLocalizeName = Resources.SnippetReplacementMarker;
            return 0;
        }

        public int GetDescription(out string pbstrDesc)
        {
            pbstrDesc = Resources.SnippetReplacementMarkerDescription;
            return 0;
        }

        #endregion


        #region IVsTextMarkerClient Members

        public int GetMarkerCommandInfo(IVsTextMarker pMarker, int iItem, string[] pbstrText, uint[] pcmdf)
        {
            return 0;
        }

        public void MarkerInvalidated()
        {
        }

        public void OnAfterSpanReload()
        {
        }

        public void OnBufferSave(string pszFileName)
        {
        }

        public void OnBeforeBufferClose()
        {
        }

        public int ExecMarkerCommand(IVsTextMarker pMarker, int iItem)
        {
            return 0;
        }

        public int OnAfterMarkerChange(IVsTextMarker pMarker)
        {
            return 0;
        }

        public int GetTipText(IVsTextMarker pMarker, string[] pbstrText)
        {
            pbstrText[0] = Resources.SnippetReplacementMarkerText;
            return 0;
        }

        #endregion

        #region IVsTextMarkerClientAdvanced Members

        public int OnMarkerTextChanged(IVsTextMarker pMarker)
        {
            var markerLine = pMarker as IVsTextLineMarker;
            var span = new TextSpan[1];
            IVsTextLines buffer;
            string text = null;
            markerLine.GetCurrentSpan(span);
            markerLine.GetLineBuffer(out buffer);
            buffer.GetLineText(span[0].iStartLine, span[0].iStartIndex, span[0].iEndLine, span[0].iEndIndex, out text);
            if (!SnippetEditorForm.ValidExistingReplacementRegex.IsMatch(text))
            {
                pMarker.Invalidate();
                pMarker.UnadviseClient();
            }
            return 0;
        }

        #endregion
    }
}