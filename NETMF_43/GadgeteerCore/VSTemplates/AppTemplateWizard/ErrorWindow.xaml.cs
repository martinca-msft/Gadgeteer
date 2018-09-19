// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Navigation;

namespace Microsoft.Gadgeteer.AppTemplateWizard
{
    public partial class ErrorWindow
    {
        public ErrorWindow()
        {
            InitializeComponent();
        }
        internal ErrorWindow(ErrorState state) : this()
        {
            StatePresenter.SetResourceReference(ContentPresenter.ContentTemplateProperty, state.ToString());
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

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            if (_shownAsDialog)
                DialogResult = false;
            else
                Close();
        }

        private void OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                // use e.g. control:Microsoft.ProgramsAndFeatures for the NavigateUri to open control panel on that page
                // http://msdn.microsoft.com/en-gb/library/windows/desktop/ee330741.aspx

                if (e.Uri.IsAbsoluteUri && e.Uri.Scheme == "control")
                    Process.Start("control", "/name " + e.Uri.AbsolutePath);
                else
                    Process.Start(e.Uri.OriginalString);
            }
            catch (Exception ex)
            {
                MessageBox.Show("The link could not be opened. " + ex.Message, ".NET Gadgeteer Application Wizard", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }    
}