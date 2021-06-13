using Easybots.Apps;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Threading;
using Exceptionless;
using Exceptionless.Models;
using Exceptionless.Models.Data;

namespace ExceptionlessApp
{
    internal class ExceptionlessBot : Easybots.Apps.Easybot
    {
        private ExceptionlessClient client = new ExceptionlessClient();
        public string ProjectName { get; set; }
        public string ApiKey { get; set; }
        
        public ExceptionlessBot(ExceptionlessBotInfo botInfo) 
            : base(botInfo.ProjectName)
        {
            if (botInfo == null)
                throw new ArgumentNullException(nameof(botInfo));

            this.ProjectName = botInfo.ProjectName;
            this.ApiKey = botInfo.ApiKey;
            this.client.Startup(this.ApiKey);            
        }

        [Action("Submits log to 'Log Messages' panel in your project in exceptionless.com")]
        public void SubmitLog(
            [ParameterDescription("Message", "The message you want to send", typeof(string), Optional = false, Order = 0)]
            [ParameterDescription("Source", "The source where you want to send the message from", typeof(string), Optional = true, Order = 1)]
            [ParameterDescription("Log Level", "The log level of the message.\nExample: Error, Fatal, Info, Critical, Debug, Trace, Warning... ", typeof(string), Optional = true, Order = 2)]
            string[] inputs)
        {
            string message = inputs[0];
            string source = inputs[1];
            string logLevel = inputs[2];
            ExceptionlessLogEvent logEvent = CreateLogEvent(message, source, logLevel);
            logEvent.CreateEvent(this.client).Submit();            
        }

        [Action("Creates an event of type 'Log Message'. This Action will not submit the event to exceptionless.com")]
        public ExceptionlessEvent CreateLog(
            [ParameterDescription("Message", "The message you want to send", typeof(string), Optional = false, Order = 0)]
            [ParameterDescription("Source", "The source of the message", typeof(string), Optional = true, Order = 1)]
            [ParameterDescription("Log level", "The log level of the message.\nExample: Error, Fatal, Info, Critical, Debug, Trace, Warn... ", typeof(string), Optional = true, Order = 2)]
            string[] inputs)
        {
            string message = inputs[0];
            string source = inputs[1];
            string logLevel = inputs[2];
            ExceptionlessLogEvent logEvent = CreateLogEvent(message, source, logLevel);
            return logEvent;
        }

        [Action("Submits event to 'All Events' in your project in exceptionless.com")]
        public void SubmitEvent(
            [ParameterDescription("Message", "The message you want to send through the event", typeof(string), Optional = false, Order = 0)]
            [ParameterDescription("Type", "The type of the event", typeof(string), Optional = false, Order = 1)]
            [ParameterDescription("Source", "The source of the event", typeof(string), Optional = true, Order = 2)]
            string[] inputs)
        {
            string message = inputs[0];
            string type = inputs[1];
            string source = inputs[2];
            ExceptionlessEvent genericEvent = CreateGenericEvent(message, type, source);
            EventBuilder eventBuilder = genericEvent.CreateEvent(client);
            eventBuilder.Submit();
        }

        [Action("Creates event that will be shown in the 'All Events' panel in exceptionless.com project. This action will not submit the event.")]
        public ExceptionlessEvent CreateEvent(
            [ParameterDescription("Message", "The message you want to send through the event", typeof(string), Optional = false, Order = 0)]
            [ParameterDescription("Type", "The type of the event", typeof(string), Optional = false, Order = 1)]
            [ParameterDescription("Source", "The source of the event", typeof(string), Optional = true, Order = 2)]
            string[] inputs)
        {
            string message = inputs[0];
            string type = inputs[1];
            string source = inputs[2];
            ExceptionlessEvent genericEvent = CreateGenericEvent(message, type, source);
            return genericEvent;
        }
        
        [Action("Submits exception to 'Exceptions' in your project in exceptionless.com")]
        public void SubmitError(
           [ParameterDescription("Message", "The exception message", typeof(string), Optional = false, Order = 0)]
           [ParameterDescription("Type", "The type of the exception/\nExample: InvalidOperationException, ArgumentException, ArgumentNullException...", typeof(string), Optional = true, Order = 1)]
           [ParameterDescription("StackTrace", "The stack trace to be shown for this exception", typeof(string), Optional = true, Order = 2)]
            string[] inputs)
        {
            string message = inputs[0];
            string type = inputs[1];
            string stackTrace = inputs[2];            
            ExceptionlessErrorEvent exceptionEvent = new ExceptionlessErrorEvent(message);
            exceptionEvent.StackTrace = stackTrace;
            exceptionEvent.ExceptionTypeFullName = type;
            exceptionEvent.CreateEvent(this.client).Submit();            
        }

        [Action("Creates an event of type 'exception'. This Action will not submit the exception to exceptionless.com")]
        public ExceptionlessEvent CreateError(
            [ParameterDescription("Message", "The message of the exception", typeof(string), Order = 0)]
            [ParameterDescription("StackTrace", "The stack trace to be shown for this exception", typeof(string), Order = 1)]
            [ParameterDescription("ExceptionTypeName", "The name of the type of the exception", typeof(string), Order = 2, Optional = true)]
            string[] inputs)
        {
            string message = inputs[0];
            string stackTrace = inputs[1];
            string exceptionTypeName = inputs[2];
            var exceptionlessEvent = new ExceptionlessErrorEvent(message);
            exceptionlessEvent.StackTrace = stackTrace;
            exceptionlessEvent.ExceptionTypeFullName = exceptionTypeName;
            return exceptionlessEvent;
        }

        [Action("Submits exception to 'Exceptions' in your project in exceptionless.com")]
        public void SubmitException(
           [ParameterDescription("Exception", "The exception to be submitted", typeof(Exception))]
            Exception exception)
        {
            this.client.CreateException(exception).Submit();
        }

        [Action("Creates an event of type 'exception'. This Action will not submit the exception to exceptionless.com")]
        public ExceptionlessEvent CreateException(
            [ParameterDescription("Exception", "The exception that will be the base for the created event", typeof(string), Order = 0)]            
            Exception exception)
        {
            var exceptionlessEvent = new ExceptionlessExceptionEvent(exception);            
            return exceptionlessEvent;
        }

        [Action("Submits usage of a feature to 'Feature Usages' in your project in Exceptionless.")]
        public void SubmitFeatureUsage(
           [ParameterDescription("Feature", "The name of the feature", typeof(string), Optional = false, Order = 0)]
           string feature)
        {
            this.client.SubmitFeatureUsage(feature);            
        }

        [Action("Creates an event of type 'Feature Usages'. This Action will not submit the event to exceptionless.com")]
        public ExceptionlessEvent CreateFeatureUsage(
            [ParameterDescription("Feature", "The name of the feature", typeof(string))]
            string feature)
        {
            var exceptionlessEvent = new ExceptionlessFeatureUsageEvent(feature);
            return exceptionlessEvent;
        }
        
        [Action("Adds a collection of tags to an exceptionless event, and returns the event with the added tags.")]
        public ExceptionlessEvent AddTags(
            [ParameterDescription("Event", "The exceptionless event object where the tags will be added", typeof(ExceptionlessEvent), Order = 0)]
            [ParameterDescription("Tags", "The collection of the tags to be added", typeof(IEnumerable<string>), Order = 1)]            
            object[] inputs)
        {
            ExceptionlessEvent exceptionlessEvent = (ExceptionlessEvent)inputs[0];
            IEnumerable<string> tags = (IEnumerable<string>) inputs[1];
            foreach (string tag in tags)
            {
                exceptionlessEvent.AddTag(tag);
            }

            return exceptionlessEvent;
        }

        [Action("Adds a tag to an exceptionless event, and returns the event with the added tag.")]
        public ExceptionlessEvent AddTag(
            [ParameterDescription("Event", "The exceptionless event object where the tag will be added", typeof(ExceptionlessEvent), Order = 0)]
            [ParameterDescription("Tag", "The tag to be added", typeof(string), Order = 1)]
            object[] inputs)
        {
            ExceptionlessEvent exceptionlessEvent = (ExceptionlessEvent)inputs[0];
            string tag = (string)inputs[1];
            exceptionlessEvent.AddTag(tag);
            return exceptionlessEvent;
        }

        [Action]
        public ExceptionlessEvent AddExtendedDataCollection(
            [ParameterDescription("Event", "The exceptionless event object where the extended data will be added", typeof(ExceptionlessEvent), Order = 0)]
            [ParameterDescription("Extended Data", "The extended data collection to be added", typeof(IDictionary<string, string>), Order = 1)]
            object[] inputs)
        {
            ExceptionlessEvent exceptionlessEvent = (ExceptionlessEvent)inputs[0];
            IDictionary<string, string> properties = (IDictionary<string, string>)inputs[1];
            foreach (KeyValuePair<string, string> keyValue in properties)
            {
                exceptionlessEvent.AddExtendedData(keyValue.Key, keyValue.Value);
            }

            return exceptionlessEvent;
        }

        [Action]
        public ExceptionlessEvent AddExtendedData(
            [ParameterDescription("Event", "The exceptionless event object where the extended data will be added", typeof(ExceptionlessEvent), Order = 0)]
            [ParameterDescription("Key", "The key of the extended data", typeof(string), Order = 1)]
            [ParameterDescription("Value", "The value of the extended data", typeof(string), Order = 2)]
            object[] inputs)
        {
            ExceptionlessEvent exceptionlessEvent = (ExceptionlessEvent)inputs[0];
            string key = (string)inputs[1];
            string value = (string)inputs[2];
            exceptionlessEvent.AddExtendedData(key, value);
            return exceptionlessEvent;
        }

        [Action]
        public ExceptionlessEvent SetSource(
            [ParameterDescription("Event", "The exceptionless event object where the source will be added", typeof(ExceptionlessEvent), Order = 0)]
            [ParameterDescription("Source", "The source of the event", typeof(string), Order = 1)]
            object[] inputs)
        {
            ExceptionlessEvent exceptionlessEvent = (ExceptionlessEvent)inputs[0];
            string source = (string)inputs[1];
            exceptionlessEvent.Source = source;
            return exceptionlessEvent;
        }


        [Action("Submits the event to exceptionless.com")]
        public void Submit(
            [ParameterDescription("Event", "The exceptionless event object to be submitted", typeof(ExceptionlessEvent))]
            ExceptionlessEvent exceptionlessEvent)
        {
            EventBuilder eventBuilder = exceptionlessEvent.CreateEvent(this.client);
            eventBuilder.Submit();
        }

        private static ExceptionlessLogEvent CreateLogEvent(string message, string source, string logLevel)
        {
            var logEvent = new ExceptionlessLogEvent(message);
            logEvent.Source = source;
            logEvent.LogLevel = logLevel;
            return logEvent;
        }

        private static ExceptionlessEvent CreateGenericEvent(string message, string type, string source)
        {
            ExceptionlessEvent genericEvent = new ExceptionlessEvent(message);
            genericEvent.Type = type;
            genericEvent.Source = source;
            return genericEvent;
        }
    }
}