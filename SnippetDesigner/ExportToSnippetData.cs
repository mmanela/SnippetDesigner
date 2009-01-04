// Copyright (C) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.SnippetDesigner
{
    /// <summary>
    /// Represents the data that is exported from a code file to the snippet codeWindowHost
    /// </summary>
    public class ExportToSnippetData
    {
        //member variables
        private string snippetCode;
        private string snippetLanguage;
        Dictionary<string, string> exportNameToSchemaName = new Dictionary<string, string>();


        /// <summary>
        /// Gets the code.
        /// </summary>
        /// <value>The code.</value>
        internal string Code
        {
            get
            {
                return snippetCode;
            }
        }

        /// <summary>
        /// Gets the language.
        /// </summary>
        /// <value>The language.</value>
        internal string Language
        {
            get
            {
                return snippetLanguage;
            }
        }
        
        internal ExportToSnippetData(string code, string language)
        {
            exportNameToSchemaName[SnippetDesigner.StringConstants.ExportNameCSharp] = SnippetDesigner.StringConstants.SchemaNameCSharp;
            exportNameToSchemaName[SnippetDesigner.StringConstants.SchemaNameCSharp] = SnippetDesigner.StringConstants.SchemaNameCSharp;
            exportNameToSchemaName[SnippetDesigner.Resources.DisplayNameCSharp] = SnippetDesigner.StringConstants.SchemaNameCSharp;

            exportNameToSchemaName[SnippetDesigner.StringConstants.ExportNameVisualBasic] = SnippetDesigner.StringConstants.SchemaNameVisualBasic;
            exportNameToSchemaName[SnippetDesigner.StringConstants.SchemaNameVisualBasic] = SnippetDesigner.StringConstants.SchemaNameVisualBasic;
            exportNameToSchemaName[SnippetDesigner.Resources.DisplayNameVisualBasic] = SnippetDesigner.StringConstants.SchemaNameVisualBasic;

            exportNameToSchemaName[SnippetDesigner.StringConstants.ExportNameXML] = SnippetDesigner.StringConstants.SchemaNameXML;
            exportNameToSchemaName[SnippetDesigner.StringConstants.SchemaNameXML] = SnippetDesigner.StringConstants.SchemaNameXML;
            exportNameToSchemaName[SnippetDesigner.Resources.DisplayNameXML] = SnippetDesigner.StringConstants.SchemaNameXML;

           snippetCode = code;
           if (exportNameToSchemaName.ContainsKey(language))
           {
               snippetLanguage = exportNameToSchemaName[language];
           }
           else
           {
               //pass empty string if we dont know the language passed in
               snippetLanguage = String.Empty;
           }
        }

    }
}
