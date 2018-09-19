// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Microsoft.Gadgeteer.Designer.Definitions;
using Microsoft.Win32;
using System.Globalization;
using System.Collections.ObjectModel;

namespace Microsoft.Gadgeteer.Designer
{
    /// <summary>
    /// Singleton class used to load and access module and mainboard information across the designer
    /// </summary>
    public sealed class GadgeteerDefinitionsManager : IDisposable
    {
        private GadgeteerDefinitionsManager() { }

        public static readonly GadgeteerDefinitionsManager Instance = new GadgeteerDefinitionsManager();
        XmlSerializer serializer = new XmlSerializer(typeof(GadgeteerDefinitions));
        GadgeteerDefinitions definitions;
        readonly Dictionary<Guid, ModuleDefinition> moduleDefs = new Dictionary<Guid, ModuleDefinition>();
        readonly Dictionary<string, MainboardDefinition> boardDefs = new Dictionary<string, MainboardDefinition>();

        internal const string RegistryRoot = @"SOFTWARE\Microsoft\.NETGadgeteer\v2\";
        private const string DefinitionsRegistryKey = RegistryRoot + @"HardwareDefinitionFolders";
        private const string ModulesFile = "GadgeteerHardware.xml";

        private bool initialized = false;
        private readonly object syncroot = new object();

#if DEBUG
        FileSystemWatcher fsw;
#endif

        /// <summary>
        /// Overload used for unit testing
        /// </summary>
        public void Initialize(IEnumerable<MainboardDefinition> mainboardDefinitions, IEnumerable<ModuleDefinition> moduleDefinitions)
        {
            boardDefs.Clear();
            moduleDefs.Clear();

            foreach (var def in mainboardDefinitions)
                boardDefs.Add(def.Name, def);

            foreach (var def in moduleDefinitions)
                moduleDefs.Add(def.UniqueId, def);

            initialized = true;
        }


        /// <summary>
        /// Loads the module and mainboard definitions from the XML files pointed to by the registry
        /// </summary>
        public void Initialize()
        {
            lock (syncroot)
            {
                if (initialized)
                    return;

                //Look "moduledefinition.xml" files                                        
                LoadDefinitions(Registry.LocalMachine);
                LoadDefinitions(Registry.CurrentUser);

                initialized = true;
            }
        }

        private void LoadDefinitions(RegistryKey hive)
        {
            try
            {
                using (RegistryKey root = hive.OpenSubKey(DefinitionsRegistryKey))
                {
                    Log.WriteWarningIf(root == null, @"Could not find the Gadgeteer registry key {0}\{1}", hive.Name, DefinitionsRegistryKey);
                    if (root == null) return;

                    string[] subKeyNames = root.GetSubKeyNames();
                    foreach (string subkeyName in subKeyNames)
                    {
                        using (RegistryKey key = root.OpenSubKey(subkeyName))
                        {
                            string directory = key.GetValue(string.Empty /*default value*/, string.Empty) as string;
                            if (!string.IsNullOrWhiteSpace(directory))
                            {
                                string filePath = Path.Combine(directory, ModulesFile);
                                if (File.Exists(filePath))
                                    Load(filePath);
                            }
                        }

                    }
                }
            }
            catch (NotSupportedException e) { Log.WriteError(e); }
            catch (Win32Exception e) { Log.WriteError(e); }
            catch (SecurityException e) { Log.WriteError(e); }

        }

        private void Load(string xmlFilePath)
        {
            string xmlDirectory = Path.GetDirectoryName(xmlFilePath);
            Debug.WriteLine("Loading {0}", xmlFilePath);

#if DEBUG
            if (fsw == null)
            {
                fsw = new FileSystemWatcher(xmlDirectory, ModulesFile);
                fsw.Changed += (sender, e) =>
                {
                    moduleDefs.Clear();
                    boardDefs.Clear();
                    Load(e.FullPath);
                };
                fsw.EnableRaisingEvents = true;
            }
#endif


            Action<Exception> logError = (e) => Log.WriteError("Error loading {0}:\n{1}", xmlFilePath, e.ToString());
            try
            {
                using (var fs = new FileStream(xmlFilePath, FileMode.Open, FileAccess.Read))
                {
                    XmlReader reader = XmlReader.Create(fs);

                    definitions = serializer.Deserialize(reader) as GadgeteerDefinitions;
                    if (definitions == null)
                    {
                        Log.WriteError("Could not deserialize {0}.", xmlFilePath);
                        return;
                    }
                    
                    foreach (var md in definitions.ModuleDefinitions)
                    {
                        if (!md.HasErrors)
                        {
                            if (IsNewerVersionRegistered(moduleDefs, md.UniqueId, md))
                                continue;

                            SetFullPaths(xmlDirectory, md);

                            //Make sure instance names start with lower case
                            md.InstanceName = ToCamelCase(md.InstanceName);
                        }
                        moduleDefs[md.UniqueId] = md;
                    }

                    foreach (var md in definitions.MainboardDefinitions)
                    {
                        if (!md.HasErrors)
                        {
                            if (IsNewerVersionRegistered(boardDefs, md.Name, md))
                                continue;

                            SetFullPaths(xmlDirectory, md);
                        }
                        boardDefs[md.Name] = md;
                    }
                }
            }
            catch (IOException e) { logError(e); }
            catch (UnauthorizedAccessException e) { logError(e); }
            catch (XmlException e) { logError(e); }
            catch (FormatException e) { logError(e); }
            catch (InvalidOperationException e) { logError(e); }
        }

        private static void SetFullPaths(string xmlDirectory, GadgeteerPart md)
        {
            if(!string.IsNullOrWhiteSpace(md.Image))
                md.Image = Path.Combine(xmlDirectory, md.Image);

            //Prefix the help url with the path name only if it is not empty and it's not an absolute url
            Uri uri;
            if (!string.IsNullOrWhiteSpace(md.HelpUrl) && !Uri.TryCreate(md.HelpUrl, UriKind.Absolute, out uri))
                md.HelpUrl = Path.Combine(xmlDirectory, md.HelpUrl);
        }

        private static bool IsNewerVersionRegistered<K, V>(Dictionary<K, V> dict, K key, V newPart) where V : GadgeteerPart
        {
            V existing;
            if (!dict.TryGetValue(key, out existing))
                return false;

            var verExisting = Version.Parse(existing.HardwareVersion);
            Version verNew;
            if (Version.TryParse(newPart.HardwareVersion, out verNew))
                return verExisting > verNew;

            return true;
        }

        public ModuleDefinition FindModuleDefinition(Guid uniqueId)
        {
            //We need to initialize on demand because T4 code generation runs in a separate appdomain, where
            //our XML files haven't been loaded
            if (!initialized) Initialize();

            ModuleDefinition m = null;
            moduleDefs.TryGetValue(uniqueId, out m);
            return m;
        }

        public IEnumerable<ModuleDefinition> Modules
        {
            get
            {
                //We need to initialize on demand because T4 code generation runs in a separate appdomain, where
                //our XML files haven't been loaded
                if (!initialized) Initialize();
                return moduleDefs.Values;
            }
        }

        public IEnumerable<MainboardDefinition> Mainboards
        {
            get
            {
                //We need to initialize on demand because T4 code generation runs in a separate appdomain, where
                //our XML files haven't been loaded
                if (!initialized) Initialize();
                return boardDefs.Values;
            }
        }

        private static string ToCamelCase(string s)
        {
            StringInfo info = new StringInfo(s);
            int length = info.LengthInTextElements;

            if (length < 1)
                return s;

            if (length == 1)
                return CultureInfo.CurrentCulture.TextInfo.ToLower(s);

            return CultureInfo.CurrentCulture.TextInfo.ToLower(info.SubstringByTextElements(0, 1)) + info.SubstringByTextElements(1);
        }

        #region IDisposable Members

        public void Dispose()
        {
#if DEBUG
            fsw.Dispose();
#endif
        }

        #endregion
    }



}
