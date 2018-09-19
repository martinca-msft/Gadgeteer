// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Resources;
using Microsoft.Gadgeteer.Designer.Definitions;
using Microsoft.VisualStudio.Modeling;
using Microsoft.VisualStudio.Modeling.Design;
using System.Linq;
using System.Reflection;

namespace Microsoft.Gadgeteer.Designer
{
    //The base class already has a virtual Dispose(bool) method we override to dispose the icon bitmap

    /// <summary>
    /// Override some Package method to populate the toolbox
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]        
    partial class GadgeteerDSLPackage
    {
        Bitmap moduleToolboxIcon = new Bitmap(typeof(GadgeteerDSLPackage), "Resources.Module_icon.bmp");

        protected override void Initialize()
        {            
            GadgeteerDefinitionsManager.Instance.Initialize();            
            base.Initialize();                     
        }

        /// <summary>
        /// Create toolbox items based on hte loaded module definitions
        /// </summary>
        protected override IList<ModelingToolboxItem> CreateToolboxItems()
        {
            var items = base.CreateToolboxItems();

            using (var store = new Store(typeof(GadgeteerDSLDomainModel)))
            {
                using (Transaction tx = store.TransactionManager.BeginTransaction())
                {
                    foreach (ModuleDefinition md in GadgeteerDefinitionsManager.Instance.Modules)
                    {
                        //We use one tab per manufacturer to distinguish similar modules
                        //E.g. Buttons from different manufacturers
                        string tabName = md.Manufacturer;
                        string itemId = string.Concat(md.Name, "_", md.UniqueId, "_ToolboxItem");
                        var prototype = CreateModuleToolPrototype(store, md);
                        var item = CreateToolboxItem(store, md, md.Description, tabName, prototype, itemId);
                        if (item != null)
                            items.Add(item);
                    }                    

                    foreach (MainboardDefinition md in GadgeteerDefinitionsManager.Instance.Mainboards)
                    {
                        string tabName = Resources.PackageUI.MainboardsTabName;
                        string itemId = string.Concat(md.Name, "_", "_ToolboxItem");
                        var prototype = CreateMainboardToolPrototype(store, md);
                        var item = CreateToolboxItem(store, md, string.Empty /*mainboards don't have a description*/, tabName, prototype, itemId);                        
                        if (item != null)
                            items.Add(item);
                    }
                    tx.Commit();
                }
            }

            return items;
        }
        
        private IEnumerable<GadgeteerPart> RemoveDuplicates(IEnumerable<GadgeteerPart> definitions)
        {
            //TODO:
            //var groups = definitions.GroupBy(d => d.Type);            
            return null;
        }

        private ModelingToolboxItem CreateToolboxItem(Store store, GadgeteerPart moduleDefinition, string description,
                                                      string tabName, ElementGroupPrototype prototype, string itemId)
        {   
            //Add a filter attribute for each of the .NETMF versions supported by this module. The DocData adds a corresponding
            //filter depending on the version of the project
            var filters = moduleDefinition.SupportedMicroframeworkVersions.Select(v => new ToolboxItemFilterAttribute(GadgeteerDSLToolboxHelperBase.ToolboxFilterString + v,
                                                                             ToolboxItemFilterType.Require)).ToArray();

            var result = new ModelingToolboxItem(
                itemId, // Unique identifier (non-localized) for the toolbox item.
                2, // Position relative to other items in the same toolbox tab.
                moduleDefinition.Name,
                moduleToolboxIcon,// Image displayed next to the toolbox item.  
                "Microsoft.Gadgeteer.Designer.GadgeteerDSLToolboxTab", // Unique identifier (non-localized) for the toolbox item tab.
                tabName, // Localized display name for the toolbox tab.                
                moduleDefinition.Name, // F1 help keyword for the toolbox item.
                description, // Localized tooltip text for the toolbox item.
                prototype,
                filters
                );

            //Imporant to make this transient. Otherwise the toolbox items remain even if modules are uninstalled
            result.IsTransient = true;

            return result;
        }



        private static ElementGroupPrototype CreateModuleToolPrototype(Store store, ModuleDefinition moduleDefinition)
        {
            var element = store.ElementFactory.CreateElement(Module.DomainClassId) as Module;
            element.ModuleType = moduleDefinition.Type;
            element.Name = Module.FixName(moduleDefinition.InstanceName);
            element.ModuleDefinitionId = moduleDefinition.UniqueId;

            return MakePrototype(element);
        }

        private static ElementGroupPrototype CreateMainboardToolPrototype(Store store, MainboardDefinition mainboardDefinition)
        {
            var element = store.ElementFactory.CreateElement(Mainboard.DomainClassId) as Mainboard;
            element.Name = mainboardDefinition.Name;

            return MakePrototype(element);
        }

        private static ElementGroupPrototype MakePrototype(GadgeteerHardware element)
        {
            var elementGroup = new ElementGroup(element.Store.DefaultPartition);
            elementGroup.AddGraph(element, true);
            return elementGroup.CreatePrototype();
        }

        protected override void  Dispose(bool disposing)
        {
 	         base.Dispose(disposing);
            if(disposing)
                moduleToolboxIcon.Dispose();
        }
    }
}
