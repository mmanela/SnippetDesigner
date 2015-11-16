using System;
using System.Collections.Generic;

namespace Microsoft.SnippetDesigner
{
    public enum Language
    {
        CPP,
        CSharp,
        VisualBasic,
        XML,
        JavaScript,
        SQL,
        SQLServerDataTools,
        HTML, 
        XAML
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
                case Language.CPP:
                    return Resources.DisplayNameCPP;
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
                case Language.SQLServerDataTools:
                    return Resources.DisplayNameSQLServerDataTools;
                case Language.HTML:
                    return Resources.DisplayNameHTML;
                case Language.XAML:
                    return Resources.DisplayNameXAML;
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
                case Language.CPP:
                    return StringConstants.SchemaNameCPP;
                case Language.CSharp:
                    return StringConstants.SchemaNameCSharp;
                case Language.VisualBasic:
                    return StringConstants.SchemaNameVisualBasic;
                case Language.XML:
                    return StringConstants.SchemaNameXML;
                case Language.JavaScript:
                    return SnippetDesignerPackage.Instance.IsVisualStudio2010
                               ? StringConstants.SchemaNameJavaScript
                               : StringConstants.SchemaNameJavaScriptVS11;
                case Language.SQL:
                    return StringConstants.SchemaNameSQL;
                case Language.SQLServerDataTools:
                    return StringConstants.SchemaNameSQLServerDataTools;
                case Language.HTML:
                    return StringConstants.SchemaNameHTML;
                case Language.XAML:
                    return StringConstants.SchemaNameXAML;
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
            if (!SnippetDesignerPackage.Instance.IsVisualStudio2010)
            {
                snippetSchemaLanguageToDisplay[StringConstants.SchemaNameCPP] = Resources.DisplayNameCPP;
            }
            snippetSchemaLanguageToDisplay[StringConstants.SchemaNameVisualBasic] = Resources.DisplayNameVisualBasic;
            snippetSchemaLanguageToDisplay[StringConstants.SchemaNameCSharp] = Resources.DisplayNameCSharp;
            snippetSchemaLanguageToDisplay[StringConstants.SchemaNameCSharp2] = Resources.DisplayNameCSharp;

            if (!SnippetDesignerPackage.Instance.IsVisualStudio2010 && !SnippetDesignerPackage.Instance.IsVisualStudio2012)
            {
                snippetSchemaLanguageToDisplay[StringConstants.SchemaNameXAML] = Resources.DisplayNameXAML;
            }
            snippetSchemaLanguageToDisplay[StringConstants.SchemaNameXML] = Resources.DisplayNameXML;
            snippetSchemaLanguageToDisplay[StringConstants.SchemaNameJavaScript] = Resources.DisplayNameJavaScript;
            snippetSchemaLanguageToDisplay[StringConstants.SchemaNameJavaScriptVS11] = Resources.DisplayNameJavaScript;
            snippetSchemaLanguageToDisplay[StringConstants.SchemaNameSQL] = Resources.DisplayNameSQL;
            snippetSchemaLanguageToDisplay[StringConstants.SchemaNameSQLServerDataTools] = Resources.DisplayNameSQLServerDataTools;
            snippetSchemaLanguageToDisplay[StringConstants.SchemaNameHTML] = Resources.DisplayNameHTML;


            snippetSchemaLanguageToDisplay[String.Empty] = String.Empty;



            //hash from display names to schema names
            if (!SnippetDesignerPackage.Instance.IsVisualStudio2010)
            {
                displayLanguageToXML[Resources.DisplayNameCPP] = StringConstants.SchemaNameCPP;
            }
            displayLanguageToXML[Resources.DisplayNameVisualBasic] = StringConstants.SchemaNameVisualBasic;
            displayLanguageToXML[Resources.DisplayNameCSharp] = StringConstants.SchemaNameCSharp;

            if (!SnippetDesignerPackage.Instance.IsVisualStudio2010 && !SnippetDesignerPackage.Instance.IsVisualStudio2012)
            {
                displayLanguageToXML[Resources.DisplayNameXAML] = StringConstants.SchemaNameXAML;
            }
            displayLanguageToXML[Resources.DisplayNameXML] = StringConstants.SchemaNameXML;
            displayLanguageToXML[Resources.DisplayNameJavaScript] = SnippetDesignerPackage.Instance.IsVisualStudio2010
                                                                        ? StringConstants.SchemaNameJavaScript
                                                                        : StringConstants.SchemaNameJavaScriptVS11;
            displayLanguageToXML[Resources.DisplayNameSQL] = StringConstants.SchemaNameSQL;
            displayLanguageToXML[Resources.DisplayNameSQLServerDataTools] = StringConstants.SchemaNameSQLServerDataTools;
            displayLanguageToXML[Resources.DisplayNameHTML] = StringConstants.SchemaNameHTML;

            displayLanguageToXML[String.Empty] = String.Empty;
        }
    }
}