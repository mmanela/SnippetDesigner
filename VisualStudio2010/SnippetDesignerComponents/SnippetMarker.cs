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
            Fill = new SolidColorBrush(Color.FromArgb(100,223, 223, 255));
            Fill.Freeze();
        }
    }
}