using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio;
using MsOle = Microsoft.VisualStudio.OLE.Interop;

namespace Microsoft.SnippetDesigner
{
    public abstract class SnippetLanguageService : LanguageService
    {
        private Language language;
        private IVsLanguageInfo languageInfo;

        public SnippetLanguageService(Language lang)
        {
            language = lang;
            languageInfo = GetLanguagInfo(lang);
        }


        /// <summary>
        /// Gets the language service.
        /// </summary>
        /// <param name="lang">The language.</param>
        /// <returns></returns>
        private IVsLanguageInfo GetLanguagInfo(Language lang)
        {

            MsOle.IServiceProvider provider =
                (MsOle.IServiceProvider)SnippetDesignerPackage.Instance.GetService(typeof(MsOle.IServiceProvider));

            Guid guidLangSvc = LanguageMaps.LanguageMap.LanguageGuids[lang];
            Guid riid = Microsoft.VisualStudio.VSConstants.IID_IUnknown;
            Guid iid = typeof(IVsLanguageInfo).GUID;
            IntPtr ppvObject = IntPtr.Zero;
            int result = provider.QueryService(ref guidLangSvc, ref iid, out ppvObject);
            object langSvc = Marshal.GetObjectForIUnknown(ppvObject);
            IVsLanguageInfo li = langSvc as IVsLanguageInfo;
            if (ppvObject != IntPtr.Zero)
                Marshal.Release(ppvObject);

            return li;
        }

        public override string GetFormatFilterList()
        {
            return "*";
        }

        public override LanguagePreferences GetLanguagePreferences()
        {
            throw new NotImplementedException();
        }

        public override int GetColorableItem(int index, out IVsColorableItem item)
        {
            
            return ((IVsProvideColorableItems)languageInfo).GetColorableItem(index, out item);
        }

        public override int GetItemCount(out int count)
        {
            return ((IVsProvideColorableItems)languageInfo).GetItemCount(out count);
        }

        public override Colorizer GetColorizer(IVsTextLines buffer)
        {

            IVsColorizer colorizer = null;
            languageInfo.GetColorizer(buffer, out colorizer);

            Colorizer colorizerWrapper = new SnippetColorizer(this, buffer, null, colorizer);
            return colorizerWrapper;
        }

        /// <summary>
        /// A colorier which delegates the colorization to a IVsColorizer interface
        /// </summary>
        class SnippetColorizer : Colorizer
        {
            IVsColorizer colorizer;
            public SnippetColorizer(LanguageService svc, IVsTextLines buffer, IScanner scanner, IVsColorizer colorizer)
                : base(svc, buffer, scanner)
            {
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
        }

        public override IScanner GetScanner(IVsTextLines buffer)
        {
            throw new NotImplementedException();
        }

        public override string Name
        {
            get
            {

                return "Snippet Designer Language Service";
            }
        }

        public override AuthoringScope ParseSource(ParseRequest req)
        {
            throw new NotImplementedException();
        }
    }
}
