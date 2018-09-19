// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Microsoft.Gadgeteer.Designer
{
    partial class DocViewPowerOverlay : UserControl
    {
        public DocViewPowerOverlay()
        {
            InitializeComponent();
        }

        internal bool HasAnythingToShow
        {
            get { return PowerGrid.HasItems; }
        }

        internal static bool UpdateFrom(System.Collections.IEnumerable elements, DocViewPowerOverlay view)
        {
            if (elements == null || view == null) return false;
            OverlayModel overlayModel = new OverlayModel();

            try
            {
                foreach (GadgeteerHardware hw in elements.OfType<GadgeteerHardware>())
                {
                    if (hw.GadgeteerPartDefinition.Power.Count > 0)
                    {
                        foreach (Definitions.PowerRequirements requirements in hw.GadgeteerPartDefinition.Power)
                        {
                            PowerRequirementsModel model;

                            if (!overlayModel.Requirements.TryGetValue(requirements.Voltage, out model))
                                overlayModel.Requirements[requirements.Voltage] = model = new PowerRequirementsModel { Voltage = requirements.Voltage };

                            model.Typical += requirements.TypicalCurrent;
                            model.Maximum += requirements.MaximumCurrent;
                        }
                    }
                    else
                    {                        
                        if (hw is MainboardBase)
                        {
                            overlayModel.MissingModels.Add(string.Format("{0} ({1})", ((MainboardBase)hw).Name, hw.GadgeteerPartDefinition.Name));
                            overlayModel.MissingMainboardCount++;
                        }
                        else if (hw is ModuleBase)
                        {
                            overlayModel.MissingModels.Add(string.Format("{0} ({1})", ((ModuleBase)hw).Name, hw.GadgeteerPartDefinition.Name));
                            overlayModel.MissingModuleCount++;
                        }
                    }
                }

                if (overlayModel.MissingModels.Count > 0)
                {
                    foreach (var model in overlayModel.Requirements.Values)
                    {
                        model.TypicalMore = model.MaximumMore = true;
                    }
                }

                view.DataContext = overlayModel;
            }
            catch
            {
                return false;
            }

            return overlayModel.Requirements.Count > 0;
        }
    }
}
