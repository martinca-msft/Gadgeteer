// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
#if DEBUG

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TemplateWizard;

namespace Microsoft.Gadgeteer.HardwareTemplateWizard
{
    internal class AppStandalone
    {
        [STAThread]
        static void Main()
        {
            try { new Wizard().RunStarted(null, new Dictionary<string, string>(), VisualStudio.TemplateWizard.WizardRunKind.AsMultiProject, new object[0]); }
            catch (WizardCancelledException) { }
        }
    }
}

#endif