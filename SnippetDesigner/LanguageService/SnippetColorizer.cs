using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Package;

namespace Microsoft.SnippetDesigner
{
    /// <summary>
    /// A colorier which delegates the colorization to a IVsColorizer interface
    /// </summary>
    public class SnippetColorizer : Colorizer
    {
        IVsColorizer colorizer;
        IVsTextLines buffer;
        SnippetLanguageService langServ;

        public IVsTextLines Buffer
        {
            get { return buffer; }
            set { buffer = value; }
        }

        public SnippetColorizer(SnippetLanguageService svc, IVsTextLines buffer, IScanner scanner, IVsColorizer colorizer)
            : base(svc, buffer, scanner)
        {
            langServ = svc;
            this.buffer = buffer;
            this.colorizer = colorizer;
        }


        public override int GetStartState(out int start)
        {
            int res = colorizer.GetStartState(out start);
            return res;
        }

        public override int GetStateMaintenanceFlag(out int flag)
        {
            int res = colorizer.GetStateMaintenanceFlag(out flag);
            return res;
        }

        public override int GetStateAtEndOfLine(int line, int length, IntPtr ptr, int state)
        {
            int res = colorizer.GetStateAtEndOfLine(line, length, ptr, state);
            return res;
        }

        public override int GetColorInfo(string line, int length, int state)
        {
            return base.GetColorInfo(line, length, state);
        }

        public override int ColorizeLine(int line, int length, IntPtr ptr, int state, uint[] attrs)
        {
            int res = colorizer.ColorizeLine(line, length, ptr, state, attrs);
            return res;
        }


        public override void CloseColorizer()
        {
            if (langServ != null)
            {
                langServ.OnCloseColorizer(this);
            }

            buffer = null;
            colorizer.CloseColorizer();
            colorizer = null;

            base.CloseColorizer();
        }
    }
}
