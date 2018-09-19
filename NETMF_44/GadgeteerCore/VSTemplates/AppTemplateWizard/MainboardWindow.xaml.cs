// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using System.Xml.Linq;
using System.Xml.Serialization;
using GadgeteerDefinitions = Microsoft.Gadgeteer.Designer.Definitions.GadgeteerDefinitions;

namespace Microsoft.Gadgeteer.AppTemplateWizard
{
    public partial class MainboardWindow
    {
        private const double MainboardImageMinimumSize = 32;
        private const double MainboardImageMaximumSize = 512;
        public string ProjectType;

        internal static readonly DependencyProperty MainboardImageSizeProperty = DependencyProperty.Register("MainboardImageSize", typeof(double), typeof(MainboardWindow), new PropertyMetadata(128.0, null, OnMainboardImageSizeCoerce));
        internal static readonly DependencyProperty SelectedMainboardProperty = DependencyProperty.Register("SelectedMainboard", typeof(Mainboard), typeof(MainboardWindow));
        internal static readonly DependencyProperty SelectedVersionProperty = DependencyProperty.Register("SelectedVersion", typeof(string), typeof(MainboardWindow));
        internal static readonly DependencyProperty InlineMessageProperty = DependencyProperty.Register("InlineMessage", typeof(InlineMessage), typeof(MainboardWindow));
        internal static readonly DependencyProperty IsWizardValidProperty = DependencyProperty.Register("IsWizardValid", typeof(bool), typeof(MainboardWindow));

        internal double MainboardImageSize
        {
            get { return (double)GetValue(MainboardImageSizeProperty); }
            set { SetValue(MainboardImageSizeProperty, value); }
        }
        internal Mainboard SelectedMainboard
        {
            get { return (Mainboard)GetValue(SelectedMainboardProperty); }
            set { SetValue(SelectedMainboardProperty, value); }
        }
        internal string SelectedVersion
        {
            get { return (string)GetValue(SelectedVersionProperty); }
            set { SetValue(SelectedVersionProperty, value); }
        }
        internal InlineMessage InlineMessage
        {
            get { return (InlineMessage)GetValue(InlineMessageProperty); }
            set { SetValue(InlineMessageProperty, value); }
        }
        internal bool IsWizardValid
        {
            get { return (bool)GetValue(IsWizardValidProperty); }
            set { SetValue(IsWizardValidProperty, value); }
        }

        private static object OnMainboardImageSizeCoerce(DependencyObject d, object baseValue)
        {
            double value = (double)baseValue;

            if (value < MainboardImageMinimumSize)
                return MainboardImageMinimumSize;

            if (value > MainboardImageMaximumSize)
                return MainboardImageMaximumSize;

            return value;
        }

        private ObservableCollection<Mainboard> _mainboards;
        private string _lastUsedMainboardType;
        private volatile bool _mainboardsEnumerated;

        public MainboardWindow()
        {
            _lastUsedMainboardType = Gadgeteer.LastUsedMainboardType;
            _mainboards = new ObservableCollection<Mainboard>();

            InitializeComponent();

            ICollectionView view = CollectionViewSource.GetDefaultView(_mainboards);

            if (view.CanSort)
                view.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));

            // if (view.CanGroup)
            //     view.GroupDescriptions.Add(new PropertyGroupDescription("Manufacturer"));

            BoardSelector.ItemsSource = view;
        }

        private bool _shownAsDialog;
        public new bool? ShowDialog()
        {
            _shownAsDialog = true;
            bool? dialogResult = base.ShowDialog();

            _shownAsDialog = false;
            return dialogResult;
        }
        internal IntPtr OwnerHandle
        {
            set { new WindowInteropHelper(this).Owner = value; }
        }

        private void OnCreate(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SelectedMainboard == null)
                    return;

                Gadgeteer.SetLastUsedMainboard(SelectedMainboard.Type, SelectedVersion);
            }
            catch { }

            if (_shownAsDialog)
                DialogResult = true;
            else
                Close();
        }
        private void OnCancel(object sender, RoutedEventArgs e)
        {
            if (_shownAsDialog)
                DialogResult = false;
            else
                Close();
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            BoardSelector.Focus();

            // we do not want to cache the assembliy list
            // because user could have un/installed SDK without restarting Visual Studio
            ThreadPool.QueueUserWorkItem(RegisteredAssembliesEnumerator);

            string[] folderPaths = Gadgeteer.EnumerateHardwareDefinitionFolders(new MainboardNamingFirstComparer()).ToArray();

            if (folderPaths != null && folderPaths.Length > 0)
            {
                BoardProgress.Maximum = folderPaths.Length;
                BoardProgress.Visibility = Visibility.Visible;

                foreach (string folderPath in folderPaths)
                    ThreadPool.QueueUserWorkItem(CheckForMainboard, folderPath);
            }
            else
                InlineMessage = States.InlineStateForNoBoards();
        }

        private void CheckForMainboard(object state)
        {
            string folderPath = state as string;
            if (folderPath == null)
                return;

            string definitionsPath = Path.Combine(folderPath, Gadgeteer.DefinitionsFile);

            try
            {
                if (File.Exists(definitionsPath))
                {
                    XDocument xml = XDocument.Load(definitionsPath);
                    XNamespace ns = Gadgeteer.DefinitionsNamespace;

                    XmlSerializer serializer = new XmlSerializer(typeof(GadgeteerDefinitions));
                    var definitions = (GadgeteerDefinitions)serializer.Deserialize(xml.CreateReader());
                    bool hasErrors = definitions.HasErrors;

                    foreach (XElement definition in xml.Root.Elements(ns + "MainboardDefinitions").Elements(ns + "MainboardDefinition"))
                        Dispatcher.BeginInvoke(new Action<Mainboard>(OnBoardFound), DispatcherPriority.Background, new Mainboard(definition, folderPath, hasErrors));
                }
            }
            catch { }
            finally { Dispatcher.BeginInvoke(new Action(OnBoardChecked), DispatcherPriority.Background); }
        }

        private void OnBoardChecked()
        {
            if (++BoardProgress.Value >= BoardProgress.Maximum)
            {
                _mainboardsEnumerated = true;

                if (_mainboards.Count < 1)
                    InlineMessage = States.InlineStateForNoBoards();

                else if (_mainboards.Count == 1 && SelectedMainboard == null)
                    SelectedMainboard = _mainboards[0];

                InvalidateProgress();
            }
        }
        private void OnBoardFound(Mainboard board)
        {
            _mainboards.Add(board);

            if (SelectedMainboard == null && board.Type == _lastUsedMainboardType)
                SelectedMainboard = board;
        }

        private void OnBoardSelectionChanged(object sender, EventArgs e)
        {
            string selectedVersion = SelectedVersion;
            InlineMessage = States.InlineStateFor(SelectedMainboard, ref selectedVersion, ProjectType);
            SelectedVersion = selectedVersion;

            if (SelectedMainboard != null)
            {
                VersionPanel.Visibility = SelectedMainboard.SupportedVersions.Length > 1 ? Visibility.Visible : Visibility.Collapsed;
            }

            CheckIfRegisteredAndInvalidate();
        }
        private void OnVersionSelectionChanged(object sender, EventArgs e)
        {
            string selectedVersion = SelectedVersion;
            InlineMessage = States.InlineStateFor(SelectedMainboard, ref selectedVersion, ProjectType);

            CheckIfRegisteredAndInvalidate();
        }
        private void InvalidateWizard(object sender, EventArgs e)
        {
            IsWizardValid =
                SelectedMainboard != null &&
                SelectedVersion != null &&
                (InlineMessage == null || InlineMessage.Severity != InlineSeverity.Error) &&
                MeetsAssemblyRegistration(SelectedMainboard, SelectedVersion) == true;
        }
        private void InvalidateProgress()
        {
            if (!_mainboardsEnumerated)
                return;

            if (!Dispatcher.CheckAccess())
                Dispatcher.Invoke(new Action(InvalidateProgress));
            else
            {
                if (_registeredAssembliesEnumerated)
                    BoardProgress.Visibility = Visibility.Collapsed;
                else
                {
                    BoardProgress.IsIndeterminate = true;
                    BoardProgress.Visibility = Visibility.Visible;
                }
            }
        }

        private void OnBoardWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                e.Handled = true;

                MainboardImageSize += e.Delta;
            }
        }
        private void OnBoardDoubleClick(object sender, RoutedEventArgs e)
        {
            if (IsWizardValid)
                OnCreate(sender, e);
        }

        private ConcurrentDictionary<string, List<string>> _registeredAssemblies = new ConcurrentDictionary<string, List<string>>();
        private volatile bool _registeredAssembliesEnumerated;

        private void RegisteredAssembliesEnumerator(object state)
        {
            foreach (string version in MicroFramework.InstalledVersions)
            {
                List<string> registeredAssemblies = new List<string>(MicroFramework.GetRegisteredAssemblyNames(version));
                registeredAssemblies.Sort();

                _registeredAssemblies.TryAdd(version, registeredAssemblies);
            }

            _registeredAssembliesEnumerated = true;
            CheckIfRegisteredAndInvalidate();
            InvalidateProgress();            
        }
        private void CheckIfRegisteredAndInvalidate()
        {
            if (!Dispatcher.CheckAccess())
                Dispatcher.Invoke(new Action(CheckIfRegisteredAndInvalidate));
            else
            {
                if (MeetsAssemblyRegistration(SelectedMainboard, SelectedVersion) == false)
                    InlineMessage = States.CreateVersionNotRegisteredState(SelectedMainboard, SelectedVersion, SelectedMainboard.SupportedVersions.Length > 1, CreateDetailsLink());

                InvalidateWizard(null, EventArgs.Empty);
            }
        }
        private bool? MeetsAssemblyRegistration(Mainboard board, string selectedVersion)
        {
            if (_registeredAssemblies == null)
                return null;

            List<string> assemblies;
            if (!_registeredAssemblies.TryGetValue("v" + selectedVersion, out assemblies))
                return _registeredAssembliesEnumerated ? true : (bool?)null;

            return board.Assemblies.Where(a => a.Version == selectedVersion).All(a => assemblies.BinarySearch(a.Name, StringComparer.OrdinalIgnoreCase) >= 0);
        }

        private Hyperlink CreateDetailsLink()
        {
            Hyperlink link = new Hyperlink(new Run("Details"));
            link.Click += delegate
            {
                try
                {
                    string path = Path.Combine(Path.GetTempPath(), "appwizlog.txt");

                    using (StreamWriter log = File.CreateText(path))
                    {
                        log.WriteLine(".NET Gadgeteer Application Wizard Registered Assemblies Report");
                        log.WriteLine("--------------------------------------------------------------");
                        log.WriteLine();
                        log.WriteLine("Selected mainboard: {0}", SelectedMainboard.Name);
                        log.WriteLine("Selected version: {0}", SelectedVersion);
                        log.WriteLine();
                        log.WriteLine("Assemblies required by the selected mainboard:");
                        foreach (var boardAssembly in SelectedMainboard.Assemblies)
                            log.WriteLine("   v{0}: {1}", boardAssembly.Version, boardAssembly.Name);
                        log.WriteLine();
                        log.WriteLine("Registered assemblies by version:");
                        foreach (var registeredAssemblies in _registeredAssemblies)
                        {
                            log.WriteLine("   {0}:", registeredAssemblies.Key);
                            foreach (var registeredAssembly in registeredAssemblies.Value)
                                log.WriteLine("   {0}", registeredAssembly);
                            
                            log.WriteLine();
                        }
                    }

                    Process.Start(path);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("The details could not be displayed. " + ex.Message, ".NET Gadgeteer Application Wizard", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            return link;
        }
    }    
}