using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.SnippetDesigner.OptionPages
{
    /// <summary>
    /// Reset options page
    /// </summary>
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid("0F9D79A2-121F-484e-8DE9-62A1EF289301")]
    public class ResetOptions : DialogPage
    {
        public ResetOptions()
        {
          
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected override IWin32Window Window
        {
            get
            {
                ResetOptionsControl control = new ResetOptionsControl();
                control.Location = new Point(0, 0);
                return control;
            }
        }
    }
}

