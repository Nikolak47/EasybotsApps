using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

[assembly: Easybots.Apps.EasybotsApp("YouTube App")]

namespace YoutubeApp
{
    public partial class Form1 : Form
    {
        private YouTubeBot youtubeBot;
        public Form1()
        {
            this.InitializeComponent();
            var link = Easybots.Apps.EasybotsLink.CreateLink();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.youtubeBot = new YouTubeBot(this.webBrowser);
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            string urlOfTheVideo = this.urlTextBox.Text;
            if (string.IsNullOrWhiteSpace(urlOfTheVideo))
            {
                MessageBox.Show("Insert YouTube video/playlist URL, then try again.", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                this.youtubeBot.PlayVideoFromURL(urlOfTheVideo);
            }
            catch(Exception exc)
            {
                MessageBox.Show(exc.Message, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            try
            {
                this.youtubeBot.StopVideo();
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void searchButton_Click(object sender, EventArgs e)
        {
            string searchQuery = this.searchTextBox.Text;
            if(string.IsNullOrWhiteSpace(searchQuery))
            {
                MessageBox.Show("Insert search query, then try again", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                try
                {
                    this.searchResultsListBox.Items.Clear();
                    YouTubeVideo[] videos = this.youtubeBot.SearchVideos(searchQuery);
                    foreach (var item in videos)
                    {
                        this.searchResultsListBox.Items.Add(item);
                    }
                }
                catch(Exception exc)
                {
                    MessageBox.Show(exc.Message, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void searchResultsListBox_DoubleClick(object sender, EventArgs e)
        {
            YouTubeVideo video = (YouTubeVideo)this.searchResultsListBox.SelectedItem;
            if(video == null)
            {
                MessageBox.Show("Perform a search first, then try again.", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else
            {
                try
                {
                    this.youtubeBot.PlayYouTubeVideo(video, false);
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.Message, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void searchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                this.searchButton_Click(this, EventArgs.Empty);
            }
        }

        private void urlTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.startButton_Click(this, EventArgs.Empty);
            }
        }

        private void playAllButton_Click(object sender, EventArgs e)
        {
            if(this.searchResultsListBox.Items.Count == 0)
            {
                MessageBox.Show("Perform a search first, then try again.", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else
            {
                try
                {
                    List<YouTubeVideo> videosAsList = new List<YouTubeVideo>();
                    var collection = this.searchResultsListBox.Items;
                    foreach (var item in collection)
                    {
                        videosAsList.Add((YouTubeVideo)item);
                    }

                    YouTubeVideo[] videos = videosAsList.ToArray();
                    this.youtubeBot.PlayAllVideos(videos);
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.Message, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}