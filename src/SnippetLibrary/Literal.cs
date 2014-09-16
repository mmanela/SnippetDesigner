using System.Runtime.InteropServices;
using System.Xml;

namespace Microsoft.SnippetLibrary
{
    [ComVisible(true)]
    public class Literal
    {
        private XmlElement element;
        private string id;
        private string toolTip;
        private string function;
        private string defaultValue;
        private string type;
        private bool editable;

        #region Properties

        public bool Object { get; set; }

        public string ID
        {
            get { return id; }
            set
            {
                id = value;
                Utility.SetTextInDescendantElement(element, "ID", id, null);
            }
        }

        public string ToolTip
        {
            get { return toolTip; }
            set
            {
                toolTip = value;
                Utility.SetTextInDescendantElement(element, "ToolTip", toolTip, null);
            }
        }

        public string Function
        {
            get { return function; }
            set
            {
                function = value;
                Utility.SetTextInDescendantElement(element, "Function", function, null);
            }
        }

        public string Type
        {
            get { return type; }
            set
            {
                type = value;
                Utility.SetTextInDescendantElement(element, "Type", type, null);
            }
        }


        public string DefaultValue
        {
            get { return defaultValue; }
            set
            {
                defaultValue = value;
                Utility.SetTextInDescendantElement(element, "Default", defaultValue, null);
            }
        }

        public bool Editable
        {
            get { return editable; }
            set
            {
                editable = value;
                element.SetAttribute("Editable", editable.ToString());
            }
        }

        #endregion

        public Literal(XmlElement element, XmlNamespaceManager nsMgr, bool Object)
        {
            BuildLiteral(element, nsMgr, Object);
        }

        public Literal(string id, string tip, string defaults, string function, bool isObj, bool isEdit, string type)
        {
            BuildLiteral(id, tip, defaults, function, isObj, isEdit, type);
        }

        public void BuildLiteral(XmlElement element, XmlNamespaceManager nsMgr, bool @object)
        {
            this.element = element;
            Object = @object;
            id = Utility.GetTextFromElement((XmlElement) this.element.SelectSingleNode("descendant::ns1:ID", nsMgr));
            toolTip = Utility.GetTextFromElement((XmlElement) this.element.SelectSingleNode("descendant::ns1:ToolTip", nsMgr));
            function = Utility.GetTextFromElement((XmlElement) this.element.SelectSingleNode("descendant::ns1:Function", nsMgr));
            defaultValue = Utility.GetTextFromElement((XmlElement) this.element.SelectSingleNode("descendant::ns1:Default", nsMgr));
            type = Utility.GetTextFromElement((XmlElement) this.element.SelectSingleNode("descendant::ns1:Type", nsMgr));
            string boolStr = this.element.GetAttribute("Editable");
            if (boolStr != string.Empty)
                editable = bool.Parse(boolStr);
            else
                editable = true;
        }

        public void BuildLiteral(string id, string tip, string defaults, string function, bool isObj, bool isEdit, string type)
        {
            Object = isObj;
            this.id = id;
            toolTip = tip;
            this.function = function;
            defaultValue = defaults;
            editable = isEdit;
            this.type = type;
        }
    }
}