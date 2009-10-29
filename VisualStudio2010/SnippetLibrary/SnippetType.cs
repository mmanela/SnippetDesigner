// Copyright (C) Microsoft Corporation. All rights reserved.

using System.Xml;


namespace Microsoft.SnippetLibrary
{
	public class SnippetType
	{
        XmlElement _element;

		protected string _value;
        
        #region Properties
    
        public string Value
        {
            get 
            { 
                return _value; 
            }
            set 
            {
                _value = value;
                _element.InnerText = _value;
            }
        }

        #endregion

        public SnippetType()
        {
        }

        public SnippetType(XmlElement element)
        {
            SetTypeElement(element);
        }

        public SnippetType(string stype)
        {
            _value = stype;
        }



        public void SetTypeElement(XmlElement element)
        {
            _element = element;
            _value = Utility.GetTextFromElement(_element);
        }
    }
}
