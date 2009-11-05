using System;
using System.Runtime.InteropServices;

namespace Microsoft.SnippetDesigner
{
    [ComVisible(true)]
    [Guid(GuidList.xmlSnippetLanguageServiceString)]
    public class XMLSnippetLanguageService : SnippetLanguageService
    {
        public XMLSnippetLanguageService()
            : base(Language.XML)
        {
        }
    }
}
