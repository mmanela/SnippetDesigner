// Copyright (C) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using Microsoft.RegistryTools;


namespace Microsoft.SnippetLibrary
{
    public class SnippetFile
    {
        string snippetFilePath = null;
        Stream snippetFileStream = null;
        XmlDocument doc;
        XmlSchemaSet schemas;
        List<Snippet> snippets = new List<Snippet>();
        XmlNamespaceManager nsMgr;
        
        //TODO: Get this from registry
        public static readonly string SnippetSchemaPathBegin = RegistryLocations.GetVSInstallDir() + @"..\..\Xml\Schemas\";
        public static readonly string SnippetSchemaPathEnd = @"\snippetformat.xsd";
        public static readonly string SnippetNS = @"http://schemas.microsoft.com/VisualStudio/2005/CodeSnippet";
        public bool showErrorMessages = false;
        public bool hasXmlErrors = false;

        #region Properties

        /// <summary>
        /// Gets the snippet doc.
        /// </summary>
        /// <value>The snippet doc.</value>
        public XmlDocument SnippetDoc
        {
            get
            {
                return doc;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has XML errors.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has XML errors; otherwise, <c>false</c>.
        /// </value>
        public bool HasXmlErrors
        {
            get
            {
                return hasXmlErrors;
            }
        }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        /// <value>The name of the file.</value>
        public string FileName
        {
            get { return snippetFilePath; }
        }

        /// <summary>
        /// Gets the snippets.
        /// </summary>
        /// <value>The snippets.</value>
        public List<Snippet> Snippets
        {
            get { return snippets; }
        }

        #endregion Properties

        public SnippetFile()
        {


        }

        public void CreateBlankSnippet()
        {
            LoadSchema();
            InitializeNewDocument();
        }

        public SnippetFile(string fileName)
        {
            snippetFilePath = fileName;
            LoadSchema();
            LoadData();

        }


        public SnippetFile(Stream fileStream, bool showErrors)
        {
            snippetFileStream = fileStream;
            showErrorMessages = showErrors;
            LoadSchema();
            LoadData();

        }
        public SnippetFile(string fileName, bool showErrors)
        {
            snippetFilePath = fileName;
            showErrorMessages = showErrors;
            LoadSchema();
            LoadData();

        }

        private void LoadSchema()
        {
            schemas = new XmlSchemaSet();
            string snippetSchema = SnippetSchemaPathBegin + CultureInfo.CurrentCulture.LCID.ToString() + SnippetSchemaPathEnd;
            if (!File.Exists(snippetSchema))
            {
                snippetSchema = SnippetSchemaPathBegin + "1033" + SnippetSchemaPathEnd;
            }
            schemas.Add(SnippetNS, snippetSchema);

        }


        public void CreateFromText(string text)
        {
            LoadSchema();
            doc = new XmlDocument();
            doc.Schemas = schemas;
            doc.LoadXml(text);
            LoadFromDoc();
        }


        public void CreateSnippetFileFromNode(XmlNode snippetNode)
        {
            doc = new XmlDocument();
            doc.Schemas = schemas;
            doc.LoadXml("<?xml version=\"1.0\" encoding=\"utf-8\" ?>" +
                         "<CodeSnippets xmlns=\"" + SnippetNS + "\">" +
                         snippetNode.OuterXml
                         +"</CodeSnippets>");

            snippets = new List<Snippet>();
            nsMgr = new XmlNamespaceManager(doc.NameTable);
            nsMgr.AddNamespace("ns1", SnippetNS);

            XmlNode node = doc.SelectSingleNode("//ns1:CodeSnippets//ns1:CodeSnippet", nsMgr);
            snippets.Add(new Snippet(node, nsMgr));
        }



        public void Save()
        {
            doc.Save(snippetFilePath);
        }

        public void SaveAs(string fileName)
        {

            doc.Save(fileName);
            snippetFilePath = fileName;
        }

        private void SchemaValidationEventHandler(object sender, ValidationEventArgs e)
        {
            switch (e.Severity)
            {
                case XmlSeverityType.Error:
                    {
                        hasXmlErrors = true;
                        if (showErrorMessages)
                        {
                            System.Windows.Forms.MessageBox.Show(String.Format("\nError: {0}", e.Message));
                        }
                        break;
                    }
                case XmlSeverityType.Warning:
                    {
                        hasXmlErrors = true;
                        if (showErrorMessages)
                        {
                            System.Windows.Forms.MessageBox.Show(String.Format("\nWarning: {0}", e.Message));
                        }
                        break;
                    }
            }
        }



        /// <summary>
        /// Appends the new snippet.
        /// </summary>
        /// <returns>Index position of added snippet</returns>
        public int AppendNewSnippet()
        {
            XmlElement newSnippet = doc.CreateElement("CodeSnippet", nsMgr.LookupNamespace("ns1"));
            newSnippet.SetAttribute("Format", "1.0.0");
            newSnippet.AppendChild(doc.CreateElement("Header", nsMgr.LookupNamespace("ns1")));
            newSnippet.AppendChild(doc.CreateElement("Snippet", nsMgr.LookupNamespace("ns1")));

            XmlNode codeSnippetsNode = doc.SelectSingleNode("//ns1:CodeSnippets", nsMgr);
            XmlNode newNode = codeSnippetsNode.AppendChild(newSnippet);
            snippets.Add(new Snippet(newNode, nsMgr));
            return snippets.Count - 1;
        }





        /// <summary>
        /// Initializes the new document.
        /// </summary>
        private void InitializeNewDocument()
        {
            doc = new XmlDocument();
            doc.Schemas = schemas;
            doc.LoadXml("<?xml version=\"1.0\" encoding=\"utf-8\" ?>" +
                         "<CodeSnippets xmlns=\"" + SnippetNS + "\">" +
                         "<CodeSnippet Format=\"1.0.0\"><Header></Header>" +
                         "<Snippet>" + //<Code></Code>
                         "</Snippet></CodeSnippet></CodeSnippets>");

            snippets = new List<Snippet>();
            nsMgr = new XmlNamespaceManager(doc.NameTable);
            nsMgr.AddNamespace("ns1", SnippetNS);

            XmlNode node = doc.SelectSingleNode("//ns1:CodeSnippets//ns1:CodeSnippet", nsMgr);
            snippets.Add(new Snippet(node, nsMgr));
        }

        private void LoadFromDoc()
        {
            nsMgr = new XmlNamespaceManager(doc.NameTable);
            nsMgr.AddNamespace("ns1", "http://schemas.microsoft.com/VisualStudio/2005/CodeSnippet");


            // If the document doesn't already have a declaration, add it
            if (doc.FirstChild.NodeType != XmlNodeType.XmlDeclaration)
            {
                XmlDeclaration decl = doc.CreateXmlDeclaration("1.0", "utf-8", string.Empty);
                doc.InsertBefore(decl, doc.DocumentElement);
            }

            // Handle the current ambiguity as to whether
            // the root node "CodeSnippets" is optional or not
            if (doc.DocumentElement.Name == "CodeSnippet")
            {
                // Since the root element was CodeSnippet, we should
                // proceed with the assumption that this file only
                // defines one snippet.
                snippets.Add(new Snippet(doc.DocumentElement, nsMgr));
                return;
            }
            else
            {
                foreach (XmlNode node in doc.DocumentElement.SelectNodes("//ns1:CodeSnippet", nsMgr))
                {
                    snippets.Add(new Snippet(node, nsMgr));
                }
            }
            doc.Schemas = schemas;
            ValidationEventHandler schemaValidator = new ValidationEventHandler(SchemaValidationEventHandler);
            doc.Validate(schemaValidator);

        }

        // Read in the xml document and extract relevant data
        private void LoadData()
        {
            doc = new XmlDocument();

            try
            {
                if(!String.IsNullOrEmpty(snippetFilePath))
                { //if file name exists use it otherweise use stream if it exists
                    doc.Load(snippetFilePath);
                }
                else if (snippetFileStream != null)
                {
                    doc.Load(snippetFileStream);
                }
                else
                {
                    throw new IOException("No data to read from");
                }
            }
            catch (IOException ioException)
            {
                //if file doesnt exist or cant be read throw the ioexcpetion
                throw ioException;
            }
            catch (System.Xml.XmlException)
            {
                //check if this file is empty if so then initialize a new file
                if (!String.IsNullOrEmpty(snippetFilePath) && File.ReadAllText(snippetFilePath).Trim() == String.Empty)
                {
                    InitializeNewDocument();
                }
                else
                {
                    //the file is not empty
                    //we shouldnt be loading this
                    throw new IOException("Not a valid XML Document");
                }
                return;

            }
            //load from the stored XMLdocument
            LoadFromDoc();

        }
    }
}