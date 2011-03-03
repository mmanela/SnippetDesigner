// Copyright (C) Microsoft Corporation. All rights reserved.


namespace Microsoft.SnippetDesigner
{
    /// <summary>
    /// Interface which defines all the required function for a code window to have.  If any other text editor implements these then we can 
    /// use that editor instead of the vs code window 
    /// </summary>
    internal interface ISnippetCodeWindow
    {
        /// <summary>
        /// Get and set the text in the code window
        /// </summary>
        string CodeText{get; set;}
    }
}
