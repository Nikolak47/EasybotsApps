using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace IpCameraApp
{
    public delegate void IpCameraDelegate(IpCameraBotInfo info);
    public partial class IpCameraSetupForm : Form
    {
        public event IpCameraDelegate IpCameraInfoCreated;
        private RouterDevicesCollection routerDevicesCollection;
        public IpCameraSetupForm()
        {
            this.InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            this.informationAboutSelectedDeviceListView.Items.Clear();
            this.routerDevicesComboBox.Items.Clear();
            this.informationAboutSelectedDeviceListView.View = View.Details;
            this.informationAboutSelectedDeviceListView.Clear();
            this.informationAboutSelectedDeviceListView.GridLines = true;
            this.informationAboutSelectedDeviceListView.FullRowSelect = true;
            this.informationAboutSelectedDeviceListView.BackColor = SystemColors.MenuBar;
            this.informationAboutSelectedDeviceListView.Columns.Add("HostName", 100);
            this.informationAboutSelectedDeviceListView.Columns.Add("IP Address", 100);
            this.informationAboutSelectedDeviceListView.Columns.Add("MAC Address", 100);
            this.informationAboutSelectedDeviceListView.Sorting = SortOrder.Descending;
            this.routerDevicesComboBox.Sorted = true;
            this.displayAsTextBox.Text = string.Empty;
            this.usernameTextBox.Text = string.Empty;
            this.passwordTextBox.Text = string.Empty;
            this.cgiTextBox.Text = string.Empty;
            this.routerDevicesComboBox.SelectedIndex = -1;
            this.routerDevicesComboBox.Text = string.Empty;
            this.infoLabel.Text = string.Empty;

            try
            {
                this.routerDevicesCollection = new RouterDevicesCollection();
                this.routerDevicesCollection.RouterDevicesLoaded += this.Collection_RouterDevicesListed;
                this.routerDevicesCollection.DevicePinged += this.RouterDevicesCollection_PingedDevice;
                this.Invoke((Action)delegate
                {
                    this.progressBar.Visible = true;
                    this.progressBar.Value = 0;
                    this.progressBar.Minimum = 0;
                    this.progressBar.Maximum = routerDevicesCollection.NumberOfDevices;
                });
                
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        private void RouterDevicesCollection_PingedDevice(List<RouterDevice> devicesList)
        {
            this.Invoke((Action)delegate
            {
                this.progressBar.Value++;
            });
            
        }

        private void Collection_RouterDevicesListed(List<RouterDevice> devicesList)
        {
            try
            {
                foreach (var item in devicesList)
                {
                    this.Invoke((Action)delegate {
                        this.progressBar.Visible = false;
                        this.routerDevicesComboBox.Items.Add(item);
                        this.infoLabel.Text = string.Format("{0} {1}", "Listed in the following format:", string.Format(item.DeviceDisplayFormat, "HOSTNAME", "IP_ADDRESS", "MAC_ADDRESS"));
                    });
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        private void createButton_Click(object sender, EventArgs e)
        {
            string username = this.usernameTextBox.Text;
            string password = this.passwordTextBox.Text;
            string displayName = this.displayAsTextBox.Text;
            string cgi = this.cgiTextBox.Text;
            RouterDevice device = (RouterDevice)this.routerDevicesComboBox.SelectedItem;
            if(string.IsNullOrWhiteSpace(displayName) || string.IsNullOrWhiteSpace(cgi) || device == null)
            {
                MessageBox.Show("Fill in all the fields, then try again.", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            IpCameraBotInfo info = new IpCameraBotInfo(displayName, username, password, cgi, device);
            this.IpCameraInfoCreated?.Invoke(info);
            this.Close();
        }

        private void routerDevicesComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.routerDevicesComboBox.SelectedIndex != -1)
            {
                RouterDevice selectedDevice = (RouterDevice)this.routerDevicesComboBox.SelectedItem;
                string[] info = new string[3];
                info[0] = selectedDevice.HostName;
                info[1] = selectedDevice.IpAddress;
                info[2] = selectedDevice.MacAddress;
                ListViewItem item;
                item = new ListViewItem(info);
                this.informationAboutSelectedDeviceListView.Items.Clear();
                this.informationAboutSelectedDeviceListView.Items.Add(item);
            }
        }

        private void linkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                this.linkLabel.LinkVisited = true;
                System.Diagnostics.Process.Start(this.linkLabel.Text);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cgiInfoButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show(CgiCommandInfo.cgiCommandInfo, "CGI Command Info", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }
    }
}