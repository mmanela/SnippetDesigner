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
            RegistryKey vsKey = RegistryLocations.GetVSRegKey(Registry.CurrentUser);
            if (vsKey != null)
            {
                location = (string)vsKey.GetValue("VisualStudioLocation", String.Empty);
            }
            return location;
        }

        public static string GetVSInstallDir()
        {
            string location = String.Empty;
            RegistryKey vsKey = RegistryLocations.GetVSRegKey(Registry.LocalMachine);
            if (vsKey != null)
            {
                location = (string)vsKey.GetValue("InstallDir", String.Empty);
            }
            return location;
        }

        public static RegistryKey GetVSRegKey(RegistryKey regKey)
        {
            RegistryKey vsKey = regKey.OpenSubKey(@"Software\Microsoft\VisualStudio\10.0");
            if (vsKey == null)
            {
                vsKey = regKey.OpenSubKey(@"Software\Microsoft\VBExpress\10.0");
            }
            if (vsKey == null)
            {
                vsKey = regKey.OpenSubKey(@"Software\Microsoft\VCSExpress\10.0");
            }
            if (vsKey == null)
            {
                vsKey = regKey.OpenSubKey(@"Software\Microsoft\VJSExpress\10.0");
            }
            if (vsKey == null)
            {
                vsKey = regKey.OpenSubKey(@"Software\Microsoft\VCExpress\10.0");
            }
            if (vsKey == null)
            {
                vsKey = regKey.OpenSubKey(@"Software\Microsoft\VWDExpress\10.0");
            }

            return vsKey;
        }
    }
}
