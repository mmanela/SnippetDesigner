using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.SnippetDesigner
{
    /// <summary>
    /// The methods needed for a class to host a visual studio code window
    /// </summary>
    internal interface ICodeWindowHost
    {

        /// <summary>
        /// The codewindow will check this to see if it should be read only or not
        /// </summary>
        bool ReadOnlyCodeWindow { get; }

       
        /// <summary>
        /// The service provider that the code window should use
        /// </summary>
        Microsoft.VisualStudio.OLE.Interop.IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Set up the custom context menu with a command filter to override 
        /// the deafult vs code window context menu
        /// </summary>
        void SetupContextMenus();





    }
}
