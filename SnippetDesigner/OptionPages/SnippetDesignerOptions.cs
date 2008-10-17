using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel;
using System.Drawing.Design;

namespace Microsoft.SnippetDesigner.OptionPages
{
    /// <summary>
    /// General options page
    /// </summary>
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid("2863817E-CD3A-45b9-A0D3-7A8547563CFB")]
    public class SnippetDesignerOptions : DialogPage
    {
        private Language language;
        bool hideCSharp;
        bool hideVisualBasic;
        bool hideXML;
        string indexedSnippetDirectoriesString;
        List<string> indexedSnippetDirectories;


        /// <summary>
        /// Initializes a new instance of the <see cref="SnippetDesignerOptions"/> class.
        /// </summary>
        /// <include file="doc\DialogPage.uex" path="docs/doc[@for=&quot;DialogPage.DialogPage&quot;]"/>
        /// <devdoc>
        /// Constructs the Dialog Page.
        /// </devdoc>
        public SnippetDesignerOptions()
        {
            // Initialize indexedSnippetDirectories to all snippet directories
            // if the user already modified this it will be overwritten
            indexedSnippetDirectories = new List<string>(SnippetDirectories.Instance.AllSnippetDirectories);
        }

        /// <summary>
        /// Joins the specified seperator with the list.
        /// </summary>
        /// <param name="seperator">The seperator.</param>
        /// <param name="list">The list.</param>
        /// <returns></returns>
        private string Join(string seperator, List<string> list)
        {
            string result = string.Empty;
            foreach (string st in list)
            {
                result += st + seperator;
            }

            if (result.Length > 0)
            {
                result = result.Remove(result.Length - 1);
            }

            return result;
        }

        /// <summary>
        /// Gets or sets the indexed snippet directories string.
        /// This is the string which gets stored in the user's settings
        /// This is a work around since it seem VS user setting wont serialize List(string) 
        /// If it can, then this isnt needed.
        /// </summary>
        /// <value>The indexed snippet directories string.</value>
        [Browsable(false)]
        public string IndexedSnippetDirectoriesString
        {
            get
            {
                return indexedSnippetDirectories!=null ? Join(";", indexedSnippetDirectories) : string.Empty;
            }
            set
            {
                if (value != null && !value.Equals(indexedSnippetDirectoriesString, StringComparison.OrdinalIgnoreCase))
                {
                    indexedSnippetDirectoriesString = value;
                    indexedSnippetDirectories = new List<string>(indexedSnippetDirectoriesString.Split(';'));
                }
                
            }
        }

        [Category("Search")]
        [Description("The directories where you want snippets to be index.  The indexer will index all sub-durectories from each of these directories.")]
        [EditorAttribute(typeof(MyStringCollectionEditor), typeof(UITypeEditor))]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<string> IndexedSnippetDirectories
        {
            get 
            {
                return indexedSnippetDirectories;
            }
            set
            {
                indexedSnippetDirectories = value;
            }
        }

        [Category("Editor")]
        [Description("The default language the Snippet Editor starts in.")]
        public Language DefaultLanguage
        {
            get 
            { 
                return language; 
            }
            set 
            { 
                language = value;
            }
        }

        [Category("Search")]
        [DisplayName("Hide C# Snippets")]
        [Description("Should search results for C# snippets be hidden?")]
        public bool HideCSharp
        {
            get
            {
                return hideCSharp;
            }
            set
            {
                hideCSharp = value;
            }
        }

        [Category("Search")]
        [DisplayName("Hide VB Snippets")]
        [Description("Should search results for Visual Basic snippets be hidden?")]
        public bool HideVisualBasic
        {
            get
            {
                return hideVisualBasic;
            }
            set
            {
                hideVisualBasic = value;
            }
        }

        [Category("Search")]
        [DisplayName("Hide XML Snippets")]
        [Description("Should search results for XML snippets be hidden?")]
        public bool HideXML
        {
            get
            {
                return hideXML;
            }
            set
            {
                hideXML = value;
            }
        }



        //TIP 1: If you want to get access this option page from a VS Package use this snippet on the VsPackage class:
        //SnippetDesignerOptions optionPage = this.GetDialogPage(typeof(SnippetDesignerOptions)) as SnippetDesignerOptions;

        //TIP 2: If you want to get access this option page from VS Automation copy this snippet:
        //DTE dte = GetService(typeof(DTE)) as DTE;
        //EnvDTE.Properties props = dte.get_Properties("Snippet Designer", "Snippet Editor");	
    }
}

