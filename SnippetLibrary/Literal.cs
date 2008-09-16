// Copyright (C) Microsoft Corporation. All rights reserved.

using System;
using System.Xml;
using System.Runtime.InteropServices;
namespace Microsoft.SnippetLibrary
{
    [ComVisible(true)]
	public class Literal
	{
        XmlElement _element;

		protected string _id;
        protected string _toolTip;
        protected string _function;
        protected string _defaultValue;
        protected string _type;
        protected bool _editable;
        protected bool _object;

        
        #region Properties
        public bool Object
        {
            get { return _object; }
            set { _object = value; }
        }
        public string ID
        {
            get 
            { 
                return _id; 
            }
            set 
            {
                _id = value;
                Utility.SetTextInElement(_element, "ID", _id,null);
            }
        }

        public string ToolTip
        {
            get 
            { 
                return _toolTip; 
            }
            set 
            {
                _toolTip = value;
                Utility.SetTextInElement(_element, "ToolTip", _toolTip, null);
            }
        }

        public string Function
        {
            get 
            { 
                return _function; 
            }
            set
            {
                _function = value;
                Utility.SetTextInElement(_element, "Function", _function, null);
            }
        }
        public string Type
        {
            get
            {
                return _type;
            }
            set
            {
                _type = value;
                Utility.SetTextInElement(_element, "Type", _type, null);
            }
        }


        public string DefaultValue
        {
            get 
            { 
                return _defaultValue; 
            }
            set
            {
                _defaultValue = value;
                Utility.SetTextInElement(_element, "Default", _defaultValue, null);
            }
        }

        public bool Editable
        {
            get 
            { 
                return _editable; 
            }
            set 
            { 
                _editable = value; 
                _element.SetAttribute("Editable", _editable.ToString());                
            }
        }

        #endregion

        public Literal(XmlElement element, XmlNamespaceManager nsMgr, bool Object)
        {
            SetLiteral(element, nsMgr, Object);
        }

        public Literal(string id, string tip, string defaults, string function, bool isObj, bool isEdit, string type)
        {
            SetLiteral(id, tip, defaults, function, isObj, isEdit, type);
        }

        public Literal()
        {

        }



        public void SetLiteral(XmlElement element, XmlNamespaceManager nsMgr, bool Object)
        {
            _element = element;
            _object = Object;
            _id = Utility.GetTextFromElement((XmlElement)_element.SelectSingleNode("descendant::ns1:ID", nsMgr));
            _toolTip = Utility.GetTextFromElement((XmlElement)_element.SelectSingleNode("descendant::ns1:ToolTip", nsMgr));
            _function = Utility.GetTextFromElement((XmlElement)_element.SelectSingleNode("descendant::ns1:Function", nsMgr));
            _defaultValue = Utility.GetTextFromElement((XmlElement)_element.SelectSingleNode("descendant::ns1:Default", nsMgr));
            _type = Utility.GetTextFromElement((XmlElement)_element.SelectSingleNode("descendant::ns1:Type", nsMgr));
            string boolStr = _element.GetAttribute("Editable");
            if (boolStr != string.Empty)
                _editable = bool.Parse(boolStr);
            else
                _editable = true;
        }

        public void SetLiteral(string id, string tip, string defaults, string function, bool isObj, bool isEdit, string type)
        {
            _object = isObj;
            _id = id;
            _toolTip = tip;
            _function = function;
            _defaultValue = defaults;
            _editable = isEdit;
            _type = type;
        }
    }
}
