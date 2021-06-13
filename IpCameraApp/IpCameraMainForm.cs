using Easybots.Apps.Data;
using Easybots.Apps.Exceptions;
using Easybots.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

[assembly: Easybots.Apps.EasybotsApp("IpCamera App")]

namespace IpCameraApp
{
    public partial class IpCameraMainForm : Form
    {
        public RouterDevicesCollection routerDevicesCollection;
        private IpCameraSetupForm ipCameraSetupForm = new IpCameraSetupForm();
        private List<IpCameraBot> ipCameraBots = new List<IpCameraBot>();
        private List<IpCameraBotInfo> ipCameraBotInfos = new List<IpCameraBotInfo>();
        private AppDatastore dataStore;
        private SplashScreen loadingScreen = new SplashScreen();
        public IpCameraMainForm()
        {
            this.InitializeComponent();
            this.ipCameraSetupForm.IpCameraInfoCreated += this.Form2IpCameraInfoCreated;
            var link = Easybots.Apps.EasybotsLink.CreateLink();
            this.dataStore = new AppDatastore("ipCameraDataStore", link);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ExecuteAndExtractDatastoreLoadExceptionIfNeeded(() => this.dataStore.Load());
            if (this.dataStore.DataWithSecrets.Keys.Count<string>() == 0)
            {
                this.LoadFromDataStore(null);
            }
            else
            {
                if (!this.IsHandleCreated)
                {
                    this.CreateHandle();
                }

                this.routerDevicesCollection = new RouterDevicesCollection();
                this.routerDevicesCollection.RouterDevicesLoaded += this.RouterDeviceCollectionLoaded;

                this.Invoke((Action)delegate {
                    this.loadingScreen.ShowDialog();
                });
            }
        }

        private static void ExecuteAndExtractDatastoreLoadExceptionIfNeeded(Action action)
        {
            try
            {
                action();
            }
            catch (AppDatastoreLoadException datastoreLoadException)
            {
                throw datastoreLoadException.InnerException;
            }
        }

        private void Form2IpCameraInfoCreated(IpCameraBotInfo info)
        {
            if (this.ipCameraBotInfos.Contains(info))
            {
                MessageBox.Show("This IP Camera already exists.", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else
            {
                this.AddingToLists(info);
            }
        }

        private void AddingToLists(IpCameraBotInfo info)
        {
            bool isSuccess = this.CreateIpCamera(info);
            if (isSuccess)
            {
                this.ipCamerasListBox.Items.Add(info);
                this.SetToDataStore(info);
            }
        }

        private void SetToDataStore(IpCameraBotInfo info)
        {
            this.ipCameraBotInfos.Add(info);
            this.SaveToDataStore(this.ipCameraBotInfos);
        }

        private void SaveToDataStore(List<IpCameraBotInfo> botInfos)
        {
            var listOfNames = this.dataStore.DataWithSecrets.Keys;
            foreach (var item in listOfNames)
            {
                this.dataStore.DataWithSecrets.Remove(item);
            }

            foreach (var item in botInfos)
            {
                DataWithSecrets dataWithSecrets = new DataWithSecrets();
                dataWithSecrets.Data.AddOrSetString("displayName", item.DisplayName);
                dataWithSecrets.Data.AddOrSetString("username", item.Username);
                dataWithSecrets.Data.AddOrSetString("cgi", item.CGI);
                dataWithSecrets.Secret = item.PasswordAsSecureString;
                dataWithSecrets.Secret2 = item.IpAddressAsSecureString;
                dataWithSecrets.Secret3 = item.MacAddressAsSecureString;
                dataWithSecrets.Secret4 = item.HostNameAsSecureString;
                this.dataStore.DataWithSecrets.AddOrSet(item.DisplayName, dataWithSecrets);
            }

            ExecuteAndExtractDatastoreLoadExceptionIfNeeded(() => this.dataStore.Save());
        }

        private bool CreateIpCamera(IpCameraBotInfo info)
        {
            bool isSuccess = true;
            try
            {
                IpCameraBot ipCamera = new IpCameraBot(info);
                this.ipCameraBots.Add(ipCamera);
            }
            catch (BotAlreadyRegisteredException exc)
            {
                MessageBox.Show(exc.Message, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
                isSuccess = false;
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
                isSuccess = false;
            }

            return isSuccess;
        }

        private void addNewCameraButton_Click(object sender, EventArgs e)
        {
            this.ipCameraSetupForm.ShowDialog();
        }

        private void RouterDeviceCollectionLoaded(List<RouterDevice> devicesList)
        {
            if (!this.IsHandleCreated)
            {
                this.CreateHandle();
            }

            this.Invoke((Action)delegate {
                this.LoadFromDataStore(devicesList);
            });

            this.Invoke((Action)delegate {
                this.loadingScreen.Close();
            });
        }

        private void LoadFromDataStore(List<RouterDevice> devicesList)
        {
            foreach (var key in this.dataStore.DataWithSecrets.Keys)
            {
                if (this.dataStore.DataWithSecrets.ContainsKey(key))
                {
                    DataWithSecrets dataWithSecrets = this.dataStore.DataWithSecrets.Get(key);
                    string password = Encryption.ToInsecureString(dataWithSecrets.Secret);
                    string ipAddress = Encryption.ToInsecureString(dataWithSecrets.Secret2);
                    string macAddress = Encryption.ToInsecureString(dataWithSecrets.Secret3);
                    string hostName = Encryption.ToInsecureString(dataWithSecrets.Secret4);
                    string displayName = dataWithSecrets.Data.GetString("displayName");
                    string cgi = dataWithSecrets.Data.GetString("cgi");
                    string username = dataWithSecrets.Data.GetString("username");
                    RouterDevice device = new RouterDevice(macAddress, ipAddress, hostName);
                    IpCameraBotInfo info = new IpCameraBotInfo(displayName, username, password, cgi, device);
                    if(devicesList != null && devicesList.Count != 0)
                    {
                        foreach (var deviceItem in devicesList)
                        {
                            if (info.Device.MacAddress == deviceItem.MacAddress)
                            {
                                if (info.Device.IpAddress != deviceItem.IpAddress)
                                {
                                    info.Device = deviceItem;
                                }

                                break;
                            }
                        }
                    }
                    
                    bool isSuccess = this.CreateIpCamera(info);
                    if (isSuccess)
                    {
                        this.ipCameraBotInfos.Add(info);
                        this.ipCamerasListBox.Items.Add(info);
                        // Should save to dataStore
                    }
                }
            }
        }

        private void takePictureButton_Click(object sender, EventArgs e)
        {
            if (this.ipCamerasListBox.SelectedItem == null)
            {
                MessageBox.Show("Select an IP Camera first, then try again.", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else
            {
                try
                {
                    IpCameraBotInfo info = (IpCameraBotInfo)this.ipCamerasListBox.SelectedItem;
                    IpCameraBot ipCamera = this.ipCameraBots.FirstOrDefault(item => item.IpCameraInfo == info);
                    SerializableImage image = ipCamera.TakePicture();
                    using (MemoryStream stream = new MemoryStream(image.Bytes))
                    {
                        this.pictureBox.Image = Image.FromStream(stream);
                    }
                }
                catch(Exception exc)
                {
                    MessageBox.Show(exc.Message, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
        }

        private void removeButton_Click(object sender, EventArgs e)
        {
            if (this.ipCamerasListBox.SelectedItem == null)
            {
                MessageBox.Show("Select an IP Camera first, then try again.", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else
            {
                DialogResult removeChoice = MessageBox.Show("Are you sure you want to remove the selected IP Camera ?", string.Empty, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (removeChoice == (DialogResult.Yes))
                {
                    IpCameraBotInfo info = (IpCameraBotInfo)this.ipCamerasListBox.SelectedItem;
                    this.RemovingFromLists(info);
                }
            }
        }

        private void RemovingFromLists(IpCameraBotInfo info)
        {
            List<IpCameraBot> toBeRemoved = new List<IpCameraBot>();
            toBeRemoved.Add(this.ipCameraBots.FirstOrDefault(item => item.IpCameraInfo == info));
            foreach (var item in toBeRemoved)
            {
                item.Unregister();
                this.ipCameraBots.Remove(item);
            }

            this.ipCamerasListBox.Items.Remove(info);
            this.ipCameraBotInfos.Remove(info);
            this.SaveToDataStore(this.ipCameraBotInfos);
        }
    }
}