using Microsoft.VisualStudio.Text.Tagging;

namespace SnippetDesignerComponents
{
    public class SnippetReplacementTag : TextMarkerTag
    {
        public SnippetReplacementTag()
            : base("snippetmarker")
        {
        }
    }
}