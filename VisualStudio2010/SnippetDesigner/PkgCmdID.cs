// Copyright (C) Microsoft Corporation. All rights reserved.

// PkgCmdID.cs must match SnippetDesigner

namespace Microsoft.SnippetDesigner
{
    public static class PkgCmdIDList
    {
        // Menus
        public const uint SnippetContextMenu = 0x2100;

        // Groups
        public const uint SnippetEditGrp = 0x1030;
        public const uint SnippetCustGrp = 0x1040;
        public const uint SnippetExportGroup = 0x1050;

        // Commands
        public const uint cmdidSnippetExplorer = 0x101;
        public const uint cmdidSnippetMakeReplacement = 0x102;
        public const uint cmdidExportToSnippet = 0x103;
        public const uint cmdidCreateSnippet = 0x104;
        public const uint cmdidExportToSnippetCommandLine = 0x105;
        public const uint cmdidSnippetRemoveReplacement = 0x106;
        public const uint cmdidSnippetInsertEnd = 0x107;
        public const uint cmdidSnippetInsertSelected = 0x108;
        public const uint cmdidSnippetReplacementMarker = 0x1100;
    };
}