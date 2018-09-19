// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualStudio.TemplateWizard;

namespace Microsoft.Gadgeteer.HardwareTemplateWizard
{
    public partial class GTHardwareWizardForm : Form
    {
        private string HardwareType;
        private const string HardwareTypeString = "HARDWARE";

        /// <summary>
        /// A form for specifying hardware params
        /// </summary>
        /// <param name="hardwareType">hardware type with initial capital, e.g. "Module"</param>
        public GTHardwareWizardForm(string hardwareType, string hardwareFullName, string hardwareShortName)
        {
            this.HardwareType = hardwareType;
            InitializeComponent();
            this.Text = this.Text.Replace(HardwareTypeString, hardwareType);
            this.hardwareFullNameLabel.Text = this.hardwareFullNameLabel.Text.Replace(HardwareTypeString, hardwareType);
            this.hardwareShortNameLabel.Text = this.hardwareShortNameLabel.Text.Replace(HardwareTypeString, hardwareType);
            this.hardwareFullNameTextBox.Text = hardwareFullName;
            this.hardwareShortNameTextBox.Text = hardwareShortName;
            this.createButton.Text = this.createButton.Text.Replace(HardwareTypeString, hardwareType);

            this.netmfSupportCheckedListBox.SetItemChecked(0, true);
            this.netmfSupportCheckedListBox.SetItemChecked(1, true);
        }

        private void createButton_Click(object sender, EventArgs e)
        {
            if(manufacturerFullNameTextBox.Text.Length == 0 || manufacturerShortNameTextBox.Text.Length == 0) 
            {
                MessageBox.Show("The manufacturer full and safe names must be filled in", "Invalid " + HardwareType);
                return;
            }

            var ms = manufacturerShortNameTextBox.Text;
            foreach (var c in ms.ToCharArray())
            {
                if (!((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '_'))
                {
                    MessageBox.Show("The manufacturer safe name can only contain alphanumerics and underscore", "Invalid " + HardwareType);
                    return;
                }
            }
            if (ms[0] >= '0' && ms[0] <= '9')
            {
                MessageBox.Show("The manufacturer safe name must not start with a number", "Invalid " + HardwareType);
                return;
            }

            if (netmfSupportCheckedListBox.CheckedItems.Count == 0)
            {
                MessageBox.Show("At least one NETMF version must be supported", "Invalid " + HardwareType);
                return;
            }

            GTHardwareWizard.manufacturerFullName = manufacturerFullNameTextBox.Text;
            GTHardwareWizard.manufacturerShortName = manufacturerShortNameTextBox.Text;
            GTHardwareWizard.hardwareShortName = hardwareShortNameTextBox.Text;
            GTHardwareWizard.hardwareFullName = hardwareFullNameTextBox.Text;
            GTHardwareWizard.netmf41Support = netmfSupportCheckedListBox.CheckedItems.Contains("NETMF 4.1");
            GTHardwareWizard.netmf42Support = netmfSupportCheckedListBox.CheckedItems.Contains("NETMF 4.2");

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        //private void GTHardwareWizardForm_FormClosed(object sender, FormClosedEventArgs e)
        //{
        //    this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        //    if (e.CloseReason == CloseReason.UserClosing) throw new WizardCancelledException("User closed wizard");
        //}

    }
}
