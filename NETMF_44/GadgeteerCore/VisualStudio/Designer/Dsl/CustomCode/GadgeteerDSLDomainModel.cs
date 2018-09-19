// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System;
using System.Collections.Generic;

namespace Microsoft.Gadgeteer.Designer
{
    /// <summary>
    /// Customizations:
    /// 
    /// - Register rules
    /// </summary>
    public partial class GadgeteerDSLDomainModel
    {
        protected override Type[] GetCustomDomainModelTypes()
        {
            var types = new List<Type>(base.GetCustomDomainModelTypes())
                            {
                                typeof (MainboardAddedRule),
                                typeof (GadgeteerModelAddRule),
                                typeof (ModuleAddRule),
                                typeof (MainboardNameChangedRule),
                                typeof (SocketRemovedRule),
                                typeof (ModuleNameChangedRule),
                                typeof (CommentAddedRule),
                                typeof(GadgeteerHardwareRemovedRule)
                                // If you add more rules, list them here. 
                            };            

            return types.ToArray();
        }
    }
}
