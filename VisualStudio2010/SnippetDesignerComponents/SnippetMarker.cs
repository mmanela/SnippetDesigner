using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace SnippetDesignerComponents
{
    [Export(typeof (EditorFormatDefinition))]
    [Name("snippetmarker")]
    internal sealed class SnippetMarker : MarkerFormatDefinition
    {
        public SnippetMarker()
        {
            ZOrder = 1;
            Fill = Brushes.Yellow;
            Fill.Freeze();
        }
    }
}