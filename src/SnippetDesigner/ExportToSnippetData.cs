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
        private readonly Dictionary<string, string> exportNameToSchemaName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);


        /// <summary>
        /// Gets the code.
        /// </summary>
        /// <value>The code.</value>
        internal string Code { get; private set; }

        /// <summary>
        /// Gets the language.
        /// </summary>
        /// <value>The language.</value>
        internal string Language { get; private set; }

        internal ExportToSnippetData(string code, string language)
        {
            exportNameToSchemaName[StringConstants.ExportNameCPP] = StringConstants.SchemaNameCPP;
            exportNameToSchemaName[StringConstants.ExportNameCSharp] = StringConstants.SchemaNameCSharp;
            exportNameToSchemaName[StringConstants.ExportNameVisualBasic] = StringConstants.SchemaNameVisualBasic;
            exportNameToSchemaName[StringConstants.ExportNameXML] = StringConstants.SchemaNameXML;
            exportNameToSchemaName[StringConstants.ExportNameJavaScript] = StringConstants.SchemaNameJavaScript;
            exportNameToSchemaName[StringConstants.ExportNameJavaScript2] = StringConstants.SchemaNameJavaScript;
            exportNameToSchemaName[StringConstants.ExportNameHTML] = StringConstants.SchemaNameHTML;
            exportNameToSchemaName[StringConstants.ExportNameSQL] = StringConstants.SchemaNameSQL;
            exportNameToSchemaName[StringConstants.ExportNameSQL2] = StringConstants.SchemaNameSQL;

            Code = code;
            if (exportNameToSchemaName.ContainsKey(language))
            {
                Language = exportNameToSchemaName[language];
            }
            else
            {
                //pass empty string if we dont know the language passed in
                Language = String.Empty;
            }
        }
    }
}