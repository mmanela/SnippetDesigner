using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Microsoft.SnippetDesigner.SnippetExplorer;
using Microsoft.SnippetLibrary;

namespace Microsoft.SnippetDesigner
{
    /// <summary>
    /// Represents the index file of snippets
    /// </summary>
    public class SnippetIndex : INotifyPropertyChanged
    {
        private readonly string snippetIndexFilePath;

        // Maps SnippetFilePath|SnippetTitle to SnippetIndexItem
        private readonly Dictionary<String, SnippetIndexItem> indexedSnippets;
        private bool isIndexLoading;
        private bool isIndexUpdating;
        private readonly ILogger logger;

        private readonly List<Tuple<Func<SnippetIndexItem, string>, int>> fieldRankings = new List<Tuple<Func<SnippetIndexItem, string>, int>>
                                    {
                                        Tuple.Create<Func<SnippetIndexItem, string>, int>(snippet => snippet.Title, 10),
                                        Tuple.Create<Func<SnippetIndexItem, string>, int>(snippet => snippet.Code, 5),
                                        Tuple.Create<Func<SnippetIndexItem, string>, int>(snippet => snippet.Description, 3),
                                        Tuple.Create<Func<SnippetIndexItem, string>, int>(snippet => snippet.Keywords, 2),
                                        Tuple.Create<Func<SnippetIndexItem, string>, int>(snippet => Path.GetFileNameWithoutExtension(snippet.File), 2)
                                    };

        /// <summary>
        /// Gets or sets a value indicating whether this instance is index updating.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is index updating; otherwise, <c>false</c>.
        /// </value>
        public bool IsIndexUpdating
        {
            get { return isIndexUpdating; }
            set
            {
                if (isIndexUpdating != value)
                {
                    isIndexUpdating = value;
                    OnPropertyChanged("IsIndexUpdating");
                }
            }
        }


        /// <summary>
        /// Gets or sets a value indicating whether this instance is index loading.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is index loading; otherwise, <c>false</c>.
        /// </value>
        public bool IsIndexLoading
        {
            get { return isIndexLoading; }
            set
            {
                if (isIndexLoading != value)
                {
                    isIndexLoading = value;
                    OnPropertyChanged("IsIndexLoading");
                }
            }
        }

        public SnippetIndex()
        {
            snippetIndexFilePath = SnippetDesignerPackage.Instance.Settings.SnippetIndexLocation;
            indexedSnippets = new Dictionary<string, SnippetIndexItem>();
            logger = SnippetDesignerPackage.Instance.Logger;
        }

        /// <summary>
        /// Gets the dictionary key.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="title">The title.</param>
        /// <returns></returns>
        private string GetDictionaryKey(string filePath, string title)
        {
            return filePath.ToUpperInvariant().Trim() + "|" + title.ToUpperInvariant().Trim();
        }


        private static Regex CreateRegex(string pattern, string arg)
        {
            return new Regex(string.Format(pattern, Regex.Escape(arg)), RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Performs a search on the  snippets on the computer and return an index item collection containing them
        /// </summary>
        /// <param name="searchString">string to search by, if null or empty get all</param>
        /// <param name="languagesToGet">The languages to get.</param>
        /// <param name="maxResultCount">The max result count.</param>
        /// <returns>collection of found snippets</returns>
        public IEnumerable<SnippetIndexItem> PerformSnippetSearch(string searchString, List<string> languagesToGet, int maxResultCount)
        {
            var filterSnippets = indexedSnippets.Where(x => languagesToGet.Any(lang => lang.Equals(x.Value.Language,StringComparison.OrdinalIgnoreCase)));
            if (String.IsNullOrEmpty(searchString))
                return filterSnippets.Select(x => x.Value).Take(maxResultCount);

            var matchRankings = new List<Tuple<Regex, double>>
                                    {
                                        Tuple.Create(CreateRegex(@"\b{0}\b", searchString), 1.0),
                                        Tuple.Create(CreateRegex(@"{0}", searchString), 0.1)
                                    };


            var search = from snippet in filterSnippets
                         from fieldRanking in fieldRankings
                         from matchRanking in matchRankings
                         where matchRanking.Item1.IsMatch(fieldRanking.Item1(snippet.Value))
                         let rank = fieldRanking.Item2*matchRanking.Item2
                         group rank by snippet.Value
                         into matchResults
                         orderby matchResults.Sum(x => x) descending 
                         select matchResults.Key;

            

            return search.Take(maxResultCount);
        }


        /// <summary>
        /// delete the content associated with this item
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="title">The title.</param>
        public void DeleteSnippetFile(string filePath, string title)
        {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException("filePath", "filePath must not be null");
            if (string.IsNullOrEmpty(title)) throw new ArgumentNullException("title", "title must not be null");

            try
            {
                lock (indexedSnippets)
                {
                    if (File.Exists(filePath))
                    {
                        //delete file on disk
                        File.Delete(filePath);
                    }

                    // Remove file from index
                    indexedSnippets.Remove(GetDictionaryKey(filePath, title));

                    //save index file changes to disk
                    SaveIndexFile();
                }
            }
            catch (IOException e)
            {
                logger.Log("Unable to delete snippet file", "SnippetIndex", e);
            }
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
            item.Delimiter = snippetData.CodeDelimiterAttribute;
        }

        /// <summary>
        /// Reads the snippet object and adds the right data to the index
        /// </summary>
        /// <param name="filePath">the path of the file</param>
        public void CreateIndexItemDataFromSnippet(Snippet currentSnippet, string filePath)
        {
            SnippetIndexItem item = new SnippetIndexItem();
            UpdateIndexItemData(item, currentSnippet);
            item.File = filePath;

            lock (indexedSnippets)
            {
                indexedSnippets[GetDictionaryKey(filePath, item.Title)] = item;
            }
        }

        /// <summary>
        /// Loads the data for this index item from a snippet file
        /// </summary>
        /// <param name="filePath">the path of the file</param>
        private bool AddOrUpdateSnippetsToIndexFromSnippetFile(string filePath)
        {
            try
            {
                SnippetFile snippetFile = new SnippetFile(filePath);
                foreach (Snippet currentSnippet in snippetFile.Snippets)
                {
                    SnippetIndexItem existingItem = null;
                    indexedSnippets.TryGetValue(GetDictionaryKey(filePath, currentSnippet.Title), out existingItem);
                    if (existingItem == null)
                    {
                        //add the item to the collection
                        CreateIndexItemDataFromSnippet(currentSnippet, filePath);
                    }
                    else
                    {
                        UpdateIndexItemData(existingItem, currentSnippet);
                    }
                }
            }
            catch (IOException e)
            {
                logger.Log("Unable to open snippet file at path: " + filePath, "SnippetIndex", e);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Rebuilds the index of the snippet.
        /// </summary>
        public void RebuildSnippetIndex()
        {
            lock (indexedSnippets)
            {
                indexedSnippets.Clear();
            }
            CreateOrUpdateIndexFile();
        }

        /// <summary>
        /// Create a new index file by reading all snippet files 
        /// and building them in internal memory then writing them to the index file
        /// </summary>
        /// <returns></returns>
        public bool CreateOrUpdateIndexFile()
        {
            try
            {
                IsIndexUpdating = true;
                foreach (string path in SnippetDesignerPackage.Instance.Settings.AllSnippetDirectories)
                {
                    if (!Directory.Exists(path))
                    {
                        continue;
                    }

                    string[] snippetFilePaths = Directory.GetFiles(path, SnippetSearch.AllSnippets, SearchOption.AllDirectories);
                    foreach (string snippetPath in snippetFilePaths)
                    {
                        AddOrUpdateSnippetsToIndexFromSnippetFile(snippetPath);
                    }
                }
                IsIndexUpdating = false;

                //write the snippetitemcolllection to disk
                return SaveIndexFile();
            }
            catch (Exception e)
            {
                logger.Log("Unable to read snippet index directories", "SnippetIndex", e);
            }

            return false;
        }


        /// <summary>
        /// Read the index file from disk into memory
        /// </summary>
        /// <returns></returns>
        public bool ReadIndexFile()
        {
            IsIndexLoading = true;
            FileStream stream = null;
            try
            {
                //load the index file into memory
                stream = new FileStream(snippetIndexFilePath, FileMode.Open);
                List<SnippetIndexItem> items = Load(stream);
                if (items == null || items.Count == 0)
                {
                    return false;
                }
                else
                {
                    foreach (SnippetIndexItem item in items)
                    {
                        lock (indexedSnippets)
                        {
                            if (File.Exists(item.File))
                            {
                                var key = GetDictionaryKey(item.File, item.Title);
                                if (!indexedSnippets.ContainsKey(key))
                                    indexedSnippets.Add(key, item);
                            }
                        }
                    }
                    return true;
                }
            }
            catch (Exception e)
            {
                logger.Log("Unable to open snippet index file at path: " + snippetIndexFilePath, "SnippetIndex", e);
                return false;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }

                IsIndexLoading = false;
            }
        }


        /// <summary>
        /// Update a  snippet item in the collection based upon the current filepath
        /// then swap the item with the new one
        /// </summary>
        /// <param name="updatedSnippet">The updated snippet.</param>
        /// <returns></returns>
        public bool UpdateSnippetFile(SnippetFile updatedSnippetFile)
        {
            // Find keys to remove
            List<string> keysToRemove = new List<string>();
            // Keys we found and updated
            List<string> foundKeys = new List<string>();

            // These have title changes to we need to create a new key for them
            List<Snippet> snippetsToAdd = new List<Snippet>();

            // Update snippets that have not changed titles
            foreach (Snippet snippet in updatedSnippetFile.Snippets)
            {
                SnippetIndexItem item = null;
                string key = GetDictionaryKey(updatedSnippetFile.FileName, snippet.Title);
                indexedSnippets.TryGetValue(key, out item);
                if (item != null)
                {
                    UpdateIndexItemData(item, snippet);
                    foundKeys.Add(key);
                }
                else
                {
                    snippetsToAdd.Add(snippet);
                }
            }


            if (snippetsToAdd.Count > 0)
            {
                // Figure out which keys are no longer valid
                foreach (string key in indexedSnippets.Keys)
                {
                    if (key.Contains(updatedSnippetFile.FileName.ToUpperInvariant()) &&
                        !foundKeys.Contains(key))
                    {
                        keysToRemove.Add(key);
                    }
                }

                // Since this file only has one snippet we know the one to update 
                // so we don't need to re-add it
                if (updatedSnippetFile.Snippets.Count == 1 && keysToRemove.Count == 1)
                {
                    SnippetIndexItem item = null;
                    indexedSnippets.TryGetValue(keysToRemove[0], out item);
                    if (item != null)
                    {
                        UpdateIndexItemData(item, updatedSnippetFile.Snippets[0]);
                    }
                }
                else
                {
                    // Remove those keys
                    foreach (string key in keysToRemove)
                    {
                        lock (indexedSnippets)
                        {
                            indexedSnippets.Remove(key);
                        }
                    }

                    // Add update snippet items
                    foreach (Snippet snippet in snippetsToAdd)
                    {
                        CreateIndexItemDataFromSnippet(snippet, updatedSnippetFile.FileName);
                    }
                }
            }

            return SaveIndexFile();
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
                var dirPath = Path.GetDirectoryName(snippetIndexFilePath);
                Directory.CreateDirectory(dirPath);
                stream = new FileStream(snippetIndexFilePath, FileMode.Create);
                return Save(stream);
            }
            catch (Exception e)
            {
                logger.Log("Unable to write to snippet index file at path: " + snippetIndexFilePath, "SnippetIndex", e);
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

            XmlSerializer ser = null;
            ser = new XmlSerializer(typeof (List<SnippetIndexItem>));
            retval = (List<SnippetIndexItem>) ser.Deserialize(stream);


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

            XmlSerializer ser = new XmlSerializer(typeof (List<SnippetIndexItem>));
            List<SnippetIndexItem> items = new List<SnippetIndexItem>(indexedSnippets.Values);
            ser.Serialize(stream, items);

            return true;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Fire the property changed event for the given property name
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
        private void OnPropertyChanged(string propertyName)
        {
            VerifyProperty(propertyName);
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Used in debug mode only.  Checks to make sure the property name string
        /// is actually a property on the object.
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
        [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object,System.Object)"), Conditional("DEBUG")]
        private void VerifyProperty(string propertyName)
        {
            bool propertyExists = TypeDescriptor.GetProperties(this).Find(propertyName, false) != null;
            if (!propertyExists)
            {
                Debug.Fail(String.Format("The property {0} could not be found in {1}", propertyName, GetType().FullName));
            }
        }
    }
}