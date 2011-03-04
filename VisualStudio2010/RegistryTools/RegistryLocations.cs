using System;
using Microsoft.Win32;

namespace Microsoft.RegistryTools
{
    /// <summary>
    /// get the correct data based on the correct vs reg path
    /// </summary>
    public static class RegistryLocations
    {
        public static string GetVisualStudioUserDataPath()
        {
            string location = String.Empty;
            RegistryKey vsKey = GetVSRegKey(Registry.CurrentUser);
            if (vsKey != null)
            {
                location = (string) vsKey.GetValue("VisualStudioLocation", String.Empty);
            }
            return location;
        }

        public static string GetVSInstallDir()
        {
            string location = String.Empty;
            RegistryKey vsKey = GetVSRegKey(Registry.LocalMachine);
            if (vsKey != null)
            {
                location = (string) vsKey.GetValue("InstallDir", String.Empty);
            }
            return location;
        }

        public static int GetVSUILanguage()
        {
            int language = 1033; // default to english
            using (RegistryKey vsKey = GetVSRegKey(Registry.CurrentUser, false))
            {
                if (vsKey != null)
                {
                    using (RegistryKey generalKey = vsKey.OpenSubKey("General"))
                    {
                        if (generalKey != null)
                        {
                            try
                            {
                                language = (int) vsKey.GetValue("UILanguage", 1033);
                            }
                            catch(InvalidCastException)
                            {
                            }
                        }
                    }
                }
            }

            return language;
        }

        public static RegistryKey GetVSRegKey(RegistryKey regKey)
        {
            return GetVSRegKey(regKey, false);
        }

        public static RegistryKey GetVSRegKey(RegistryKey regKey, bool configSection)
        {
            string versionPath = configSection ? "10.0_Config" : "10.0";
            RegistryKey vsKey = regKey.OpenSubKey(@"Software\Microsoft\VisualStudio\" + versionPath);
            if (vsKey == null)
            {
                vsKey = regKey.OpenSubKey(@"Software\Microsoft\VBExpress\" + versionPath);
            }
            if (vsKey == null)
            {
                vsKey = regKey.OpenSubKey(@"Software\Microsoft\VCSExpress\" + versionPath);
            }
            if (vsKey == null)
            {
                vsKey = regKey.OpenSubKey(@"Software\Microsoft\VJSExpress\" + versionPath);
            }
            if (vsKey == null)
            {
                vsKey = regKey.OpenSubKey(@"Software\Microsoft\VCExpress\" + versionPath);
            }
            if (vsKey == null)
            {
                vsKey = regKey.OpenSubKey(@"Software\Microsoft\VWDExpress\" + versionPath);
            }

            return vsKey;
        }
    }
}