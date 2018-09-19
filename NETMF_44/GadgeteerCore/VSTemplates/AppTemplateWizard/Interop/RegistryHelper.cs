// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using Microsoft.Win32;

namespace Microsoft.Gadgeteer.AppTemplateWizard
{
    internal static class RegistryHelper
    {
        internal static RegistryKey OpenRegistryKeyOrNull(RegistryHive hive, RegistryView view, string subkeyName, bool writeable = false)
        {
            if (subkeyName == null)
                return null;

            try
            {
                if (writeable)
                    return RegistryKey.OpenBaseKey(hive, view).CreateSubKey(subkeyName);
                else
                    return RegistryKey.OpenBaseKey(hive, view).OpenSubKey(subkeyName, writeable);
            }
            catch (UnauthorizedAccessException) { return null; }
            catch (SecurityException) { return null; }
        }
        internal static RegistryKey OpenRegistryKeyOrNull(RegistryKey key, string subkeyName)
        {
            if (key == null || subkeyName == null)
                return null;

            try { return key.OpenSubKey(subkeyName); }
            catch (UnauthorizedAccessException) { return null; }
            catch (SecurityException) { return null; }
        }
        internal static bool HasSubKey(RegistryKey key, string subkeyName)
        {
            return OpenRegistryKeyOrNull(key, subkeyName) != null;
        }

        internal static bool TryGetValue<T>(RegistryKey key, out T value, string name = null)
        {
            value = default(T);

            if (key == null)
                return false;

            try
            {
                object valueRaw = key.GetValue(name);

                if (valueRaw == null)
                    return false;

                value = (T)valueRaw;
                return true;
            }
            catch { return false; }
        }

        internal static T GetValueOrDefault<T>(RegistryKey key, string name, T defaultValue)
        {
            try
            {
                return (T)key.GetValue(name, defaultValue);
            }
            catch { return defaultValue; }
        }

        internal static IEnumerable<RegistryKey> EnumerateSubkeys(RegistryKey key, StringComparer orderPreference = null)
        {
            if (key == null)
                yield break;

            string[] subkeyNames;
            try { subkeyNames = key.GetSubKeyNames(); }
            catch (UnauthorizedAccessException) { yield break; }
            catch (SecurityException) { yield break; }
            catch (IOException) { yield break; }

            if (orderPreference != null)
                Array.Sort(subkeyNames, orderPreference);

            for (int i = 0; i < subkeyNames.Length; i++)
                if (subkeyNames[i] != null)
                {
                    RegistryKey subkey;
                    try { subkey = key.OpenSubKey(subkeyNames[i]); }
                    catch (UnauthorizedAccessException) { continue; }
                    catch (SecurityException) { continue; }

                    if (subkey != null)
                        yield return subkey;
                }
        }

        internal static string GetLocalName(this RegistryKey key)
        {
            if (key == null)
                return null;

            string name = key.Name;

            int index = name.LastIndexOf('\\');
            if (index == -1)
                return name;

            return name.Substring(index + 1);
        }
    }
}
