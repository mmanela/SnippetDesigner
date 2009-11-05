using System;
using System.ComponentModel;

namespace Microsoft.SnippetDesigner
{
    /// <summary>
    /// A type descirption provider which will be registered with the TypeDescriptor.  This provider will be called
    /// whenever the properties are accessed.
    /// </summary>
    public class FilteredPropertiesTypeDescriptorProvider : TypeDescriptionProvider
    {
        private TypeDescriptionProvider baseProvider;

        public FilteredPropertiesTypeDescriptorProvider(Type type)
        {
            baseProvider = TypeDescriptor.GetProvider(type);
        }


        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
        {
            return new FilteredPropertiesTypeDescriptor(
                this, baseProvider.GetTypeDescriptor(objectType, instance), objectType);
        }

        /// <summary>
        /// Our custom type provider will return this type descriptor which will filter the properties
        /// </summary>
        private class FilteredPropertiesTypeDescriptor : CustomTypeDescriptor
        {
            private Type objectType;

            public FilteredPropertiesTypeDescriptor(FilteredPropertiesTypeDescriptorProvider provider, ICustomTypeDescriptor descriptor, Type objType)
                : base(descriptor)
            {
                if (provider == null) throw new ArgumentNullException("provider");
                if (descriptor == null) throw new ArgumentNullException("descriptor");
                if (objType == null) throw new ArgumentNullException("objectType");
                objectType = objType;
            }

            public override PropertyDescriptorCollection GetProperties()
            {
                return GetProperties(null);
            }


            public override string GetClassName()
            {
                return SnippetDesignerPackage.Instance.ActiveSnippetTitle;
            }

            public override string GetComponentName()
            {
                return Resources.SnippetFormTitlesLabelText;
            }


            public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
            {
                //create the property collection
                PropertyDescriptorCollection props = new PropertyDescriptorCollection(null);
                string currentLanguage = SnippetDesignerPackage.Instance.ActiveSnippetLanguage;
                foreach (PropertyDescriptor prop in base.GetProperties(attributes))
                {
                    props.Add(prop);
                }

                // Return the computed properties
                return props;
            }
        }
    }
}
