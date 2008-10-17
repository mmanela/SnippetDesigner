// Copyright (C) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Collections;
using System.Collections.ObjectModel;
using System.Text;
using System.IO;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Package;
using Microsoft.SnippetLibrary;
using Microsoft.VisualStudio.Shell;
using System.Drawing.Design;

namespace Microsoft.SnippetDesigner
{
    /// <summary>
    /// Enumeration of the different snippet types
    /// </summary>
    public enum TypeOfSnippet
    {
        Expansion,
        SurroundsWith
    }

    /// <summary>
    /// Enumeration of the kinds of snippets
    /// </summary>
    public enum KindOfSnippet
    {
        MethodBody,
        MethodDecl,
        TypeDecl
    }



    /// <summary>
    /// Create my own version of CollectionEditor that will use strings as its data item
    /// </summary>
    public class MyStringCollectionEditor : CollectionEditor
    {
        public MyStringCollectionEditor(Type type)
            : base(typeof(List<String>))
        {

        }

        /// <summary>
        /// Hardcode that this collectioneditor deals with strings
        /// </summary>
        /// <returns></returns>
        protected override Type CreateCollectionItemType()
        {
            return typeof(String);
        }

        protected override object CreateInstance(Type itemType)
		{
            string newString = String.Empty;
            return newString;
		}

        protected override string GetDisplayText(object value)
        {
            return value.ToString();
        }
    }


    /// <summary>
    /// The properties to display in the properties window
    /// This works through reflection.  The properties window will scan this file and pick up all the properties.
    /// Based upon the attributes you set it will give the properties window the title, category and description for each property.
    /// Also, based upon the return type of the property the property window will display a different form for editing the value
    ///
    /// LocalizableProperties is inherited and one method of it is overriden.  This allows us to set the name displayed
    /// in the drop down menu of the properties menu
    /// </summary>
    public class EditorProperties
    {
       

        private ISnippetEditor snippetEditor;
        private Dictionary<KindOfSnippet, String> kindEnumToString = new Dictionary<KindOfSnippet, string>();
        private Dictionary<String, KindOfSnippet> stringToKindEnum = new Dictionary<String, KindOfSnippet>();

        public EditorProperties(ISnippetEditor snipEditor)
        {
            
            snippetEditor = snipEditor;
            kindEnumToString.Add(KindOfSnippet.MethodBody, ConstantStrings.SnippetTypeMethodBody);
            kindEnumToString.Add(KindOfSnippet.MethodDecl, ConstantStrings.SnippetTypeMethodDeclaration);
            kindEnumToString.Add(KindOfSnippet.TypeDecl, ConstantStrings.SnippetTypeTypeDeclaration);

            stringToKindEnum.Add(ConstantStrings.SnippetTypeMethodBody, KindOfSnippet.MethodBody);
            stringToKindEnum.Add(ConstantStrings.SnippetTypeMethodDeclaration, KindOfSnippet.MethodDecl);
            stringToKindEnum.Add(ConstantStrings.SnippetTypeTypeDeclaration, KindOfSnippet.TypeDecl);

        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [LocalizableCategoryAttribute(SR.PropCategoryFileInfo)]
        [LocalizableDescriptionAttribute(SR.PropDescriptionSnippetPath)]
        [LocalizableDisplayName(SR.PropNameSnippetPath)]
        public string FilePath
        {
            get 
            { 
                return snippetEditor.SnippetFileName; 
            }

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [LocalizableCategoryAttribute(SR.PropCategorySnippData)]
        [LocalizableDescriptionAttribute(SR.PropDescriptionSnippetDescription)]
        [LocalizableDisplayName(SR.PropNameSnippetDescription)]
        public string Description
        {
            get 
            { 
                return snippetEditor.SnippetDescription;
            }
            set
            {
                snippetEditor.SnippetDescription = value;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [LocalizableCategoryAttribute(SR.PropCategorySnippData)]
        [LocalizableDescriptionAttribute(SR.PropDescriptionSnippetAuthor)]
        [LocalizableDisplayName(SR.PropNameSnippetAuthor)]
        public string Author
        {
            get
            {
                return snippetEditor.SnippetAuthor;
            }
            set
            {
                snippetEditor.SnippetAuthor = value;
            }
        }

        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [LocalizableCategoryAttribute(SR.PropCategorySnippData)]
        [LocalizableDescriptionAttribute(SR.PropDescriptionSnippetKeywords)]
        [LocalizableDisplayName(SR.PropNameSnippetKeywords)]
        public string Keywords
        {

             get
            {
                return String.Join(",",snippetEditor.SnippetKeywords.ToArray());
            }

            set
            {
                snippetEditor.SnippetKeywords = new List<string>(value.Split(','));
            }

        }
        


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [LocalizableCategoryAttribute(SR.PropCategorySnippData)]
        [LocalizableDescriptionAttribute(SR.PropDescriptionSnippetHelpUrl)]
        [LocalizableDisplayName(SR.PropNameSnippetHelpUrl)]
        public string HelpUrl
        {
            get
            {
                return snippetEditor.SnippetHelpUrl;
            }
            set
            {
                snippetEditor.SnippetHelpUrl = value;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [LocalizableCategoryAttribute(SR.PropCategorySnippData)]
        [LocalizableDescriptionAttribute(SR.PropDescriptionSnippetShortcut)]
        [LocalizableDisplayName(SR.PropNameSnippetShortcut)]
        public string Shortcut
        {
            get
            {
                return snippetEditor.SnippetShortcut;
            }
            set
            {
                snippetEditor.SnippetShortcut = value;
            }
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [LocalizableCategoryAttribute(SR.PropCategorySnippData)]
        [LocalizableDescriptionAttribute(SR.PropDescriptionSnippetImports)]
        [LocalizableDisplayName(SR.PropNameSnippetImports)]
        [EditorAttribute(typeof(MyStringCollectionEditor), typeof(UITypeEditor))]
        public List<string> Imports
        {
            get
            {
                return snippetEditor.SnippetImports;
            }
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [LocalizableCategoryAttribute(SR.PropCategorySnippData)]
        [LocalizableDescriptionAttribute(SR.PropDescriptionSnippetReferences)]
        [LocalizableDisplayName(SR.PropNameSnippetReferences)]
        [EditorAttribute(typeof(MyStringCollectionEditor), typeof(UITypeEditor))]
        public List<string> References
        {
            get
            {
                return snippetEditor.SnippetReferences;
            }
        }

        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [LocalizableCategoryAttribute(SR.PropCategorySnippData)]
        [LocalizableDescriptionAttribute(SR.PropDescriptionSnippetType)]
        [LocalizableDisplayName(SR.PropNameSnippetType)]
        public TypeOfSnippet Type
        {
            get
            {
                //get the type of the snippet but make sure type is correct
                bool containsSurroundWith = false;
                foreach (SnippetType snipType in snippetEditor.SnippetTypes)
                {
                    string surroundWithName = TypeOfSnippet.SurroundsWith.ToString().ToLower();
                    string typeValue = snipType.Value.ToLower();
                    if (typeValue == surroundWithName)
                    {
                        containsSurroundWith = true;
                        break;
                    }
                }
                if (containsSurroundWith && //does it have the surround with tag
                    snippetEditor.SnippetCode.Contains(ConstantStrings.SymbolSelected)//does it have correct selected symbol
                    )
                {
                    return TypeOfSnippet.SurroundsWith;
                }
                else
                {
                    return TypeOfSnippet.Expansion;
                }
            }
            set
            {
                List<SnippetType> types = new List<SnippetType>();
                types.Add(new SnippetType(value.ToString()));
                snippetEditor.SnippetTypes = types;
            }
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [LocalizableCategoryAttribute(SR.PropCategorySnippData)]
        [LocalizableDescriptionAttribute(SR.PropDescriptionSnippetKind)]
        [LocalizableDisplayName(SR.PropNameSnippetKind)]
        public KindOfSnippet Kind
        {
            get
            {
                KindOfSnippet retValue;
                if(!String.IsNullOrEmpty(snippetEditor.SnippetKind) && stringToKindEnum.ContainsKey(snippetEditor.SnippetKind))
                {
                    retValue = stringToKindEnum[snippetEditor.SnippetKind];
                }
                else
                {
                    retValue = KindOfSnippet.MethodBody;
                }
                return retValue;
            }
            set
            {
                if (kindEnumToString.ContainsKey(value))
                {
                    snippetEditor.SnippetKind = kindEnumToString[value];
                }
                else
                {
                    snippetEditor.SnippetKind = String.Empty;
                }
            }
        }
    }
}
