using System;
using System.Collections.Generic;

namespace Microsoft.SnippetDesigner
{
    public enum Language
    {
        CSharp,
        VisualBasic,
        XML
    }

    /// <summary>
    /// Provides maps of different forms of the programming language names to eachother
    /// </summary>
    public class LanguageMaps
    {
        public static LanguageMaps LanguageMap = new LanguageMaps();

        //hash that maps what the scnippet schema names of the programming languages are to the dispaly names we use
        private readonly Dictionary<string, string> snippetSchemaLanguageToDisplay = new Dictionary<string, string>();
        //hash that maps what the display names of the programming languages are to the xml names the snippet schema specifies
        private readonly Dictionary<string, string> displayLanguageToXML = new Dictionary<string, string>();

        private readonly Dictionary<Language, Guid> languageGuids = new Dictionary<Language, Guid>();

        public Dictionary<Language, Guid> LanguageGuids
        {
            get { return languageGuids; }
        }

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
            snippetSchemaLanguageToDisplay[String.Empty] = String.Empty;

            //has from display names to schema names
            displayLanguageToXML[Resources.DisplayNameVisualBasic] = StringConstants.SchemaNameVisualBasic;
            displayLanguageToXML[Resources.DisplayNameCSharp] = StringConstants.SchemaNameCSharp;
            displayLanguageToXML[Resources.DisplayNameXML] = StringConstants.SchemaNameXML;
            displayLanguageToXML[String.Empty] = String.Empty;

            languageGuids[Language.CSharp] = GuidList.csLangSvc;
            languageGuids[Language.VisualBasic] = GuidList.vbLangSvc;
            languageGuids[Language.XML] = GuidList.xmlLangSvc;
        }
    }
}