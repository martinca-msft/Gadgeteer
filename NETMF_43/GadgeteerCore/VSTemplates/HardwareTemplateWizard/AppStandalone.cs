// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
#if DEBUG

using System;
using System.Collections.Generic;

namespace Microsoft.Gadgeteer.HardwareTemplateWizard
{
    internal class AppStandalone
    {
        [STAThread]
        static void Main()
        {
            new Wizard().RunStarted(null, new Dictionary<string, string>(), VisualStudio.TemplateWizard.WizardRunKind.AsNewProject, new object[0]);
        }
    }
}

#endif