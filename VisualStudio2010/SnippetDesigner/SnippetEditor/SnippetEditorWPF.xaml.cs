using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Microsoft.SnippetDesigner
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class SnippetEditorWPF : UserControl
    {
        private CodeWindowWPF wpfCodeWindow;
        public SnippetEditorWPF()
        {
            InitializeComponent();
            wpfCodeWindow = new CodeWindowWPF();
            codeEditorContianer.Children.Add(wpfCodeWindow.Control);
        }

        public bool IsFormDirty { get; set; }

        public void RefreshReplacementMarkers()
        {
        }

        public void LoadSnippet(string file)
        {
        }

        public void SaveSnippet()
        {
        }

        public void SaveSnippetAs(string file)
        {
        }
    }
}
