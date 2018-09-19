// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System;

namespace Microsoft.Gadgeteer.Designer
{
    /// <summary>
    /// Override some Package method to populate the toolbox
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    partial class GadgeteerDSLPackage : Microsoft.VisualStudio.Shell.Interop.IVsPackage
    {
        protected override void Initialize()
        {
            GadgeteerDefinitionsManager.Instance.Initialize();
            base.Initialize();
        }

        protected override void OnToolboxUpgraded(object sender, EventArgs e)
        {

        }
    }
}
