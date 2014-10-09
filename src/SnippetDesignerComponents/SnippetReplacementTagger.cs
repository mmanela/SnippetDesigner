using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;

namespace SnippetDesignerComponents
{
    public class SnippetReplacementTagger : ITagger<ClassificationTag>
    {
        private readonly IClassificationType classificationType;
        public const string ReplacementDelimiter = "ReplacementDelimiter";
        public const string TaggerInstance = "TaggerInstance";

        private ITextView View { get; set; }
        private ITextBuffer SourceBuffer { get; set; }
        private ITextSearchService TextSearchService { get; set; }
        private ITextStructureNavigator TextStructureNavigator { get; set; }
        private readonly object updateLock = new object();


        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        // The current set of replacements to highlight
        private NormalizedSnapshotSpanCollection WordSpans { get; set; }


        public SnippetReplacementTagger(ITextView view, ITextBuffer sourceBuffer, ITextSearchService textSearchService,
                                        ITextStructureNavigator textStructureNavigator, IClassificationType classificationType)
        {
            this.classificationType = classificationType;
            View = view;
            SourceBuffer = sourceBuffer;
            TextSearchService = textSearchService;
            TextStructureNavigator = textStructureNavigator;
            
            WordSpans = new NormalizedSnapshotSpanCollection();

            View.LayoutChanged += ViewLayoutChanged;

            View.Properties[TaggerInstance] = this;

            UpdateSnippetReplacementAdornmentsAsync();

        }

        public void UpdateSnippetReplacementAdornmentsAsync()
        {
            ThreadPool.QueueUserWorkItem(UpdateSnippetReplacementAdornments);
        }


        private void ViewLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            // If a new snapshot wasn't generated, then skip this layout
            if (e.NewViewState.EditSnapshot != e.OldViewState.EditSnapshot)
            {
                ThreadPool.QueueUserWorkItem(UpdateSnippetReplacementAdornments);
            }
        }

        private void UpdateSnippetReplacementAdornments(object threadContext)
        {
            try
            {
                var delimiter = !View.Properties.ContainsProperty(ReplacementDelimiter) ? null : View.Properties[ReplacementDelimiter] as string;
                delimiter = string.IsNullOrEmpty(delimiter) ? "$" : delimiter;

                var validReplacementString = SnippetRegexPatterns.BuildValidReplacementString(delimiter);

                var wordSpans = new List<SnapshotSpan>();
                var findOptions = FindOptions.UseRegularExpressions;
                var findData = new FindData(validReplacementString, View.TextBuffer.CurrentSnapshot, findOptions, null);
                wordSpans.AddRange(TextSearchService.FindAll(findData));

                SynchronousUpdate(new NormalizedSnapshotSpanCollection(wordSpans));
            }
            catch (ArgumentException)
            {
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Perform a synchronous update, in case multiple background threads are running
        /// </summary>
        private void SynchronousUpdate(NormalizedSnapshotSpanCollection newSpans)
        {
            lock (updateLock)
            {
                WordSpans = newSpans;

                var tempEvent = TagsChanged;
                if (tempEvent != null)
                    tempEvent(this,
                              new SnapshotSpanEventArgs(new SnapshotSpan(SourceBuffer.CurrentSnapshot, 0,
                                                                         SourceBuffer.CurrentSnapshot.Length)));
            }
        }


        public IEnumerable<ITagSpan<ClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
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
                yield return new TagSpan<ClassificationTag>(span, new ClassificationTag(classificationType));
            }
        }
    }
}