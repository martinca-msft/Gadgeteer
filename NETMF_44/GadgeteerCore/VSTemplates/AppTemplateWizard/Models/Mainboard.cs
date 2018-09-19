// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using System.ComponentModel;

namespace Microsoft.Gadgeteer.AppTemplateWizard
{
    internal class Mainboard
    {
        private static readonly XNamespace ns = Gadgeteer.DefinitionsNamespace;
        private static readonly string[] InstalledVersions;

        public bool HasDefinitionErrors { get; private set; }

        public string Name { get; private set; }
        public string Type { get; private set; }
        public string Manufacturer { get; private set; }
        public string Description { get; private set; }
        public string HelpUrl { get; private set; }
        public string ImagePath { get; private set; }

        public string MinimumCoreVersion { get; private set; }
        public string[] SupportedVersions { get; private set; }
        public string[] AvailableVersions { get; private set; }

        public AssemblyReference[] Assemblies { get; private set; }

        static Mainboard()
        {
            IEnumerable<string> dotted = from installedVersion in MicroFramework.InstalledVersions
                                         select installedVersion + ".";

            InstalledVersions = dotted.ToArray();
        }

        public Mainboard(XElement definition, string basePath, bool hasErrors)
        {
            HasDefinitionErrors = hasErrors;

            Name         = (string)definition.Attribute("Name");
            Type         = (string)definition.Attribute("Type");            
            Manufacturer = (string)definition.Attribute("Manufacturer");
            Description  = (string)definition.Attribute("Description");
            HelpUrl      = (string)definition.Attribute("HelpUrl");
            ImagePath    = (string)definition.Attribute("Image");

            if (ImagePath != null && basePath != null)
                ImagePath = Path.Combine(basePath, ImagePath);

            MinimumCoreVersion = (string)definition.Attribute("MinimumGadgeteerCoreVersion");

            IEnumerable<string> mfVersions = from assembly in definition.Elements(ns + "Assemblies").Elements(ns + "Assembly")
                                             let mfVersion = (string)assembly.Attribute("MFVersion")
                                             where !string.IsNullOrEmpty(mfVersion)
                                             select mfVersion;

            SupportedVersions = mfVersions.Distinct().ToArray();

            IEnumerable<string> availableVersions = from supportedVersion in SupportedVersions
                                                    where InstalledVersions.Any(installedVersion => installedVersion.StartsWith("v" + supportedVersion + "."))
                                                    select supportedVersion;

            AvailableVersions = availableVersions.ToArray();

            Assemblies = definition.Elements(ns + "Assemblies").Elements(ns + "Assembly").Select(a => new AssemblyReference(a)).ToArray();
        }
    }

    internal class AssemblyReference
    {
        public string Version { get; private set; }
        public string Name { get; private set; }

        public AssemblyReference(XElement assembly)
        {
            Version = (string)assembly.Attribute("MFVersion");
            Name = (string)assembly.Attribute("Name");
        }
    }
}
