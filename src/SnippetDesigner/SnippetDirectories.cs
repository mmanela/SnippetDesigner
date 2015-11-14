using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security;
using System.Text.RegularExpressions;
using Microsoft.RegistryTools;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;

namespace Microsoft.SnippetDesigner
{
    /// <summary>
    /// Contains the directories where snippet files are found
    /// </summary>
    internal class SnippetDirectories
    {
        public static SnippetDirectories Instance = new SnippetDirectories();


        private readonly Dictionary<string, string> registryPathReplacements = new Dictionary<string, string>();
        private readonly Regex replaceRegex;

        //snippet directories
        private readonly Dictionary<string, string> userSnippetDirectories = new Dictionary<string, string>();
        private readonly List<string> defaultSnippetDirectories = new List<string>();

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
        public List<string> DefaultSnippetDirectories
        {
            get { return defaultSnippetDirectories; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SnippetDirectories"/> class.
        /// </summary>
        private SnippetDirectories()
        {
            var version = SnippetDesignerPackage.Instance.VSVersion;
            IUIHostLocale localeHost = (IUIHostLocale)SnippetDesignerPackage.Instance.GetService(typeof(IUIHostLocale));
            uint lcid = (uint)CultureInfo.CurrentCulture.LCID;
            localeHost.GetUILocale(out lcid);

            registryPathReplacements.Add("%InstallRoot%", GetInstallRoot(version));
            registryPathReplacements.Add("%LCID%", lcid.ToString());
            registryPathReplacements.Add("%MyDocs%", RegistryLocations.GetVisualStudioUserDataPath(version));
            replaceRegex = new Regex("(%InstallRoot%)|(%LCID%)|(%MyDocs%)", RegexOptions.Compiled);

            GetUserSnippetDirectories();
            GetSnippetDirectoriesFromRegistry(Registry.LocalMachine, false, version);
            GetSnippetDirectoriesFromRegistry(Registry.CurrentUser, true, version);
        }

        private void AddPathsFromRegistryKey(RegistryKey key, string subKeyName)
        {
            try
            {
                using (RegistryKey subKey = key.OpenSubKey(subKeyName))
                {
                    foreach (string name in subKey.GetValueNames())
                    {
                        string possiblePathString = subKey.GetValue(name) as string;
                        ProcessPathString(possiblePathString);
                    }
                }
            }
            catch (NullReferenceException e)
            {
                SnippetDesignerPackage.Instance.Logger.Log(string.Format("Cannot find registry values in {0} for {1}", key, subKeyName), "SnippetDirectories", e);
            }
            catch (ArgumentException e)
            {
                SnippetDesignerPackage.Instance.Logger.Log(string.Format("Cannot find registry values in {0} for {1}", key, subKeyName), "SnippetDirectories", e);
            }
        }


        private void GetSnippetDirectoriesFromRegistry(RegistryKey rootKey, bool configSection, string version)
        {
            try
            {
                using (RegistryKey vsKey = RegistryLocations.GetVSRegKey(rootKey, configSection, version))
                {
                    if (vsKey == null) return;

                    using (RegistryKey codeExpansionKey = vsKey.OpenSubKey("Languages\\CodeExpansions"))
                    {
                        foreach (string lang in codeExpansionKey.GetSubKeyNames())
                        {
                            try
                            {
                                using (var langKey = codeExpansionKey.OpenSubKey(lang))
                                {
                                    AddPathsFromRegistryKey(langKey, "ForceCreateDirs");
                                    AddPathsFromRegistryKey(langKey, "Paths");
                                }

                            }
                            catch (NullReferenceException e)
                            {
                                SnippetDesignerPackage.Instance.Logger.Log(string.Format("Cannot find registry values for {0}", lang), "SnippetDirectories", e);
                            }
                            catch (ArgumentException e)
                            {
                                SnippetDesignerPackage.Instance.Logger.Log(string.Format("Cannot find registry values for {0}", lang), "SnippetDirectories", e);
                            }
                        }
                    }
                }
            }
            catch (ArgumentException e)
            {
                SnippetDesignerPackage.Instance.Logger.Log("Cannot acces registry", "SnippetDirectories", e);
            }
            catch (NullReferenceException e)
            {
                SnippetDesignerPackage.Instance.Logger.Log("Cannot acces registry", "SnippetDirectories", e);
            }
            catch (SecurityException e)
            {
                SnippetDesignerPackage.Instance.Logger.Log("Cannot acces registry", "SnippetDirectories", e);
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
                parsedPath = NormalizeSlashes(parsedPath);

                string[] pathArray = parsedPath.Split(';');

                foreach (string pathToAdd in pathArray)
                {
                    if (defaultSnippetDirectories.Contains(pathToAdd)) continue;

                    if (Directory.Exists(pathToAdd))
                    {
                        List<string> pathsToRemove = new List<string>();

                        // Check if pathToAdd is a more general version of a path we already found
                        // if so we use that since when we get snippets we do it recursivly from a root
                        foreach (string existingPath in defaultSnippetDirectories)
                        {
                            if (pathToAdd.Contains(existingPath) && !pathToAdd.Equals(existingPath, StringComparison.InvariantCultureIgnoreCase))
                            {
                                pathsToRemove.Add(existingPath);
                            }
                        }

                        foreach (string remove in pathsToRemove)
                        {
                            defaultSnippetDirectories.Remove(remove);
                        }

                        bool shouldAdd = true;
                        // Check if there is a path more general than pathToAdd, if so dont add pathToAdd
                        foreach (string existingPath in defaultSnippetDirectories)
                        {
                            if (existingPath.Contains(pathToAdd))
                            {
                                shouldAdd = false;
                                break;
                            }
                        }

                        if (shouldAdd)
                        {
                            defaultSnippetDirectories.Add(pathToAdd);
                        }
                    }
                }
            }
        }

        private static string NormalizeSlashes(string pathString)
        {
            return pathString.Replace(@"\\", @"\");
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
        /// <param name="version"> </param>
        /// <returns></returns>
        private static string GetInstallRoot(string version)
        {
            string fullName = SnippetDesignerPackage.Instance.Dte.Application.FullName;
            string pathRoot = Path.GetPathRoot(fullName);
            string[] parts = fullName.Split(Path.DirectorySeparatorChar);
            string vsDirPath = "";
            if (parts.Length >= 3)
            {
                vsDirPath = Path.Combine(pathRoot, Path.Combine(parts[1], parts[2]));
            }
            else
            {
                vsDirPath = RegistryLocations.GetVSInstallDir(version) + @"..\..\";
            }

            return vsDirPath;
        }

        /// <summary>
        /// Gets the user snippet directories. These are used for the save as dialog
        /// </summary>
        private void GetUserSnippetDirectories()
        {
            string vsDocDir = RegistryLocations.GetVisualStudioUserDataPath(SnippetDesignerPackage.Instance.VSVersion);
            string snippetDir = Path.Combine(vsDocDir, StringConstants.SnippetDirectoryName);
            userSnippetDirectories[Resources.DisplayNameCSharp] = Path.Combine(snippetDir, Path.Combine(StringConstants.SnippetDirNameCSharp, StringConstants.MySnippetsDir));
            userSnippetDirectories[Resources.DisplayNameVisualBasic] = Path.Combine(snippetDir, Path.Combine(StringConstants.SnippetDirNameVisualBasic, StringConstants.MySnippetsDir));
            userSnippetDirectories[Resources.DisplayNameXML] = Path.Combine(snippetDir, Path.Combine(StringConstants.SnippetDirNameXML, StringConstants.MyXmlSnippetsDir));
            userSnippetDirectories[Resources.DisplayNameSQL] = Path.Combine(snippetDir, Path.Combine(StringConstants.SnippetDirNameSQL, StringConstants.MySnippetsDir));
            userSnippetDirectories[Resources.DisplayNameSQLServerDataTools] = Path.Combine(snippetDir, Path.Combine(StringConstants.SnippetDirNameSQLServerDataTools, StringConstants.MySnippetsDir));

            if (!SnippetDesignerPackage.Instance.IsVisualStudio2010)
            {
                userSnippetDirectories[Resources.DisplayNameCPP] = Path.Combine(snippetDir, Path.Combine(StringConstants.SnippetDirNameCPP, StringConstants.MySnippetsDir));
                userSnippetDirectories[Resources.DisplayNameJavaScript] = Path.Combine(snippetDir, Path.Combine(StringConstants.SnippetDirNameJavaScriptVS11, StringConstants.MySnippetsDir));
            }


            if (!SnippetDesignerPackage.Instance.IsVisualStudio2010 && !SnippetDesignerPackage.Instance.IsVisualStudio2012)
            {
                userSnippetDirectories[Resources.DisplayNameXAML] = Path.Combine(snippetDir, StringConstants.SnippetDirNameXAML);
            }

            var webDevSnippetDir = Path.Combine(snippetDir, StringConstants.VisualWebDeveloper);
            if (SnippetDesignerPackage.Instance.IsVisualStudio2010)
            {
                userSnippetDirectories[Resources.DisplayNameJavaScript] = Path.Combine(webDevSnippetDir, StringConstants.SnippetDirNameJavaScript);
            }


            userSnippetDirectories[Resources.DisplayNameHTML] = Path.Combine(webDevSnippetDir, StringConstants.SnippetDirNameHTML);

            userSnippetDirectories[String.Empty] = snippetDir;
        }
    }
}