// Copyright (C) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO.IsolatedStorage;
using System.IO;
using System.Globalization;
using Microsoft.SnippetLibrary;
using Microsoft.SnippetDesigner.SnippetExplorer;
using System.Windows.Forms;
using Microsoft.RegistryTools;
using System.Text.RegularExpressions;

namespace Microsoft.SnippetDesigner.ContentTypes
{
    /// <summary>
    /// Represents the index file of snippets
    /// </summary>
    public class SnippetIndex
    {
        private string snippetIndexFilePath;
        private readonly string snippetIndexFileName = "SnippetIndex.xml";
        private List<SnippetIndexItem> snippetItemCollection;

        //snippet directories
        public static List<string> allSnippetDirectories = new List<string>();

        public SnippetIndex()
        {
            string snippetIndexFileDir = Application.CommonAppDataPath;
            //make sure this directory exists if now make it
            if (!Directory.Exists(snippetIndexFileDir))
            {
                Directory.CreateDirectory(snippetIndexFileDir);
            }
            snippetIndexFilePath = Path.Combine(snippetIndexFileDir, snippetIndexFileName);

            allSnippetDirectories = SnippetDirectories.Instance.AllSnippetDirectories;

        }


        /// <summary>
        /// Collection of all the snippet items
        /// </summary>
        public List<SnippetIndexItem> SnippetItems
        {
            get
            {
                return snippetItemCollection;
            }

        }

        /// <summary>
        /// Performs a search on the  snippets on the computer and return an index item collection containing them
        /// </summary>
        /// <param name="searchString">string to search by, if null or empty get all</param>
        /// <param name="languagesToGet">The languages to get.</param>
        /// <param name="maxResultCount">The max result count.</param>
        /// <returns>collection of found snippets</returns>
        public List<SnippetIndexItem> PerformSnippetSearch(string searchString, List<string> languagesToGet, int maxResultCount)
        {


            List<SnippetIndexItem> foundSnippets = new List<SnippetIndexItem>();
            int found = 0;
            for (int i = 0; i < SnippetItems.Count && found < maxResultCount; i++)
            {
                SnippetIndexItem item = SnippetItems[i];

                //filter out the languages we dont want to show
                if (!languagesToGet.Contains(item.Language.ToLower()))
                {
                    continue;
                }

                List<string> fieldsToSearch = new List<string>();
                fieldsToSearch.Add(item.Title);
                fieldsToSearch.Add(item.Description);
                fieldsToSearch.Add(item.Keywords);
                fieldsToSearch.Add(item.Code);



                if (String.IsNullOrEmpty(searchString))
                {
                    foundSnippets.Add(item);
                    found++;
                }
                else
                {
                    foreach (string searchField in fieldsToSearch)
                    {
                        if (searchField != null)
                        {
                            bool allMatch = true;
                            foreach (string part in searchString.Split(' '))
                            {
                                string regexString = String.Format(@"\b{0}\b", part);
                                Regex findRegex = new Regex(regexString, RegexOptions.Multiline | RegexOptions.IgnoreCase);

                                if (!findRegex.IsMatch((searchField)))
                                {
                                    allMatch = false;
                                    break;
                                }

                            }
                            if (allMatch)
                            {
                                foundSnippets.Add(item);
                                found++;
                                break;
                            }
                        }
                    }
                }

            }

            return foundSnippets;

        }


        /// <summary>
        /// delete the content associated with this item
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public void DeleteFileFromIndex(string filePath)
        {
            try
            {

                if (!String.IsNullOrEmpty(filePath) && File.Exists(filePath))
                {
                    //delete file on disk
                    File.Delete(filePath);

                    SnippetIndexItem itemToRemove = GetSnippetItem(filePath);
                    if (itemToRemove != null)
                    {
                        snippetItemCollection.Remove(itemToRemove);
                    }

                    //save index file changes to disk
                    SaveIndexFile();

                }
            }
            catch (IOException)
            {
                throw;
            }
        }


        /// <summary>
        /// Create a new index file by reading all snippet files 
        /// and building them in internal memory then writing them to the index file
        /// </summary>
        /// <returns></returns>
        public bool CreateIndexFile()
        {

            //create the index item collection
            snippetItemCollection = new List<SnippetIndexItem>();


            //add snippet files
            AddSnippetsToIndex();
            

            //write the snippetitemcolllection to disk
            return SaveIndexFile();

        }




        /// <summary>
        /// Read the index file from disk into memory
        /// </summary>
        /// <returns></returns>
        public bool ReadIndexFile()
        {
            FileStream stream = null;
            try
            {
                //load the index file into memory
                stream = new FileStream(snippetIndexFilePath, FileMode.Open);
                snippetItemCollection = Load(stream);

                if (snippetItemCollection == null || snippetItemCollection.Count == 0)
                    return false;
                else
                    return true;

            }
            catch (FileNotFoundException)
            {
                return false;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }

        }

        /// <summary>
        /// Get the entry in the local snipept index
        /// </summary>
        /// <param name="filePath">full path to snippet</param>
        /// <returns>item found null if not found</returns>
        public SnippetIndexItem GetSnippetItem(string filePath)
        {
            foreach (SnippetIndexItem item in snippetItemCollection)
            {
                if (String.Compare(item.File, filePath, true) == 0)
                {
                    return item;
                }

            }
            return null;
        }



        /// <summary>
        /// Update a  snippet item in the collection based upon the current filepath
        /// 
        /// then swap the item with the new one
        /// </summary>
        /// <param name="updatedItem"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public bool UpdateSnippetItem(Snippet updatedSnippet, string path)
        {
            SnippetIndexItem currentItem = GetSnippetItem(path);
            if (currentItem != null)
            {
                UpdateIndexItemData(currentItem, updatedSnippet);
                SaveIndexFile();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Reads the snippet object and adds the right data to the index
        /// </summary>
        /// <param name="filePath">the path of the file</param>
        public void CreateIndexItemDataFromSnippet(Snippet currentSnippet, string filePath)
        {
            SnippetIndexItem item = new SnippetIndexItem();
            item.Title = currentSnippet.Title;
            item.Author = currentSnippet.Author;
            item.Code = currentSnippet.Code;
            item.Description = currentSnippet.Description;
            item.File = filePath;
            item.Keywords = String.Join(",", currentSnippet.Keywords.ToArray()); ;
            item.Language = currentSnippet.CodeLanguageAttribute.ToLower();
            item.UsesNum = "0";
            item.DateAdded = DateTime.Today.ToFileTime().ToString();

            snippetItemCollection.Add(item);
        }


        /// <summary>
        /// Write the current SnippetIndexItemCOllection to the index file
        /// </summary>
        /// <returns>true if success</returns>
        private bool SaveIndexFile()
        {
            //write the index to disk
            FileStream stream = null;
            try
            {
                stream = new FileStream(snippetIndexFilePath, FileMode.Create);
                return Save(stream);
            }
            catch (System.IO.IOException)
            {

            }
            finally
            {
                if (stream != null)
                {

                    stream.Close();
                }
            }


            return false;

        }



        /// <summary>
        /// Update a snippet index item with new values
        /// </summary>
        /// <param name="item">item to update</param>
        /// <param name="snippetData">snipept data to update it with</param>
        private void UpdateIndexItemData(SnippetIndexItem item, Snippet snippetData)
        {
            item.Title = snippetData.Title;
            item.Author = snippetData.Author;
            item.Description = snippetData.Description;
            item.Keywords = String.Join(",", snippetData.Keywords.ToArray());
            item.Language = snippetData.CodeLanguageAttribute;
            item.Code = snippetData.Code;

        }

        /// <summary>
        /// Loads the data for this index item from a snippet file
        /// </summary>
        /// <param name="filePath">the path of the file</param>
        private bool AddItemDataFromSnippetFile(string filePath)
        {
            try
            {
                SnippetFile snippetFile = new SnippetFile(filePath);
                foreach (Snippet currentSnippet in snippetFile.Snippets)
                {
                    //add the item to the collection
                    CreateIndexItemDataFromSnippet(currentSnippet, filePath);
                }
            }
            catch (IOException)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        ///  Deserialize or Load this object member values from an XML file
        /// </summary>
        /// <param name="stream">Stream for the file to load</param>
        /// <returns>a List of snippetIndexItems or null if failure</returns>
        private List<SnippetIndexItem> Load(Stream stream)
        {
            if (stream == null)
            {
                return null;
            }
            List<SnippetIndexItem> retval = null;
            try
            {
                XmlSerializer ser = null;
                ser = new XmlSerializer(typeof(List<SnippetIndexItem>));
                retval = (List<SnippetIndexItem>)ser.Deserialize(stream);
            }
            catch (System.IO.FileNotFoundException)
            {

            }
            catch (System.UnauthorizedAccessException)
            {

            }
            catch (System.Xml.XmlException)
            {

            }
            catch (System.InvalidOperationException)
            {

            }

            return retval;
        }


        /// <summary>
        /// Serialize this object as an XML file to disk.
        /// </summary>
        /// <param name="stream">file stream</param>
        /// <returns>Succeed or failure</returns>
        private bool Save(Stream stream)
        {
            if (stream == null)
            {
                return false;
            }
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(List<SnippetIndexItem>));
                ser.Serialize(stream, snippetItemCollection);

            }
            catch (System.IO.PathTooLongException)
            {
                return false;
            }
            catch (System.ArgumentNullException)
            {
                return false;
            }
            catch (System.UnauthorizedAccessException)
            {
                return false;
            }
            catch (NullReferenceException)
            {
                return false;
            }
            return true;

        }

        /// <summary>
        /// Parse snippets on the compuer and add to index
        /// </summary>
        private void AddSnippetsToIndex()
        {
            foreach (string path in allSnippetDirectories)
            {

                if (!Directory.Exists(path))
                {
                    continue;
                }

                string[] snippets = Directory.GetFiles(path, SnippetSearch.AllSnippets, SearchOption.AllDirectories);
                foreach (string snippet in snippets)
                {
                    AddItemDataFromSnippetFile(snippet);

                }

            }

        }
    }
}
