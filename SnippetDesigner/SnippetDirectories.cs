using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.RegistryTools;
using System.Globalization;
using System.IO;
using Microsoft.SnippetDesigner.SnippetExplorer;
using Microsoft.Win32;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Microsoft.SnippetDesigner
{
    /// <summary>
    /// Contains the directories where snippet files are found
    /// </summary>
    internal class SnippetDirectories
    {
        public static SnippetDirectories Instance = new SnippetDirectories();


        private Dictionary<string, string> registryPathReplacements = new Dictionary<string, string>();
        private Regex replaceRegex;

        //snippet directories
        private Dictionary<string, string> userSnippetDirectories = new Dictionary<string, string>();
        private List<string> allSnippetDirectories = new List<string>();

        /// <summary>
        /// Getsthe user snippet directories. This is used to know where to save to
        /// </summary>
        /// <value>The user snippet directories.</value>
        public Dictionary<string, string> UserSnippetDirectories
        {
            get { return userSnippetDirectories; }
        }


        /// <summary>
        /// Gets the paths to all snippets
        /// </summary>
        /// <value>The vs snippet directories.</value>
        public List<string> AllSnippetDirectories
        {
            get { return allSnippetDirectories; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SnippetDirectories"/> class.
        /// </summary>
        private SnippetDirectories()
        {
            registryPathReplacements.Add("%InstallRoot%", GetInstallRoot());
            registryPathReplacements.Add("%LCID%", CultureInfo.CurrentCulture.LCID.ToString());
            registryPathReplacements.Add("%MyDocs%", RegistryLocations.GetVisualStudioUserDataPath());
            replaceRegex = new Regex("(%InstallRoot%)|(%LCID%)|(%MyDocs%)", RegexOptions.Compiled);

            GetUserSnippetDirectories();
            GetSnippetDirectoriesFromRegistry();
        }

        private void GetSnippetDirectoriesFromRegistry()
        {
            using (RegistryKey vsKey = RegistryLocations.GetVSRegKey(Registry.LocalMachine))
            using (RegistryKey codeExpansionKey = vsKey.OpenSubKey("Languages\\CodeExpansions"))
                foreach (string lang in codeExpansionKey.GetSubKeyNames())
                {
                    if (lang.Equals("CSharp", StringComparison.InvariantCulture) ||
                        lang.Equals("Basic", StringComparison.InvariantCulture) ||
                        lang.Equals("XML", StringComparison.InvariantCulture))
                    {
                        try
                        {
                            using (RegistryKey forceCreate = codeExpansionKey.OpenSubKey(lang + "\\ForceCreateDirs"))
                            {
                                foreach (string value in forceCreate.GetValueNames())
                                {
                                    string possiblePathString = forceCreate.GetValue(value) as string;
                                    ProcessPathString(possiblePathString);

                                }
                            }
                        }
                        catch (ArgumentNullException)
                        {
                            Debug.WriteLine("Cannot find ForceCreateDirs for " + lang);
                        }

                        try
                        {
                            using (RegistryKey paths = codeExpansionKey.OpenSubKey(lang + "\\Paths"))
                            {
                                foreach (string value in paths.GetValueNames())
                                {
                                    string possiblePathString = paths.GetValue(value) as string;
                                    ProcessPathString(possiblePathString);

                                }
                            }
                        }
                        catch (ArgumentNullException)
                        {
                            Debug.WriteLine("Cannot find Paths for " + lang);
                        }
                    }
                }

        }

        /// <summary>
        /// Processes the path string.
        /// </summary>
        /// <param name="pathString">The path string.</param>
        private void ProcessPathString(string pathString)
        {
            if (!String.IsNullOrEmpty(pathString))
            {
                string parsedPath = ReplacePathVariables(pathString);
                string[] pathArray = parsedPath.Split(';');

                foreach (string pathToAdd in pathArray)
                {
                    if (allSnippetDirectories.Contains(pathToAdd)) continue;

                    if (Directory.Exists(pathToAdd))
                    {
                        List<string> pathsToRemove = new List<string>();

                        // Check if pathToAdd is a more general version of a path we already found
                        // if so we use that since when we get snippets we do it recursivly from a root
                        foreach (string existingPath in allSnippetDirectories)
                        {
                            if (pathToAdd.Contains(existingPath) && !pathToAdd.Equals(existingPath,StringComparison.InvariantCultureIgnoreCase))
                            {
                                pathsToRemove.Add(existingPath);
                            }
                        }

                        foreach (string remove in pathsToRemove)
                        {
                            allSnippetDirectories.Remove(remove);
                        }

                        bool shouldAdd = true;
                        // Check if there is a path more general than pathToAdd, if so dont add pathToAdd
                        foreach (string existingPath in allSnippetDirectories)
                        {
                            if (existingPath.Contains(pathToAdd))
                            {
                                shouldAdd = false;
                                break;
                            }
                        }

                        if (shouldAdd)
                        {
                            allSnippetDirectories.Add(pathToAdd);
                        }
                    }

                }

            }
        }

        /// <summary>
        /// Replaces the path variables.
        /// </summary>
        /// <param name="pathString">The path string.</param>
        /// <returns></returns>
        private string ReplacePathVariables(string pathString)
        {
            string newPath = replaceRegex.Replace(
                    pathString,
                    new MatchEvaluator(match =>
                    {
                        if (registryPathReplacements.ContainsKey(match.Value))
                        {
                            return registryPathReplacements[match.Value];
                        }
                        else
                        {
                            return match.Value;
                        }

                    })

                );
            return newPath;
        }

        /// <summary>
        /// Gets the install root.
        /// </summary>
        /// <returns></returns>
        private string GetInstallRoot()
        {
            string fullName = SnippetDesignerPackage.Instance.DTE.Application.FullName;
            string pathRoot = Path.GetPathRoot(fullName);
            string[] parts = fullName.Split(Path.DirectorySeparatorChar);
            string vsDirPath = "";
            if (parts.Length >= 3)
            {
                vsDirPath = Path.Combine(pathRoot, Path.Combine(parts[1], parts[2]));
            }
            else
            {
                vsDirPath = RegistryLocations.GetVSInstallDir() + @"..\..\";
            }

            return vsDirPath;
        }

        /// <summary>
        /// Gets the user snippet directories. These are used for the save as dialog
        /// </summary>
        private void GetUserSnippetDirectories()
        {
            string vsDocDir = RegistryLocations.GetVisualStudioUserDataPath();
            string snippetDir = Path.Combine(vsDocDir, ConstantStrings.SnippetDirectoryName);
            userSnippetDirectories[Resources.DisplayNameCSharp] = Path.Combine(snippetDir, Path.Combine(ConstantStrings.SnippetDirNameCSharp, ConstantStrings.MySnippetsDir));
            userSnippetDirectories[Resources.DisplayNameVisualBasic] = Path.Combine(snippetDir, Path.Combine(ConstantStrings.SnippetDirNameVisualBasic, ConstantStrings.MySnippetsDir));
            userSnippetDirectories[Resources.DisplayNameXML] = Path.Combine(snippetDir, Path.Combine(ConstantStrings.SnippetDirNameXML, ConstantStrings.MyXmlSnippetsDir)); ;
            userSnippetDirectories[String.Empty] = snippetDir;
        }
    }
}
