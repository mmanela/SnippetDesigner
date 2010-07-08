using System;
using System.Windows.Controls;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.SnippetDesigner
{
    public class CodeWindowWPF : ISnippetCodeWindow
    {
        private IWpfTextViewHost textViewHost;
        private IWpfTextView textView;

        private ICodeWindowHost codeWindowHost;
        private SnippetEditor snippetEditor;
        private IntPtr hWndCodeWindow;
        private uint cookieTextViewEvents;
        private uint cookieTextLineEvents;
        private bool isHandleCreated;
        private bool isTextInitialized;
        private readonly IVsEditorAdaptersFactoryService editorAdapterFactoryService;
        private ITextSearchService textSearchService;
        private readonly IContentTypeRegistryService contentTypeService;

        private readonly ITextEditorFactoryService textEditorFactoryService;
        private ITextBufferFactoryService textBufferFactoryService;

        public CodeWindowWPF()
        {
            textEditorFactoryService = SnippetDesignerPackage.Instance.ComponentModel.GetService<ITextEditorFactoryService>();
            textBufferFactoryService = SnippetDesignerPackage.Instance.ComponentModel.GetService<ITextBufferFactoryService>();
            editorAdapterFactoryService =
                SnippetDesignerPackage.Instance.ComponentModel.GetService<IVsEditorAdaptersFactoryService>();
            textSearchService = SnippetDesignerPackage.Instance.ComponentModel.GetService<ITextSearchService>();
            contentTypeService =
                SnippetDesignerPackage.Instance.ComponentModel.GetService<IContentTypeRegistryService>();

            Initialize();
        }

        public CodeWindowWPF(
            ITextEditorFactoryService textEditorFactoryService,
            ITextBufferFactoryService textBufferFactoryService,
            IVsEditorAdaptersFactoryService editorAdapterFactoryService,
            ITextSearchService textSearchService,
            IContentTypeRegistryService contentTypeService)
        {
            this.textEditorFactoryService = textEditorFactoryService;
            this.textBufferFactoryService = textBufferFactoryService;
            this.editorAdapterFactoryService = editorAdapterFactoryService;
            this.textSearchService = textSearchService;
            this.contentTypeService = contentTypeService;
        }


        public void Initialize()
        {
            textView = textEditorFactoryService.CreateTextView();
            textViewHost = textEditorFactoryService.CreateTextViewHost(textView, true);
        }

        public Control Control
        {
            get { return textViewHost.HostControl; }
        }


        public string CodeText
        {
            get { return ""; }
            set { textView.TextBuffer.Insert(0, value); }
        }

        public void SetContentType()
        {
        }
    }
}