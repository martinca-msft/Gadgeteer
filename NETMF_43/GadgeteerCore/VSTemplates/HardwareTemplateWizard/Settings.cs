// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;

namespace Microsoft.Gadgeteer.HardwareTemplateWizard
{
    partial class Settings
    {
        public static string GetLastManufacturerName()
        {
            string name = Default.LastManufacturerName;

            if (string.IsNullOrEmpty(name))
                try
                {
                    using (RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
                    {
                        name = (string)key.GetValue("RegisteredOrganization");

                        if (string.IsNullOrEmpty(name))
                            name = (string)key.GetValue("RegisteredOwner");
                    }
                }
                catch (SecurityException) { }
                
            return name;
        }

        public static string GetLastManufacturerSafeName()
        {
            string name = Default.LastManufacturerSafeName;

            if (string.IsNullOrEmpty(name))
                return null;

            return name;
        }
    }
}
