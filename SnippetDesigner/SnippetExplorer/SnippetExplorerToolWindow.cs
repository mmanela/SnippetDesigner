// Copyright (C) Microsoft Corporation. All rights reserved.

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio;

using Microsoft.VisualStudio.OLE.Interop;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using MsOle = Microsoft.VisualStudio.OLE.Interop;

namespace Microsoft.SnippetDesigner.SnippetExplorer
{
    /// <summary>
    /// This class implements the tool window exposed by this sacPackage and hosts a user snippetExplorerForm.
    ///
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane, 
    /// usually implemented by the sacPackage implementer.
    ///
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
            this.BitmapIndex = 1;
           

            snippetExplorerForm = new SnippetExplorerForm();
        }




       
        public override void OnToolBarAdded()
        {
            base.OnToolBarAdded();




            // In general it is not useful to override this method,
            // but it is useful when the tool window hosts a toolbar
            // with a drop-down (combo box) that needs to be initialized.
            // If that were the case, the initalization would happen here.
        }






        /// <summary>
        /// This method can be overriden by the derived class to execute any code that
        /// needs to run after the IVsWindowFrame is created.  If the toolwindow has
        /// a toolbar with a combobox, it should make sure its command handler are set
        /// by the time they return from this method.  This is called when someone set
        /// the Frame property.
        /// </summary>
        public override void OnToolWindowCreated()
        {
            base.OnToolWindowCreated();
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

        /// <summary>
        /// Define a command handler.
        /// When the user presses the button corresponding to the CommandID,
        /// then the EventHandler will be called.
        /// </summary>
        /// <param name="id">The CommandID (Guid/ID pair) as defined in the .ctc file</param>
        /// <param name="handler">Method that should be called to implement the command</param>
        /// <returns>The menu command. This can be used to set parameter such as the default visibility once the package is loaded</returns>
        private OleMenuCommand DefineCommandHandler(EventHandler handler, CommandID id)
        {
            // First add it to the package. This is to keep the visibility
            // of the command on the toolbar constant when the tool window does
            // not have focus. In addition, it creates the command object for us.
            SnippetDesignerPackage package = SnippetDesignerPackage.Instance;
            OleMenuCommand command = package.DefineCommandHandler(handler, id);
            // Verify that the command was added
            if (command == null)
                return command;

            // Get the OleCommandService object provided by the base window pane class; this object is the one
            // responsible for handling the collection of commands implemented by the package.
            OleMenuCommandService menuService = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;

            if (null != menuService)
            {
                // Add the command handler
                menuService.AddCommand(command);
            }
            return command;
        }


    }
}
