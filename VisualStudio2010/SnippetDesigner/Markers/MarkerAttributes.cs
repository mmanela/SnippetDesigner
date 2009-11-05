// Copyright (C) Microsoft Corporation. All rights reserved.

using System;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.SnippetDesigner
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class ProvideCustomMarkerAttribute : RegistrationAttribute
    {
        private string markerName;
        private string resID;
        private string packageGuid;
        private string serviceGuid;
        private string markerGuid;

        private string GetGuidStrFromObject(object obj)
        {
            if (obj is string)
                return (string)obj;
            else if (obj is Type)
                return ((Type)obj).GUID.ToString("B");
            else if (obj is Guid)
                return ((Guid)obj).ToString("B");
            else
                throw new ArgumentException(Resources.ErrorMarkerAttributeLanguageService);
        }

        public ProvideCustomMarkerAttribute(string markerName, int resID, object markerType, object package, object service)
        {
            this.markerName = markerName;
            this.resID = string.Format("#{0}", resID.ToString());
            this.markerGuid = GetGuidStrFromObject(markerType);
            this.packageGuid = GetGuidStrFromObject(package);
            this.serviceGuid = GetGuidStrFromObject(service);
        }

        public override void Register(RegistrationContext context)
        {
            context.Log.WriteLine("Custom Marker:    " + markerGuid);
            context.Log.WriteLine(" - Name           " + markerName);
            context.Log.WriteLine(" - DisplayName    " + resID);
            context.Log.WriteLine(" - Package        " + packageGuid);
            context.Log.WriteLine(" - Service        " + serviceGuid);

            Key childKey = context.CreateKey(RegKeyName);
            childKey.SetValue("", markerName);
            childKey.SetValue("DisplayName", resID);
            childKey.SetValue("Package", packageGuid);
            childKey.SetValue("Service", serviceGuid);
            childKey.Close();
        }

        public override void Unregister(RegistrationContext context)
        {
            context.RemoveKey(RegKeyName);
        }

        private string RegKeyName
        {
            get { return string.Format("Text Editor\\External Markers\\{0}", markerGuid); }
        }

    }


}
