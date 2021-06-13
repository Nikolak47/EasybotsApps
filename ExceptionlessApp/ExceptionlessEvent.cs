using Exceptionless;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExceptionlessApp
{
    [Serializable]
    [Easybots.DataModels.EasybotsDataModel]
    public class ExceptionlessEvent
    {
        protected const string StackTracePropertyName = "Stack Trace";

        [Easybots.DataModels.EasybotsDataMember]
        public string Message;

        [Easybots.DataModels.EasybotsDataMember]
        public HashSet<string> Tags = new HashSet<string>();

        [Easybots.DataModels.EasybotsDataMember]
        public Dictionary<string, string> ExtendedData = new Dictionary<string, string>();

        [Easybots.DataModels.EasybotsDataMember]
        public string Source;

        [Easybots.DataModels.EasybotsDataMember]
        public string Type;

        public ExceptionlessEvent(string message)
        {
            this.Message = message;
        }

        public void AddTag(string tag)
        {
            this.Tags.Add(tag);
        }

        public void AddExtendedData(string key, string value)
        {
            this.ExtendedData[key] = value;
        }

        internal EventBuilder CreateEvent(ExceptionlessClient exceptionlessClient)
        {
            EventBuilder eventBuilder = this.CreateEventObjectWithMessage(exceptionlessClient);
            if (this.Tags != null)
            {
                eventBuilder.AddTags(this.Tags.ToArray());
            }

            if (this.ExtendedData != null)
            {
                foreach (var propertyKeyValue in this.ExtendedData)
                {
                    eventBuilder.SetProperty(propertyKeyValue.Key, propertyKeyValue.Value);
                }
            }

            return eventBuilder;
        }

        protected virtual EventBuilder CreateEventObjectWithMessage(ExceptionlessClient exceptionlessClient)
        {
            return exceptionlessClient.CreateEvent().SetMessage(this.Message);
        }
    }

    [Serializable]
    [Easybots.DataModels.EasybotsDataModel]
    public class ExceptionlessFeatureUsageEvent : ExceptionlessEvent
    {
        public string Feature { get; private set; }

        public ExceptionlessFeatureUsageEvent(string feature)
            : base(feature)
        {
            this.Feature = feature;
        }

        protected override EventBuilder CreateEventObjectWithMessage(ExceptionlessClient exceptionlessClient)
        {
            EventBuilder eventBuilder = exceptionlessClient.CreateFeatureUsage(this.Feature);
            return eventBuilder;
        }
    }

    [Serializable]
    [Easybots.DataModels.EasybotsDataModel]
    public class ExceptionlessExceptionEvent : ExceptionlessEvent
    {
        public Exception Exception { get; private set; }
                
        public ExceptionlessExceptionEvent(Exception exception)
            : base(exception.Message)
        {
            this.Exception = exception;
        }

        protected override EventBuilder CreateEventObjectWithMessage(ExceptionlessClient exceptionlessClient)
        {
            EventBuilder exceptionBuilder = exceptionlessClient.CreateException(this.Exception)
                .SetProperty(StackTracePropertyName, this.Exception.StackTrace);
            return exceptionBuilder;
        }
    }

    [Serializable]
    [Easybots.DataModels.EasybotsDataModel]
    public class ExceptionlessErrorEvent : ExceptionlessEvent
    {
        [Easybots.DataModels.EasybotsDataMember]
        public string ExceptionTypeFullName;

        [Easybots.DataModels.EasybotsDataMember]
        public string StackTrace;
                
        public ExceptionlessErrorEvent(string message)
            : base(message)
        {
        }

        protected override EventBuilder CreateEventObjectWithMessage(ExceptionlessClient exceptionlessClient)
        {
            var myException = new Error(this.Message, this.StackTrace);
            EventBuilder exceptionBuilder = exceptionlessClient.CreateException(myException)
                .SetProperty(StackTracePropertyName, this.StackTrace)
                .SetProperty("Exception Type", this.ExceptionTypeFullName);
            return exceptionBuilder;
        }
    }

    [Serializable]
    [Easybots.DataModels.EasybotsDataModel]
    public class ExceptionlessLogEvent : ExceptionlessEvent
    {
        [Easybots.DataModels.EasybotsDataMember]
        public string LogLevel;

        public ExceptionlessLogEvent(string message)
            : base(message)
        {
        }

        protected override EventBuilder CreateEventObjectWithMessage(ExceptionlessClient exceptionlessClient)
        {
            Exceptionless.Logging.LogLevel logLevel = Exceptionless.Logging.LogLevel.FromString(this.LogLevel == null ? string.Empty : this.LogLevel);
            EventBuilder logBuilder = exceptionlessClient.CreateLog(this.Source, this.Message, logLevel);
            return logBuilder;
        }
    }
}
