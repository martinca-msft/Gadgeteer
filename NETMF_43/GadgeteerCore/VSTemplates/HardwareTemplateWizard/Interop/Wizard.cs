// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.TemplateWizard;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Debug = System.Diagnostics.Debug;
using System;

namespace Microsoft.Gadgeteer.HardwareTemplateWizard
{
    internal class Wizard : IWizard
    {
        private const string xmlTagStart = "<";
        private const string xmlTagEnd = ">";
        private const string xmlCommentStart = "<!-- ";
        private const string xmlCommentEnd = " -->";

        private string _solutionName;
        private string _solutionSafeName;

        private static Dictionary<string, string> GlobalReplacementsDictionary;

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            if (runKind == WizardRunKind.AsMultiProject)
            {
                GlobalReplacementsDictionary = new Dictionary<string, string>();

                _DTE dte = automationObject as _DTE;
                replacementsDictionary.TryGetValue("$projectname$", out _solutionName);
                replacementsDictionary.TryGetValue("$safeprojectname$", out _solutionSafeName);

                TemplateType type = TemplateType.Module;
                if (replacementsDictionary.ContainsKey("$HardwareType$"))
                    Enum.TryParse(replacementsDictionary["$HardwareType$"], out type);

                WizardWindow window = new WizardWindow(type);
                window.ProjectName = _solutionName;
                window.ProjectSafeName = _solutionSafeName;

                if (window.ShowDialog() != true)
                    throw new WizardCancelledException();

                GlobalReplacementsDictionary.Add("$HardwareFullName$", window.ProjectName);
                GlobalReplacementsDictionary.Add("$HardwareShortName$", window.ProjectSafeName);
                GlobalReplacementsDictionary.Add("$HardwareFileSystemName$", window.ProjectSafeName);
                GlobalReplacementsDictionary.Add("$ManufacturerFullName$", window.ManufacturerName);
                GlobalReplacementsDictionary.Add("$ManufacturerShortName$", window.ManufacturerSafeName);

                GlobalReplacementsDictionary.Add("$Netmf41XmlPrefix$", window.SupportsNETMF41 ? xmlTagStart : xmlCommentStart + xmlTagStart);
                GlobalReplacementsDictionary.Add("$Netmf41XmlSuffix$", window.SupportsNETMF41 ? xmlTagEnd : xmlTagEnd + xmlCommentEnd);
                GlobalReplacementsDictionary.Add("$Netmf42XmlPrefix$", window.SupportsNETMF42 ? xmlTagStart : xmlCommentStart + xmlTagStart);
                GlobalReplacementsDictionary.Add("$Netmf42XmlSuffix$", window.SupportsNETMF42 ? xmlTagEnd : xmlTagEnd + xmlCommentEnd);
                GlobalReplacementsDictionary.Add("$Netmf43XmlPrefix$", window.SupportsNETMF43 ? xmlTagStart : xmlCommentStart + xmlTagStart);
                GlobalReplacementsDictionary.Add("$Netmf43XmlSuffix$", window.SupportsNETMF43 ? xmlTagEnd : xmlTagEnd + xmlCommentEnd);

                GlobalReplacementsDictionary.Add("$WizardGuid41$", Guid.NewGuid().ToString());
                GlobalReplacementsDictionary.Add("$WizardGuid41be$", Guid.NewGuid().ToString());
                GlobalReplacementsDictionary.Add("$WizardGuid41le$", Guid.NewGuid().ToString());
                GlobalReplacementsDictionary.Add("$WizardGuid42$", Guid.NewGuid().ToString());
                GlobalReplacementsDictionary.Add("$WizardGuid42be$", Guid.NewGuid().ToString());
                GlobalReplacementsDictionary.Add("$WizardGuid42le$", Guid.NewGuid().ToString());
                GlobalReplacementsDictionary.Add("$WizardGuid43$", Guid.NewGuid().ToString());
                GlobalReplacementsDictionary.Add("$WizardGuid43be$", Guid.NewGuid().ToString());
                GlobalReplacementsDictionary.Add("$WizardGuid43le$", Guid.NewGuid().ToString());

                if (window.HardwareImage != null)
                    GlobalReplacementsDictionary.Add("$HardwareImagePath$", window.HardwareImagePath);

                GlobalReplacementsDictionary.Add("$SocketsXml$", window.GetSocketsXml());
                GlobalReplacementsDictionary.Add("$ProvidedSocketsXml$", window.GetProvidedSocketsXml());
                GlobalReplacementsDictionary.Add("$PowerXml$", window.GetPowerXml());
            }

            if (GlobalReplacementsDictionary != null)
                foreach (KeyValuePair<string, string> replacementItem in GlobalReplacementsDictionary)
                    replacementsDictionary.Add(replacementItem.Key, replacementItem.Value);
        }

        public void BeforeOpeningFile(ProjectItem projectItem)
        {

        }

        public void ProjectFinishedGenerating(Project project)
        {
            if (project != null && project.FileName != null)
            {
                if (GlobalReplacementsDictionary != null && 
                    GlobalReplacementsDictionary.ContainsKey("$HardwareFileSystemName$") && 
                    GlobalReplacementsDictionary.ContainsKey("$HardwareImagePath$"))
                {
                    if (project.Name == GlobalReplacementsDictionary["$HardwareFileSystemName$"])
                    {
                        string projectDirectory = Path.GetDirectoryName(project.FileName);

                        try { File.Copy(GlobalReplacementsDictionary["$HardwareImagePath$"], Path.Combine(projectDirectory, "Image.jpg"), true); }
                        catch (Exception e) { System.Windows.MessageBox.Show("An error occured when trying to copy the hardware image: " +e.Message); }
                    }
                }
            }
        }

        public void ProjectItemFinishedGenerating(ProjectItem projectItem)
        {
            
        }

        public void RunFinished()
        {

        }

        public bool ShouldAddProjectItem(string filePath)
        {
            return true;
        }
    }
}
