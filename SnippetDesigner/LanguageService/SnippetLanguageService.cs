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
    /// <summary>
    /// The base snipet language service.  This is a kind of "proxy" language service.  
    /// It deferers certain language service opertaions to another language service like C#.
    /// This lets us get the C# language service to colorize the snippet code but not report errors from it.
    /// </summary>
    public abstract class SnippetLanguageService : LanguageService
    {
        private Language language;
        private IVsLanguageInfo languageInfo;

        // Cached colorizers
        private List<SnippetColorizer> colorizers = new List<SnippetColorizer>();

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

        /// <summary>
        /// Gets the colorizer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns></returns>
        public override Colorizer GetColorizer(IVsTextLines buffer)
        {

            foreach (SnippetColorizer cachedColorizer in colorizers)
            {
                if (cachedColorizer.Buffer == buffer)
                {
                    return cachedColorizer;
                }
            }

            IVsColorizer colorizer = null;
            languageInfo.GetColorizer(buffer, out colorizer);
            SnippetColorizer colorizerWrapper = new SnippetColorizer(this, buffer, null, colorizer);
            colorizers.Add(colorizerWrapper);

            return colorizerWrapper;
        }

        public virtual void OnCloseColorizer(SnippetColorizer sc)
        {
            if ((this.colorizers != null) && colorizers.Contains(sc))
            {
                colorizers.Remove(sc);
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

        #region IDisposable Members

        public override void Dispose()
        {
            base.Dispose();
            if (colorizers != null)
            {
                foreach (SnippetColorizer colorizer in colorizers)
                {
                    colorizer.Dispose();
                }
                colorizers.Clear();
                colorizers = null;
            }
        }

        #endregion
    }
}
