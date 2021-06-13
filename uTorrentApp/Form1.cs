using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using UTorrent.Api;
using UTorrent.Api.Data;
using Easybots.Data;
using Easybots.Apps.Data;
using System.Security;
using Easybots.Apps.Exceptions;

[assembly: Easybots.Apps.EasybotsApp("uTorrent App")]

namespace uTorrentApp
{
    public partial class Form1 : Form
    {
        private AppDatastore dataStore;
        private uTorrentClientBot uTorrentBot;
        public Form1()
        {
            this.InitializeComponent();
            var link = Easybots.Apps.EasybotsLink.CreateLink();
            this.dataStore = new AppDatastore("Torrents Datastore", link);
            this.usernameTextbox.TextChanged += this.TextBoxChanged;
            this.passwordTextbox.TextChanged += this.TextBoxChanged;
            this.portTextbox.TextChanged += this.TextBoxChanged;
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

        private void Form1_Load(object sender, EventArgs e)
        {
            ExecuteAndExtractDatastoreLoadExceptionIfNeeded(() => this.dataStore.Load());
            this.LoadDataStore();
        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }

            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        private void SaveCredentialsButtonClick(object sender, EventArgs e)
        {
            if(string.IsNullOrWhiteSpace(this.usernameTextbox.Text) || string.IsNullOrWhiteSpace(this.passwordTextbox.Text) || string.IsNullOrWhiteSpace(this.portTextbox.Text))
            {
                MessageBox.Show("Fill in the fields first, then try again.", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if(this.portTextbox.Text.Any(x => char.IsLetter(x)))
            {
                MessageBox.Show("Insert valid input for the port.", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            this.SaveCredentials();
            this.LoadDataStore();
        }

        private void LoginToUTorrentAccount(string localIpAddress, string username, string password, int port)
        {
            try
            {
                bool isSuccess = this.CreateUTorrentBot(localIpAddress, port, username, password);
                if(isSuccess)
                {
                    this.saveButton.Enabled = false;
                    this.saveButton.Text = "Saved";
                    this.saveButton.BackColor = Color.DarkSeaGreen;
                }
            }
            catch (ArgumentException exc)
            {
                MessageBox.Show(exc.Message, String.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (FormatException)
            {
                MessageBox.Show("Make sure that the inputs are in correct format, then try again.", String.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (ServerUnavailableException)
            {
                MessageBox.Show("Make sure that you provide the correct credentials and the uTorrent Windows Application is running, then try again.", String.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (InvalidCredentialException)
            {
                MessageBox.Show("Make sure that you provide valid credentials, then try again.", String.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TextBoxChanged(object sender, EventArgs e)
        {
            if(this.uTorrentBot != null)
            {
                this.ButtonBackToNormal();
                this.UnregisterUTorrentBotWithAllActiveBots();
            }
        }

        private void ButtonBackToNormal()
        {
            this.saveButton.Enabled = true;
            this.saveButton.Text = "Save";
            this.saveButton.BackColor = Color.LightGreen;
        }

        private void UnregisterUTorrentBotWithAllActiveBots()
        {
            List<TorrentBot> torrentList = this.uTorrentBot.GetRegisteredTorrentBotsList();
            foreach (var item in torrentList)
            {
                item.DownloadFinished -= item.TorrentBot_DownloadFinished;
                item.Unregister();
            }

            this.uTorrentBot.Unregister();
        }

        private bool CreateUTorrentBot(string localIpAddress, int port, string username, string password)
        {
            bool isSuccess = true;
            try
            {
                this.uTorrentBot = new uTorrentClientBot(localIpAddress, port, username, password);
            }
            catch(Easybots.Apps.Exceptions.BotAlreadyRegisteredException)
            {
                isSuccess = false;
                MessageBox.Show("This uTorrent bot already exists.", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return isSuccess;
        }

        private void LoadDataStore()
        {
            foreach (var key in this.dataStore.DataWithSecrets.Keys)
            {
                DataWithSecrets dataWithSecrets = this.dataStore.DataWithSecrets.Get(key);
                string username = key;
                int port = (Int32)dataWithSecrets.Data.GetObject("port");
                string password = Encryption.ToInsecureString(dataWithSecrets.Secret);
                string localIpAddress = GetLocalIPAddress();
                if(!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password) && !string.IsNullOrEmpty(localIpAddress))
                {
                    this.usernameTextbox.Text = username;
                    this.portTextbox.Text = port.ToString();
                    this.LoginToUTorrentAccount(localIpAddress, username, password, port);
                }
            }
        }

        private void SaveCredentials()
        {
            var listOfNames = this.dataStore.DataWithSecrets.Keys;
            foreach (var item in listOfNames)
            {
                this.dataStore.DataWithSecrets.Remove(item);
            }

            int port = Int32.Parse(portTextbox.Text);
            string username = usernameTextbox.Text;
            string password = passwordTextbox.Text;
            SecureString passwordSecured = Encryption.ToSecureString(password);
            DataWithSecrets dataWithSecrets = new DataWithSecrets();
            dataWithSecrets.Secret = passwordSecured;
            dataWithSecrets.Data.AddOrSetObject("port", port);
            this.dataStore.DataWithSecrets.AddOrSet(username, dataWithSecrets);
            ExecuteAndExtractDatastoreLoadExceptionIfNeeded(() => this.dataStore.Save());
        }

        private void portTextbox_KeyPress_1(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }
    }
}
