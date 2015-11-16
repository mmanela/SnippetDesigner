namespace Microsoft.SnippetDesigner
{
    /// <summary>
    /// String constants that don't belong in a resource file
    /// These are strings which are passed around internally and should not change
    /// </summary>
    public static class StringConstants
    {
        public const string NewFileFormat = @"General\Text File";
        public const string NewSnippetTitleFormat = "SnippetFile{0}.snippet";

        //the extension associated with a snippet file
        public const string SnippetExtension = ".snippet";

        //yellow marker name for attribute - this is defined also in a resource but needs to also be here to use in attirbue ProvideCustomMarker
        public const string SnippetReplacementMarker = "Snippet Replacement Marker";
        public const string ActiveSnippetReplacementMarker = "Active Snippet Replacement Marker";
        public const string MarkerServiceName = "HighlightMarkerService";

        public const string ArgumentStartMarker = " ";

        public const string ColumnDefault = "DefaultsTo";
        public const string ColumnEditable = "Editable";
        public const string ColumnFunction = "Function";
        public const string ColumnID = "ID";
        public const string ColumnReplacementKind = "ReplacementKind";
        public const string ColumnTooltip = "Tooltip";
        public const string ColumnType = "Type";
        public const string DoubleQuoteString = "\"";

        public const string ExportNameCPP = "C/C++";
        public const string ExportNameCSharp = "csharp";
        public const string ExportNameVisualBasic = "basic";
        public const string ExportNameXML = "xml";
        public const string ExportNameJavaScript = "jscript";
        public const string ExportNameJavaScript2 = "javascript";
        public const string ExportNameHTML = "html";
        public const string ExportNameSQL = "sql";
        public const string ExportNameSQL2 = "SQL Server Tools";
        
        public const string MySnippetsDir = "My Code Snippets";
        public const string MyXmlSnippetsDir = "My Xml Snippets";
        public const string SchemaNameCPP = "cpp";
        public const string SchemaNameCSharp = "csharp";
        public const string SchemaNameCSharp2 = "vcsharp";
        public const string SchemaNameVisualBasic = "vb";
        public const string SchemaNameXML = "xml";
        public const string SchemaNameJavaScript = "jscript";
        public const string SchemaNameJavaScriptVS11 = "javascript";
        public const string SchemaNameSQL = "sql";
        public const string SchemaNameSQLServerDataTools = "SQL_SSDT";
        public const string SchemaNameHTML = "html";
        public const string SchemaNameXAML = "xaml";

        public const string SnippetDirectoryName = "Code Snippets";
        public const string SnippetDirNameCPP = "Visual C++";
        public const string SnippetDirNameCSharp = "Visual C#";
        public const string SnippetDirNameVisualBasic = "Visual Basic";
        public const string SnippetDirNameXML = "XML";
        public const string SnippetDirNameSQL = "SQL_SSDT";
        public const string SnippetDirNameSQLServerDataTools = "SQL_SSDT";
        public const string SnippetDirNameHTML = "My HTML Snippets";
        public const string SnippetDirNameJavaScript = "My JScript Snippets";
        public const string SnippetDirNameJavaScriptVS11 = "JavaScript";
        public const string SnippetDirNameXAML = "My XAML Snippets";

        public const string VisualWebDeveloper = "Visual Web Developer";
        public const string SnippetTypeExpansion = "Expansion";
        public const string SnippetTypeMethodBody = "method body";
        public const string SnippetTypeMethodDeclaration = "method decl";
        public const string SnippetTypeTypeDeclaration = "type decl";
        public const string SymbolEndWord = "end";
        public const string SymbolSelected = "$selected$";
        public const string SymbolSelectedWord = "selected";
        public const string VSRegistryRegistrationName = "Registration";
        public const string VSRegistryRegistrationNameEntry = "UserName";
    }
}