// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;

namespace Microsoft.Gadgeteer.AppTemplateWizard
{
    internal static class MicroFramework
    {
        internal static readonly Guid ProjectFlavorGuid = new Guid("b69e3092-b931-443c-abe7-7e7b65f2a37f");

        private const string RegistryRoot = @"SOFTWARE\Microsoft\.NETMicroFramework";
        private const string InstallRoot = "InstallRoot";
        private const string AssemblyFoldersEx = "AssemblyFoldersEx";

        private const string ProductName = "Microsoft .NET Micro Framework";
        private const string ProductPath = @"InstalledProducts\" + ProductName;
        private const string EditionProductPath = @"SOFTWARE\Microsoft\{0}\{1}.{2}\InstalledProducts\" + ProductName;
        private static readonly string[] EditionsToTest = new string[] { "VBExpress", "VCSExpress", "VisualStudio", "WDExpress" };

        internal static RegistryKey MachineRootKey
        {
            get { return RegistryHelper.OpenRegistryKeyOrNull(RegistryHive.LocalMachine, RegistryView.Registry32, RegistryRoot); }
        }
        internal static RegistryKey GetAssemblyFoldersExKey(string version)
        {
            return RegistryHelper.OpenRegistryKeyOrNull(MachineRootKey, version + "\\" + AssemblyFoldersEx);
        }

        internal static bool IsInstalled
        {
            get
            {
                string instalRoot;

                foreach (RegistryKey subkey in RegistryHelper.EnumerateSubkeys(MachineRootKey))
                    if (RegistryHelper.TryGetValue(subkey, out instalRoot, InstallRoot))
                        return true;

                return false;
            }
        }
        internal static bool IsVersionInstalled(string version)
        {
            RegistryKey versionKey = RegistryHelper.OpenRegistryKeyOrNull(MachineRootKey, "v" + version);
            
            if (versionKey == null)
                return false;

            return RegistryHelper.GetValueOrDefault<string>(versionKey, InstallRoot, null) != null;
        }
        internal static bool IsInVisualStudioEditionInstalled(string rootKeyPath)
        {
            RegistryKey machineRootKey = RegistryHelper.OpenRegistryKeyOrNull(RegistryHive.LocalMachine, RegistryView.Registry32, rootKeyPath);

            if (machineRootKey == null)
                return false;

            if (RegistryHelper.HasSubKey(machineRootKey, ProductPath))
                return true;

            // HACK: VSIX hot fix

            RegistryKey userKey = RegistryHelper.OpenRegistryKeyOrNull(RegistryHive.CurrentUser, RegistryView.Registry32, rootKeyPath);
            if (userKey == null)
                return false;

            RegistryKey extensionKey = RegistryHelper.OpenRegistryKeyOrNull(userKey, "ExtensionManager\\EnabledExtensions");
            if (extensionKey == null)
                return false;

            foreach (string name in extensionKey.GetValueNames())
                if (name.StartsWith("Microsoft.NETMF.CoreDebug"))
                    return true;

            return false;
        }
        
        internal static bool IsInAnyVisualStudioEditionInstalled(Version runningVersion)
        {
            foreach (string edition in EditionsToTest)
                if (RegistryHelper.OpenRegistryKeyOrNull(RegistryHive.LocalMachine, RegistryView.Registry32, string.Format(EditionProductPath, edition, runningVersion.Major, runningVersion.Minor)) != null)
                    return true;

            return false;
        }

        internal static IEnumerable<string> InstalledVersions
        {
            get
            {
                return from subkey in RegistryHelper.EnumerateSubkeys(MachineRootKey)
                       select subkey.GetLocalName();
            }
        }
        internal static IEnumerable<string> InstalledBuildNumbers
        {
            get
            {
                foreach (RegistryKey subkey in RegistryHelper.EnumerateSubkeys(MachineRootKey))
                {
                    string version = subkey.GetLocalName();

                    if (version.StartsWith("v"))
                    {
                        string buildNumber = RegistryHelper.GetValueOrDefault(subkey, "BuildNumber", "0");
                        version += "." + buildNumber + ".0";
                    }

                    yield return version;
                }
            }
        }

        internal static bool IsMicroFrameworkMoniker(string moniker)
        {
            return moniker != null && moniker.StartsWith(".NETMicroFramework");
        }
        internal static string GetMonikerVersion(string moniker)
        {
            if (moniker == null)
                throw new ArgumentNullException("moniker");

            string[] components = moniker.Split(',');
            foreach (string component in components)
            {
                string[] tokens = component.Split('=');
                if (tokens[0] == "Version" && tokens.Length > 1)
                    return tokens[1];
            }

            return null;
        }

        internal static IEnumerable<string> GetRegisteredAssemblyNames(string version)
        {
            RegistryKey exKey = GetAssemblyFoldersExKey(version);
            foreach (RegistryKey assemblyKey in RegistryHelper.EnumerateSubkeys(exKey))
            {
                string assemblyFolderPath = RegistryHelper.GetValueOrDefault<string>(assemblyKey, null, null);

                if (assemblyFolderPath != null)
                    if (Directory.Exists(assemblyFolderPath))
                    {
                        string name;                        
                        foreach (string assemblyPath in UntilException(Directory.EnumerateFiles(assemblyFolderPath, "*.dll")))
                        {
                            try { name = Path.GetFileNameWithoutExtension(assemblyPath); }
                            catch (ArgumentException) { continue; }

                            yield return name;
                        }
                        foreach (string assemblyPath in UntilException(Directory.EnumerateFiles(assemblyFolderPath, "*.exe")))
                        {
                            try { name = Path.GetFileNameWithoutExtension(assemblyPath); }
                            catch (ArgumentException) { continue; }

                            yield return name;
                        }
                    }
            }
        }

        private static IEnumerable<T> UntilException<T>(IEnumerable<T> enumerable)
        {
            // cannot yield from try block
            IEnumerator<T> enumerator = null;

            try { enumerator = enumerable.GetEnumerator(); }
            catch { yield break; }

            while (true)
            {
                T current;

                try
                {
                    if (enumerator.MoveNext())
                        current = enumerator.Current;
                    else
                        yield break;
                }
                catch { yield break; }

                yield return current;
            }
        }
    }
}
