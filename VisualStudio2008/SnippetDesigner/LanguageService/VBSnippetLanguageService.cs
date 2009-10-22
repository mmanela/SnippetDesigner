using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Microsoft.SnippetDesigner
{
    [ComVisible(true)]
    [Guid(GuidList.vbSnippetLanguageServiceString)]
    public class VBSnippetLanguageService : SnippetLanguageService
    {
        public VBSnippetLanguageService()
            : base(Language.VisualBasic)
        {
        }
    }
}
