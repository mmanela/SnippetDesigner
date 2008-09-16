// Copyright (C) Microsoft Corporation. All rights reserved.

// PkgCmdID.cs must match SnippetDesigner
using System;

namespace Microsoft.SnippetDesigner
{
    static class PkgCmdIDList
    {
        // Menus
        internal const uint SnippetContextMenu = 0x2100;

        // Groups
        internal const uint SnippetEditGrp = 0x1030;
        internal const uint SnippetCustGrp = 0x1040;
        internal const uint SnippetExportGroup = 0x1050;

        // Commands
        internal const uint cmdidSnippetExplorer = 0x101;
        internal const uint cmdidSnippetMakeReplacement = 0x102;
        internal const uint cmdidExportToSnippet = 0x103;
        internal const uint cmdidCreateSnippet = 0x104;
        internal const uint cmdidExportToSnippetCommandLine = 0x105;
        internal const uint cmdidSnippetRemoveReplacement = 0x106;
        internal const uint cmdidYellowHighlightMarker = 0x1100;
        internal const uint cmdidYellowHighlightMarkerWithBorder = 0x1200;
    };
}