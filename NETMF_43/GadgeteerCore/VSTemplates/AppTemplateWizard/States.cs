// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Navigation;

namespace Microsoft.Gadgeteer.AppTemplateWizard
{
    internal enum ErrorState
    {
        None,
        ErrorNotInstalledRunning10,
        ErrorNotInstalledRunning10Having11,
        ErrorNotInstalledRunning10Having12,
        ErrorNotInstalledRunning10Having14,
        ErrorNotInstalledRunning11,
        ErrorNotInstalledRunning11Having12,
        ErrorNotInstalledRunning11Having14,
        ErrorNotInstalledRunning12,
        ErrorNotInstalledRunning12Having14,
        ErrorNotInstalledRunning14,    
        ErrorNotInstalledRunningUnknown,
        ErrorNotPresentRunning10,
        ErrorNotPresentRunning10Having11,
        ErrorNotPresentRunning10Having12,
        ErrorNotPresentRunning10Having14,
        ErrorNotPresentRunning11,
        ErrorNotPresentRunning11Having12,
        ErrorNotPresentRunning11Having14,
        ErrorNotPresentRunning12,
        ErrorNotPresentRunning12Having14,
        ErrorNotPresentRunning14,
        ErrorNotPresentRunningUnknown,
        ErrorNotPresentInEdition,
    }

    internal static class States
    {
        private const string CoreAssemblyFileName = "Gadgeteer.dll";
        private static readonly Dictionary<string, Version> CoreVersions = new Dictionary<string, Version>();

        private static readonly Version VisualStudio11 = new Version(11, 0);
        private static readonly Version VisualStudio12 = new Version(12, 0);
        private static readonly Version VisualStudio14 = new Version(14, 0);

        internal static ErrorState GetEnvironmentErrorState(Version runningVersion, string projectType, string runningRootPath, string runningRegistryKey)
        {
            if (MicroFramework.IsInstalled)
            {
                if (MicroFramework.IsInVisualStudioEditionInstalled(runningRegistryKey))
                    return ErrorState.None;

                if (MicroFramework.IsInAnyVisualStudioEditionInstalled(runningVersion))
                    return ErrorState.ErrorNotPresentInEdition;

                if (runningVersion.Major == 10)
                {
                    if (IsVisualStudioInstalled(VisualStudio14))
                        return ErrorState.ErrorNotPresentRunning10Having14;

                    if (IsVisualStudioInstalled(VisualStudio12))
                        return ErrorState.ErrorNotPresentRunning10Having12;

                    if (IsVisualStudioInstalled(VisualStudio11))
                        return ErrorState.ErrorNotPresentRunning10Having11;

                    return ErrorState.ErrorNotPresentRunning10;
                }
                else if (runningVersion.Major == 11)
                {
                    if (IsVisualStudioInstalled(VisualStudio14))
                        return ErrorState.ErrorNotPresentRunning11Having14;

                    if (IsVisualStudioInstalled(VisualStudio12))
                        return ErrorState.ErrorNotPresentRunning11Having12;

                    return ErrorState.ErrorNotPresentRunning11;
                }
                else if (runningVersion.Major == 12)
                {
                    if (IsVisualStudioInstalled(VisualStudio14))
                        return ErrorState.ErrorNotPresentRunning12Having14;

                    return ErrorState.ErrorNotPresentRunning12;
                }
                else if (runningVersion.Major == 14)
                    return ErrorState.ErrorNotPresentRunning14;
                else
                    return ErrorState.ErrorNotPresentRunningUnknown;
            }
            else
            {
                if (runningVersion.Major == 10)
                {
                    if (IsVisualStudioInstalled(VisualStudio14))
                        return ErrorState.ErrorNotInstalledRunning10Having14;

                    if (IsVisualStudioInstalled(VisualStudio12))
                        return ErrorState.ErrorNotInstalledRunning10Having12;

                    if (IsVisualStudioInstalled(VisualStudio11))
                        return ErrorState.ErrorNotInstalledRunning10Having11;

                    return ErrorState.ErrorNotInstalledRunning10;
                }
                else if (runningVersion.Major == 11)
                {
                    if (IsVisualStudioInstalled(VisualStudio14))
                        return ErrorState.ErrorNotInstalledRunning11Having14;

                    if (IsVisualStudioInstalled(VisualStudio12))
                        return ErrorState.ErrorNotInstalledRunning11Having12;

                    return ErrorState.ErrorNotInstalledRunning11;
                }
                else if (runningVersion.Major == 12)
                {
                    if (IsVisualStudioInstalled(VisualStudio14))
                        return ErrorState.ErrorNotInstalledRunning12Having14;

                    return ErrorState.ErrorNotInstalledRunning12;
                }
                else if (runningVersion.Major == 14)
                    return ErrorState.ErrorNotInstalledRunning14;
                else
                    return ErrorState.ErrorNotInstalledRunningUnknown;
            }
        }
        private static bool IsVisualStudioInstalled(Version version)
        {
            RegistryKey sxs = RegistryHelper.OpenRegistryKeyOrNull(RegistryHive.LocalMachine, RegistryView.Registry32, @"SOFTWARE\Wow6432Node\Microsoft\VisualStudio\SxS\VS7");
            if (sxs == null)
                return false;

            string rootPath;
            return RegistryHelper.TryGetValue(sxs, out rootPath, version.ToString(2)) && !string.IsNullOrEmpty(rootPath);
        }

        internal static InlineMessage InlineStateFor(Mainboard board, ref string selectedVersion, string projectType)
        {
            if (board == null)
                return null;

            if (board.HasDefinitionErrors)
                return CreateInvalidXmlState(board);

            if (board.SupportedVersions.Length < 1)
                return CreateInvalidXmlState(board);

            if (board.SupportedVersions.Length == 1)
                selectedVersion = board.SupportedVersions[0];
            else if (selectedVersion == null)
                SelectVersionFor(board, ref selectedVersion, projectType);

            return InlineStateForVersion(board, selectedVersion, projectType);
        }
        private static InlineMessage InlineStateForVersion(Mainboard board, string selectedVersion, string projectType)
        {
            bool multipleChoices = board.SupportedVersions.Length > 1;

            if (board.AvailableVersions.Contains(selectedVersion))
            {
                if (MeetsMinimumGadgeteerCoreVersion(board, selectedVersion))
                {
                    if (MeetsProjectType(selectedVersion, projectType))
                    {
                        return CreateVersionInstalledState(board, selectedVersion, multipleChoices);
                    }
                    else
                        return CreateProjectTypeMismatchState(selectedVersion, projectType, multipleChoices);
                }
                else
                    return CreateCoreRequiredState(board);
            }
            else
                return CreateVersionNotInstalledState(board, selectedVersion, multipleChoices);
        }

        internal static InlineMessage InlineStateForNoBoards()
        {
            return new InlineMessage(InlineSeverity.Warning, "No mainboards were found on this system. Please install a SDK from the manufacturer of your mainboard.");
        }

        private static void SelectVersionFor(Mainboard board, ref string selectedVersion, string projectType)
        {
            if (board.Type == Gadgeteer.LastUsedMainboardType)
            {
                string lastUsedVersion = Gadgeteer.GetLastUsedMicroFrameworkVersionForMainboard(board.Type);
                if (board.SupportedVersions.Contains(lastUsedVersion))
                {
                    selectedVersion = lastUsedVersion;
                    return;
                }
            }

            if (board.AvailableVersions.Length < 1)
            {
                selectedVersion = board.SupportedVersions.FirstOrDefault();
            }
            else
            {
                foreach (string availableVersion in board.AvailableVersions)
                    if (MeetsProjectType(availableVersion, projectType) &&
                        MeetsMinimumGadgeteerCoreVersion(board, availableVersion))
                    {
                        selectedVersion = availableVersion;
                        return;
                    }

                selectedVersion = board.AvailableVersions.FirstOrDefault();
            }
        }
        private static bool MeetsMinimumGadgeteerCoreVersion(Mainboard board, string selectedVersion)
        {
            Version minimumCoreVersion;
            if (!Version.TryParse(board.MinimumCoreVersion, out minimumCoreVersion))
                return false;

            Version coreVersion;
            if (!CoreVersions.TryGetValue(selectedVersion, out coreVersion))
            {
                string path = Gadgeteer.GetAssembliesPath("v" + selectedVersion);
                if (path == null)
                    coreVersion = new Version();
                else
                {
                    path = Path.Combine(path, CoreAssemblyFileName);

                    try
                    {
                        FileVersionInfo coreInfo = FileVersionInfo.GetVersionInfo(path);
                        coreVersion = new Version(coreInfo.FileVersion);
                    }
                    catch { coreVersion = new Version(); }
                }

                CoreVersions[selectedVersion] = coreVersion;
            }

            return coreVersion >= minimumCoreVersion;
        }
        private static bool MeetsProjectType(string selectedVersion, string projectType)
        {
            return !(selectedVersion == "4.1" && projectType == "VisualBasic");
        }

        private static InlineMessage CreateInvalidXmlState(Mainboard board)
        {
            if (IsLink(board.HelpUrl))
                return new InlineMessage(InlineSeverity.Error, "This mainboard’s software has errors preventing it from being used by .NET Gadgeteer. Check the ", Link("manufacturer-provided help", board.HelpUrl), " or contact the manufacturer.");
            else
                return new InlineMessage(InlineSeverity.Error, "This mainboard’s software has errors preventing it from being used by .NET Gadgeteer. Please contact the manufacturer.");
        }
        private static InlineMessage CreateVersionInstalledState(Mainboard board, string version, bool multipleChoices)
        {
            if (multipleChoices)
            {
                return null; // go ahead without warning
            }
            else
            {
                if (IsLink(board.HelpUrl))
                    return new InlineMessage(InlineSeverity.Information, "The project will target .NET Micro Framework ", version, ". If you expect a different version, check the ", Link("manufacturer-provided help", board.HelpUrl), " or contact the manufacturer.");
                else
                    return new InlineMessage(InlineSeverity.Information, "The project will target .NET Micro Framework ", version, ". If you expect a different version, contact the manufacturer.");
            }
        }
        private static InlineMessage CreateVersionNotInstalledState(Mainboard board, string version, bool multipleChoices)
        {
            if (multipleChoices)
            {
                return new InlineMessage(InlineSeverity.Error, "This .NET Micro Framework version is not installed. You can download it at ", Link("http://netmf.codeplex.com/", "http://netmf.codeplex.com/releases/"), ".");
            }
            else
            {
                return new InlineMessage(InlineSeverity.Error, "This mainboard supports .NET Micro Framework ", version, " only, which is not installed. You can download it at ", Link("http://netmf.codeplex.com/", "http://netmf.codeplex.com/releases/"), ".");
            }
        }
        internal static InlineMessage CreateVersionNotRegisteredState(Mainboard board, string version, bool multipleChoices, Hyperlink details)
        {
            if (multipleChoices)
            {
                if (IsLink(board.HelpUrl))
                    return new InlineMessage(InlineSeverity.Error, "This mainboard was not installed correctly into .NET Micro Framework ", version, ". Check the ", Link("manufacturer-provided help", board.HelpUrl), " or contact the manufacturer. ", details);
                else
                    return new InlineMessage(InlineSeverity.Error, "This mainboard was not installed correctly into .NET Micro Framework ", version, ". Please contact the manufacturer. ", details);
            }
            else
            {
                if (IsLink(board.HelpUrl))
                    return new InlineMessage(InlineSeverity.Error, "This mainboard was not installed correctly. Check the ", Link("manufacturer-provided help", board.HelpUrl), " or contact the manufacturer. ", details);
                else
                    return new InlineMessage(InlineSeverity.Error, "This mainboard was not installed correctly. Please contact the manufacturer. ", details);
            }
        }
        private static InlineMessage CreateProjectTypeMismatchState(string version, string projectType, bool multipleChoices)
        {
            if (multipleChoices)
            {
                return new InlineMessage(InlineSeverity.Error, "This .NET Micro Framework version does not support Visual Basic.");
            }
            else
            {
                return new InlineMessage(InlineSeverity.Error, "This mainboard supports .NET Micro Framework 4.1 only, which does not support Visual Basic.");
            }
        }
        private static InlineMessage CreateCoreRequiredState(Mainboard board)
        {
            return new InlineMessage(InlineSeverity.Error, "This mainboard requires .NET Gadgeteer Core version ", board.MinimumCoreVersion, " or newer. Download the latest version at ", Link("http://gadgeteer.codeplex.com/", "http://gadgeteer.codeplex.com/releases"), ".");
        }

        private static Inline LineBreak { get { return new LineBreak(); } }
        private static Inline Link(string text, string uri = null)
        {
            if (uri == null)
                uri = text;

            Hyperlink hyperlink = new Hyperlink(new Run(text));
            hyperlink.NavigateUri = new Uri(uri, UriKind.Absolute);
            hyperlink.ToolTip = hyperlink.NavigateUri.AbsoluteUri;
            hyperlink.RequestNavigate += OpenLink;

            return hyperlink;
        }
        private static bool IsLink(string uri)
        {
            Uri u; return Uri.TryCreate(uri, UriKind.Absolute, out u);
        }
        private static void OpenLink(object sender, RequestNavigateEventArgs e)
        {
            try { Process.Start(e.Uri.OriginalString); }
            catch (Exception ex) { MessageBox.Show("The link could not be opened. " + ex.Message, ".NET Gadgeteer Application Wizard", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
    }
}
