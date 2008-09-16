// Copyright (C) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.IO;
using System.Security.Permissions;
using System.Text;


namespace Microsoft.SnippetDesigner
{
    /// <summary>
    ///  NewSnippetCommand from the vs command line
    /// </summary>
	internal class NewSnippetCommand
    {
        #region Global
        private AppCmdLineArguments cmdLineArguments;
        private string language = String.Empty;
        private string code = String.Empty;
        #endregion

        #region Constructor
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
        #endregion

        #region Public Property

        [AppCmdLineArgument("lang","Language of the code snippet")]
        internal string Language
        {
            get
            {
                return language;
            }
            set
            {
                language = value;
            }
        }

        [AppCmdLineArgument("code", "Text in the code snippet")]
        internal string Code
        {
            get
            {
                return code;
            }
            set
            {
                code = value;
            }
        }

        #endregion
    }
}
