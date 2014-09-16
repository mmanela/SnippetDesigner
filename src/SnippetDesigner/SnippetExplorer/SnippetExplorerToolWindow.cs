// Copyright (C) Microsoft Corporation. All rights reserved.

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;


namespace Microsoft.SnippetDesigner.SnippetExplorer
{
    /// <summary>
    /// This class implements the tool window exposed by this sacPackage and hosts a user snippetExplorerForm.
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the sacPackage implementer.
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsWindowPane interface.
    /// </summary>
    [Guid(GuidList.snippetExplorerString)]
    [ComVisible(true)]
    public class SnippetExplorerToolWindow : ToolWindowPane
    {
        // This is the user snippetExplorerForm hosted by the tool window; it is exposed to the base class 
        // using the Window property. Note that, even if this class implements IDispose, we are
        // not calling Dispose on this object. This is because ToolWindowPane calls Dispose on 
        // the object returned by the Window property.
        private SnippetExplorerForm snippetExplorerForm;




        /// <summary>
        /// Standard constructor for the tool window.
        /// </summary>
        public SnippetExplorerToolWindow()
            :
            base(null)
        {
            // Set the window title reading it from the resources.
            this.Caption = Resources.ToolWindowTitle;
            // Set the image that will appear on the tab of the window frame
            // when docked with an other window
            // The resource ID correspond to the one defined in the resx file
            // while the Index is the offset in the bitmap strip. Each image in
            // the strip being 16x16.
            this.BitmapResourceID = 301;
            this.BitmapIndex = 2;

            snippetExplorerForm = new SnippetExplorerForm();
        }


        /// <summary>
        /// This property returns the handle to the user snippetExplorerForm that should
        /// be hosted in the Tool Window.
        /// </summary>
        override public IWin32Window Window
        {
            get
            {
                return (IWin32Window)snippetExplorerForm;
            }
        }
    }
}
