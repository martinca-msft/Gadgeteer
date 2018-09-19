// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using EnvDTE;
using EnvDTE80;
using Microsoft.Gadgeteer.Designer.Definitions;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Modeling;
using Microsoft.VisualStudio.Shell.Interop;
using VSLangProj;
using VSLangProj80;
using System.Diagnostics;
using System.ComponentModel;
using Microsoft.Gadgeteer.Designer.Resources;
using System.Globalization;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing.Design;
using System.Drawing;
using Microsoft.VisualStudio.Shell.Design;
using System.ComponentModel.Design;

namespace Microsoft.Gadgeteer.Designer
{

	partial class GadgeteerDSLDocData
	{
		Project project;
		private const string CSExtension = ".cs";
        private const string VBExtension = ".vb";
        private string GeneratedSuffix = ".generated";
		
        private const string GadgeteerCoreAssemblyName = "Gadgeteer";

		private string ProjectMFVersion = "";
        private Version ProjectGVersion = null;
		private const string VersionMonikerPrefix = "Version=v";

		public override void Initialize(Store sharedStore)
		{
			base.Initialize(sharedStore);

            //Tag this store as the main designer store. Other activities such as loading the toolbox or generating code use different stores
            this.Store.PropertyBag.Add(GadgeteerModel.DesignerStoreCookie, null);
			SetupAssemblyReferencing();
			SetupModuleRenameHandler();
            SetupToolbox();         
		}

        private string toolboxMFversion;
        private Version toolboxGversion;

        private void SetupToolbox()
        {
            if (ProjectMFVersion == toolboxMFversion && ProjectGVersion == toolboxGversion)
                return;

            var service = GetService(typeof(IToolboxService)) as IToolboxService;
            var toolbox = GetService(typeof(SVsToolbox)) as IVsToolbox3;
            if (service == null || toolbox == null)
                return;

            using (var txStore = new Store(typeof(GadgeteerDSLDomainModel)))
            using (var tx = txStore.TransactionManager.BeginTransaction())
            {
                foreach (GadgeteerToolboxItem item in service.GetToolboxItems().OfType<GadgeteerToolboxItem>())
                    service.RemoveToolboxItem(item);

                int position = 2;

                foreach (MainboardDefinition md in GadgeteerDefinitionsManager.Instance.Mainboards)
                    AddToolboxItem(GadgeteerToolboxItem.CreateFor(md, txStore, ProjectMFVersion, ProjectGVersion, position++), service, toolbox);

                foreach (ModuleDefinition md in GadgeteerDefinitionsManager.Instance.Modules.OrderBy(m => m.Name))
                    AddToolboxItem(GadgeteerToolboxItem.CreateFor(md, txStore, ProjectMFVersion, ProjectGVersion, position++), service, toolbox);

                tx.Commit();
            }

            this.toolboxMFversion = this.ProjectMFVersion;
            this.toolboxGversion = this.ProjectGVersion;
        }
        private void AddToolboxItem(GadgeteerToolboxItem item, IToolboxService service, IVsToolbox3 toolbox)
        {
            service.AddToolboxItem(item, item.TabName);

            string testTabId = null;
            if (toolbox.GetIDOfTab(item.TabName, out testTabId) != 0 || testTabId != item.TabNameId + item.TabName)
            {
                toolbox.SetIDOfTab(item.TabName, item.TabNameId + item.TabName);

                // add a dummy item so that the group is not visible outside of Gadgeteer designer (empty groups are by design)
                IVsToolbox2 toolbox2 = toolbox as IVsToolbox2;
                if (toolbox2 != null)
                {
                    Guid pkgGuid = new Guid(Constants.GadgeteerDSLPackageId);

                    TBXITEMINFO[] info = new[] { new TBXITEMINFO { bstrText = "(hidden placeholder)" } };
                    toolbox2.AddItem2(new Microsoft.VisualStudio.Shell.OleDataObject(), info, item.TabName, ref pkgGuid);
                }
            }
        }

		protected override void OnDocumentLoaded(EventArgs e)
		{			
            var diagram = Store.ElementDirectory.FindElements(GadgeteerDSLDiagram.DomainClassId).FirstOrDefault() as GadgeteerDSLDiagram;
            if (diagram != null)
                diagram.AddToolboxFilter(new ToolboxItemFilterAttribute(GadgeteerDSLToolboxHelperBase.ToolboxFilterString + "Hardware", ToolboxItemFilterType.Require));

            base.OnDocumentLoaded(e);

			GenerateCodeIfNeeded();
		}

		private void GenerateCodeIfNeeded()
		{
            if (ModelingDocStore == null)
                return;

            bool genCode = false;
            Store store = ModelingDocStore.Store;
            Mainboard board = store.ElementDirectory.FindElements<Mainboard>().FirstOrDefault();
            if (board != null && board.CreatedByNewModel)
            {
                //This means we just added a board when the diagram got created (see DSL\Rules\MainboardRules). Let VS know the doc data
                //is dirty so that the file contents are written out to disk. Without this, the code generation will be based on an empty
                //diagram.
                this.SetDocDataDirty(1);
                genCode = true;
            }

			//When a Gadgeteer diagram first gets instantiated we want to trigger the generation of the
			//Program.generated.cs file right away			
            genCode |= !File.Exists(GetGeneratedCodeFileName());
            
            if(genCode)
                RunCodeGeneration();
		}

        private ReferencesEvents referencesEvents;

		/// <summary>
		/// Configure store events to add assembly references when modules or the mainboard are added to the diagram
		/// </summary>
		private void SetupAssemblyReferencing()
		{
			//Get the DTE Project from the IVsHierarchy
			project = GetProject();

			if (project != null)
			{
				//Get the MF version we are running under (e.g. 4.1 or 4.2)
				var prop = project.Properties.Item("TargetFrameworkMoniker");
				if (prop != null)
					ProjectMFVersion = ParseTargetVersion(prop.Value as string);
                
                StoreGadgeteerCoreVersionNumber();
                var vsproject = (VSProject2)project.Object;
                referencesEvents = vsproject.Events.ReferencesEvents;
                if (referencesEvents != null)
                {
                    referencesEvents.ReferenceAdded += ReferencesEvents_ReferenceChangeHandler;
                    referencesEvents.ReferenceRemoved += ReferencesEvents_ReferenceChangeHandler;
                    referencesEvents.ReferenceChanged += ReferencesEvents_ReferenceChangeHandler;
                }

                //This is how the DSL objects access the MF version being used
                this.Store.PropertyBag[GadgeteerModel.ProjectMicroframeworkVersionKey] = ProjectMFVersion;

				//Hookup to Module creation events 
				var hardware = this.Store.DomainDataDirectory.FindDomainClass(GadgeteerHardware.DomainClassId);
				this.Store.EventManagerDirectory.ElementAdded.Add(hardware, new EventHandler<ElementAddedEventArgs>(GadgeteerHardwareAdded));
				this.Store.EventManagerDirectory.ElementDeleted.Add(hardware, new EventHandler<ElementDeletedEventArgs>(GadgeteerHardwareDeleted));
			}            
		}

        void ReferencesEvents_ReferenceChangeHandler(Reference pReference)
        {
            if (pReference.Name == GadgeteerCoreAssemblyName)
            {
                StoreGadgeteerCoreVersionNumber();
                SetupToolbox();
            }
        }


        private void StoreGadgeteerCoreVersionNumber()
        {
            var vsproject = (VSProject2)project.Object;
            Reference gadgeteerRef = vsproject.References.Cast<Reference>().Where(r => r.Name == GadgeteerCoreAssemblyName).FirstOrDefault();
            if (gadgeteerRef != null && !string.IsNullOrWhiteSpace(gadgeteerRef.Path))
            {
                FileVersionInfo ver = FileVersionInfo.GetVersionInfo(gadgeteerRef.Path);
                this.Store.PropertyBag[GadgeteerModel.GadgeteerCoreVersionKey] = ProjectGVersion = new Version(ver.FileVersion);
            }
            else
            {
                this.ProjectGVersion = null;
                this.Store.PropertyBag.Remove(GadgeteerModel.GadgeteerCoreVersionKey);
            }
        }

		private void SetupModuleRenameHandler()
		{
			DomainClassInfo modDc = this.Store.DomainDataDirectory.FindDomainClass(Module.DomainClassId);
			DomainPropertyInfo nameProp = this.Store.DomainDataDirectory.FindDomainProperty(Module.NameDomainPropertyId);
			this.Store.EventManagerDirectory.ElementPropertyChanged.Add(modDc, nameProp,
																		new EventHandler<ElementPropertyChangedEventArgs>(ModuleRenamed));
		}

		private void ModuleRenamed(object source, ElementPropertyChangedEventArgs e)
		{
			//If an element with the old name exists it means the designer is auto-assigning a new name 
			//to a newly dropped element and there's no need to rename the corresponding code element. In
			//fact that would be wrong because we would end up renaming the code element corresponding to the
			//model element with the old name, whose name hasn't changed.
			if (((Module)e.ModelElement).CountModulesWithName((string)e.OldValue) > 0)
				return;

			FileCodeModel2 fcm = GetGeneratedFileCodeModel();

			if (fcm == null) return;
			CodeElement2 field = FindElement(fcm.CodeElements, (string)e.OldValue, vsCMElement.vsCMElementVariable);
			if (field != null)
				field.RenameSymbol((string)e.NewValue);

            UpdateView();
		}


		CodeElement2 FindElement(CodeElements elements, string name, vsCMElement kind)
		{
			foreach (CodeElement2 ce in elements)
			{
				if (ce.Kind == kind && ce.Name == name)
					return ce;

				CodeElement2 found = FindElement(ce.Children, name, kind);
				if (found != null)
					return found;
			}
			return null;
		}

		private FileCodeModel2 GetGeneratedFileCodeModel()
		{
			string fileName = GetGeneratedCodeFileName();
            if (fileName == null) 
                return null;

			DTE dte = GetService(typeof(DTE)) as DTE;
			if (dte == null || dte.Solution == null)
				return null;

			ProjectItem userCodeItem = dte.Solution.FindProjectItem(fileName);

			if (userCodeItem == null)
				return null; //Can't find user file, bail ... 

			//We need to use FileCodeModel2 from EnvDTE80 to get AddImport
			var fcm = (FileCodeModel2)userCodeItem.FileCodeModel;
			return fcm;
		}

		private Project GetProject()
		{
			object objProj;
			int ret = this.Hierarchy.GetProperty(VSConstants.VSITEMID_ROOT,
									(int)__VSHPROPID.VSHPROPID_ExtObject,
									out objProj);

			if (ret == VSConstants.S_OK)
				return (Project)objProj;

			Debug.Fail("Could not find the project");
			Log.WriteError("Could not get the DTE.Project for {0}", this.FileName);
			return null;
		}

		/// <summary>
		/// Add a reference to the assembly when a module/mainboard gets added
		/// </summary>
		private void GadgeteerHardwareAdded(object sender, ElementAddedEventArgs e)
		{            
			var hw = e.ModelElement as GadgeteerHardware;
			if (hw == null) return;

            AddAssemblyReferences(hw);

            var mb = hw as Mainboard;
            if (mb != null)
                GadgeteerModel.StoreLastUsedMainboardDefinition(mb.GadgeteerPartDefinition as MainboardDefinition, ProjectMFVersion);

            UpdateView();
		}

        private void AddAssemblyReferences(GadgeteerHardware hw)
        {
            IEnumerable<Assembly> assemblies = GetAssemblyDefinitions(hw);
            if (assemblies.Count()>0)
            {
                foreach(Assembly assembly in assemblies)
                    AddAssemblyReference(hw, assembly);
            }
            else
                Log.WriteError("Could not find a suitable assembly for module '{0}' and MF Version {1}", hw, ProjectMFVersion);
        }

        private void AddAssemblyReference(GadgeteerHardware hw, Assembly assembly)
        {
            try
            {
                var vsproject = (VSProject2)project.Object;
                //This is a no-op if the reference already exists
                var reference = (Reference3)vsproject.References.Add(assembly.Name);
                reference.SpecificVersion = false;
            }
            catch (COMException ex)
            {
                string message = assembly.ErrorMessage;
                if(string.IsNullOrWhiteSpace(message))
                    message = string.Format(PackageUI.MissingAssembly, hw.GetType().Name.ToLower(CultureInfo.CurrentCulture), ProjectMFVersion);

                MessageBox.Show(message, PackageUI.ErrorDialogTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Log.WriteError("Could not add a reference to assembly '{0}': {1}", assembly.Name, ex.Message);

                //TODO: Is it possible to rollback the addition of the module? Doing Undo and FlushRedoStack here results in a null
                //reference exception. Need to investigate why. Also if this happens during the opening of the designer because
                //of an improperly configured mainboard the designer fails to open.  Overall seems like the DSL framework is not too
                //happy about us calling Undo here.
                //Store.UndoManager.Undo();
                //Store.UndoManager.FlushRedoStack();
            }
        }

		/// <summary>
		///		Remove reference when hardware is deleted
		/// </summary>
		private void GadgeteerHardwareDeleted(object sender, ElementDeletedEventArgs e)
		{
			var hw = e.ModelElement as GadgeteerHardware;
			if (hw == null)
				return;

            RemoveAssemblyReferences(hw);
            RemoveUsingStatements(hw);

            UpdateView();
		}

        private void UpdateView()
        {
            foreach (GadgeteerDSLDocView view in this.DocViews.OfType<GadgeteerDSLDocView>())
            {
                view.UpdateFromStore(this.Store);
            }
        }
        
        private void RemoveAssemblyReferences(GadgeteerHardware hw)
        {
            var delAssemblies = GetAssemblyDefinitions(hw);
            foreach (Assembly assembly in delAssemblies)
            {
                bool assemblyInUse = false;
                // check if there are any more elements of the same kind before removing the reference
                var elements = this.Store.ElementDirectory.FindElements(GadgeteerHardware.DomainClassId);
                foreach (var element in elements)
                {
                    var curAssemblies = GetAssemblyDefinitions(element as GadgeteerHardware);
                    if (curAssemblies.Select(a => a.Name).Contains(assembly.Name))
                    {
                        assemblyInUse = true;
                        break;
                    }
                }

                if (!assemblyInUse)
                {
                    var reference = GetAssemblyReference(assembly.Name);
                    if (reference != null)
                        reference.Remove();
                }
            }
        }

        private void RemoveUsingStatements(GadgeteerHardware hw)
        {
            var ns = GadgeteerModel.GetNamespace(hw.GadgeteerPartDefinition.Type);
            var elements = this.Store.ElementDirectory.FindElements(GadgeteerHardware.DomainClassId);

            // check for other elements that have the same namespace before removing the using statement
            foreach (var element in elements)
            {
                if (GadgeteerModel.GetNamespace(((GadgeteerHardware)element).GadgeteerPartDefinition.Type) == ns)
                    return;
            }

            var vsp = (VSProject2)project.Object;
            var userCodeItem = vsp.DTE.Solution.FindProjectItem(this.GetCodeFileName());
            if (userCodeItem == null)
                return;

            var fcm = (FileCodeModel2)userCodeItem.FileCodeModel;

            foreach (CodeElement codeElement in fcm.CodeElements)
            {
                if (codeElement.Kind == vsCMElement.vsCMElementImportStmt)
                {
                    var s = ((CodeImport)codeElement).Namespace;
                    if (s == ns)
                        fcm.Remove(codeElement);
                }
            }
        }

        internal IEnumerable<Reference> GetAssemblyReferences(GadgeteerHardware hw)
        {
            return GetAssemblyDefinitions(hw).Select(asmDef => GetAssemblyReference(asmDef.Name));
        }

		internal Reference GetAssemblyReference(string assemblyName)
		{
			var vsproject = (VSProject2)project.Object;
			return vsproject.References.Cast<Reference>().Where(r => r.Name == assemblyName).FirstOrDefault();
		}

		private IEnumerable<Assembly> GetAssemblyDefinitions(GadgeteerHardware hw)
		{
			GadgeteerPart definition = hw.GadgeteerPartDefinition;
			if (definition == null || definition.Assemblies == null)
			{
				Log.WriteError("Could not find definition for module {0}", hw);
				return new Assembly[0];
			}

			return definition.Assemblies.Where(assembly => assembly.MFVersion == ProjectMFVersion);
		}

		private string ParseTargetVersion(string targetMoniker)
		{
			//The moniker looks like this:   ".NETMicroFramework,Version=v4.1"

			if (string.IsNullOrWhiteSpace(targetMoniker))
			{
				Log.WriteError("Empty TargetFrameworkMoniker for project {0}. The GTMAdornment will not show any modules.", project.Name);
				return string.Empty;
			}
			int i = targetMoniker.IndexOf(VersionMonikerPrefix);
			if (i < 0)
			{
				Log.WriteError("Invalid TargetFrameworkMoniker for project {0}: '{1}'. The GTMAdornment will not show any modules.", project.Name, targetMoniker);
				return string.Empty;
			}
			string version = targetMoniker.Substring(i + VersionMonikerPrefix.Length);
			Log.WriteInfo("{0} using MF Version {1}", project.Name, version);
			return version;
		}

		private void RunCodeGeneration()
		{
			//The designer package associates its own CodeGenerationCustomTool as the custom tool for 
			//.gadeteer files. So calling VSProjectItem.RunCustomTool triggers the code generation
			object objItem;
			int ret = Hierarchy.GetProperty(ItemId,
								(int)__VSHPROPID.VSHPROPID_ExtObject,
								out objItem);

			var item = objItem as ProjectItem;
			if (item != null)
			{
				var vsItem = item.Object as VSProjectItem;
				if (vsItem != null)
					vsItem.RunCustomTool();
			}
		}

        protected override Store CreateStore()
        {
            Store store = base.CreateStore();

            var typeService = (DynamicTypeService)this.GetService(typeof(DynamicTypeService));
            if (typeService != null)
            {
                ITypeResolutionService resolutionService = typeService.GetTypeResolutionService(Hierarchy);
                store.PropertyBag.Add(GadgeteerModel.TypeResolutionServiceKey, resolutionService);
            }
            
            return store;
        }

        protected override void OnDocumentClosing(EventArgs e)
        {
            //If the doc data is dirty we need to generate the code before the file closes. Otherwise we can leave the model and the code file
            //out of sync
            if (IsDocDirty)
                RunCodeGeneration();

            base.OnDocumentClosing(e);

            if (referencesEvents != null)
            {
                referencesEvents.ReferenceAdded -= ReferencesEvents_ReferenceChangeHandler;
                referencesEvents.ReferenceRemoved -= ReferencesEvents_ReferenceChangeHandler;
                referencesEvents.ReferenceChanged -= ReferencesEvents_ReferenceChangeHandler;
                referencesEvents = null;
            }
        }

		internal bool IsDocDirty
		{
			get
			{
				int dirty;
				IsDirty(out dirty);
				return dirty == 1;
			}
		}

        private string GeneratedExtension
        {
            get
            {                
                return GeneratedSuffix + LanguageExtension;                
            }
        }

        private string LanguageExtension
        {
            get
            {
                if (project.Kind == PrjKind.prjKindCSharpProject)
                    return CSExtension;
                else if (project.Kind == PrjKind.prjKindVBProject)
                    return VBExtension;
                else
                {
                    Debug.Fail("Unknown project kind: " + project.Kind);
                    Log.WriteError("Unknown project kind: " + project.Kind);
                    return string.Empty;
                }
            }
        }


		internal string GetCodeFileName()
		{
			return GetCodeFileName(LanguageExtension);
		}

		internal string GetGeneratedCodeFileName()
		{
			return GetCodeFileName(GeneratedExtension);
		}

		private string GetCodeFileName(string extension)
		{
			//If the designer name is Program.gadgeteer, try to find Program.cs
			string fileName = this.FileName;
            if (string.IsNullOrEmpty(fileName))
                return null;

            string path = Path.Combine(Path.GetDirectoryName(fileName),
						 string.Concat(Path.GetFileNameWithoutExtension(fileName), extension));
			return path;
		}

	}
}
