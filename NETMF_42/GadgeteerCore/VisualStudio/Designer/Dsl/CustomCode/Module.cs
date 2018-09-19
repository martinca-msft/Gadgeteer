// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System.CodeDom.Compiler;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Microsoft.Gadgeteer.Designer.Definitions;
using System.Collections.Generic;
using Microsoft.Gadgeteer.Designer.Resources;

namespace Microsoft.Gadgeteer.Designer
{
    /// <summary>
    /// This class is required by the two Calculated properties we have: Manufacturer and Type
    /// </summary>
	partial class ModuleBase
	{
		public string GetManufacturerValue()
		{
			return ((ModuleDefinition)this.GadgeteerPartDefinition).Manufacturer;
		}

		public string GetTypeValue()
		{
			var type = ((ModuleDefinition)this.GadgeteerPartDefinition).Type;
			var s = string.Concat(Module.GadgeteerModuleRootNamespace, ".");
			return (type.IndexOf(s) == 0) ? type.Substring(s.Length) : type;
		}
	}

	partial class Module
	{
        //These two are used to make sure we generate valid C# identifiers from the module names
        private static readonly Regex NameRegex = new Regex(@"[^\w]");
        private static readonly CodeDomProvider CsharpProvider = CodeDomProvider.CreateProvider("C#");

		private static readonly XmlSerializer serializer = new XmlSerializer(typeof(ModuleDefinition));

		private const string MissingSocket = "Socket.Unused";
		private ModuleDefinition definition;

        //Module manufacturers are encouraged to use this root namespace
        public static readonly string GadgeteerModuleRootNamespace = "Gadgeteer.Modules";
        public static readonly string GadgeteerModuleRootNamespaceAlias = "GTM";        

		private static readonly ModuleDefinition EmptyDefinition = new ModuleDefinition()
		{
			BoardHeight = 20,
			BoardWidth = 20,
			Name = "Module Definition Not Found",
			InstanceName = "notFound",
			ProvidedSockets = new Collection<ProvidedSocket>(),
			Sockets = new Collection<Definitions.SocketUse>()
		};

		/// <summary>
		/// The associated ModuleDefinition
		/// </summary>
		internal override GadgeteerPart GadgeteerPartDefinition
		{
			get
			{
				if (definition == null)
					definition = GadgeteerDefinitionsManager.Instance.FindModuleDefinition(this.ModuleDefinitionId);

				if (definition == null)
					return CachedPartDefinition ?? EmptyDefinition;

				return definition;
			}
		}

		protected override XmlSerializer CachedDefinitionSerializer
		{
			get { return serializer; }
		}


		public bool HasDefinitions
		{
			get
			{
				if (definition == null)
					return false;

				return Sockets.All(s => s.HasDefinition) &&
					   SocketUses.All(s => s.HasDefinition);
			}
		}


        /// <summary>
        /// Instantiate SocketUses based on the xml definition
        /// </summary>
		internal void CreateSocketUses()
		{
			var modDef = this.GadgeteerPartDefinition as ModuleDefinition;

			if (modDef == null)
				return;

			if (modDef.Sockets.Count == 0)
				return;

			SocketUses.Clear();

			foreach (var socketDef in modDef.Sockets)
			{
				var s = new SocketUse(this.Store) { Label = socketDef.TypesLabel };
				this.SocketUses.Add(s);
			}
		}

		/// <summary>
		/// Whether all the required sockets are connected. Used by the T4 template
		/// </summary>
		public bool Connected
		{
			get
			{
				return this.SocketUses.All(s => s.Optional || s.Socket != null);
			}
		}

		/// <summary>
		/// Generates the string of constructor parameters. Used by the T4 template
		/// </summary>
		public string GenerateConstructorString()
		{
			var sockets = this.SocketUses.OrderBy(s => ((Definitions.SocketUse)(s.Definition)).ConstructorOrder);

			var parameters = sockets.Select(s => GenerateConstructorParameter(s.Socket));

			return string.Join(", ", parameters.ToArray());
		}


		/// <summary>
		/// For the given socket returns the constructor argument that needs to be generated in the code. The rules are:
		/// 
		/// null socket -> Socket.Missing
		/// if socket constructor parameter is a number -> The number (as a string, usually the case for mainboard sockets)
		/// else: module name + . + constructor parameter    (this supports module provided socket)
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		private static string GenerateConstructorParameter(Socket s)
		{
			if (s == null)
				return MissingSocket;

			var ps = s.Definition as ProvidedSocket;
			if (ps == null)
			{
				Log.WriteError("Could not get the definition for socket {0} on module {1}", s.Label, s.GadgeteerHardware);
				return MissingSocket;
			}
			int socketNumber;
			if (int.TryParse(ps.ConstructorParameter, out socketNumber) || !(s.GadgeteerHardware is Module))
				return ps.ConstructorParameter;

			return string.Concat(((Module)(s.GadgeteerHardware)).Name, '.', ps.ConstructorParameter);
		}

        /// <summary>
        /// The type name but with "Gadgeteer.Modules." replaced by "GTM."
        /// </summary>
	    public string AliasedTypeName
	    {
	        get
	        {
                if (this.ModuleType.StartsWith(GadgeteerModuleRootNamespace + "."))
                    return this.ModuleType.Replace(GadgeteerModuleRootNamespace, GadgeteerModuleRootNamespaceAlias);

                return this.ModuleType;
	        }

	    }

		//Prevent pasting of sockets
		protected override bool CanMerge(VisualStudio.Modeling.ProtoElementBase rootElement, VisualStudio.Modeling.ElementGroupPrototype elementGroupPrototype)
		{
			return false;
		}

		public override string ToString()
		{
			return string.Format("{0}: {1}", typeof(Module).Name, this.Name);
		}

        /// <summary>
        /// Fixes module names to remove invalid characters and make the name a valid C# identifier
        /// </summary>
        internal static string FixName(string name)
        {
            name = NameRegex.Replace(name, string.Empty);
            name = CsharpProvider.CreateValidIdentifier(name);
            if (!CsharpProvider.IsValidIdentifier(name))
                return string.Empty;
            return name;
        }

        internal int CountModulesWithName(string name)
        {
            var peers = Store.ElementDirectory.FindElements<Module>();
            return peers.Count((m) => m.Name == name);
        }

        internal IEnumerable<string> GetMissingDependencyErrors()
        {
            // the errors are a set so that duplicate error messages are avoided.
            HashSet<string> errors = new HashSet<string>();
            if (this.GadgeteerModel == null)
                return errors;

            var definition = this.GadgeteerPartDefinition as ModuleDefinition;
            if (definition == null || definition.ExtraLibrariesRequired == null || definition.ExtraLibrariesRequired.Count==0)
                return errors;

            //Don't do checks if this is not executing in the context of a project
            if (!this.GadgeteerModel.Store.PropertyBag.ContainsKey(GadgeteerModel.DesignerStoreCookie))
                return errors;

            object oVersion = null;
            this.GadgeteerModel.Store.PropertyBag.TryGetValue(GadgeteerModel.ProjectMicroframeworkVersionKey, out oVersion);
            string netMFVersion = oVersion as string;
            if(string.IsNullOrEmpty(netMFVersion)) 
            {
                errors.Add(UI.CannotDetermineMFVersion);
                return errors;
            }

            var librariesAvailable = this.GadgeteerModel.GadgeteerHardware.SelectMany(
                                          hw=>(hw.GadgeteerPartDefinition.LibrariesProvided ?? new Collection<Assembly>())
                                              .Where(asm => asm.MFVersion == netMFVersion).Select(asm => asm.Name));
            
            foreach (Assembly asm in definition.ExtraLibrariesRequired)
            {
                if (asm.MFVersion != netMFVersion) continue;
                if(!librariesAvailable.Contains(asm.Name))
                {
                    if(string.IsNullOrWhiteSpace(asm.ErrorMessage))
                        errors.Add(string.Format(UI.MissingRequiredLibrary, asm.Name));
                    else
                        errors.Add(asm.ErrorMessage);
                }
            }

            return errors;
        }

	}
}
