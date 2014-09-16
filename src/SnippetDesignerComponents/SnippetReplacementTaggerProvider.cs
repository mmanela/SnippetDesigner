using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace SnippetDesignerComponents
{
    [Export(typeof (IViewTaggerProvider))]
    [ContentType("codesnippet")]
    [TagType(typeof (ClassificationTag))]
    public class SnippetReplacementTaggerProvider : IViewTaggerProvider
    {
        [Import] private IClassificationTypeRegistryService Registry;

        [Import]
        internal ITextSearchService TextSearchService { get; set; }

        [Import]
        internal ITextStructureNavigatorSelectorService TextStructureNavigatorSelector { get; set; }

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            // Only provide highlighting on the top-level buffer
            if (textView.TextBuffer != buffer)
                return null;

            ITextStructureNavigator textStructureNavigator = TextStructureNavigatorSelector.GetTextStructureNavigator(buffer);

            return new SnippetReplacementTagger(textView, buffer, TextSearchService, textStructureNavigator, Registry.GetClassificationType("snippet-replacement")) as ITagger<T>;
        }
    }
}