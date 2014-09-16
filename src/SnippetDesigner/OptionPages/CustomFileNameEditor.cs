using System.Windows.Forms.Design;

namespace Microsoft.SnippetDesigner.OptionPages
{
    public class CustomFileNameEditor : FileNameEditor
    {
        protected override void InitializeDialog(System.Windows.Forms.OpenFileDialog openFileDialog)
        {
            base.InitializeDialog(openFileDialog);
            openFileDialog.DefaultExt = "xml";
            openFileDialog.AddExtension = true;
            openFileDialog.CheckFileExists = false;
            openFileDialog.CheckPathExists = false;
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "XML Files (*.xml)|*.xml|All files (*.*)|*.*";
        }
    }
}
