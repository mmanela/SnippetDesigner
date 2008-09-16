// Copyright (C) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.SnippetDesigner
{

    /// <summary>
    /// String constants that don't belong in a resource file
    /// These are strings which are passed around internally and should not change
    /// </summary>
    internal class StringConstants
    {

        // Most of the string constants are stored in The resource file named ConstantStrings
        // the strings below are store here since they wont work in a resource file since they need to have the const key word
        // to be used in attributes
        // or that they lose their formating in the resource file

        //the vs commands to make a new snippet file
        internal const int TemplateNameResourceID = 106;
        internal static readonly string MakeSnippetDTEArgs = @"/template:" + "\"General\\" + SnippetDesignerPackage.GetResourceString(TemplateNameResourceID) + "\"";
        internal static readonly string NewFileDTECommand = "File.NewFile";

        //the extension associated with a snippet file
        internal const string SnippetExtension = ".snippet";

        //yellow marker name for attribute - this is defined also in a resource but needs to also be here to use in attirbue ProvideCustomMarker
        internal const string YellowHighlightMarkerName = "Yellow Highlight Marker";
        internal const string YellowHighlightMarkerWithBorderName = "Yellow Highlight Marker with Border";
        internal const string MarkerServiceName = "HighlightMarkerService";
    }
}
