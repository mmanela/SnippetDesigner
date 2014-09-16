using System;

namespace Microsoft.SnippetDesigner
{
    public static class GuidList
    {
        //string representation of the guids described below
        //needed for use in attributes since you can only use const strings in attributes
        internal const string SnippetDesignerPkgString = "5a3ff802-5398-465e-8cc6-882fd3cbae55";

        internal const string SnippetDesignerCmdSetString = "d761bf5e-28df-41a8-9168-07703f46cac1";

        internal const string toolWindowPersistanceString = "ec6213e6-b398-4895-9ab2-4a8babe30515";

        internal const string editorFactoryString = "9325357b-c69b-4f03-9d82-0e28723378fe";

        internal const string bitmapStripGuid = "{8410C7B1-EA41-4466-B68E-FDB2E9A41237}";

        internal const string snippetReplacementString = "2F9008C7-282F-4fa7-ADB9-31219A86BBD4";

        internal const string activeSnippetReplacementMarker = "35F885CA-0D3F-4bfa-8A05-69C810690FDA";

        internal const string markerServiceString = "BD9FE285-9066-4553-9C6A-C2F49A24777E";

        internal const string autoLoadOnSolutionExists = "f1536ef8-92ec-443c-9ed7-fdadf150da82";
        internal const string autoLoadOnNoSolution = "adfc4e64-0397-11d1-9f4e-00a0c911004f";

        internal const string editorFactoryLogicalView = "{7651a703-06e5-11d1-8ebd-00a0c90f26ea}";

        internal const string miscellaneousFilesProject = "{A2FE74E1-B743-11d0-AE1A-00A0C90FFFC3}";

        internal const string snippetExplorerString = "8332D89B-E52F-46d7-8D9E-6D201943C631";

        internal const string csharpSnippetLanguageServiceString = "6F34A178-46D5-485a-A636-18DEDEFA0299";
        internal const string vbSnippetLanguageServiceString = "D98C5203-FF7B-4690-9253-8EA75DFA1878";
        internal const string xmlSnippetLanguageServiceString = "005A3641-D402-40af-8782-1BA0FFB83D73";

        public static Guid VsEnvironmentPackage = new Guid("DA9FB551-C724-11d0-AE1F-00A0C90FFFC3");

        //guid for sacPackage
        public static readonly Guid SnippetDesignerPkg = new Guid(SnippetDesignerPkgString);

        //guid for commmand set that all SnippetDesigner commands belong to
        public static readonly Guid SnippetDesignerCmdSet = new Guid(SnippetDesignerCmdSetString);

        //guid for tool window object
        public static readonly Guid toolWindowPersistance = new Guid(toolWindowPersistanceString);

        //guid for codeWindowHost factory
        public static readonly Guid snippetEditorFactory = new Guid(editorFactoryString);

        //Guids for the Snippet Language services
        public static readonly Guid csharpSnippetLanguageService = new Guid(csharpSnippetLanguageServiceString);
        public static readonly Guid vbSnippetLanguageService = new Guid(vbSnippetLanguageServiceString);
        public static readonly Guid xmlSnippetLanguageService = new Guid(xmlSnippetLanguageServiceString);

        //Guid defined by enviorment for a text codeWindowHost factory, we use this for those keybinding 
        //in setsite 
        public static readonly Guid textEditorFactory = new Guid("{8B382828-6202-11d1-8870-0000F87579D2}");


        //Language service guids - these are used to tell a text buffer which language service to attach to
        internal static readonly Guid textLangSvc = new Guid("{8239bec4-ee87-11d0-8c98-00c04fc2ab22}");
        internal static readonly Guid sqlLangSvc = new Guid("{FA6E5E79-C8EE-4D37-B79A-5067F8BD5630}");
        internal static readonly Guid vbLangSvc = new Guid("{E34ACDC0-BAAE-11D0-88BF-00A0C9110049}");
        internal static readonly Guid csLangSvc = new Guid("{694dd9b6-b865-4c5b-ad85-86356e9c88dc}");
        internal static readonly Guid cppLangSvc = new Guid("{b2f072b0-abc1-11d0-9d62-00c04fd9dfd9}");
        internal static readonly Guid jsLangSvc = new Guid("{58e975a0-f8fe-11d2-a6ae-00104bcc7269}");
        internal static readonly Guid xmlLangSvc = new Guid("{f6819a78-a205-47b5-be1c-675b3c7f0b8e}");
        internal static readonly Guid vbScriptLangSvc = new Guid("{f6819a78-a205-47b5-be1c-675b3c7f0b8e}");

        //markers

        internal static readonly Guid markerSvc = new Guid(markerServiceString);
        internal static readonly Guid yellowMarker = new Guid(snippetReplacementString);
        internal static readonly Guid yellowMarkerWithBorder = new Guid(activeSnippetReplacementMarker);
    } ;
}