using System.Xml;

namespace Microsoft.SnippetLibrary
{
    public class SnippetType
    {
        private XmlElement element;

        private string value;

        public string Value
        {
            get { return value; }
            set
            {
                this.value = value;
                element.InnerText = this.value;
            }
        }


        public SnippetType()
        {
        }

        public SnippetType(XmlElement element)
        {
            BuildTypeElement(element);
        }

        public SnippetType(string stype)
        {
            value = stype;
        }


        public void BuildTypeElement(XmlElement element)
        {
            this.element = element;
            value = Utility.GetTextFromElement(this.element);
        }
    }
}