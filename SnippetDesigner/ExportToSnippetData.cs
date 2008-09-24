// Copyright (C) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.SnippetDesigner
{
    /// <summary>
    /// Represents the data that is exported from a code file to the snippet codeWindowHost
    /// </summary>
    internal class ExportToSnippetData
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
            exportNameToSchemaName[SnippetDesigner.ConstantStrings.ExportNameCSharp] = SnippetDesigner.ConstantStrings.SchemaNameCSharp;
            exportNameToSchemaName[SnippetDesigner.ConstantStrings.SchemaNameCSharp] = SnippetDesigner.ConstantStrings.SchemaNameCSharp;
            exportNameToSchemaName[SnippetDesigner.Resources.DisplayNameCSharp] = SnippetDesigner.ConstantStrings.SchemaNameCSharp;

            exportNameToSchemaName[SnippetDesigner.ConstantStrings.ExportNameVisualBasic] = SnippetDesigner.ConstantStrings.SchemaNameVisualBasic;
            exportNameToSchemaName[SnippetDesigner.ConstantStrings.SchemaNameVisualBasic] = SnippetDesigner.ConstantStrings.SchemaNameVisualBasic;
            exportNameToSchemaName[SnippetDesigner.Resources.DisplayNameVisualBasic] = SnippetDesigner.ConstantStrings.SchemaNameVisualBasic;

            exportNameToSchemaName[SnippetDesigner.ConstantStrings.ExportNameXML] = SnippetDesigner.ConstantStrings.SchemaNameXML;
            exportNameToSchemaName[SnippetDesigner.ConstantStrings.SchemaNameXML] = SnippetDesigner.ConstantStrings.SchemaNameXML;
            exportNameToSchemaName[SnippetDesigner.Resources.DisplayNameXML] = SnippetDesigner.ConstantStrings.SchemaNameXML;

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
