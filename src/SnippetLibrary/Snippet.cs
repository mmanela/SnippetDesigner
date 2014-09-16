using System;
using System.Collections.Generic;
using System.Xml;

namespace Microsoft.SnippetLibrary
{
    public class Snippet
    {
        private XmlNode codeSnippetNode;

        private string title;
        private string shortcut;
        private string description;
        private string helpUrl;
        private string code;
        private string codeLanguageAttribute;
        private string codeKindAttribute;
        private string author;


        private readonly List<SnippetType> snippetTypes = new List<SnippetType>();
        private readonly List<string> imports = new List<string>();
        private readonly List<string> references = new List<string>();
        private readonly List<Literal> literals = new List<Literal>();
        private readonly List<string> keywords = new List<string>();
        private readonly XmlNamespaceManager nsMgr;
        private readonly List<AlternativeShortcut> alternativeShortcuts = new List<AlternativeShortcut>();

        #region Properties

        public XmlNode CodeSnippetNode
        {
            get { return codeSnippetNode; }
        }

        public string Author
        {
            get { return author; }
            set
            {
                author = value;
                Utility.SetTextInDescendantElement((XmlElement) codeSnippetNode.SelectSingleNode("descendant::ns1:Header", nsMgr), "Author", author, nsMgr);
            }
        }

        public string Title
        {
            get { return title; }
            set
            {
                title = value;
                Utility.SetTextInDescendantElement((XmlElement) codeSnippetNode.SelectSingleNode("descendant::ns1:Header", nsMgr), "Title", title, nsMgr);
            }
        }

        public string Shortcut
        {
            get { return shortcut; }
            set
            {
                shortcut = value;
                Utility.SetTextInChildElement((XmlElement)codeSnippetNode.SelectSingleNode("descendant::ns1:Header", nsMgr), "Shortcut", shortcut, nsMgr);
            }
        }

        public string Description
        {
            get { return description; }
            set
            {
                description = value;
                Utility.SetTextInDescendantElement((XmlElement) codeSnippetNode.SelectSingleNode("descendant::ns1:Header", nsMgr), "Description", description, nsMgr);
            }
        }

        public string HelpUrl
        {
            get { return helpUrl; }
            set
            {
                helpUrl = value;
                Utility.SetTextInDescendantElement((XmlElement) codeSnippetNode.SelectSingleNode("descendant::ns1:Header", nsMgr), "HelpUrl", helpUrl, nsMgr);
            }
        }

        public string CodeKindAttribute
        {
            get { return codeKindAttribute; }
            set
            {
                codeKindAttribute = value;
                XmlNode codeNode = codeSnippetNode.SelectSingleNode("descendant::ns1:Snippet//ns1:Code", nsMgr);

                if (codeNode == null)
                {
                    return;
                }
                if (value != null)
                {
                    XmlNode kindAttribute = codeSnippetNode.OwnerDocument.CreateAttribute("Kind");
                    kindAttribute.Value = codeKindAttribute;
                    codeNode.Attributes.SetNamedItem(kindAttribute);
                }
                else
                {
                    XmlNode kindAttribute = codeSnippetNode.OwnerDocument.CreateAttribute("Kind");
                    kindAttribute.Value = codeKindAttribute;
                    codeNode.Attributes.SetNamedItem(kindAttribute);
                    if (codeNode.Attributes.Count > 0 && codeNode.Attributes["Kind"] != null)
                        codeNode.Attributes.Remove(codeNode.Attributes["Kind"]);
                }
            }
        }

        public string CodeLanguageAttribute
        {
            get { return codeLanguageAttribute; }
            set
            {
                codeLanguageAttribute = value;
                XmlNode codeNode = codeSnippetNode.SelectSingleNode("descendant::ns1:Snippet//ns1:Code", nsMgr);

                if (codeNode == null)
                {
                    return;
                }

                XmlNode langAttribute = codeSnippetNode.OwnerDocument.CreateAttribute("Language");
                langAttribute.Value = codeLanguageAttribute;
                codeNode.Attributes.SetNamedItem(langAttribute);
            }
        }

        public string Code
        {
            get { return code; }
            set
            {
                code = value;

                XmlNode codeNode = codeSnippetNode.SelectSingleNode("descendant::ns1:Snippet//ns1:Code", nsMgr);

                if (codeNode == null)
                {
                    codeNode =
                        codeSnippetNode.SelectSingleNode("descendant::ns1:Snippet", nsMgr).AppendChild(codeSnippetNode.OwnerDocument.CreateElement("Code", nsMgr.LookupNamespace("ns1")));
                }
                XmlCDataSection cdataCode = codeNode.OwnerDocument.CreateCDataSection(code);

                if (codeNode.ChildNodes.Count > 0)
                {
                    for (int i = 0; i < codeNode.ChildNodes.Count; i++)
                        codeNode.RemoveChild(codeNode.ChildNodes[i]);
                }

                codeNode.AppendChild(cdataCode);
            }
        }

        public IEnumerable<SnippetType> SnippetTypes
        {
            get { return snippetTypes; }
            set
            {
                ClearSnippetTypes();
                foreach (SnippetType types in value)
                {
                    AddSnippetType(types.Value);
                }
            }
        }

        public IEnumerable<Literal> Literals
        {
            get { return literals; }

            set
            {
                ClearLiterals();
                foreach (Literal lit in value)
                {
                    AddLiteral(lit.ID, lit.ToolTip, lit.DefaultValue, lit.Function, lit.Editable, lit.Object, lit.Type);
                }
            }
        }

        public IEnumerable<AlternativeShortcut> AlternativeShortcuts
        {
            get { return alternativeShortcuts; }

            set
            {
                ClearAlternativeShortcuts();
                foreach (AlternativeShortcut alternativeShortcut in value)
                {
                    AddAlternativeShortcut(alternativeShortcut.Name, alternativeShortcut.Value);
                }
            }
        }

        public IEnumerable<String> Keywords
        {
            get { return keywords; }
            set
            {
                ClearKeywords();
                foreach (string keyword in value)
                {
                    AddKeyword(keyword.Trim());
                }
            }
        }

        public IEnumerable<String> Imports
        {
            get { return imports; }

            set
            {
                ClearImports();
                foreach (string import in value)
                {
                    AddImport(import);
                }
            }
        }

        public IEnumerable<String> References
        {
            get { return references; }

            set
            {
                ClearReferences();
                foreach (string reference in value)
                {
                    AddReference(reference);
                }
            }
        }

        #endregion Properties

        /// <summary>
        /// Initializes a new instance of the <see cref="Snippet"/> class.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="nsMgr">The ns MGR.</param>
        public Snippet(XmlNode node, XmlNamespaceManager nsMgr)
        {
            this.nsMgr = nsMgr;
            codeSnippetNode = node;
            LoadData(codeSnippetNode);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Snippet"/> class.
        /// </summary>
        public Snippet()
        {
        }

        /// <summary>
        /// Adds the snippet node.
        /// </summary>
        /// <param name="snippetNode">The snippet node.</param>
        public void AddSnippetNode(XmlNode snippetNode)
        {
            codeSnippetNode = snippetNode;
        }

        /// <summary>
        /// Clears out all snippet type elements and in memory representation
        /// </summary>
        public void ClearSnippetTypes()
        {
            // Remove all existing snippettype elements
            XmlNode snippetTypesNode = codeSnippetNode.SelectSingleNode("descendant::ns1:Header//ns1:SnippetTypes", nsMgr);

            if (snippetTypesNode != null)
            {
                snippetTypesNode.RemoveAll();
            }

            // Clear out the in-memory snippet types
            snippetTypes.Clear();
        }

        /// <summary>
        /// Clears out all snippet keyword elements and in memory representation
        /// </summary>
        public void ClearKeywords()
        {
            // Remove all existing snippettype elements
            XmlNode snippetKeywordsNode = codeSnippetNode.SelectSingleNode("descendant::ns1:Header//ns1:Keywords", nsMgr);

            if (snippetKeywordsNode != null)
            {
                snippetKeywordsNode.RemoveAll();
            }

            // Clear out the in-memory snippet types
            keywords.Clear();
        }

        /// <summary>
        /// Clears out all literal elements and in memory representation
        /// </summary>
        public void ClearLiterals()
        {
            // Remove all existing literal elements
            XmlNode literalsNode = codeSnippetNode.SelectSingleNode("descendant::ns1:Snippet//ns1:Declarations", nsMgr);

            if (literalsNode != null)
                literalsNode.RemoveAll();

            // Clear out the in-memory literals
            literals.Clear();
        }

        public void ClearImports()
        {
            // Remove all existing literal elements
            XmlNode importsNode = codeSnippetNode.SelectSingleNode("descendant::ns1:Snippet//ns1:Imports", nsMgr);

            if (importsNode != null)
                importsNode.RemoveAll();

            // Clear out the in-memory literals
            imports.Clear();
        }

        public void ClearReferences()
        {
            // Remove all existing literal elements
            XmlNode referencesNode = codeSnippetNode.SelectSingleNode("descendant::ns1:Snippet//ns1:References", nsMgr);

            if (referencesNode != null)
                referencesNode.RemoveAll();

            // Clear out the in-memory literals
            references.Clear();
        }

        private void ClearAlternativeShortcuts()
        {
            XmlNode alternativeShortcutsNode = codeSnippetNode.SelectSingleNode("descendant::ns1:Header//ns1:AlternativeShortcuts", nsMgr);

            if (alternativeShortcutsNode != null)
                alternativeShortcutsNode.RemoveAll();

            alternativeShortcuts.Clear();
        }

        private void AddAlternativeShortcut(string name, string value)
        {
            if (string.IsNullOrEmpty(name))
                return;
            XmlDocument doc = codeSnippetNode.OwnerDocument;
            XmlNode headerNode = codeSnippetNode.SelectSingleNode("descendant::ns1:Header", nsMgr);
            XmlElement shortcutNode = doc.CreateElement("Shortcut", nsMgr.LookupNamespace("ns1"));
            shortcutNode.InnerText = name;
            if(!string.IsNullOrEmpty(value))
                shortcutNode.SetAttribute("Value", value);

            XmlNode alternativeShortcutsElement = headerNode.SelectSingleNode("descendant::ns1:AlternativeShortcuts", nsMgr);
            if (alternativeShortcutsElement == null)
            {
                alternativeShortcutsElement = doc.CreateElement("AlternativeShortcuts", nsMgr.LookupNamespace("ns1"));
                alternativeShortcutsElement = headerNode.AppendChild(alternativeShortcutsElement);
            }
            alternativeShortcutsElement.AppendChild(shortcutNode);
            alternativeShortcuts.Add(new AlternativeShortcut(shortcutNode, nsMgr));

        }

        public void AddSnippetType(string snippetTypeString)
        {
            XmlElement parent = (XmlElement) codeSnippetNode.SelectSingleNode("descendant::ns1:Header//ns1:SnippetTypes", nsMgr);
            if (parent == null)
            {
                parent = Utility.CreateElement((XmlElement) codeSnippetNode.SelectSingleNode("descendant::ns1:Header", nsMgr), "SnippetTypes", string.Empty, nsMgr);
            }

            XmlElement element = Utility.CreateElement(parent, "SnippetType", snippetTypeString, nsMgr);
            snippetTypes.Add(new SnippetType(element));
        }

        public void AddKeyword(string keywordString)

        {
            XmlDocument doc = codeSnippetNode.OwnerDocument;
            XmlNode headerNode = codeSnippetNode.SelectSingleNode("descendant::ns1:Header", nsMgr);
            XmlElement keywordElement = doc.CreateElement("Keyword", nsMgr.LookupNamespace("ns1"));
            keywordElement.InnerText = keywordString;

            XmlNode keywordsElement = headerNode.SelectSingleNode("descendant::ns1:Keywords", nsMgr);
            if (keywordsElement == null)
            {
                keywordsElement = doc.CreateElement("Keywords", nsMgr.LookupNamespace("ns1"));
                keywordsElement = headerNode.PrependChild(keywordsElement);
            }
            keywordsElement.AppendChild(keywordElement);
            keywords.Add(keywordString);
        }

        public void AddImport(string importString)
        {
            XmlDocument doc = codeSnippetNode.OwnerDocument;

            XmlElement importElement = doc.CreateElement("Import", nsMgr.LookupNamespace("ns1"));
            XmlElement namespaceElement = doc.CreateElement("Namespace", nsMgr.LookupNamespace("ns1"));
            namespaceElement.InnerText = importString;
            importElement.PrependChild(namespaceElement);

            XmlNode importsElement = codeSnippetNode.SelectSingleNode("descendant::ns1:Snippet//ns1:Imports", nsMgr);
            if (importsElement == null)
            {
                XmlNode snippetNode = codeSnippetNode.SelectSingleNode("descendant::ns1:Snippet", nsMgr);
                importsElement = doc.CreateElement("Imports", nsMgr.LookupNamespace("ns1"));
                importsElement = snippetNode.PrependChild(importsElement);
            }
            importsElement.AppendChild(importElement);
            imports.Add(importString);
        }

        public void AddReference(string referenceString)
        {
            XmlDocument doc = codeSnippetNode.OwnerDocument;

            XmlElement referenceElement = doc.CreateElement("Reference", nsMgr.LookupNamespace("ns1"));
            XmlElement assemblyElement = doc.CreateElement("Assembly", nsMgr.LookupNamespace("ns1"));
            assemblyElement.InnerText = referenceString;
            referenceElement.PrependChild(assemblyElement);

            XmlNode referencesElement = codeSnippetNode.SelectSingleNode("descendant::ns1:Snippet//ns1:References", nsMgr);
            if (referencesElement == null)
            {
                XmlNode snippetNode = codeSnippetNode.SelectSingleNode("descendant::ns1:Snippet", nsMgr);
                referencesElement = doc.CreateElement("References", nsMgr.LookupNamespace("ns1"));
                referencesElement = snippetNode.PrependChild(referencesElement);
            }
            referencesElement.AppendChild(referenceElement);
            references.Add(referenceString);
        }

        public void AddLiteral(string id, string toolTip, string defaultVal, string function, bool editable, bool isObject, string type)
        {
            XmlDocument doc = codeSnippetNode.OwnerDocument;

            // Create a new Literal element
            XmlElement literalElement;
            if (isObject == false)
                literalElement = doc.CreateElement("Literal", nsMgr.LookupNamespace("ns1"));
            else
                literalElement = doc.CreateElement("Object", nsMgr.LookupNamespace("ns1"));
            literalElement.SetAttribute("Editable", editable.ToString().ToLower());

            // Create the literal element's children
            XmlElement idElement = doc.CreateElement("ID", nsMgr.LookupNamespace("ns1"));
            idElement.InnerText = id;
            XmlElement toolTipElement = doc.CreateElement("ToolTip", nsMgr.LookupNamespace("ns1"));
            toolTipElement.InnerText = toolTip;
            XmlElement defaultElement = doc.CreateElement("Default", nsMgr.LookupNamespace("ns1"));
            defaultElement.InnerText = defaultVal;
            XmlElement functionElement = doc.CreateElement("Function", nsMgr.LookupNamespace("ns1"));
            functionElement.InnerText = function;
            XmlElement typeElement = null;
            if (isObject)
            {
                typeElement = doc.CreateElement("Type", nsMgr.LookupNamespace("ns1"));
                typeElement.InnerText = type;
            }


            // Find or create the declarations element
            XmlNode declarationsNode = codeSnippetNode.SelectSingleNode("descendant::ns1:Snippet//ns1:Declarations", nsMgr);
            if (declarationsNode == null)
            {
                XmlNode snippetNode = codeSnippetNode.SelectSingleNode("descendant::ns1:Snippet", nsMgr);
                declarationsNode = doc.CreateElement("Declarations", nsMgr.LookupNamespace("ns1"));
                XmlNode codeNode = snippetNode.SelectSingleNode("descendant::ns1:Code", nsMgr);
                if (codeNode != null)
                    declarationsNode = snippetNode.InsertBefore(declarationsNode, codeNode);
                else
                    declarationsNode = snippetNode.AppendChild(declarationsNode);
            }

            // Hook them all up together accordingly
            XmlElement literalNode = (XmlElement) declarationsNode.AppendChild(literalElement);
            literalNode.AppendChild(idElement);
            literalNode.AppendChild(toolTipElement);
            literalNode.AppendChild(defaultElement);
            literalNode.AppendChild(functionElement);
            if (isObject)
            {
                literalNode.AppendChild(typeElement);
            }

            // Add the literal element to the actual xml doc
            declarationsNode.AppendChild(literalNode);

            literals.Add(new Literal(literalNode, nsMgr, isObject));
        }

        // Read in the xml document and extract relevant data
        private void LoadData(XmlNode node)
        {
            extractHeader(node.SelectSingleNode("descendant::ns1:Header", nsMgr));
            extractSnippet(node.SelectSingleNode("descendant::ns1:Snippet", nsMgr));
        }

        #region Extract Methods

        // Process the data in the Header element
        private void extractHeader(XmlNode node)
        {
            if (node == null)
            {
                title = string.Empty;
                shortcut = string.Empty;
                description = string.Empty;
                author = string.Empty;
                return;
            }

            title = Utility.GetTextFromElement((XmlElement) node.SelectSingleNode("descendant::ns1:Title", nsMgr));
            shortcut = Utility.GetTextFromElement((XmlElement) node.SelectSingleNode("descendant::ns1:Shortcut", nsMgr));
            description = Utility.GetTextFromElement((XmlElement) node.SelectSingleNode("descendant::ns1:Description", nsMgr));
            helpUrl = Utility.GetTextFromElement((XmlElement) node.SelectSingleNode("descendant::ns1:HelpUrl", nsMgr));
            author = Utility.GetTextFromElement((XmlElement) node.SelectSingleNode("descendant::ns1:Author", nsMgr));
            extractSnippetTypes(node.SelectSingleNode("descendant::ns1:SnippetTypes", nsMgr));
            extractKeywords(node.SelectSingleNode("descendant::ns1:Keywords", nsMgr));
            extractAlternativeShortcuts(node.SelectSingleNode("descendant::ns1:AlternativeShortcuts", nsMgr));
        }

        // Process the data in the SnippetTypes elements
        private void extractSnippetTypes(XmlNode node)
        {
            if (node == null)
                return;

            foreach (XmlElement snippetTypeElement in node.SelectNodes("descendant::ns1:SnippetType", nsMgr))
            {
                snippetTypes.Add(new SnippetType(snippetTypeElement));
            }
        }

        // Process the data in the Keywords elements
        private void extractKeywords(XmlNode node)
        {
            if (node == null)
                return;

            foreach (XmlElement keywordElement in node.SelectNodes("descendant::ns1:Keyword", nsMgr))
            {
                keywords.Add(keywordElement.InnerText);
            }
        }

        private void extractAlternativeShortcuts(XmlNode node)
        {
            if (node == null)
                return;

            foreach (XmlElement alternativeShortcutElement in node.SelectNodes("descendant::ns1:Shortcut", nsMgr))
            {
                alternativeShortcuts.Add(new AlternativeShortcut(alternativeShortcutElement,nsMgr));
            }
        }

        // Process the data in the Snippet elements
        private void extractSnippet(XmlNode node)
        {
            if (node == null)
            {
                code = string.Empty;
                return;
            }
            XmlNode codeNode = node.SelectSingleNode("descendant::ns1:Code", nsMgr);
            code = Utility.GetTextFromElement((XmlElement) codeNode);
            if (codeNode != null && codeNode.Attributes.Count > 0)
            {
                if (codeNode.Attributes["Language"] != null)
                    CodeLanguageAttribute = codeNode.Attributes["Language"].Value;
                if (codeNode.Attributes["Kind"] != null)
                    CodeKindAttribute = codeNode.Attributes["Kind"].Value;
            }
            extractDeclarations(node.SelectSingleNode("descendant::ns1:Declarations", nsMgr));
            extractImports(node.SelectSingleNode("descendant::ns1:Imports", nsMgr));
            extractReferences(node.SelectSingleNode("descendant::ns1:References", nsMgr));
        }

        private void extractImports(XmlNode node)
        {
            if (node == null)
                return;

            XmlNodeList xnl = node.SelectNodes("descendant::ns1:Import//ns1:Namespace", nsMgr);

            if (xnl == null)
                return;

            // Add each literal node to the snippet
            foreach (XmlElement importElement in xnl)
            {
                imports.Add(importElement.InnerText);
            }
        }

        private void extractReferences(XmlNode node)
        {
            if (node == null)
                return;

            XmlNodeList xnl = node.SelectNodes("descendant::ns1:Reference//ns1:Assembly", nsMgr);

            if (xnl == null)
                return;

            // Add each literal node to the snippet
            foreach (XmlElement referenceElement in xnl)
            {
                references.Add(referenceElement.InnerText);
            }
        }

        // Process the data in the Declarations elements
        private void extractDeclarations(XmlNode node)
        {
            if (node == null)
                return;

            XmlNodeList xnl = node.SelectNodes("descendant::ns1:Literal", nsMgr);

            if (xnl != null)
            {
                // Add each literal node to the snippet
                foreach (XmlElement literalElement in xnl)
                {
                    literals.Add(new Literal(literalElement, nsMgr, false));
                }
            }
            XmlNodeList xno = node.SelectNodes("descendant::ns1:Object", nsMgr);

            if (xno != null)
            {
                // Add each literal node to the snippet
                foreach (XmlElement objectElement in xno)
                {
                    literals.Add(new Literal(objectElement, nsMgr, true));
                }
            }
        }

        #endregion
    }
}