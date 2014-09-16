using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace SnippetDesignerComponents
{
    [Export(typeof (EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "snippet-replacement")]
    [Name("snippet-replacement")]
    [UserVisible(true)]
    [Order(After = Priority.High)]
    internal sealed class SnippetReplacementFormat : ClassificationFormatDefinition
    {
        public SnippetReplacementFormat()
        {
            DisplayName = "Snippet Replacement";
            BackgroundColor = Colors.Yellow;
        }
    }
}