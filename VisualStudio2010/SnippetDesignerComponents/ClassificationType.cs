using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace SnippetDesignerComponents
{
    internal static class ClassificationType
    {
        [Export(typeof (ClassificationTypeDefinition))] 
        [Name("snippet-replacement")] 
        private static ClassificationTypeDefinition SnippetReplacementClassificationType;
    }
}