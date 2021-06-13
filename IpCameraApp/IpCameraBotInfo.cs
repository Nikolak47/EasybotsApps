using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace IpCameraApp
{
    public class IpCameraBotInfo
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string CGI { get; set; }
        public string DisplayName { get; set; }
        public RouterDevice Device { get; set; }
        public SecureString PasswordAsSecureString { get; internal set; }
        public SecureString IpAddressAsSecureString { get; internal set; }
        public SecureString MacAddressAsSecureString { get; internal set; }
        public SecureString HostNameAsSecureString { get; internal set; }

        public IpCameraBotInfo(string displayName, string username, string password, string cgi, RouterDevice device)
        {
            this.DisplayName = displayName;
            this.Username = username;
            this.Password = password;
            this.CGI = cgi;
            this.Device = device;
            this.PasswordAsSecureString = Encryption.ToSecureString(password);
            this.IpAddressAsSecureString = Encryption.ToSecureString(device.IpAddress);
            this.MacAddressAsSecureString = Encryption.ToSecureString(device.MacAddress);
            this.HostNameAsSecureString = Encryption.ToSecureString(device.HostName);
        }

        public override string ToString()
        {
            return string.Format("{0} [{1}]",this.DisplayName, this.Device.IpAddress);
        }
    }
}
