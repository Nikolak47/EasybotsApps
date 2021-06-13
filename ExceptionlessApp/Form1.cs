using Easybots.Apps;
using Easybots.Apps.Data;
using Easybots.Apps.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

[assembly: Easybots.Apps.EasybotsApp("Exceptionless App")]

namespace ExceptionlessApp
{
    public partial class Form1 : Form
    {
        private const string SecretKey = "secret";
        private static EasybotsLink link;
        private object syncLock = new object();
        private List<ExceptionlessBotInfo> botInfos = new List<ExceptionlessBotInfo>();
        private List<ExceptionlessBot> bots = new List<ExceptionlessBot>();
        private AppDatastore dataStore;
        public Form1()
        {
            this.InitializeComponent();
            try
            {
                link = Easybots.Apps.EasybotsLink.CreateLink();
                this.dataStore = new AppDatastore("exceptionlessDataStore", link);
            }
            catch (Exception e)
            {
                EasybotsLink.Instance.LogError(e.ToString());
                if (Environment.UserInteractive)
                    System.Windows.Forms.MessageBox.Show(this, e.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);

                this.Close();
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

        private void AddButtonClicked(object sender, EventArgs e)
        {
            lock (this.syncLock)
            {
                string projectName = this.projectNameTextBox.Text;
                string apiKey = this.apiKeyTextBox.Text;
                ExceptionlessBotInfo botInfo = new ExceptionlessBotInfo(projectName, apiKey);
                if (string.IsNullOrWhiteSpace(projectName) || string.IsNullOrWhiteSpace(apiKey))
                {
                    MessageBox.Show("Insert project name and api key for the project, then try again.", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                bool alreadyExists = this.botInfos.Contains(botInfo);
                if (!alreadyExists)
                {
                    this.AddingToLists(botInfo);
                    this.projectNameTextBox.Text = string.Empty;
                    this.apiKeyTextBox.Text = string.Empty;
                }
                else
                {
                    MessageBox.Show("This project already exists.", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.projectNameTextBox.Text = string.Empty;
                    this.apiKeyTextBox.Text = string.Empty;
                    return;
                }
            }
        }

        private void AddingToLists(ExceptionlessBotInfo botInfo)
        {
            bool isSuccess = this.CreateExceptionlessBot(botInfo);
            if (isSuccess)
            {
                this.projectsListBox.Items.Add(botInfo);
                this.SetToDataStore(botInfo);
            }
        }

        private void SetToDataStore(ExceptionlessBotInfo botInfo)
        {
            this.botInfos.Add(botInfo);
            this.SaveToDataStore(this.botInfos);
        }

        private void SaveToDataStore(List<ExceptionlessBotInfo> botInfos)
        {
            var listOfNames = this.dataStore.DataWithSecrets.Keys;
            int countOfListOfNames = listOfNames.Count();
            foreach (var item in listOfNames)
            {
                this.dataStore.DataWithSecrets.Remove(item);
            }

            int countOfDataStoreNow = this.dataStore.DataWithSecrets.Keys.Count();
            foreach (var item in botInfos)
            {
                DataWithSecrets dataWithSecrets = new DataWithSecrets();
                dataWithSecrets.Data.AddOrSetObject("projectName", item.ProjectName);
                dataWithSecrets.Secret = item.ApiKeyAsSecureString;
                this.dataStore.DataWithSecrets.AddOrSet(item.ProjectName, dataWithSecrets);
            }

            ExecuteAndExtractDatastoreLoadExceptionIfNeeded(() => this.dataStore.Save());
        }

        private bool CreateExceptionlessBot(ExceptionlessBotInfo botInfo)
        {
            bool isSuccess = true;
            try
            {
                ExceptionlessBot exceptionlessBot = new ExceptionlessBot(botInfo);
                this.bots.Add(exceptionlessBot);
            }
           
            catch (BotAlreadyRegisteredException)
            {
                MessageBox.Show("This project already exists.", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
                isSuccess = false;
            }

            return isSuccess;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ExecuteAndExtractDatastoreLoadExceptionIfNeeded(() => this.dataStore.Load());
            this.LoadFromDataStore();
        }

        private void LoadFromDataStore()
        {
            foreach (var key in this.dataStore.DataWithSecrets.Keys)
            {
                string projectName = key;
                if (!this.dataStore.DataWithSecrets.ContainsKey(key))
                {
                    string errorMessage = string.Format("Could not load data for the project '{0}'. " +
                        "This can happen when the bot is stored on a location not accessible to the user '{1}'.",
                        projectName, Environment.UserName);
                    EasybotsLink.Instance.LogError(errorMessage);
                    continue;
                }


                if (!this.dataStore.DataWithSecrets.ContainsKey(key))
                    continue;

                //string projectName = key;
                DataWithSecrets dataWithSecrets = this.dataStore.DataWithSecrets.Get(key);
                string apiKey = Encryption.ToInsecureString(dataWithSecrets.Secret);
                ExceptionlessBotInfo botInfo = new ExceptionlessBotInfo(projectName, apiKey);
                bool isSuccess = this.CreateExceptionlessBot(botInfo);
                if (isSuccess)
                {
                    this.botInfos.Add(botInfo);
                    this.projectsListBox.Items.Add(botInfo);
                }
            }
        }

        private void RemoveButtonClicked(object sender, EventArgs e)
        {
            lock (this.syncLock)
            {
                if (this.projectsListBox.SelectedItem == null)
                {
                    MessageBox.Show("Select an available project, then try again.", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else
                {
                    DialogResult removeChoice = MessageBox.Show("Are you sure you want to remove the selected project ?", string.Empty, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (removeChoice == (DialogResult.Yes))
                    {
                        ExceptionlessBotInfo botInfo = (ExceptionlessBotInfo)this.projectsListBox.SelectedItem;
                        this.RemovingFromList(botInfo);
                    }
                }
            }
        }

        private void RemovingFromList(ExceptionlessBotInfo botInfo)
        {
            List<ExceptionlessBot> toBeRemoved = new List<ExceptionlessBot>();
            toBeRemoved.Add(this.bots.FirstOrDefault(item => item.ProjectName == botInfo.ProjectName));
            foreach (var item in toBeRemoved)
            {
                item.Unregister();
                this.bots.Remove(item);
            }

            this.projectsListBox.Items.Remove(botInfo);
            this.botInfos.Remove(botInfo);
            this.SaveToDataStore(this.botInfos);
        }
    }
}
