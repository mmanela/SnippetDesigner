using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.SnippetDesigner;
namespace Microsoft.ShareAndCollaborate.ContentTypes
{
    /// <summary>
    /// A collection of snippet index items
    /// </summary>
    [XmlRoot()]
    public class SnippetIndexItemCollection
    {
        private List<SnippetIndexItem> snippetItemCollection = new List<SnippetIndexItem>();


        /// <summary>
        /// Regular array of index items to be added to the index file
        /// </summary>
        [XmlElement("SnippetIndexItems")]
        public List<SnippetIndexItem> SnippetIndexItems
        {
            get
            {
                return snippetItemCollection;
            }
        }


        public SnippetIndexItemCollection()
        {


        }
        public SnippetIndexItemCollection(SnippetIndexItem[] items)
        {

            snippetItemCollection = new List<SnippetIndexItem>(items);
        }




        /// <summary>
        /// clear the collection of snippets
        /// </summary>
        public void Clear()
        {
            snippetItemCollection.Clear();
        }


        /// <summary>
        /// add snippet item
        /// </summary>
        /// <param name="item"></param>
        public void Add(SnippetIndexItem item)
        {
            snippetItemCollection.Add(item);
        }

    }
}
