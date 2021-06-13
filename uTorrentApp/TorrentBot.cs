using Easybots.Apps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UTorrent.Api;
using UTorrent.Api.Data;
using Easybots.Apps.Exceptions;
using System.Windows.Forms;

namespace uTorrentApp
{
    public delegate void TorrentStatusEventHandler(Torrent torrent);
    class TorrentBot : Easybot
    {
        public const int CheckProgressIntervalInMs = 5000;
        public event TorrentStatusEventHandler DownloadFinished;
        public event TorrentStatusEventHandler StartDownloadingActionCalled;
        public event TorrentStatusEventHandler StopDownloadingActionCalled;
        public event TorrentStatusEventHandler PauseDownloadingActionCalled;
        public event TorrentStatusEventHandler DeleteTorrentActionCalled;
        public Torrent Torrent { get; set; }
        private uTorrentClientBot uTorrentBot;
        private System.Timers.Timer timerForCheckingDownloadFinished = new System.Timers.Timer();
        public TorrentBot(uTorrentClientBot uTorrentBot, Torrent torrent) : base(torrent.Name + " Torrent", registerNow: true)
        {
            this.uTorrentBot = uTorrentBot;
            this.Torrent = torrent;
            this.DownloadFinished += this.TorrentBot_DownloadFinished;
        }

        public void TorrentBot_DownloadFinished(Torrent torrent)
        {
            this.OnDownloadFinished(torrent.Name);
        }

        public string GetTorrentHash()
        {
            return this.Torrent.Hash;
        }

        public string GetTorrentName()
        {
            return this.Torrent.Name;
        }

        [Action("Starts downloading the torrent.")]
        public void StartDownloading()
        {
            this.StartDownloadingActionCalled?.Invoke(this.Torrent);
        }

        [Action("Stops downloading the torrent.")]
        public void StopDownloading()
        {
            this.StopDownloadingActionCalled?.Invoke(this.Torrent);
        }

        [Action("Pauses the download process of the torrent.")]
        public void PauseDownloading()
        {
            this.PauseDownloadingActionCalled?.Invoke(this.Torrent);
        }

        [Action("Deletes the torrent.")]
        public void DeleteTorrent()
        {
            this.DeleteTorrentActionCalled?.Invoke(this.Torrent);
        }

        [return: ParameterDescription("download speed", "The download speed of the torrent (Mb/s).", typeof(double), AllowUserInput = false)]
        [Action("Returns the download speed of the torrent (Mb/s).")]
        public double GetDownloadSpeedInMb()
        {
            return this.uTorrentBot.GetDownloadSpeedInMb(this.Torrent);
        }

        [return: ParameterDescription("connected seeds", "The number of seeds connected to the torrent.", typeof(int), AllowUserInput = false)]
        [Action("Returns the number of connected seeds to the torrent")]
        public int GetNumberOfConnectedSeeds()
        {
            return this.uTorrentBot.GetNumberOfConnectedSeeds(this.Torrent);
        }

        [return: ParameterDescription("progress", "The progress of the torrent (%).", typeof(double), AllowUserInput = false)]
        [Action("Returns the progress of the torrent (in %)")]
        public double GetProgress()
        {
            return this.GetProgressInPercent();
        }

        [return: ParameterDescription("size", "The size of the torrent in megabytes.", typeof(double), AllowUserInput = false)]
        [Action("Returns the size of the torrent in megabytes.")]
        public double GetTorrentSizeInMb()
        {
            return this.uTorrentBot.GetTorrentSizeInMegabytes(this.Torrent);
        }

        public double GetProgressInPercent()
        {
            return this.uTorrentBot.GetProgress(this.Torrent);
        }

        public void StartMonitoringProgress()
        {
            this.timerForCheckingDownloadFinished.Elapsed += this.CheckProgress;
            this.timerForCheckingDownloadFinished.Interval = CheckProgressIntervalInMs;
            this.timerForCheckingDownloadFinished.AutoReset = true;
            this.timerForCheckingDownloadFinished.Start();
        }

        private void CheckProgress(object sender, System.Timers.ElapsedEventArgs e)
        {
            double getProgressValue = this.GetProgressInPercent();
            if (getProgressValue == 100.0)
            {
                this.timerForCheckingDownloadFinished.Stop();
                this.timerForCheckingDownloadFinished.AutoReset = false;
                this.timerForCheckingDownloadFinished.Enabled = false;
                this.DownloadFinished?.Invoke(this.Torrent);
            }
        }

        [return: ParameterDescription("torrent name", "The name of the torrent. ", typeof(string), AllowUserInput = false)]
        [Trigger("Notifies when the torrent is downloaded and returns its name. ")]
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        public string OnDownloadFinished(string torrentName)
        {
            this.DownloadFinished -= this.TorrentBot_DownloadFinished;
            this.TriggerInEasybotsPlatform(torrentName);
            return torrentName;
        }
    }
}