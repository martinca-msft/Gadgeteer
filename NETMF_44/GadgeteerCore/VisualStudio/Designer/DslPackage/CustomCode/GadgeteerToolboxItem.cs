// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using Microsoft.Gadgeteer.Designer.Definitions;
using Microsoft.VisualStudio.Modeling;
using Microsoft.VisualStudio.Modeling.Design;
using Microsoft.VisualStudio.Modeling.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Runtime.Serialization;

namespace Microsoft.Gadgeteer.Designer
{
    [Serializable]
    public class GadgeteerToolboxItem : ModelingToolboxItem
    {
        private const string ToolboxItemTabName = "Microsoft.Gadgeteer.Designer.GadgeteerDSLToolboxTab";
        private static readonly ToolboxItemFilterAttribute[] FilterAttributes = new[] { new ToolboxItemFilterAttribute(GadgeteerDSLToolboxHelperBase.ToolboxFilterString + "Hardware", ToolboxItemFilterType.Require) };

        private static readonly Bitmap ToolboxIcon = new Bitmap(typeof(GadgeteerDSLPackage), "Resources.ToolboxIcon.bmp");
        private static readonly Bitmap ToolboxIconError = new Bitmap(typeof(GadgeteerDSLPackage), "Resources.ToolboxIconError.bmp");
        private static readonly Bitmap ToolboxIconWarning = new Bitmap(typeof(GadgeteerDSLPackage), "Resources.ToolboxIconWarning.bmp");

        public GadgeteerPart GadgeteerPart { get; private set; }
        public Version MinimumGadgeteerCoreVersion { get; private set; }

        private GadgeteerToolboxItem(MainboardDefinition part, Store store, Bitmap bitmap, int position, string error = null)
            : base(
                GenerateId(part), // Unique identifier (non-localized) for the toolbox item.
                position, // Position relative to other items in the same toolbox tab.
                GenerateName(part),
                bitmap, // Image displayed next to the toolbox item.  
                ToolboxItemTabName, // Unique identifier (non-localized) for the toolbox item tab.
                Resources.PackageUI.MainboardsTabName, // Localized display name for the toolbox tab.
                part.Name, // F1 help keyword for the toolbox item.
                GenerateDescription(part, error), // Localized tooltip text for the toolbox item.
                GeneratePrototype(part, store),
                FilterAttributes)
        {
            Initialize(part, store);
        }
        private GadgeteerToolboxItem(ModuleDefinition part, Store store, Bitmap bitmap, int position, string error = null)
            : base(
                GenerateId(part), // Unique identifier (non-localized) for the toolbox item.
                position, // Position relative to other items in the same toolbox tab.
                GenerateName(part),
                bitmap, // Image displayed next to the toolbox item.  
                ToolboxItemTabName, // Unique identifier (non-localized) for the toolbox item tab.
                part.Manufacturer, // Localized display name for the toolbox tab.
                part.Name, // F1 help keyword for the toolbox item.
                GenerateDescription(part, error), // Localized tooltip text for the toolbox item.
                GeneratePrototype(part, store),
                FilterAttributes)
        {
            Initialize(part, store);
        }
        protected GadgeteerToolboxItem(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        private void Initialize(GadgeteerPart part, Store store)
        {
            IsTransient = true;
            GadgeteerPart = part;

            if (!part.HasErrors)
                MinimumGadgeteerCoreVersion = System.Version.Parse(part.MinimumGadgeteerCoreVersion);
        }

        private static string GenerateId(MainboardDefinition part)
        {
            return string.Concat(part.Name, "_", "_ToolboxItem");
        }
        private static string GenerateId(ModuleDefinition part)
        {
            return string.Concat(part.Name, "_", part.UniqueId, "_ToolboxItem");
        }

        private static string GenerateName(GadgeteerPart part)
        {
            if (string.IsNullOrWhiteSpace(part.Name))
                return Resources.PackageUI.DefinitionNoName;

            return part.Name;
        }
        private static string GenerateDescription(GadgeteerPart part, string error)
        {
            if (!string.IsNullOrWhiteSpace(error))
                return error + Environment.NewLine + Environment.NewLine + part.Description;

            return part.Description;
        }

        private static ElementGroupPrototype GeneratePrototype(ModuleDefinition part, Store store)
        {
            var element = store.ElementFactory.CreateElement(Module.DomainClassId) as Module;
            element.ModuleType = part.Type;
            element.Name = Module.FixName(part.InstanceName);
            element.ModuleDefinitionId = part.UniqueId;

            return MakePrototype(element);
        }
        private static ElementGroupPrototype GeneratePrototype(MainboardDefinition part, Store store)
        {
            var element = store.ElementFactory.CreateElement(Mainboard.DomainClassId) as Mainboard;
            element.Name = part.Name;

            return MakePrototype(element);
        }
        private static ElementGroupPrototype MakePrototype(GadgeteerHardware element)
        {
            var elementGroup = new ElementGroup(element.Store.DefaultPartition);
            elementGroup.AddGraph(element, true);
            return elementGroup.CreatePrototype();
        }

        public static GadgeteerToolboxItem CreateFor(GadgeteerPart part, Store store, string mfVersion, Version gVersion, int position = 2)
        {
            Bitmap icon = ToolboxIcon;
            string errorMessage = null;

            if (part.HasErrors)
            {
                errorMessage = string.Format(Resources.PackageUI.DefinitionError, string.Join(Environment.NewLine, part.GetErrors()));

                icon = ToolboxIconError;
            }
            else
            {
                if (gVersion != null && gVersion < System.Version.Parse(part.MinimumGadgeteerCoreVersion))
                {
                    // do not block the designer if Gadgeteer version cannot be found (e.g. referenced as project)
                    // if (gVersion == null)
                    //     errorMessage = string.Format(Resources.PackageUI.WarningNoG, part.MinimumGadgeteerCoreVersion);
                    // else
                        errorMessage = string.Format(Resources.PackageUI.WarningGVersion, part.MinimumGadgeteerCoreVersion, gVersion);

                    icon = ToolboxIconWarning;
                }

                else if (!part.SupportedMicroframeworkVersions.Contains(mfVersion))
                {
                    errorMessage = string.Format(Resources.PackageUI.WarningMFVersion, string.Join(", ", part.SupportedMicroframeworkVersions), mfVersion);

                    icon = ToolboxIconWarning;
                }
            }

            part.ErrorMessage = errorMessage;

            ModuleDefinition module = part as ModuleDefinition;
            if (module != null)
                return new GadgeteerToolboxItem(module, store, icon, position, errorMessage);

            MainboardDefinition mainboard = part as MainboardDefinition;
            if (mainboard != null)
                return new GadgeteerToolboxItem(mainboard, store, icon, position, errorMessage);

            throw new NotSupportedException();
        }
    }
}
