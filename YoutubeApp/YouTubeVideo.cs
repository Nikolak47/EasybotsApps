using Easybots.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace YoutubeApp
{
    [EasybotsDataModel]
    [Serializable]
    public class YouTubeVideo
    {
        [EasybotsDataMember]
        public string Url;

        [EasybotsDataMember]
        public string Title;

        [EasybotsDataMember]
        public TimeSpan DurationInSeconds;

        public string DurationString;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url">The url of the YouTube video</param>
        /// <param name="title">The title of the YouTube video</param>
        /// <param name="duration">The duration of the YouTube video, as string, in this format (hours:minutes:seconds) or (minutes:seconds)</param>
        public YouTubeVideo(string url, string title, string duration)
        {
            this.Url = url;
            this.Title = title;
            this.DurationString = duration;
            this.DurationInSeconds = this.ConvertDurationStringToSeconds(duration);
        }

        private TimeSpan ConvertDurationStringToSeconds(string duration)
        {
            string[] parts = duration.Split(':');
            int partsCount = parts.Length;
            if(partsCount == 3)
            {
                int hours = Int32.Parse(parts[0]);
                int minutes = Int32.Parse(parts[1]);
                int seconds = Int32.Parse(parts[2]);
                TimeSpan t = new TimeSpan(hours, minutes, seconds);
                return t;
            }
            else if(partsCount == 2)
            {
                int minutes = Int32.Parse(parts[0]);
                int seconds = Int32.Parse(parts[1]);
                TimeSpan t = new TimeSpan(0, minutes, seconds);
                return t;
            }
            else
            {
                int seconds = Int32.Parse(parts[0]);
                TimeSpan t = new TimeSpan(0, 0, seconds);
                return t;
            }
        }

        public override string ToString()
        {
            string result = string.Format("{0} ({1})", HttpUtility.HtmlDecode(this.Title), this.DurationString);
            byte[] bytes = Encoding.Default.GetBytes(result);
            result = Encoding.UTF8.GetString(bytes);
            return result;
        }
    }
}
