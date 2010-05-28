using System;
using System.Collections.Generic;

namespace Microsoft.SnippetDesigner
{
    public enum Language
    {
        CSharp,
        VisualBasic,
        XML,
        JavaScript,
        SQL,
        HTML
    }

    /// <summary>
    /// Provides maps of different forms of the programming language names to eachother
    /// </summary>
    public class LanguageMaps
    {
        public static LanguageMaps LanguageMap = new LanguageMaps();

        //hash that maps what the snippet schema names of the programming languages are to the display names we use
        private readonly Dictionary<string, string> snippetSchemaLanguageToDisplay = new Dictionary<string, string>();

        //hash that maps what the display names of the programming languages are to the xml names the snippet schema specifies
        private readonly Dictionary<string, string> displayLanguageToXML = new Dictionary<string, string>();

        public Dictionary<string, string> SnippetSchemaLanguageToDisplay
        {
            get { return snippetSchemaLanguageToDisplay; }
        }

        public Dictionary<string, string> DisplayLanguageToXML
        {
            get { return displayLanguageToXML; }
        }

        /// <summary>
        /// Toes the display form.
        /// </summary>
        /// <param name="lang">The lang.</param>
        /// <returns></returns>
        public String ToDisplayForm(Language lang)
        {
            switch (lang)
            {
                case Language.CSharp:
                    return Resources.DisplayNameCSharp;
                case Language.VisualBasic:
                    return Resources.DisplayNameVisualBasic;
                case Language.XML:
                    return Resources.DisplayNameXML;
                case Language.JavaScript:
                    return Resources.DisplayNameJavaScript;
                case Language.SQL:
                    return Resources.DisplayNameSQL;
                case Language.HTML:
                    return Resources.DisplayNameHTML;
                default:
                    return String.Empty;
            }
        }

        /// <summary>
        /// Toes the schema form.
        /// </summary>
        /// <param name="lang">The lang.</param>
        /// <returns></returns>
        public String ToSchemaForm(Language lang)
        {
            switch (lang)
            {
                case Language.CSharp:
                    return StringConstants.SchemaNameCSharp;
                case Language.VisualBasic:
                    return StringConstants.SchemaNameVisualBasic;
                case Language.XML:
                    return StringConstants.SchemaNameXML;                
                case Language.JavaScript:
                    return StringConstants.SchemaNameJavaScript;                
                case Language.SQL:
                    return StringConstants.SchemaNameSQL;                
                case Language.HTML:
                    return StringConstants.SchemaNameHTML;
                default:
                    return String.Empty;
            }
        }

        /// <summary>
        /// maps form one lang form to another
        /// </summary>
        public LanguageMaps()
        {
            //hash from schema names to display names
            snippetSchemaLanguageToDisplay[StringConstants.SchemaNameVisualBasic] = Resources.DisplayNameVisualBasic;
            snippetSchemaLanguageToDisplay[StringConstants.SchemaNameCSharp] = Resources.DisplayNameCSharp;
            snippetSchemaLanguageToDisplay[StringConstants.SchemaNameCSharp2] = Resources.DisplayNameCSharp;
            snippetSchemaLanguageToDisplay[StringConstants.SchemaNameXML] = Resources.DisplayNameXML;
            snippetSchemaLanguageToDisplay[StringConstants.SchemaNameJavaScript] = Resources.DisplayNameJavaScript;
            snippetSchemaLanguageToDisplay[StringConstants.SchemaNameSQL] = Resources.DisplayNameSQL;
            snippetSchemaLanguageToDisplay[StringConstants.SchemaNameHTML] = Resources.DisplayNameHTML;
            snippetSchemaLanguageToDisplay[String.Empty] = String.Empty;

            //has from display names to schema names
            displayLanguageToXML[Resources.DisplayNameVisualBasic] = StringConstants.SchemaNameVisualBasic;
            displayLanguageToXML[Resources.DisplayNameCSharp] = StringConstants.SchemaNameCSharp;
            displayLanguageToXML[Resources.DisplayNameXML] = StringConstants.SchemaNameXML;
            displayLanguageToXML[Resources.DisplayNameJavaScript] = StringConstants.SchemaNameJavaScript;
            displayLanguageToXML[Resources.DisplayNameSQL] = StringConstants.SchemaNameSQL;
            displayLanguageToXML[Resources.DisplayNameHTML] = StringConstants.SchemaNameHTML;
            displayLanguageToXML[String.Empty] = String.Empty;
        }
    }
}