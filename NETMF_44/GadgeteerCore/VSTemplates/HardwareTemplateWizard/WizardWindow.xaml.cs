// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Microsoft.Win32;

namespace Microsoft.Gadgeteer.HardwareTemplateWizard
{
    public partial class WizardWindow
    {
        static readonly char[] SupportedSocketTypes = { 'X', 'Y', 'A', 'C', 'D', 'E', 'F', 'H', 'I', 'K', 'O', 'P', 'S', 'T', 'U', 'R', 'G', 'B', 'Z', '*' };

        public WizardWindow(TemplateType type)
        {
            TemplateType = type;

            InitializeSockets();
            InitializeProvidedSockets();
            InitializePower();
            InitializeComponent();

            Loaded += OnLoaded;
            SocketsGrid.SelectedCellsChanged += OnSocketsSelectionChanged;
            ProvidedSocketsGrid.SelectedCellsChanged += OnProvidedSocketsSelectionChanged;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            string name = Settings.GetLastManufacturerName();

            if (name != null)
                SetValue(ManufacturerNameProperty, name);

            name = Settings.GetLastManufacturerSafeName();

            if (name != null)
                SetValue(ManufacturerSafeNameProperty, name);

            ProjectNameBox.Focus();
        }

        #region Dialog Behaviour

        private bool _shownAsDialog;

        public new bool? ShowDialog()
        {
            _shownAsDialog = true;
            bool? dialogResult = base.ShowDialog();

            _shownAsDialog = false;
            return dialogResult;
        }

        private void OnCreate(object sender, RoutedEventArgs e)
        {
            try
            {
                Settings.Default.LastManufacturerName = ManufacturerName;
                Settings.Default.LastManufacturerSafeName = ManufacturerSafeName;
                Settings.Default.Save();
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

        #endregion

        #region User Interface

        protected override void OnSourceInitialized(EventArgs e)
        {
            if (NativeMethods.IsDwmIsCompositionEnabled)
            {
                // Technically this can change during runtime, which we are currently not handling.

                IntPtr hWnd = new WindowInteropHelper(this).Handle;
                if (hWnd != IntPtr.Zero)
                {
                    HwndSource window = HwndSource.FromHwnd(hWnd);
                    window.CompositionTarget.BackgroundColor = Colors.Transparent;
                    Background = Brushes.Transparent;

                    NativeMethods.DwmExtendFrameIntoClientArea(hWnd, 0, 0, (int)HeaderPanel.ActualHeight, 0);
                }
            }
        }

        private void DragWindow(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            DragMove();
        }

        #endregion

        #region Validation

        private static readonly DependencyPropertyKey IsInputValidPropertyKey = DependencyProperty.RegisterReadOnly("IsInputValid", typeof(bool), typeof(WizardWindow), new PropertyMetadata(false));
        public static readonly DependencyProperty IsInputValidProperty = IsInputValidPropertyKey.DependencyProperty;

        public bool IsInputValid
        {
            get { return (bool)GetValue(IsInputValidProperty); }
            private set { SetValue(IsInputValidPropertyKey, value); }
        }

        public void Validate()
        {
            IsInputValid =
                SafeCheck.IsSafe(ProjectSafeNameBox.Text) &&
                SafeCheck.IsSafe(ManufacturerSafeNameBox.Text);
        }

        protected void InvalidateWizard(object sender, EventArgs e)
        {
            Validate();
        }

        #endregion

        #region About Tab

        public static readonly DependencyProperty TemplateTypeProperty = DependencyProperty.Register("TemplateType", typeof(TemplateType), typeof(WizardWindow), new PropertyMetadata(TemplateType.Module));

        public static readonly DependencyProperty ProjectNameProperty = DependencyProperty.Register("ProjectName", typeof(string), typeof(WizardWindow));
        public static readonly DependencyProperty ProjectSafeNameProperty = DependencyProperty.Register("ProjectSafeName", typeof(string), typeof(WizardWindow));
        public static readonly DependencyProperty ManufacturerNameProperty = DependencyProperty.Register("ManufacturerName", typeof(string), typeof(WizardWindow));
        public static readonly DependencyProperty ManufacturerSafeNameProperty = DependencyProperty.Register("ManufacturerSafeName", typeof(string), typeof(WizardWindow));

        public TemplateType TemplateType
        {
            get { return (TemplateType)GetValue(TemplateTypeProperty); }
            set { SetValue(TemplateTypeProperty, value); }
        }

        public string ProjectName
        {
            get { return (string)GetValue(ProjectNameProperty); }
            set { SetValue(ProjectNameProperty, value); }
        }
        public string ProjectSafeName
        {
            get { return (string)GetValue(ProjectSafeNameProperty); }
            set { SetValue(ProjectSafeNameProperty, value); }
        }
        public string ManufacturerName
        {
            get { return (string)GetValue(ManufacturerNameProperty); }
            set { SetValue(ManufacturerNameProperty, value); }
        }
        public string ManufacturerSafeName
        {
            get { return (string)GetValue(ManufacturerSafeNameProperty); }
            set { SetValue(ManufacturerSafeNameProperty, value); }
        }

        public bool SupportsNETMF41 { get { return NETMF41.IsChecked == true; } }
        public bool SupportsNETMF42 { get { return NETMF42.IsChecked == true; } }
        public bool SupportsNETMF43 { get { return NETMF43.IsChecked == true; } }
        public bool SupportsNETMF44 { get { return NETMF44.IsChecked == true; } }

        #endregion

        #region Designer

        public static readonly DependencyProperty HardwareWidthProperty = DependencyProperty.Register("HardwareWidth", typeof(double), typeof(WizardWindow), new PropertyMetadata(40.0));
        public static readonly DependencyProperty HardwareHeightProperty = DependencyProperty.Register("HardwareHeight", typeof(double), typeof(WizardWindow), new PropertyMetadata(30.0));
        public static readonly DependencyProperty HardwareImageProperty = DependencyProperty.Register("HardwareImage", typeof(ImageSource), typeof(WizardWindow));

        public double HardwareWidth
        {
            get { return (double)GetValue(HardwareWidthProperty); }
            set { SetValue(HardwareWidthProperty, value); }
        }
        public double HardwareHeight
        {
            get { return (double)GetValue(HardwareHeightProperty); }
            set { SetValue(HardwareHeightProperty, value); }
        }
        public ImageSource HardwareImage
        {
            get { return (ImageSource)GetValue(HardwareImageProperty); }
            set { SetValue(HardwareImageProperty, value); }
        }

        private string _hardwareImagePath;
        public string HardwareImagePath { get { return _hardwareImagePath; } }

        private void OnUsePicture(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "JPEG images (*.jpg;*.jpeg;*.jpe;*.jfif)|*.jpg;*.jpeg;*.jpe;*.jfif|All files (*.*)|*.*";
            if (dialog.ShowDialog() == true)
                try
                {
                    using (Stream file = dialog.OpenFile())
                        HardwareImage = new JpegBitmapDecoder(file, BitmapCreateOptions.None, BitmapCacheOption.OnLoad).Frames[0];

                    _hardwareImagePath = dialog.FileName;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, ".NET Gadgeteer Template Wizard", MessageBoxButton.OK, MessageBoxImage.Error);
                }
        }

        #endregion

        #region Sockets Tab

        private static DependencyPropertyKey _socketSelectedColumnIndexKey = DependencyProperty.RegisterReadOnly("SocketSelectedColumnIndex", typeof(int), typeof(WizardWindow), new PropertyMetadata(-1));
        public static DependencyProperty _socketSelectedColumnIndex = _socketSelectedColumnIndexKey.DependencyProperty;

        public int SocketSelectedColumnIndex
        {
            get { return (int)GetValue(_socketSelectedColumnIndex); }
            private set { SetValue(_socketSelectedColumnIndexKey, value); }
        }

        private void OnSocketsSelectionChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (SocketsGrid == null || SocketsGrid.SelectedCells.Count < 1)
                SocketSelectedColumnIndex = -1;

            else
                SocketSelectedColumnIndex = SocketsGrid.SelectedCells[0].Column.DisplayIndex;
        }

        private bool _socketsRemoved = false;
        private ObservableCollection<SocketViewModel> _sockets;
        public ObservableCollection<SocketViewModel> Sockets { get { return _sockets; } }

        private void InitializeSockets()
        {
            _sockets = new ObservableCollection<SocketViewModel>();

            if (TemplateType == TemplateType.Module)
            {
                _sockets.Add(new SocketViewModel
                {
                    Left = 10,
                    Top = 10,
                    Notch = Dock.Top,
                    TypesLabel = "XY",
                    Pin3 = true,
                    Pin4 = true
                });

                _sockets.Add(new SocketViewModel
                {
                    Left = 30,
                    Top = 10,
                    Notch = Dock.Top,
                    TypesLabel = "S",
                    Optional = true,
                    Pin3 = true,
                    Pin4 = true,
                    Pin5 = true,
                    Pin7 = null,
                    Pin8 = null,
                    Pin9 = null
                });

                foreach (SocketViewModel socket in _sockets)
                    socket.IsDirty = false;
            }
        }

        private void OnAddSocket(object sender, RoutedEventArgs e)
        {
            _sockets.Add(new SocketViewModel());
        }
        private void OnDeleteSocket(object sender, RoutedEventArgs e)
        {
            SocketViewModel[] selectedSockets = SocketsGrid.SelectedCells.Select(c => c.Item).OfType<SocketViewModel>().ToArray();
            foreach (SocketViewModel selectedSocket in selectedSockets)
                _sockets.Remove(selectedSocket);

            _socketsRemoved = true;
        }

        public string GetSocketsXml()
        {
            if (_sockets.Count < 1 || (!_socketsRemoved && _sockets.All(s => !s.IsDirty)))
                return DefaultXml.DefaultModuleSocketsXml;

            XElement sockets = new XElement("Sockets");

            for (int i = 0; i < _sockets.Count; i++)
            {
                XElement socket = new XElement("Socket",
                    new XAttribute("Left", _sockets[i].Left),
                    new XAttribute("Top", _sockets[i].Top),
                    new XAttribute("Orientation", _sockets[i].Orientation),
                    new XAttribute("ConstructorOrder", i + 1),
                    new XAttribute("TypesLabel", _sockets[i].TypesLabel));

                if (_sockets[i].Optional)
                    socket.Add(new XAttribute("Optional", true));

                socket.Add(GenerateTypes(_sockets[i].TypesLabel)); // TODO: support composite types

                {
                    XElement pins = new XElement("Pins");

                    if (_sockets[i].Pin3 != false) pins.Add(new XElement("Pin", new XAttribute("Shared", _sockets[i].Pin3 == null), 3));
                    if (_sockets[i].Pin4 != false) pins.Add(new XElement("Pin", new XAttribute("Shared", _sockets[i].Pin4 == null), 4));
                    if (_sockets[i].Pin5 != false) pins.Add(new XElement("Pin", new XAttribute("Shared", _sockets[i].Pin5 == null), 5));
                    if (_sockets[i].Pin6 != false) pins.Add(new XElement("Pin", new XAttribute("Shared", _sockets[i].Pin6 == null), 6));
                    if (_sockets[i].Pin7 != false) pins.Add(new XElement("Pin", new XAttribute("Shared", _sockets[i].Pin7 == null), 7));
                    if (_sockets[i].Pin8 != false) pins.Add(new XElement("Pin", new XAttribute("Shared", _sockets[i].Pin8 == null), 8));
                    if (_sockets[i].Pin9 != false) pins.Add(new XElement("Pin", new XAttribute("Shared", _sockets[i].Pin9 == null), 9));

                    socket.Add(pins);
                }

                sockets.Add(socket);
            }

            return sockets.ToString().Replace("\r\n", "\r\n      ");
        }

        private static XElement GenerateTypes(string s)
        {
            XElement types = new XElement("Types");
            foreach (char c in s)
                if (Array.IndexOf(SupportedSocketTypes, c) >= 0)
                    types.Add(new XElement("Type", c));

            return types;
        }

        #endregion

        #region Provided Sockets Tab

        private static DependencyPropertyKey _providedSocketSelectedColumnIndexKey = DependencyProperty.RegisterReadOnly("ProvidedSocketSelectedColumnIndex", typeof(int), typeof(WizardWindow), new PropertyMetadata(-1));
        public static DependencyProperty _providedSocketSelectedColumnIndex = _providedSocketSelectedColumnIndexKey.DependencyProperty;

        public int ProvidedSocketSelectedColumnIndex
        {
            get { return (int)GetValue(_providedSocketSelectedColumnIndex); }
            private set { SetValue(_providedSocketSelectedColumnIndexKey, value); }
        }

        private void OnProvidedSocketsSelectionChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (ProvidedSocketsGrid == null || ProvidedSocketsGrid.SelectedCells.Count < 1)
                ProvidedSocketSelectedColumnIndex = -1;

            else
                ProvidedSocketSelectedColumnIndex = ProvidedSocketsGrid.SelectedCells[0].Column.DisplayIndex;
        }

        private bool _providedSocketsRemoved = false;
        private ObservableCollection<ProvidedSocketViewModel> _providedSockets;
        public ObservableCollection<ProvidedSocketViewModel> ProvidedSockets { get { return _providedSockets; } }

        private void InitializeProvidedSockets()
        {
            _providedSockets = new ObservableCollection<ProvidedSocketViewModel>();

            if (TemplateType == TemplateType.Mainboard)
            {
                _providedSockets.Add(new ProvidedSocketViewModel
                {
                    Left = 10,
                    Top = 20,
                    Notch = Dock.Top,
                    Label = "1",
                    Types = "AIX",
                    Net8 = "I2CSDA",
                    Net9 = "I2CSDL"
                });

                _providedSockets.Add(new ProvidedSocketViewModel
                {
                    Left = 30,
                    Top = 20,
                    Notch = Dock.Top,
                    Label = "2",
                    Types = "DI",
                    Net8 = "I2CSDA",
                    Net9 = "I2CSDL"
                });

                foreach (ProvidedSocketViewModel providedSocket in _providedSockets)
                    providedSocket.IsDirty = false;
            }
        }

        private void OnAddProvidedSocket(object sender, RoutedEventArgs e)
        {
            _providedSockets.Add(new ProvidedSocketViewModel { Label = (_providedSockets.Count + 1).ToString() });
        }
        private void OnDeleteProvidedSocket(object sender, RoutedEventArgs e)
        {
            ProvidedSocketViewModel[] selectedSockets = ProvidedSocketsGrid.SelectedCells.Select(c => c.Item).OfType<ProvidedSocketViewModel>().ToArray();
            foreach (ProvidedSocketViewModel selectedSocket in selectedSockets)
                _providedSockets.Remove(selectedSocket);

            _providedSocketsRemoved = true;
        }

        public string GetProvidedSocketsXml()
        {
            if (_providedSockets.Count < 1 || (!_providedSocketsRemoved && _providedSockets.All(s => !s.IsDirty)))
                return TemplateType == TemplateType.Mainboard ? DefaultXml.DefaultMainboardProvidedSocketsXml : DefaultXml.DefaultModuleProvidedSocketsXml;

            XElement providedSockets = new XElement("ProvidedSockets");

            for (int i = 0; i < _providedSockets.Count; i++)
            {
                XElement providedSocket = new XElement("ProvidedSocket",
                    new XAttribute("Label", _providedSockets[i].Label),
                    new XAttribute("Left", _providedSockets[i].Left),
                    new XAttribute("Top", _providedSockets[i].Top),
                    new XAttribute("Orientation", _providedSockets[i].Orientation),
                    new XAttribute("ConstructorParameter", i + 1));

                providedSocket.Add(GenerateTypes(_providedSockets[i].Types));

                {
                    XElement sharedPinMaps = new XElement("SharedPinMaps");

                    if (!string.IsNullOrWhiteSpace(_providedSockets[i].Net3)) sharedPinMaps.Add(new XElement("SharedPinMap", new XAttribute("NetId", _providedSockets[i].Net3), new XAttribute("SocketPin", 3), new XAttribute("SharedOnly", true)));
                    if (!string.IsNullOrWhiteSpace(_providedSockets[i].Net4)) sharedPinMaps.Add(new XElement("SharedPinMap", new XAttribute("NetId", _providedSockets[i].Net4), new XAttribute("SocketPin", 4), new XAttribute("SharedOnly", true)));
                    if (!string.IsNullOrWhiteSpace(_providedSockets[i].Net5)) sharedPinMaps.Add(new XElement("SharedPinMap", new XAttribute("NetId", _providedSockets[i].Net5), new XAttribute("SocketPin", 5), new XAttribute("SharedOnly", true)));
                    if (!string.IsNullOrWhiteSpace(_providedSockets[i].Net6)) sharedPinMaps.Add(new XElement("SharedPinMap", new XAttribute("NetId", _providedSockets[i].Net6), new XAttribute("SocketPin", 6), new XAttribute("SharedOnly", true)));
                    if (!string.IsNullOrWhiteSpace(_providedSockets[i].Net7)) sharedPinMaps.Add(new XElement("SharedPinMap", new XAttribute("NetId", _providedSockets[i].Net7), new XAttribute("SocketPin", 7), new XAttribute("SharedOnly", true)));
                    if (!string.IsNullOrWhiteSpace(_providedSockets[i].Net8)) sharedPinMaps.Add(new XElement("SharedPinMap", new XAttribute("NetId", _providedSockets[i].Net8), new XAttribute("SocketPin", 8), new XAttribute("SharedOnly", true)));
                    if (!string.IsNullOrWhiteSpace(_providedSockets[i].Net9)) sharedPinMaps.Add(new XElement("SharedPinMap", new XAttribute("NetId", _providedSockets[i].Net9), new XAttribute("SocketPin", 9), new XAttribute("SharedOnly", true)));

                    providedSocket.Add(sharedPinMaps);
                }

                providedSockets.Add(providedSocket);
            }

            return providedSockets.ToString().Replace("\r\n", "\r\n      ");
        }

        #endregion

        #region Power Tab

        public static readonly DependencyProperty SpecifyPowerProperty = DependencyProperty.Register("SpecifyPower", typeof(bool), typeof(WizardWindow), new PropertyMetadata(false));

        public bool SpecifyPower
        {
            get { return (bool)GetValue(SpecifyPowerProperty); }
            set { SetValue(SpecifyPowerProperty, value); }
        }

        private ObservableCollection<PowerViewModel> _power;
        public ObservableCollection<PowerViewModel> Power { get { return _power; } }

        private void InitializePower()
        {
            _power = new ObservableCollection<PowerViewModel>();

            _power.Add(new PowerViewModel { Voltage = 3.3, TypicalCurrent = 0.123, MaximumCurrent = 0.456 });
            _power.Add(new PowerViewModel { Voltage = 5.0, TypicalCurrent = 0.789, MaximumCurrent = double.PositiveInfinity });
        }

        public string GetPowerXml()
        {
            if (!SpecifyPower || _power.Count < 1)
                return DefaultXml.DefaultPowerXml;

            XElement power = new XElement("Power");

            foreach (PowerViewModel line in _power)
                power.Add(
                    new XElement("PowerRequirements", new XAttribute("Voltage", line.Voltage),
                        new XElement("TypicalCurrent", line.TypicalCurrent.ToString(CultureInfo.InvariantCulture)),
                        new XElement("MaximumCurrent", line.MaximumCurrent.ToString(CultureInfo.InvariantCulture))
                ));

            return power.ToString().Replace("\r\n", "\r\n      ");
        }

        #endregion

    }
}