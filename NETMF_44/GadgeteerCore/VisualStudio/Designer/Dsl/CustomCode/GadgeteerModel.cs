// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using EnvDTE;
using EnvDTE80;
using Microsoft.Gadgeteer.Designer.Definitions;
using Microsoft.Win32;
using Microsoft.Gadgeteer.Designer.Resources;

namespace Microsoft.Gadgeteer.Designer
{
    /// <summary>
    /// The root class for the model
    /// </summary>
    partial class GadgeteerModel
    {
        public const string ProjectMicroframeworkVersionKey = "ProjectMFVersion";
        public const string GadgeteerCoreVersionKey         = "GadgeteerCoreVersion";
        public const string TypeResolutionServiceKey       = "ITypeResolutionService";

        public enum IssueLevel
        {
            Critical,
            Error,
            Warning,
            Info
        }

        public class Issue : IEquatable<Issue>
        {
            public IssueLevel Level;
            public string Message;

            public Issue(IssueLevel level, string message, params object[] args)
            {
                Level = level;
                Message = string.Format(message, args);
            }

            public bool Equals(Issue other)
            {
                return this.Message == other.Message;
            }

            public override bool Equals(object obj)
            {
                if (obj is Issue)
                    return base.Equals((Issue)obj);

                return false;
            }

            public override int GetHashCode()
            {
                return Message.GetHashCode();
            }
        }

        /// <summary>
        /// Cookie used to distinguish the main designer store from others used for code gen or toolbox population
        /// </summary>
        public static readonly object DesignerStoreCookie = new object();

        private const string LastUsedMainboardKeyPrefix = "LastUsedMainboard";

        /// <summary>
        /// Helper to select the mainboard out of the GadgeteerHardware collection
        /// </summary>
        public Mainboard Mainboard
        {
            get
            {
                return this.GadgeteerHardware.Where(h => h is Mainboard).FirstOrDefault() as Mainboard;
            }
        }

        /// <summary>
        /// Helper to select the modules out of the GadgeteerHardware collection
        /// </summary>
        public IEnumerable<Module> Modules
        {
            get
            {
                return this.GadgeteerHardware.Where(h => h is Module).Cast<Module>();
            }
        }

        /// <summary>
        /// The constructors for modules need to be emitted in a "from the mainboard out" order. For instance, modules
        /// connected directly to the mainboard should have their constructor called before modules connected via extenders
        /// </summary>
        public IEnumerable<Module> SortModulesInCodeGenerationOrder(out IEnumerable<Issue> issues)
        {
            Mainboard mb = this.Mainboard;
            if (mb == null)
            {
                issues = new Issue[0];
                return new Module[0];
            }

            List<Issue> issueBag = new List<Issue>();
            var modules = new List<Module>();

            while (true)
            {
                Module module = this.Modules.FirstOrDefault(m => !modules.Contains(m));
                if (module == null)
                    break;

                GetConnectedModules(module, modules, issueBag);
            }

            issues = issueBag.Distinct();
            return modules;
        }

        /// <summary>
        /// Recursively adds the modules attached to the given GadgeteerHardware in dependency order.
        /// </summary>        
        private static void GetConnectedModules(Module m, List<Module> modules, IList<Issue> issues)
        {
            GetConnectedModules(m, modules, new List<Module>(), issues);
        }

        private static void GetConnectedModules(Module m, List<Module> modules, List<Module> evaluating, IList<Issue> issues)
        {
            if (modules.Contains(m))
            {
                return;
            }

            if (evaluating.Contains(m))
            {
                issues.Add(new Issue(IssueLevel.Error, UI.GenerateLoop, m.Name));
                return;
            }

            evaluating.Add(m);

            foreach (SocketUse u in m.SocketUses)
            {
                if (u.Socket != null && u.Socket.GadgeteerHardware is Module)
                {
                    GetConnectedModules((Module)u.Socket.GadgeteerHardware, modules, evaluating, issues);
                }
            }

            evaluating.Remove(m);

            if (!modules.Contains(m))
                modules.Add(m);

            //foreach (Socket s in hw.Sockets)
            //{
            //    if (s.SocketUse != null && s.SocketUse.Module != null)
            //    {
            //        GetConnectedModules(s.SocketUse.Module, modules, evaluating, issues);
            //    }
            //}
        }


        /// <summary>
        /// This will insert "using" statements into the user code file (Program.cs) for the namespaces of all modules currently in 
        /// the diagram. 
        /// 
        /// This method is called by the DslPackage\Resources\CustomToolTemplateFile.t4 template
        /// </summary>        
        public void GenerateUsingsInUserCode(IServiceProvider sp, string fileName)
        {
            DTE dte = sp.GetService(typeof(DTE)) as DTE;

            if(dte==null || dte.Solution==null)
                return; //Did someone just open a .gadgeteer diagram without a project or solution?

            ProjectItem userCodeItem = dte.Solution.FindProjectItem(fileName);

            if(userCodeItem==null)
                return; //Can't find user file, bail ... 
            
            //We need to use FileCodeModel2 from EnvDTE80 to get AddImport
            var fcm = (FileCodeModel2)userCodeItem.FileCodeModel;
            Log.WriteWarningIf(fcm == null, "Could not generate usings in {0} because the FileCodeModel was unavailable", fileName);
            if (fcm == null)
                return;

            var moduleNamespaces = new List<string>(GetModuleNamespaces());            

            //Remove from the list any namespaces already imported in the file
            foreach (CodeElement c in fcm.CodeElements)
            {
                if (c.Kind == vsCMElement.vsCMElementImportStmt)
                {                   
                    var ci = (CodeImport)c;
                    moduleNamespaces.Remove(ci.Namespace);
                }
            }

            //Add "usings" for remaining 
            foreach (string ns in moduleNamespaces)
            {
                //HACK: FileCodeModel2.AddImport is broken for C# and doesn't like when the position argument is specified.
                //C# by default will add the import at the end but VB will add it at the beginning unless we pass -1
                if(fileName.ToLowerInvariant().EndsWith(".vb"))
                    fcm.AddImport(ns, -1);
                else
                    fcm.AddImport(ns);
            }
        }

        /// <summary>
        /// The a list of unique namespaces for the various modules present. Internal for easier unit testing.
        /// </summary>
        internal IEnumerable<string> GetModuleNamespaces()
        {
            //Isn't LINQ neat? :)
            return (from m in Modules select GetNamespace(m.ModuleType)).Where(ns => ns != null).Distinct();
        }

        /// <summary>
        /// Parse the namespace out of a type name. Internal for easier unit testing.
        /// </summary>
        internal static string GetNamespace(string typeName)
        {
            int lastDot = typeName.LastIndexOf('.');
            if (lastDot < 1)
                return null;

            return typeName.Substring(0, lastDot);                 
        }
        

        internal static MainboardDefinition GetLastUsedMainboardDefinition(string netMFVersion)
        {
            MainboardDefinition definition = null;
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(GadgeteerDefinitionsManager.RegistryRoot, false))
                {
                    if (key == null)
                        return null;

                    string lastUsedMBType = key.GetValue(LastUsedMainboardKeyPrefix + netMFVersion) as string;
                    if (!string.IsNullOrWhiteSpace(lastUsedMBType))
                    {
                        definition = GadgeteerDefinitionsManager.Instance.Mainboards.Where(d => d.Type == lastUsedMBType && d.SupportedMicroframeworkVersions.Contains(netMFVersion)).FirstOrDefault();
                    }
                }
            }
            catch (SecurityException e) { Log.WriteError(e); }
            catch (UnauthorizedAccessException e) { Log.WriteError(e); }
            catch (IOException e) { Log.WriteError(e); }
            return definition;
        }

        internal static void StoreLastUsedMainboardDefinition(MainboardDefinition definition, string netMFVersion)
        {
            Log.WriteErrorIf(definition == null,                      "StoreLastUsedMainboardDefinition called with a null definition");
            Log.WriteErrorIf(string.IsNullOrWhiteSpace(netMFVersion), "StoreLastUsedMainboardDefinition called with an empty .NET MF Version");

            if (definition == null || string.IsNullOrWhiteSpace(netMFVersion))
                return;

            RegistryKey key = null;
            try
            {
                key = Registry.CurrentUser.OpenSubKey(GadgeteerDefinitionsManager.RegistryRoot, true);

                if (key == null)
                    key = Registry.CurrentUser.CreateSubKey(GadgeteerDefinitionsManager.RegistryRoot, RegistryKeyPermissionCheck.ReadWriteSubTree);

                key.SetValue(LastUsedMainboardKeyPrefix + netMFVersion, definition.Type);
            }
            catch (SecurityException e) { Log.WriteError(e); }
            catch (UnauthorizedAccessException e) { Log.WriteError(e); }
            catch (IOException e) { Log.WriteError(e); }
            finally
            {
                if (key != null)
                    key.Dispose();
            }
        }
    }
}
