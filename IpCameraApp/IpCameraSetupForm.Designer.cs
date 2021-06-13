namespace IpCameraApp
{
    partial class IpCameraSetupForm
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

            if(this.routerDevicesCollection != null)
            {
                this.routerDevicesCollection.RouterDevicesLoaded -= this.Collection_RouterDevicesListed;
                this.routerDevicesCollection.DevicePinged -= this.RouterDevicesCollection_PingedDevice;
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IpCameraSetupForm));
            this.label1 = new System.Windows.Forms.Label();
            this.usernameTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.passwordTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cgiTextBox = new System.Windows.Forms.TextBox();
            this.routerDevicesComboBox = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.createButton = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.displayAsTextBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.informationAboutSelectedDeviceListView = new System.Windows.Forms.ListView();
            this.infoLabel = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.linkLabel = new System.Windows.Forms.LinkLabel();
            this.cgiInfoButton = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 61);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 16);
            this.label1.TabIndex = 1;
            this.label1.Text = "Username:";
            // 
            // usernameTextBox
            // 
            this.usernameTextBox.Location = new System.Drawing.Point(102, 58);
            this.usernameTextBox.Name = "usernameTextBox";
            this.usernameTextBox.Size = new System.Drawing.Size(204, 22);
            this.usernameTextBox.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 89);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 16);
            this.label2.TabIndex = 2;
            this.label2.Text = "Password:";
            // 
            // passwordTextBox
            // 
            this.passwordTextBox.Location = new System.Drawing.Point(102, 86);
            this.passwordTextBox.Name = "passwordTextBox";
            this.passwordTextBox.Size = new System.Drawing.Size(204, 22);
            this.passwordTextBox.TabIndex = 3;
            this.passwordTextBox.UseSystemPasswordChar = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(14, 117);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(33, 16);
            this.label3.TabIndex = 4;
            this.label3.Text = "CGI:";
            // 
            // cgiTextBox
            // 
            this.cgiTextBox.Location = new System.Drawing.Point(102, 114);
            this.cgiTextBox.Name = "cgiTextBox";
            this.cgiTextBox.Size = new System.Drawing.Size(204, 22);
            this.cgiTextBox.TabIndex = 4;
            // 
            // routerDevicesComboBox
            // 
            this.routerDevicesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.routerDevicesComboBox.FormattingEnabled = true;
            this.routerDevicesComboBox.Location = new System.Drawing.Point(17, 299);
            this.routerDevicesComboBox.Name = "routerDevicesComboBox";
            this.routerDevicesComboBox.Size = new System.Drawing.Size(341, 24);
            this.routerDevicesComboBox.TabIndex = 5;
            this.routerDevicesComboBox.SelectedIndexChanged += new System.EventHandler(this.routerDevicesComboBox_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(14, 241);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(188, 16);
            this.label4.TabIndex = 7;
            this.label4.Text = "Select your IP Camera device:";
            // 
            // createButton
            // 
            this.createButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("createButton.BackgroundImage")));
            this.createButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.createButton.FlatAppearance.BorderSize = 0;
            this.createButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.createButton.Location = new System.Drawing.Point(422, 360);
            this.createButton.Name = "createButton";
            this.createButton.Size = new System.Drawing.Size(69, 63);
            this.createButton.TabIndex = 6;
            this.createButton.UseVisualStyleBackColor = true;
            this.createButton.Click += new System.EventHandler(this.createButton_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(14, 33);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(76, 16);
            this.label5.TabIndex = 9;
            this.label5.Text = "Display As:";
            // 
            // displayAsTextBox
            // 
            this.displayAsTextBox.Location = new System.Drawing.Point(102, 30);
            this.displayAsTextBox.Name = "displayAsTextBox";
            this.displayAsTextBox.Size = new System.Drawing.Size(204, 22);
            this.displayAsTextBox.TabIndex = 1;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(14, 349);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(233, 16);
            this.label6.TabIndex = 11;
            this.label6.Text = "Information about the selected device:";
            // 
            // informationAboutSelectedDeviceListView
            // 
            this.informationAboutSelectedDeviceListView.Location = new System.Drawing.Point(17, 378);
            this.informationAboutSelectedDeviceListView.Name = "informationAboutSelectedDeviceListView";
            this.informationAboutSelectedDeviceListView.Size = new System.Drawing.Size(341, 63);
            this.informationAboutSelectedDeviceListView.TabIndex = 12;
            this.informationAboutSelectedDeviceListView.TabStop = false;
            this.informationAboutSelectedDeviceListView.UseCompatibleStateImageBehavior = false;
            // 
            // infoLabel
            // 
            this.infoLabel.AutoSize = true;
            this.infoLabel.BackColor = System.Drawing.SystemColors.Control;
            this.infoLabel.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.infoLabel.Location = new System.Drawing.Point(14, 267);
            this.infoLabel.Name = "infoLabel";
            this.infoLabel.Size = new System.Drawing.Size(467, 16);
            this.infoLabel.TabIndex = 13;
            this.infoLabel.Text = "Listed in the following format: HOSTNAME :: IP_ADDRESS :: MAC_ADDRESS";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(433, 426);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(48, 16);
            this.label7.TabIndex = 14;
            this.label7.Text = "Create";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.BackColor = System.Drawing.SystemColors.Control;
            this.label8.ForeColor = System.Drawing.SystemColors.Desktop;
            this.label8.Location = new System.Drawing.Point(14, 153);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(417, 48);
            this.label8.TabIndex = 15;
            this.label8.Text = "IMPORTANT:\r\nIn \'CGI\' field, insert CGI command without type, protocol or credenti" +
    "als.\r\nAll CGI commands for your camera can be found on:";
            // 
            // linkLabel
            // 
            this.linkLabel.AutoSize = true;
            this.linkLabel.Location = new System.Drawing.Point(14, 202);
            this.linkLabel.Name = "linkLabel";
            this.linkLabel.Size = new System.Drawing.Size(262, 16);
            this.linkLabel.TabIndex = 16;
            this.linkLabel.TabStop = true;
            this.linkLabel.Text = "https://www.ispyconnect.com/sources.aspx";
            this.linkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel_LinkClicked);
            // 
            // cgiInfoButton
            // 
            this.cgiInfoButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("cgiInfoButton.BackgroundImage")));
            this.cgiInfoButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.cgiInfoButton.FlatAppearance.BorderSize = 0;
            this.cgiInfoButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cgiInfoButton.Location = new System.Drawing.Point(312, 117);
            this.cgiInfoButton.Name = "cgiInfoButton";
            this.cgiInfoButton.Size = new System.Drawing.Size(20, 20);
            this.cgiInfoButton.TabIndex = 17;
            this.cgiInfoButton.UseVisualStyleBackColor = true;
            this.cgiInfoButton.Click += new System.EventHandler(this.cgiInfoButton_Click);
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(17, 267);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(341, 18);
            this.progressBar.TabIndex = 18;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.progressBar);
            this.groupBox1.Controls.Add(this.cgiInfoButton);
            this.groupBox1.Controls.Add(this.linkLabel);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.infoLabel);
            this.groupBox1.Controls.Add(this.informationAboutSelectedDeviceListView);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.displayAsTextBox);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.createButton);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.routerDevicesComboBox);
            this.groupBox1.Controls.Add(this.cgiTextBox);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.passwordTextBox);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.usernameTextBox);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(522, 468);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "IP Camera Setup";
            // 
            // IpCameraSetupForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(546, 498);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "IpCameraSetupForm";
            this.Text = "IP Camera Setup";
            this.Load += new System.EventHandler(this.Form2_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox usernameTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox passwordTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox cgiTextBox;
        private System.Windows.Forms.ComboBox routerDevicesComboBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button createButton;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox displayAsTextBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ListView informationAboutSelectedDeviceListView;
        private System.Windows.Forms.Label infoLabel;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.LinkLabel linkLabel;
        private System.Windows.Forms.Button cgiInfoButton;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}