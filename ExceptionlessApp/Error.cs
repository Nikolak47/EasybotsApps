using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExceptionlessApp
{
    internal class Error : Exception
    {
        /* 
         * Dev Note: We need this class so that we can set our own stacktrace to the thrown exception 
         * (otherwise the .NET runtime set the stack trace where the exception was thrown) 
         */

        private readonly string _stackTrace;

        public override string StackTrace
        {
            get { return this._stackTrace; }
        }

        public Error(string message, string stackTraceOrNull)
            : base(message)
        {
            this._stackTrace = stackTraceOrNull == null ? string.Empty : stackTraceOrNull;
        }
    }
}
