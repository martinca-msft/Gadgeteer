// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;

namespace Microsoft.Gadgeteer.AppTemplateWizard
{
    internal static class Gadgeteer
    {
        internal const string DefinitionsFile = "GadgeteerHardware.xml";
        internal const string DefinitionsNamespace = "http://schemas.microsoft.com/Gadgeteer/2011/Hardware";

        private const string RegistryRoot = @"SOFTWARE\Microsoft\.NETGadgeteer\v2\";
        private const string HardwareDefinitionFolders = "HardwareDefinitionFolders";
        private const string LastUsedMainboardTypeName = "LastUsedMainboard";
        private const string LastUsedMicroFrameworkVersionName = "LastUsedMicroFrameworkVersion";
        private const string GadgeteerAssemblyFolderEx = "Microsoft .NET Gadgeteer";

        private static RegistryKey MachineRootKey
        {
            get { return RegistryHelper.OpenRegistryKeyOrNull(RegistryHive.LocalMachine, RegistryView.Registry32, RegistryRoot); }
        }
        private static RegistryKey MachineHardwareDefinitionFoldersKey
        {
            get { return RegistryHelper.OpenRegistryKeyOrNull(MachineRootKey, HardwareDefinitionFolders); }
        }
        private static RegistryKey UserRootKey
        {
            get { return RegistryHelper.OpenRegistryKeyOrNull(RegistryHive.CurrentUser, RegistryView.Registry64, RegistryRoot); }
        }

        internal static IEnumerable<string> EnumerateHardwareDefinitionFolders(StringComparer orderPreference = null)
        {
            string lastUsedMainboard = LastUsedMainboardType;
            string lastUsedPath;

            RegistryKey lastUsedKey = RegistryHelper.OpenRegistryKeyOrNull(MachineHardwareDefinitionFoldersKey, lastUsedMainboard);
            if (RegistryHelper.TryGetValue(lastUsedKey, out lastUsedPath))
                yield return lastUsedPath;

            foreach (RegistryKey folderKey in RegistryHelper.EnumerateSubkeys(MachineHardwareDefinitionFoldersKey, orderPreference))
            {
                string folderPath = null;
                if (RegistryHelper.TryGetValue(folderKey, out folderPath))
                    if (folderPath != lastUsedPath)
                        yield return folderPath;
            }
        }

        internal static string LastUsedMainboardType
        {
            get { return RegistryHelper.GetValueOrDefault<string>(UserRootKey, LastUsedMainboardTypeName, null); }
        }
        internal static string LastUsedMicroFrameworkVersion
        {
            get { return RegistryHelper.GetValueOrDefault<string>(UserRootKey, LastUsedMicroFrameworkVersionName, null); }
        }

        internal static string GetLastUsedMicroFrameworkVersionForMainboard(string type)
        {
            return RegistryHelper.GetValueOrDefault<string>(UserRootKey, type, LastUsedMicroFrameworkVersion);
        }

        internal static void SetLastUsedMainboard(string type, string version)
        {
            RegistryKey root = RegistryHelper.OpenRegistryKeyOrNull(RegistryHive.CurrentUser, RegistryView.Registry64, RegistryRoot, true);
            if (root == null)
                return;

            try
            {
                root.SetValue(LastUsedMainboardTypeName, type);
                root.SetValue(LastUsedMicroFrameworkVersionName, version);
                root.SetValue(type, version);
            }
            catch (UnauthorizedAccessException) { }
            catch (SecurityException) { }
        }

        internal static string GetAssembliesPath(string version)
        {
            RegistryKey exKey = MicroFramework.GetAssemblyFoldersExKey(version);
            RegistryKey exGadgeteerKey = RegistryHelper.OpenRegistryKeyOrNull(exKey, GadgeteerAssemblyFolderEx);

            string assembliesPath = null;
            RegistryHelper.TryGetValue(exGadgeteerKey, out assembliesPath);
                
            return assembliesPath;
        }
    }
}
