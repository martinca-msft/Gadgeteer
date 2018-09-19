namespace Microsoft.Gadgeteer.HardwareTemplateWizard
{
    partial class GTHardwareWizardForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GTHardwareWizardForm));
            this.manufacturerFullNameTextBox = new System.Windows.Forms.TextBox();
            this.manufacturerFullNameLabel = new System.Windows.Forms.Label();
            this.cancelButton = new System.Windows.Forms.Button();
            this.createButton = new System.Windows.Forms.Button();
            this.manufacturerShortNameTextBox = new System.Windows.Forms.TextBox();
            this.manufacturerShortNameLabel = new System.Windows.Forms.Label();
            this.hardwareFullNameTextBox = new System.Windows.Forms.TextBox();
            this.hardwareFullNameLabel = new System.Windows.Forms.Label();
            this.netmfSupportCheckedListBox = new System.Windows.Forms.CheckedListBox();
            this.netmfVersionsLabel = new System.Windows.Forms.Label();
            this.hardwareShortNameLabel = new System.Windows.Forms.Label();
            this.hardwareShortNameTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // manufacturerFullNameTextBox
            // 
            this.manufacturerFullNameTextBox.Location = new System.Drawing.Point(235, 80);
            this.manufacturerFullNameTextBox.Name = "manufacturerFullNameTextBox";
            this.manufacturerFullNameTextBox.Size = new System.Drawing.Size(343, 20);
            this.manufacturerFullNameTextBox.TabIndex = 0;
            // 
            // manufacturerFullNameLabel
            // 
            this.manufacturerFullNameLabel.AutoSize = true;
            this.manufacturerFullNameLabel.Location = new System.Drawing.Point(12, 83);
            this.manufacturerFullNameLabel.Name = "manufacturerFullNameLabel";
            this.manufacturerFullNameLabel.Size = new System.Drawing.Size(120, 13);
            this.manufacturerFullNameLabel.TabIndex = 1;
            this.manufacturerFullNameLabel.Text = "Manufacturer Full Name";
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(374, 213);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 4;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // createButton
            // 
            this.createButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.createButton.Location = new System.Drawing.Point(455, 213);
            this.createButton.Name = "createButton";
            this.createButton.Size = new System.Drawing.Size(123, 23);
            this.createButton.TabIndex = 3;
            this.createButton.Text = "Create HARDWARE";
            this.createButton.UseVisualStyleBackColor = true;
            this.createButton.Click += new System.EventHandler(this.createButton_Click);
            // 
            // manufacturerShortNameTextBox
            // 
            this.manufacturerShortNameTextBox.Location = new System.Drawing.Point(235, 115);
            this.manufacturerShortNameTextBox.Name = "manufacturerShortNameTextBox";
            this.manufacturerShortNameTextBox.Size = new System.Drawing.Size(343, 20);
            this.manufacturerShortNameTextBox.TabIndex = 1;
            // 
            // manufacturerShortNameLabel
            // 
            this.manufacturerShortNameLabel.AutoSize = true;
            this.manufacturerShortNameLabel.Location = new System.Drawing.Point(12, 118);
            this.manufacturerShortNameLabel.Name = "manufacturerShortNameLabel";
            this.manufacturerShortNameLabel.Size = new System.Drawing.Size(126, 26);
            this.manufacturerShortNameLabel.TabIndex = 1;
            this.manufacturerShortNameLabel.Text = "Manufacturer Safe Name\r\n(no spaces/punctuation)";
            // 
            // hardwareFullNameTextBox
            // 
            this.hardwareFullNameTextBox.Location = new System.Drawing.Point(235, 10);
            this.hardwareFullNameTextBox.Name = "hardwareFullNameTextBox";
            this.hardwareFullNameTextBox.ReadOnly = true;
            this.hardwareFullNameTextBox.Size = new System.Drawing.Size(343, 20);
            this.hardwareFullNameTextBox.TabIndex = 0;
            this.hardwareFullNameTextBox.TabStop = false;
            // 
            // hardwareFullNameLabel
            // 
            this.hardwareFullNameLabel.AutoSize = true;
            this.hardwareFullNameLabel.Location = new System.Drawing.Point(12, 13);
            this.hardwareFullNameLabel.Name = "hardwareFullNameLabel";
            this.hardwareFullNameLabel.Size = new System.Drawing.Size(102, 13);
            this.hardwareFullNameLabel.TabIndex = 1;
            this.hardwareFullNameLabel.Text = "HARDWARE Name";
            // 
            // netmfSupportCheckedListBox
            // 
            this.netmfSupportCheckedListBox.BackColor = System.Drawing.SystemColors.Control;
            this.netmfSupportCheckedListBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.netmfSupportCheckedListBox.CheckOnClick = true;
            this.netmfSupportCheckedListBox.FormattingEnabled = true;
            this.netmfSupportCheckedListBox.Items.AddRange(new object[] {
            "NETMF 4.1",
            "NETMF 4.2"});
            this.netmfSupportCheckedListBox.Location = new System.Drawing.Point(235, 158);
            this.netmfSupportCheckedListBox.Name = "netmfSupportCheckedListBox";
            this.netmfSupportCheckedListBox.Size = new System.Drawing.Size(86, 30);
            this.netmfSupportCheckedListBox.TabIndex = 2;
            // 
            // netmfVersionsLabel
            // 
            this.netmfVersionsLabel.AutoSize = true;
            this.netmfVersionsLabel.Location = new System.Drawing.Point(12, 158);
            this.netmfVersionsLabel.Name = "netmfVersionsLabel";
            this.netmfVersionsLabel.Size = new System.Drawing.Size(208, 13);
            this.netmfVersionsLabel.TabIndex = 1;
            this.netmfVersionsLabel.Text = ".NET Micro Framework versions supported\r\n";
            // 
            // hardwareShortNameLabel
            // 
            this.hardwareShortNameLabel.AutoSize = true;
            this.hardwareShortNameLabel.Location = new System.Drawing.Point(12, 48);
            this.hardwareShortNameLabel.Name = "hardwareShortNameLabel";
            this.hardwareShortNameLabel.Size = new System.Drawing.Size(127, 26);
            this.hardwareShortNameLabel.TabIndex = 7;
            this.hardwareShortNameLabel.Text = "HARDWARE Safe Name\r\n(no spaces/punctuation)";
            // 
            // hardwareShortNameTextBox
            // 
            this.hardwareShortNameTextBox.Location = new System.Drawing.Point(235, 45);
            this.hardwareShortNameTextBox.Name = "hardwareShortNameTextBox";
            this.hardwareShortNameTextBox.ReadOnly = true;
            this.hardwareShortNameTextBox.Size = new System.Drawing.Size(343, 20);
            this.hardwareShortNameTextBox.TabIndex = 6;
            this.hardwareShortNameTextBox.TabStop = false;
            // 
            // GTHardwareWizardForm
            // 
            this.AcceptButton = this.createButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(594, 248);
            this.Controls.Add(this.hardwareShortNameLabel);
            this.Controls.Add(this.hardwareShortNameTextBox);
            this.Controls.Add(this.netmfSupportCheckedListBox);
            this.Controls.Add(this.createButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.hardwareFullNameLabel);
            this.Controls.Add(this.manufacturerShortNameLabel);
            this.Controls.Add(this.netmfVersionsLabel);
            this.Controls.Add(this.manufacturerFullNameLabel);
            this.Controls.Add(this.hardwareFullNameTextBox);
            this.Controls.Add(this.manufacturerShortNameTextBox);
            this.Controls.Add(this.manufacturerFullNameTextBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GTHardwareWizardForm";
            this.ShowInTaskbar = false;
            this.Text = ".NET Gadgeteer HARDWARE Project Creator";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox manufacturerFullNameTextBox;
        private System.Windows.Forms.Label manufacturerFullNameLabel;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button createButton;
        private System.Windows.Forms.TextBox manufacturerShortNameTextBox;
        private System.Windows.Forms.Label manufacturerShortNameLabel;
        private System.Windows.Forms.TextBox hardwareFullNameTextBox;
        private System.Windows.Forms.Label hardwareFullNameLabel;
        private System.Windows.Forms.CheckedListBox netmfSupportCheckedListBox;
        private System.Windows.Forms.Label netmfVersionsLabel;
        private System.Windows.Forms.Label hardwareShortNameLabel;
        private System.Windows.Forms.TextBox hardwareShortNameTextBox;
    }
}