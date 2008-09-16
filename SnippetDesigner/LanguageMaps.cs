using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.SnippetDesigner
{

    /// <summary>
    /// Provides maps of different forms of the programming language names to eachother
    /// </summary>
    public class LanguageMaps
    {

        public static LanguageMaps languageMap = null;

        //hash that maps what the xml names of the programming langyages are to the dispaly names we use
        private Dictionary<string, string> xmlLanguageToDisplay = new Dictionary<string, string>();
        //hash that maps what the display names of the programming langyages are to the xml names the schema specifies
        private Dictionary<string, string> displayLanguageToXML = new Dictionary<string, string>();

        public Dictionary<string, string> XmlLanguageToDisplay
        {
            get
            {
                return xmlLanguageToDisplay;
            }
        }

        public Dictionary<string, string> DisplayLanguageToXML
        {
            get
            {
                return displayLanguageToXML;
            }
        }


        public static LanguageMaps LanguageMap
        {
            get
            {
                if (languageMap == null)
                {
                    languageMap = new LanguageMaps();
                }
                return languageMap;
            }
        }


        /// <summary>
        /// maps form one lang form to another
        /// </summary>
        public LanguageMaps()
        {
            //hash from schema names to display names
            xmlLanguageToDisplay[ConstantStrings.SchemaNameVisualBasic] = Resources.DisplayNameVisualBasic;
            xmlLanguageToDisplay[ConstantStrings.SchemaNameCSharp] = Resources.DisplayNameCSharp;
            xmlLanguageToDisplay[ConstantStrings.SchemaNameCSharp2] = Resources.DisplayNameCSharp;
            xmlLanguageToDisplay[ConstantStrings.SchemaNameXML] = Resources.DisplayNameXML;
            xmlLanguageToDisplay[String.Empty] = String.Empty;

            //has from display names to schema names
            displayLanguageToXML[Resources.DisplayNameVisualBasic] = ConstantStrings.SchemaNameVisualBasic;
            displayLanguageToXML[Resources.DisplayNameCSharp] = ConstantStrings.SchemaNameCSharp;
            displayLanguageToXML[Resources.DisplayNameXML] = ConstantStrings.SchemaNameXML;
            displayLanguageToXML[String.Empty] = String.Empty;
            
        }
    }
}
