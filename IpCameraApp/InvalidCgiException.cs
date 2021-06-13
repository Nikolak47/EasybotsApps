using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IpCameraApp
{
    public class InvalidArgumentsException : Exception
    {
        public InvalidArgumentsException(string cgiParameter, Exception innerException) : base(string.Format("Invalid arguments. Check your credentials of CGI command. Inserted CGI command: {0}", cgiParameter), innerException)
        {
        }
    }
}
