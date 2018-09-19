// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Gadgeteer.Designer
{
    /// <summary>
    /// Customizations for the ModelExplorer (the tree view)
    /// </summary>
    partial class GadgeteerDSLExplorer
    {
        public override bool AllowDrop
        {
            get
            {
                return false;
            }
            set
            {
                base.AllowDrop = value;
            }
        }

        protected override bool IsAddableRoleForElement(VisualStudio.Modeling.ModelElement element, VisualStudio.Modeling.DomainRoleInfo role)
        {
            //This prevents the model explorer from showing an "Add New Module" context menu
            return false;            
        }
    }

}
