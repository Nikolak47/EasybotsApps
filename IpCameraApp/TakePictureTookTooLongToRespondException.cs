using System;
using System.Runtime.Serialization;

namespace IpCameraApp
{
    public class ActionTookTooLongToRespondException : Exception
    {
        public ActionTookTooLongToRespondException(string message) : base(message)
        {
        }
    }
}