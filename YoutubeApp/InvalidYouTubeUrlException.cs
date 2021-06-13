using System;
using System.Runtime.Serialization;

namespace YoutubeApp
{
    internal class InvalidYouTubeUrlException : Exception
    {
        public InvalidYouTubeUrlException(string urlParameter) 
            : base(CreateMessage(urlParameter))
        {   
        }

        public InvalidYouTubeUrlException(string urlParameter, Exception innerException) 
            : base(CreateMessage(urlParameter), innerException)
        {         
        }    
        
        private static string CreateMessage(string urlParameter)
        {
            return string.Format("Invalid URL. The URL is not from a YouTube video or YouTube playlist. Url: {0}", urlParameter);
        }
    }
}