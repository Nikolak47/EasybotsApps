using Easybots.Apps;
using Easybots.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IpCameraApp
{
    public class IpCameraBot : Easybot
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string CGI { get; set; }
        public string DisplayName { get; set; }
        public RouterDevice Device { get; set; }
        public IpCameraBotInfo IpCameraInfo { get; set; }
        private int timeOutDelayInMilliSeconds = Int32.Parse(ConfigurationManager.AppSettings["timeOutForTakingPictureInMilliseconds"]);
        public IpCameraBot(IpCameraBotInfo info) : base(info.DisplayName)
        {
            this.Username = info.Username;
            this.Password = info.Password;
            this.Device = info.Device;
            this.CGI = info.CGI;
            this.DisplayName = info.DisplayName;
            this.IpCameraInfo = info;
        }

        [Action("Takes picture and returns it as SerializableImage")]
        public SerializableImage TakePicture()
        {
            try
            {
                var result = this.TakePictureMethodAsync().Result;
                return result;
            }
            catch (AggregateException aggregate)
            {
                // Algorythm description:
                // GetAsync(...) always throws AggregateException, which contains more and different exceptions as inner exceptions.
                // If aggregateException.Message is shown in MessageBox, the first exception message will be shown (usually 'One or more exceptions occured')
                // which is not so usefull for the user. In the first part, I'm extracting TaskCanceledException and ArgumentException (invalid CGI or credentials) to give
                // the user the correct info about the exception. In the second part (if the exception isn't TaskCanceled or Argument), I don't want to be 'too smart'
                // and I just show the user the info from ALL INNER EXCEPTIONS in one message (otherwise, they are hidden and only the first one is shown).
                // The user gets the original exceptions as innerExceptions, so I'm not hidding anything.

                foreach (var exception in aggregate.InnerExceptions)
                {
                    if (exception.InnerException is TaskCanceledException)
                    {
                        throw new ActionTookTooLongToRespondException(string.Format("'{0}' took too long to respond. Make sure the correct CGI command is inserted.", nameof(this.TakePicture)));
                    }
                }

                int countOfInnerExceptions = this.GetCountOfInnerExceptions(aggregate.InnerException, 0);
                if (this.ContainsArgumentException(aggregate, countOfInnerExceptions))
                {
                    throw new InvalidArgumentsException(this.CGI, aggregate);
                }

                string groupExceptionMessage = this.GetGroupExceptionMessage(aggregate, string.Empty, countOfInnerExceptions, 0);
                throw new AggregateException(string.Format("{0} exceptions has occured. Messages from this exceptions:{1}{2}", countOfInnerExceptions, Environment.NewLine, groupExceptionMessage), aggregate.InnerExceptions);
            }
        }

        private bool ContainsArgumentException(Exception exception, int countOfInnerExceptions)
        {
            Exception exc = exception;
            for (int i = 0; i < countOfInnerExceptions; i++)
            {
                if(this.GetInnerExceptionType(exc) == typeof(ArgumentException))
                {
                    return true;
                }

                exc = exc.InnerException;
            }

            return false;
        }
    
        private Type GetInnerExceptionType(Exception exc)
        {
            return exc.InnerException.GetType();
        }

        private string GetGroupExceptionMessage(Exception exception, string groupMessage, int countOfInnerExceptions, int counter)
        {
            if(counter == countOfInnerExceptions)
            {
                return groupMessage;
            }
            else
            {
                counter++;
                groupMessage += string.Format("{0}{1}{2}", "- ", exception.Message, Environment.NewLine);
                return this.GetGroupExceptionMessage(exception.InnerException, groupMessage, countOfInnerExceptions, counter);
            }
        }

        private int GetCountOfInnerExceptions(Exception exception, int numberOfInnerExceptions)
        {
            if(exception == null)
            {
                return numberOfInnerExceptions;
            }
            else
            {
                numberOfInnerExceptions++;
                return this.GetCountOfInnerExceptions(exception.InnerException, numberOfInnerExceptions);
            }
        }

        private async Task<SerializableImage> TakePictureMethodAsync()
        {
            string cgiUri = string.Format("http://{0}/{1}", this.Device.IpAddress, this.CGI);
            NetworkCredential credentials = new NetworkCredential(this.Username, this.Password);
            var cts = new CancellationTokenSource(this.timeOutDelayInMilliSeconds);
            var handler = new HttpClientHandler();
            handler.Credentials = credentials;
            HttpClient c = new HttpClient(handler);
            using (var imageAsResponse = await c.GetAsync(cgiUri, cts.Token).Result.Content.ReadAsStreamAsync())
            {
                return new SerializableImage(Bitmap.FromStream(imageAsResponse));
            }
        }
    }
}