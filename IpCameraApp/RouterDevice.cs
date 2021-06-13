using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IpCameraApp
{
    public class RouterDevice
    {
        public readonly string DeviceDisplayFormat = "{0} :: {1} :: {2}";
        public string HostName { get; set; }
        public string MacAddress { get; set; }
        public string IpAddress { get; set; }
        public RouterDevice(string macAddress, string ipAddress, string hostName)
        {
            this.MacAddress = macAddress;
            this.IpAddress = ipAddress;
            this.HostName = hostName;
        }

        public override string ToString()
        {
            return string.Format(DeviceDisplayFormat, this.HostName, this.IpAddress, this.MacAddress);
        }
    }
}
