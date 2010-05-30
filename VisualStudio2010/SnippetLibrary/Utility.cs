using System;
using System.Xml;

namespace Microsoft.SnippetLibrary
{
    /// <summary>
    /// Summary description for Util.
    /// </summary>
    public class Utility
    {
        private Utility()
        {
        }

        /// <summary>
        /// Returns the InnerText value from a node 
        /// or string.Empty if the node is null.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static string GetTextFromElement(XmlElement element)
        {
            if (element == null)
                return string.Empty;
            else
                return element.InnerText;
        }

        /// <summary>
        /// Sets the inner text in a node.  If the node doesn't
        /// exist, it creates a new one and adds the text to it.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="name">The name.</param>
        /// <param name="text">The text.</param>
        /// <param name="nsMgr">The ns MGR.</param>
        /// <returns></returns>
        public static XmlNode SetTextInDescendantElement(XmlElement element, string name, string text, XmlNamespaceManager nsMgr)
        {
            return SetTextInElement(element,name,text,nsMgr,false);
        }

        public static XmlNode SetTextInChildElement(XmlElement element, string name, string text, XmlNamespaceManager nsMgr)
        {
            return SetTextInElement(element, name, text, nsMgr, true);
        }


        private static XmlNode SetTextInElement(XmlElement element, string name, string text, XmlNamespaceManager nsMgr, bool isChild)
        {
            if (element == null)
                throw new Exception("Passed in a null node, which should never happen.");

            var selector = "descendant";
            if (isChild)
                selector = "child";

            XmlElement newElement = (XmlElement)element.SelectSingleNode(selector + "::ns1:" + name, nsMgr);

            if (newElement == null)
            {
                newElement = (XmlElement)element.AppendChild(element.OwnerDocument.CreateElement(name, nsMgr.LookupNamespace("ns1")));
            }

            newElement.InnerText = text;
            return element.AppendChild(newElement);
        }

        /// <summary>
        /// Creates the element.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="name">The name.</param>
        /// <param name="innerText">The inner text.</param>
        /// <param name="nsMgr">The ns MGR.</param>
        /// <returns></returns>
        public static XmlElement CreateElement(XmlElement parent, string name, string innerText, XmlNamespaceManager nsMgr)
        {
            if (parent == null)
                throw new Exception("Passed in a null node, which should never happen.");

            XmlElement element = parent.OwnerDocument.CreateElement(name, nsMgr.LookupNamespace("ns1"));
            XmlElement newElement = (XmlElement) parent.AppendChild(element);
            newElement.InnerText = innerText;

            return (XmlElement) parent.AppendChild(newElement);
        }
    }
}