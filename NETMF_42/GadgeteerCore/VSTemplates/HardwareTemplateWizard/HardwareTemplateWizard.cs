// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TemplateWizard;
using System.Windows.Forms;

namespace Microsoft.Gadgeteer.HardwareTemplateWizard
{
    public class GTHardwareWizard : IWizard
    {
        public static bool netmf41Support;
        public static bool netmf42Support;
        public static string manufacturerShortName;
        public static string manufacturerFullName;
        public static string hardwareShortName;
        public static string hardwareFullName;
        public bool cancelled;

        GTHardwareWizardForm inputForm;

        public void BeforeOpeningFile(EnvDTE.ProjectItem projectItem)
        {
        }

        public void ProjectFinishedGenerating(EnvDTE.Project project)
        {
        }

        public void ProjectItemFinishedGenerating(EnvDTE.ProjectItem projectItem)
        {
        }

        public void RunFinished()
        {
        }

        const string xmlTagStart = "<";
        const string xmlTagEnd = ">";
        const string xmlCommentStart = "<!-- ";
        const string xmlCommentEnd = " -->";

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            try
            {
                if (runKind == WizardRunKind.AsMultiProject)
                {
                    // reinitialize all variables
                    netmf41Support = true;
                    netmf42Support = true;
                    manufacturerShortName = "";
                    manufacturerFullName = "";
                    cancelled = false; 

                    hardwareFullName = replacementsDictionary["$projectname$"];
                    hardwareShortName = "";
                    foreach (var c in hardwareFullName.ToCharArray())
                    {
                        if ( (c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
                        {
                            hardwareShortName += c;
                        }
                        else
                        {
                            hardwareShortName += '_';
                        }
                    }
                    if (hardwareShortName.Length == 0 || hardwareShortName[0] >= '0' && hardwareShortName[0] <= '9') hardwareShortName = "_" + hardwareShortName;

                    // show dialog
                    inputForm = new GTHardwareWizardForm(replacementsDictionary["$HardwareType$"], hardwareFullName,hardwareShortName);
                    var result = inputForm.ShowDialog();
                    if (result != DialogResult.OK)
                    {
                        cancelled = true;
                        throw new WizardCancelledException("User cancelled project creation");
                    }
                }

                if (cancelled)
                {
                    throw new WizardCancelledException();
                }

                // Add custom parameters.  We do this even if we are not in the multiproject template - and it will pick up the static data that was set 
                // when the form was run on the multi project template
                replacementsDictionary.Add("$HardwareFullName$", hardwareFullName);
                replacementsDictionary.Add("$HardwareShortName$", hardwareShortName);
                replacementsDictionary.Add("$HardwareFileSystemName$", hardwareShortName);
                replacementsDictionary.Add("$ManufacturerFullName$", manufacturerFullName);
                replacementsDictionary.Add("$ManufacturerShortName$", manufacturerShortName);

                replacementsDictionary.Add("$Netmf41XmlPrefix$", netmf41Support ? xmlTagStart : xmlCommentStart + xmlTagStart);
                replacementsDictionary.Add("$Netmf41XmlSuffix$", netmf41Support ? xmlTagEnd : xmlTagEnd + xmlCommentEnd);
                replacementsDictionary.Add("$Netmf42XmlPrefix$", netmf42Support ? xmlTagStart : xmlCommentStart + xmlTagStart);
                replacementsDictionary.Add("$Netmf42XmlSuffix$", netmf42Support ? xmlTagEnd : xmlTagEnd + xmlCommentEnd);

                replacementsDictionary.Add("$WizardGuid41$", Guid.NewGuid().ToString());
                replacementsDictionary.Add("$WizardGuid41be$", Guid.NewGuid().ToString());
                replacementsDictionary.Add("$WizardGuid41le$", Guid.NewGuid().ToString());
                replacementsDictionary.Add("$WizardGuid42$", Guid.NewGuid().ToString());
                replacementsDictionary.Add("$WizardGuid42be$", Guid.NewGuid().ToString());
                replacementsDictionary.Add("$WizardGuid42le$", Guid.NewGuid().ToString());
            }
            catch (Exception ex)
            {
                if (ex is WizardCancelledException)
                {
                    throw ex;
                }
                else
                {
                    MessageBox.Show("Exception in template wizard:" + ex.ToString());
                }
            }

        }

        public bool ShouldAddProjectItem(string filePath)
        {
            return true;
        }
    }
}
