// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using EnvDTE;
using Microsoft.VisualStudio.TemplateWizard;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Microsoft.Gadgeteer.AppTemplateWizard
{
    internal class Wizard : IWizard
    {
        private const string RootPathKey = "$installpath$";
        private static readonly char[] Space = new char[] { ' ' };

        private static readonly Dictionary<string, string[]> AdditionalReferencesByVersion = new Dictionary<string, string[]> 
        {
            { "4.1", new[] { "Gadgeteer.WebServer", "Gadgeteer.WebClient" } }
        };

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            _DTE dte = automationObject as _DTE;

            replacementsDictionary["$References$"] = string.Empty;

            string runningRootPath = null;
            replacementsDictionary.TryGetValue("$installpath$", out runningRootPath);

            string projectType = null;
            replacementsDictionary.TryGetValue("$ProjectType$", out projectType);

            Version runningVersion = new Version();
            if (dte.Version != null)
            {
                string dteVersion = dte.Version.Split(Space, 2)[0];
                Version.TryParse(dteVersion, out runningVersion);
            }

            ErrorState state = States.GetEnvironmentErrorState(runningVersion, projectType, runningRootPath, dte.RegistryRoot);

            if (state == ErrorState.None)
            {
                MainboardWindow window = new MainboardWindow();
                window.ProjectType = projectType;
                window.OwnerHandle = new IntPtr(dte.MainWindow.HWnd);

                if (window.ShowDialog() != true)
                    throw new WizardCancelledException();

                if (window.SelectedVersion == null)
                    throw new WizardCancelledException();

                replacementsDictionary["$TargetFrameworkVersion$"] = "v" + window.SelectedVersion;

                string[] additionalReferences;
                if (AdditionalReferencesByVersion.TryGetValue(window.SelectedVersion, out additionalReferences))
                    replacementsDictionary["$References$"] = 
                        string.Concat(from r in additionalReferences
                                      select string.Format("{0}    <Reference Include=\"{1}\" />", Environment.NewLine, r));
            }
            else
            {
                ErrorWindow window = new ErrorWindow(state);
                window.OwnerHandle = new IntPtr(dte.MainWindow.HWnd);

                window.ShowDialog();
                throw new WizardCancelledException();
            }
        }

        public void BeforeOpeningFile(ProjectItem projectItem)
        {

        }

        public void ProjectFinishedGenerating(Project project)
        {

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
