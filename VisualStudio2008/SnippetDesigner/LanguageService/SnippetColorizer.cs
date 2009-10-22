using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Package;
using System.Runtime.InteropServices;

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

        public override TokenInfo[] GetLineInfo(IVsTextLines buffer, int line, IVsTextColorState colorState)
        {
            return base.GetLineInfo(buffer, line, colorState);
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

        /// <summary>
        /// Obtains color and font attribute information for each character in the specified line of text.
        /// </summary>
        /// <param name="line">[in] The line number from which the line of text came from.</param>
        /// <param name="length">[in] The number of characters in the given text.</param>
        /// <param name="ptr">[in] An unmarshaled pointer to a line of text.</param>
        /// <param name="state">[in] The current state as maintained by the parser.</param>
        /// <param name="attrs">[in, out] An array that is filled in with indices into the <see cref="M:Microsoft.VisualStudio.Package.LanguageService.GetColorableItem(System.Int32,Microsoft.VisualStudio.TextManager.Interop.IVsColorableItem@)"/> list as maintained by the <see cref="T:Microsoft.VisualStudio.Package.LanguageService"/> class.</param>
        /// <returns>Returns the updated state value.</returns>
        public override int ColorizeLine(int line, int length, IntPtr ptr, int state, uint[] attrs)
        {
            int res = colorizer.ColorizeLine(line, length, ptr, state, attrs);
            return 0;
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
