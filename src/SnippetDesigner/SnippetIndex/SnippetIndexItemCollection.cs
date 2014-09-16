using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.SnippetDesigner;

namespace Microsoft.ShareAndCollaborate.ContentTypes
{
    [XmlRoot]
    public class SnippetIndexItemCollection
    {
        private List<SnippetIndexItem> snippetItemCollection = new List<SnippetIndexItem>();

        [XmlElement("SnippetIndexItems")]
        public List<SnippetIndexItem> SnippetIndexItems
        {
            get { return snippetItemCollection; }
        }

        public SnippetIndexItemCollection()
        {
        }

        public SnippetIndexItemCollection(SnippetIndexItem[] items)
        {
            snippetItemCollection = new List<SnippetIndexItem>(items);
        }

        public void Clear()
        {
            snippetItemCollection.Clear();
        }

        public void Add(SnippetIndexItem item)
        {
            snippetItemCollection.Add(item);
        }
    }
}