using System;
using System.Collections.Generic;

namespace Microsoft.SnippetDesigner
{
    /// <summary>
    /// Represents the data that is exported from a code file to the snippet codeWindowHost
    /// </summary>
    public class ExportToSnippetData
    {
        //member variables
        private readonly string snippetCode;
        private readonly string snippetLanguage;
        private readonly Dictionary<string, string> exportNameToSchemaName = new Dictionary<string, string>();


        /// <summary>
        /// Gets the code.
        /// </summary>
        /// <value>The code.</value>
        internal string Code
        {
            get { return snippetCode; }
        }

        /// <summary>
        /// Gets the language.
        /// </summary>
        /// <value>The language.</value>
        internal string Language
        {
            get { return snippetLanguage; }
        }

        internal ExportToSnippetData(string code, string language)
        {
            exportNameToSchemaName[StringConstants.ExportNameCSharp] = StringConstants.SchemaNameCSharp;
            exportNameToSchemaName[StringConstants.ExportNameVisualBasic] = StringConstants.SchemaNameVisualBasic;
            exportNameToSchemaName[StringConstants.ExportNameXML] = StringConstants.SchemaNameXML;
            exportNameToSchemaName[StringConstants.ExportNameJavaScript] = StringConstants.SchemaNameJavaScript;
            exportNameToSchemaName[StringConstants.ExportNameHTML] = StringConstants.SchemaNameHTML;
            exportNameToSchemaName[StringConstants.ExportNameSQL] = StringConstants.SchemaNameSQL;


            //exportNameToSchemaName[StringConstants.SchemaNameCSharp] = StringConstants.SchemaNameCSharp;
           // exportNameToSchemaName[Resources.DisplayNameCSharp] = StringConstants.SchemaNameCSharp;


            //exportNameToSchemaName[StringConstants.SchemaNameVisualBasic] = StringConstants.SchemaNameVisualBasic;
           // exportNameToSchemaName[Resources.DisplayNameVisualBasic] = StringConstants.SchemaNameVisualBasic;


            //exportNameToSchemaName[StringConstants.SchemaNameXML] = StringConstants.SchemaNameXML;
           // exportNameToSchemaName[Resources.DisplayNameXML] = StringConstants.SchemaNameXML;

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