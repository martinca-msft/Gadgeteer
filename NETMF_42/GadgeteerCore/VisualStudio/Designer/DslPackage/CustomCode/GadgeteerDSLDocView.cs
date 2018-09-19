// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using EnvDTE;

namespace Microsoft.Gadgeteer.Designer
{
    /// <summary>
    /// Customize the doc view to:
    /// 
    /// - Show the toolbox when the diagram is opened
    /// </summary>
    partial class GadgeteerDSLDocView
    {
        private const string ViewToolBoxCommand = "View.Toolbox";

        protected override void Initialize()
        {
            base.Initialize();
            OpenToolbox();
        }

        private void OpenToolbox()
        {
            var dte = this.GetService(typeof(DTE)) as DTE;
            if (dte != null) dte.ExecuteCommand(ViewToolBoxCommand);
        }
    }
}
