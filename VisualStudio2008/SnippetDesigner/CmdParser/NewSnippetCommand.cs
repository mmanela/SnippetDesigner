using System;

namespace Microsoft.SnippetDesigner
{
    /// <summary>
    ///  NewSnippetCommand from the vs command line
    /// </summary>
    internal class NewSnippetCommand
    {
        private readonly AppCmdLineArguments cmdLineArguments;
        private string language = String.Empty;
        private string code = String.Empty;


        /// <summary>
        ///    Constructor that takes the list of command line arguments.
        /// </summary>
        /// <param name="args">Command line arguments</param>
        internal NewSnippetCommand(string[] args)
        {
            // Create Command Line argument parser.
            cmdLineArguments = new AppCmdLineArguments(args);

            // Update this class properties like PluginName, ListPluins etc based on 
            // user supplied command line arguments.
            cmdLineArguments.UpdateParams(this);
        }


        [AppCmdLineArgument("lang", "Language of the code snippet")]
        internal string Language
        {
            get { return language; }
            set { language = value; }
        }

        [AppCmdLineArgument("code", "Text in the code snippet")]
        internal string Code
        {
            get { return code; }
            set { code = value; }
        }
    }
}