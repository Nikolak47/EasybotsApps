using Easybots.Apps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using YoutubeSearch;
using System.Text.RegularExpressions;
using static YoutubeApp.YouTubeBot;

namespace YoutubeApp
{
    public class YouTubeBot : Easybot
    {
        public string UrlOfTheVideo { get; set; }
        private WebBrowser webBrowser;
        private object syncLockPlayVideo = new object();
        public bool isAllowedToPlayAllVideos = false;
        public YouTubeBot(WebBrowser webBrowser) : base("YouTube Bot")
        {
            this.webBrowser = webBrowser;
        }

        [Action("Starts a YouTube video/playlist from a specified URL. If invalid URL is inserted, an exception will be thrown.")]
        public void PlayVideoFromURL(
            [ParameterDescription("url", "The url of the youtube video", typeof(string))]
            string url)
        {
            lock (this.syncLockPlayVideo)
            {
                bool isValid = this.IsValidYouTubeUrl(url);
                if (!isValid)
                {
                    throw new InvalidYouTubeUrlException(url);
                }

                this.PlayYouTubeVideoFromUrl(url, false);
            }
        }

        [Action("Returns an array of YouTubeVideos matching the inserted search word.\nExample: If you insert 'the beatles' as a search word, the first few videos of 'The Beatles' will be returned.")]
        public YouTubeVideo[] SearchVideos(
            [ParameterDescription("search query", "Search query", typeof(string), AllowUserInput = true)]
            string searchQuery)
        {
            List<YouTubeVideo> youTubeVideos = new List<YouTubeVideo>();
            int queryPages = 1;
            var search = new VideoSearch();
            foreach (VideoInformation item in search.SearchQuery(searchQuery, queryPages))
            {
                YouTubeVideo youTubeVideo = new YouTubeVideo(item.Url, item.Title, item.Duration);
                youTubeVideos.Add(youTubeVideo);
            }

            return youTubeVideos.ToArray();
        }

        [Action("Plays all inserted YouTube videos as a playlist.")]
        public async void PlayAllVideos(
            [ParameterDescription("YouTubeVideoCollection", "'YouTubeVideo' collection.", typeof(YouTubeVideo[]))]
            object videos)
        {
            this.isAllowedToPlayAllVideos = true;
            var vids = (YouTubeVideo[])videos;
            foreach (var item in vids)
            {
                if(!this.isAllowedToPlayAllVideos)
                {
                    break;
                }

                this.PlayYouTubeVideo(item, true);
                await Task.Delay(item.DurationInSeconds);
            }
        }

        [Action("Returns a YouTube video from 'YouTubeVideo' collection, by index")]
        public YouTubeVideo GetVideoByIndex(
            [ParameterDescription("YouTubeVideoCollection", "'YouTubeVideo' collection", typeof(YouTubeVideo[]), Order = 0)]
            [ParameterDescription("index", "Index - zero based", typeof(int), AllowUserInput = true, Order = 1)]
            object[] inputs)
        {
            YouTubeVideo[] videos = (YouTubeVideo[])inputs[0];
            int index = (Int32)inputs[1];
            int maximumAllowedIndex = videos.Count<YouTubeVideo>() - 1;
            if (index >= videos.Count<YouTubeVideo>())
            {
                throw new ArgumentException("Insert valid index, then try again.\nMaximum allowed index: " + maximumAllowedIndex);
            }

            return videos[index];
        }

        [Action("Starts a YouTube video.")]
        public void PlayVideo(
            [ParameterDescription("YouTubeVideo", "'YouTubeVideo' got from 'SearchYouTubeVideos' action.", typeof(YouTubeVideo))]
            YouTubeVideo video)
        {
            this.PlayYouTubeVideo(video, false);
        }

        [Action("Stops the YouTube video that plays at the moment.")]
        public void StopVideo()
        {
            this.webBrowser.DocumentText = StopVideoHtml.stopVideo;
        }

        public void PlayYouTubeVideo(YouTubeVideo video, bool isFromPlayAllVideos)
        {
            lock (this.syncLockPlayVideo)
            {
                this.UrlOfTheVideo = video.Url;
                this.PlayYouTubeVideoFromUrl(this.UrlOfTheVideo, isFromPlayAllVideos);
            }
        }


        private void PlayYouTubeVideoFromUrl(string url, bool isFromPlayAllVideos)
        {
            if(isFromPlayAllVideos)
            {
                this.isAllowedToPlayAllVideos = true;
            }
            else
            {
                this.isAllowedToPlayAllVideos = false;
            }

            this.UrlOfTheVideo = url;
            this.webBrowser.ScriptErrorsSuppressed = true;
            this.webBrowser.Navigate(this.UrlOfTheVideo);
        }

        private bool IsValidYouTubeUrl(string url)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "HEAD";
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                string responseString = response.ResponseUri.ToString();
                if(responseString == "https://www.youtube.com/")
                {
                    throw new InvalidYouTubeUrlException(url);
                }

                return responseString.Contains("youtube.com");
            }
        }
    }
}