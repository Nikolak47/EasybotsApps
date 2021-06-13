using Easybots.Apps;
using Easybots.Apps.Exceptions;
using Easybots.Data;
using Easybots.DataModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UTorrent.Api;
using UTorrent.Api.Data;

namespace uTorrentApp
{
    class uTorrentClientBot : Easybot
    {
        private int reloadTimeIntervalInMilliseconds = Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["uTorrentUpdateTimeIntervalInMs"]);
        private UTorrentClient client;
        private IList<Torrent> alreadyRegisteredTorrents;
        private List<TorrentBot> torrentBots = new List<TorrentBot>();
        private IList<Torrent> oldListOfTorrents = new List<Torrent>();
        private System.Timers.Timer timerForTorrentUpdate = new System.Timers.Timer();
        private object syncLock = new object();
        private object syncLockCommands = new object();
        public uTorrentClientBot (string ip, int port, string username, string password) : base("uTorrentClient Bot", false)
        {
            if(string.IsNullOrWhiteSpace(ip) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentNullException();
            }

            this.client = new UTorrentClient(ip, port, username, password);
            this.alreadyRegisteredTorrents = this.client.GetList().Result.Torrents;
            this.Register();
            this.LoadTorrents();
            this.StartUTorrentUpdateTimer();
        }

        [Action("Starts downloading the torrent from the torrent file and creates torrent bot.")]
        public void DownloadTorrent(
            [ParameterDescription("torrent file","The torrent file with extension '.torrent'", typeof(SerializableFile), AllowUserInput = true)]
            SerializableFile torrentFile)
        {
            this.UpdateTorrents(this.client.GetList().Result.Torrents);
            using (Stream stream = new MemoryStream(torrentFile.FileBytes))
            {
                this.client.PostTorrent(stream);
                var newListOfTorrents = this.client.GetList().Result.Torrents;
                this.UpdateTorrents(newListOfTorrents);
            }
        }

        [Trigger("Notifies when the torrent is downloaded and returns its name.")]
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        [return: ParameterDescription("torrent name", "The name of the torrent.", typeof(string), AllowUserInput = false)]
        public string OnDownloadFinished(string torrentName)
        {
            this.TriggerInEasybotsPlatform(torrentName);
            return torrentName;
        }

        [Action("Returns an array of the names of all available torrents in your uTorrent Client.")]
        [return: ParameterDescription("torrentNames", "Array of the names of all available torrents.", typeof(string[]))]
        public string[] GetTorrentNames()
        {
            return this.client.GetList().Result.Torrents.Select(t => t.Name).ToArray();
        }

        [Action("Starts downloading torrent from your available torrents in your uTorrent Client, by a given torrent name. " +
            "If the torrent doesn't exist, InvalidOperationException will be thrown.")]
        public void StartDownloadingByTorrentName(
            [ParameterDescription("torrentName", "The name of the torrent.", typeof(string), AllowUserInput = true)]
            string torrentName)
        {
            if (string.IsNullOrWhiteSpace(torrentName))
                throw new ArgumentException("Insert valid torrent name. Your input: '" + torrentName + "'");

            lock (this.syncLockCommands)
            {
                TorrentBot torrentBot = this.torrentBots.FirstOrDefault(t => t.Torrent.Name == torrentName);
                if(torrentBot == null)
                    throw new InvalidOperationException(string.Format("Torrent with name '{0}', doesn't exist.", torrentName));

                torrentBot.StartDownloading();
            }
        }

        [Action("Stops downloading torrent from your available torrents in your uTorrent Client, by a given torrent name. " +
            "If the torrent doesn't exist, InvalidOperationException will be thrown.")]
        public void StopDownloadingByTorrentName(
            [ParameterDescription("torrentName", "The name of the torrent.", typeof(string), AllowUserInput = true)]
            string torrentName
            )
        {
            if (string.IsNullOrWhiteSpace(torrentName))
                throw new ArgumentException("Insert valid torrent name. Your input: '" + torrentName + "'");

            lock (this.syncLockCommands)
            {
                TorrentBot torrentBot = this.torrentBots.FirstOrDefault(t => t.Torrent.Name == torrentName);
                if (torrentBot == null)
                    throw new InvalidOperationException(string.Format("Torrent with name '{0}', doesn't exist.", torrentName));

                torrentBot.StopDownloading();
            }
        }

        [Action("Pauses downloading torrent from your available torrents in your uTorrent Client, by a given torrent name. " +
            "If the torrent doesn't exist, InvalidOperationException will be thrown.")]
        public void PauseDownloadingByTorrentName(
            [ParameterDescription("torrentName", "The name of the torrent.", typeof(string), AllowUserInput = true)]
            string torrentName
            )
        {
            if (string.IsNullOrWhiteSpace(torrentName))
                throw new ArgumentException("Insert valid torrent name. Your input: '" + torrentName + "'");

            lock (this.syncLockCommands)
            {
                TorrentBot torrentBot = this.torrentBots.FirstOrDefault(t => t.Torrent.Name == torrentName);
                if (torrentBot == null)
                    throw new InvalidOperationException(string.Format("Torrent with name '{0}', doesn't exist.", torrentName));

                torrentBot.PauseDownloading();
            }
        }

        [Action("Deletes torrent from your available torrents in your uTorrent Client, by a given torrent name. " +
            "If the torrent doesn't exist, InvalidOperationException will be thrown.")]
        public void DeleteTorrentByTorrentName(
            [ParameterDescription("torrentName", "The name of the torrent.", typeof(string), AllowUserInput = true)]
            string torrentName
            )
        {
            if (string.IsNullOrWhiteSpace(torrentName))
                throw new ArgumentException("Insert valid torrent name. Your input: '" + torrentName + "'");

            lock (this.syncLockCommands)
            {
                TorrentBot torrentBot = this.torrentBots.FirstOrDefault(t => t.Torrent.Name == torrentName);
                if (torrentBot == null)
                    throw new InvalidOperationException(string.Format("Torrent with name '{0}', doesn't exist.", torrentName));

                torrentBot.DeleteTorrent();
            }
        }

        [Action("Returns the progress of the torrent (in %), by a given torrent name. " +
            "If the torrent doesn't exist, InvalidOperationException will be thrown.")]
        public double GetProgressByTorrentName(
            [ParameterDescription("torrentName", "The name of the torrent.", typeof(string), AllowUserInput = true)]
            string torrentName
            )
        {
            if (string.IsNullOrWhiteSpace(torrentName))
                throw new ArgumentException("Insert valid torrent name. Your input: '" + torrentName + "'");

            lock (this.syncLockCommands)
            {
                TorrentBot torrentBot = this.torrentBots.FirstOrDefault(t => t.Torrent.Name == torrentName);
                if (torrentBot == null)
                    throw new InvalidOperationException(string.Format("Torrent with name '{0}', doesn't exist.", torrentName));

                return torrentBot.GetProgress();
            }
        }

        public List<TorrentBot> GetRegisteredTorrentBotsList()
        {
            return this.torrentBots;
        }

        public int GetNumberOfConnectedSeeds(Torrent torrent)
        {
            int numberOfSeeds = 0;
            var torrentList = this.client.GetList().Result.Torrents;
            foreach (var item in torrentList)
            {
                if (item.Hash == torrent.Hash)
                {
                    numberOfSeeds = item.SeedsConnected;
                    break;
                }
            }

            return numberOfSeeds;
        }

        public double GetProgress(Torrent torrent)
        {
            double progress = 0;
            var torrentList = this.client.GetList().Result.Torrents;
            foreach (var item in torrentList)
            {
                if (item.Hash == torrent.Hash)
                {
                    progress = item.Progress / 10f;
                    break;
                }
            }

            return progress;
        }

        public double GetDownloadSpeedInMb(Torrent torrent)
        {
            double downloadSpeedInMb = 1;
            var torrentList = this.client.GetList().Result.Torrents;
            foreach (var item in torrentList)
            {
                if (item.Hash == torrent.Hash)
                {
                    downloadSpeedInMb = (double)item.DownloadSpeed / 1000000;
                    break;
                }
            }

            return downloadSpeedInMb;
        }

        public double GetTorrentSizeInMegabytes(Torrent torrent)
        {
            double size = 0;
            var torrentList = this.client.GetList().Result.Torrents;
            foreach (var item in torrentList)
            {
                if (item.Hash == torrent.Hash)
                {
                    size = item.Size/1000000;
                    break;
                }
            }

            return size;
        }

        private void LoadTorrents()
        {
            if(alreadyRegisteredTorrents.Count != 0)
            {
                foreach (var torrent in alreadyRegisteredTorrents)
                {
                    this.GenerateTorrentBotFromTorrent(torrent);
                    this.oldListOfTorrents = this.client.GetList().Result.Torrents;
                }
            }
        }

        private void StartUTorrentUpdateTimer()
        {
            this.timerForTorrentUpdate.Elapsed += this.TimerForUTorrentUpdateElapsed;
            this.timerForTorrentUpdate.AutoReset = true;
            this.timerForTorrentUpdate.Interval = this.reloadTimeIntervalInMilliseconds;
            this.timerForTorrentUpdate.Start();
        }

        private void GenerateTorrentBotFromTorrent(Torrent bot)
        {
            TorrentBot torrentBot = new TorrentBot(this, bot);
            this.torrentBots.Add(torrentBot);
            torrentBot.StartMonitoringProgress();
            torrentBot.DownloadFinished += this.TorrentBot_DownloadFinished;
            torrentBot.StartDownloadingActionCalled += this.TorrentBot_StartDownloadingActionCalled;
            torrentBot.StopDownloadingActionCalled += this.TorrentBot_StopDownloadingActionCalled;
            torrentBot.PauseDownloadingActionCalled += this.TorrentBot_PauseDownloadingActionCalled;
            torrentBot.DeleteTorrentActionCalled += (tor) => this.TorrentBotDeleteTorrentActionCalledWithActiveBot(tor, torrentBot);
        }

        private void TorrentBotDeleteTorrentActionCalledWithActiveBot(Torrent torrent, TorrentBot bot)
        {
            Action action = () => {
                try
                {
                    this.client.RemoveTorrent(torrent.Hash);
                    bot.Unregister();
                }
                catch (BotIdNotRecognizedException e)
                {
                    throw new InvalidOperationException("The torrent is not found. ", e);
                }
            };

            try
            {
                this.TriggerFromTorrentReceived(action);
            }
            catch
            {
                this.UpdateTorrents(this.client.GetList().Result.Torrents);
            }
        }

        private void TorrentBot_PauseDownloadingActionCalled(Torrent torrent)
        {
            Action action = () => this.client.PauseTorrent(torrent.Hash);
            this.TriggerFromTorrentReceived(action);
        }

        private void TorrentBot_StopDownloadingActionCalled(Torrent torrent)
        {
            Action action = () => this.client.StopTorrent(torrent.Hash); ;
            this.TriggerFromTorrentReceived(action);
        }

        private void TorrentBot_StartDownloadingActionCalled(Torrent torrent)
        {
            Action action = () => this.client.StartTorrent(torrent.Hash);
            this.TriggerFromTorrentReceived(action);

        }

        private void TriggerFromTorrentReceived(Action action)
        {
            this.UpdateTorrents(this.client.GetList().Result.Torrents);
            action();
            this.UpdateTorrents(this.client.GetList().Result.Torrents);
        }

        private void RemoveFromActiveBotsList(TorrentBot item)
        {
            this.torrentBots.Remove(item);
        }

        private void TorrentBot_DownloadFinished(Torrent torrent)
        {
            this.OnDownloadFinished(torrent.Name);
        }

        private void TimerForUTorrentUpdateElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.timerForTorrentUpdate.Stop();
            var newListOfTorrents = client.GetList().Result.Torrents;
            if (!this.AreEqual(this.oldListOfTorrents, newListOfTorrents))
            {
                this.UpdateTorrents(newListOfTorrents);
            }

            this.timerForTorrentUpdate.Start();
        }

        private void UpdateTorrents(IList<Torrent> newListOfTorrents)
        {
            this.UpdateForDelete(newListOfTorrents);
            this.UpdateForAdd(newListOfTorrents);
            this.SetNewOldListOfTorrents(newListOfTorrents);
        }

        private void SetNewOldListOfTorrents(IList<Torrent> newListOfTorrents)
        {
            lock(this.syncLock)
            {
                this.oldListOfTorrents.Clear();
                foreach (var item in newListOfTorrents)
                {
                    this.oldListOfTorrents.Add(item);
                }
            }
        }

        private void UpdateForAdd(IList<Torrent> newListOfTorrents)
        {
            lock (this.syncLock)
            {
                foreach (var item in newListOfTorrents)
                {
                    int count = this.oldListOfTorrents.Count;
                    if (!this.oldListOfTorrents.Any(torrent => torrent.Hash == item.Hash))
                    {
                        string name = item.Name;
                        this.GenerateTorrentBotFromTorrent(item);
                    }
                }
            }    
        }

        private void UpdateForDelete(IList<Torrent> newListOfTorrents)
        {
            lock (this.syncLock)
            {
                foreach (var item in this.oldListOfTorrents)
                {
                    //if (!newListOfTorrents.Contains(item))
                    if (!newListOfTorrents.Any(torrent => torrent.Hash == item.Hash))
                    {
                        this.DeleteTheBotViaTorrent(item);
                    }
                }
            }   
        }

        private bool AreEqual(IList<Torrent> oldListOfTorrents, IList<Torrent> newListOfTorrents)
        {
            if (oldListOfTorrents.Count != newListOfTorrents.Count)
            {
                return false;
            }

            foreach (var itemOld in oldListOfTorrents)
            {
                bool containsItem = newListOfTorrents.Any(item => item.Hash == itemOld.Hash);
                if (!containsItem)
                    return false;
            }

            return true;
        }

        private void DeleteTheBotViaTorrent(Torrent torrent)
        {
            List<TorrentBot> toBeRemoved = new List<TorrentBot>();
            foreach (var item in this.torrentBots)
            { 
                if (torrent.Hash == item.GetTorrentHash())
                {
                    toBeRemoved.Add(item);
                }   
            }

            foreach (var item in toBeRemoved)
            {
                item.Unregister();
            }
        }
    }
}
