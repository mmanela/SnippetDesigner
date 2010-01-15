using System.Text;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System.Text.RegularExpressions;
using System.Windows.Threading;


namespace SnippetDesignerComponents
{

    [Export(typeof(IViewTaggerProvider))]
    [ContentType("text")]
    [TagType(typeof(SnippetReplacementTag))]
    public class SnippetReplacementTaggerProvider : IViewTaggerProvider
    {
        [Import]
        internal ITextSearchService TextSearchService { get; set; }

        [Import]
        internal ITextStructureNavigatorSelectorService TextStructureNavigatorSelector { get; set; }

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            // Only provide highlighting on the top-level buffer
            if (textView.TextBuffer != buffer)
                return null;

            ITextStructureNavigator textStructureNavigator =
                TextStructureNavigatorSelector.GetTextStructureNavigator(buffer);

            return new SnippetReplacementTagger(textView, buffer, TextSearchService, textStructureNavigator) as ITagger<T>;
        }
    }

    public class SnippetReplacementTag : TextMarkerTag
    {
        public SnippetReplacementTag() : base("blue") { }
    }

    public class SnippetReplacementTagger : ITagger<SnippetReplacementTag>
    {
        public const string ReplacementListKey = "CurrentReplacements";

        ITextView View { get; set; }
        ITextBuffer SourceBuffer { get; set; }
        ITextSearchService TextSearchService { get; set; }
        ITextStructureNavigator TextStructureNavigator { get; set; }
        object updateLock = new object();


        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        // The current set of replacements to highlight
        NormalizedSnapshotSpanCollection WordSpans { get; set; }

        int ReplacementCount { get; set; }


        public SnippetReplacementTagger(ITextView view, ITextBuffer sourceBuffer, ITextSearchService textSearchService,
                                   ITextStructureNavigator textStructureNavigator)
        {
            this.View = view;
            this.SourceBuffer = sourceBuffer;
            this.TextSearchService = textSearchService;
            this.TextStructureNavigator = textStructureNavigator;

            this.WordSpans = new NormalizedSnapshotSpanCollection();

            this.View.LayoutChanged += ViewLayoutChanged;
        
            ThreadPool.QueueUserWorkItem(UpdateSnippetReplacementAdornments);
        }



        void ViewLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            // If a new snapshot wasn't generated, then skip this layout
            if (e.NewViewState.EditSnapshot != e.OldViewState.EditSnapshot)
            {
                ThreadPool.QueueUserWorkItem(UpdateSnippetReplacementAdornments);
            }
        }

        void UpdateSnippetReplacementAdornments(object threadContext)
        {
            try
            {
                const string DecoratedReplacement = @"\${0}\$";
                for(int i = 0; i < 1000 && !View.Properties.ContainsProperty(ReplacementListKey); i++)
                {
                    Thread.Sleep(10);
                }

                if (!View.Properties.ContainsProperty(ReplacementListKey)) return;

                List<string> currentReplacements = View.Properties[ReplacementListKey] as List<string>;
                ReplacementCount = currentReplacements.Count;

                if (currentReplacements == null || currentReplacements.Count == 0)
                    SynchronousUpdate(ReplacementCount, new NormalizedSnapshotSpanCollection()); ;

                List<SnapshotSpan> wordSpans = new List<SnapshotSpan>();

                foreach (var replacement in currentReplacements)
                {
                    var findOptions = FindOptions.UseRegularExpressions | FindOptions.MatchCase;
                    var findData = new FindData(string.Format(DecoratedReplacement, replacement), View.TextBuffer.CurrentSnapshot, findOptions, null);
                    wordSpans.AddRange(TextSearchService.FindAll(findData));

                }

                if (ReplacementCount == currentReplacements.Count)
                    SynchronousUpdate(ReplacementCount, new NormalizedSnapshotSpanCollection(wordSpans));
            }
            catch (ArgumentException)
            {
            }
            catch (Exception) { }

        }

        /// <summary>
        /// Perform a synchronous update, in case multiple background threads are running
        /// </summary>
        void SynchronousUpdate(int replacementCount, NormalizedSnapshotSpanCollection newSpans)
        {
            lock (updateLock)
            {
                if (replacementCount != ReplacementCount)
                    return;

                WordSpans = newSpans;

                var tempEvent = TagsChanged;
                if (tempEvent != null)
                    tempEvent(this, new SnapshotSpanEventArgs(new SnapshotSpan(SourceBuffer.CurrentSnapshot, 0, SourceBuffer.CurrentSnapshot.Length)));
            }
        }


        public IEnumerable<ITagSpan<SnippetReplacementTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (ReplacementCount == 0)
                yield break;

            // Hold on to a "snapshot" of the word spans, so that we maintain the same
            // collection throughout
            NormalizedSnapshotSpanCollection wordSpans = WordSpans;

            if (spans.Count == 0 || WordSpans.Count == 0)
                yield break;

            // If the requested snapshot isn't the same as the one our words are on, translate our spans
            // to the expected snapshot
            if (spans[0].Snapshot != wordSpans[0].Snapshot)
            {
                wordSpans = new NormalizedSnapshotSpanCollection(
                    wordSpans.Select(span => span.TranslateTo(spans[0].Snapshot, SpanTrackingMode.EdgeExclusive)));
            }

            // Yield all the replacement spans in the file
            foreach (SnapshotSpan span in NormalizedSnapshotSpanCollection.Overlap(spans, wordSpans))
            {
                yield return new TagSpan<SnippetReplacementTag>(span, new SnippetReplacementTag());
            }
        }
    }

}


