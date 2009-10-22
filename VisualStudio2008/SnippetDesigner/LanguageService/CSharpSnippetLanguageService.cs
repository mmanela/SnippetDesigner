using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Microsoft.SnippetDesigner
{
    [ComVisible(true)]
    [Guid(GuidList.csharpSnippetLanguageServiceString)]
    public class CSharpSnippetLanguageService : SnippetLanguageService
    {
        public CSharpSnippetLanguageService()
            : base(Language.CSharp)
        {
        }
    }
}
