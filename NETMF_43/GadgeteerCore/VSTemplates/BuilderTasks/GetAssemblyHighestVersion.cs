// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;

namespace Microsoft.Gadgeteer.BuilderTasks
{
    public class GetAssemblyHighestVersion : Task
    {
        private ITaskItem[] _assemblies;
        private ITaskItem _assembly;
        private ITaskItem _assemblyVersion;

        [Required]
        public ITaskItem[] Assemblies
        {
            get { return Error.IfNull(_assemblies, "Assemblies"); }
            set { _assemblies = value; }
        }

        [Output]
        public ITaskItem Assembly
        {
            get { return _assembly; }
            set { _assembly = value; }
        }

        [Output]
        public ITaskItem AssemblyVersion
        {
            get { return _assemblyVersion; }
            set { _assemblyVersion = value; }
        }

        public override bool Execute()
        {
            Version _highestVersion = null;

            for (int i = 0; i < Assemblies.Length; i++)
            {
                string versionMetadata = Assemblies[i].GetMetadata("Version");
                Version version;

                if (Version.TryParse(versionMetadata, out version))
                    if (_highestVersion == null || version > _highestVersion)
                    {
                        Assembly = Assemblies[i];
                        _highestVersion = version;
                    }
            }

            if (_highestVersion != null)
            {
                ITaskItem versionItem = new TaskItem(_highestVersion.ToString());
                versionItem.SetMetadata("Major", _highestVersion.Major.ToString());
                versionItem.SetMetadata("Minor", _highestVersion.Minor.ToString());
                versionItem.SetMetadata("Revision", _highestVersion.Revision.ToString());
                versionItem.SetMetadata("Build", _highestVersion.Build.ToString());

                AssemblyVersion = versionItem;
            }

            return true;
        }
    }
}
