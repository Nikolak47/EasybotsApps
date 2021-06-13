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
    public delegate void DevicesListed(List<RouterDevice> devicesList);
    public class RouterDevicesCollection
    {
        public event DevicesListed RouterDevicesLoaded;
        public event DevicesListed DevicePinged;
        public int NumberOfDevices { get; set; }
        private List<RouterDevice> devices = new List<RouterDevice>();
        private Dictionary<string, bool> pingedDevicesDictionary = new Dictionary<string, bool>();
        private const string ApiUrlToGetVendorNameFromMacAddress = @"http://api.macvendors.com/{0}";
        private object syncLock = new object();
        public RouterDevicesCollection()
        {
            ThreadPool.SetMinThreads(64, 64);
            this.PingAll();
        }

        private static string[] NetworkGateway()
        {
            List<string> ipAddresses = new List<string>();
            foreach (NetworkInterface f in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (f.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (GatewayIPAddressInformation d in f.GetIPProperties().GatewayAddresses)
                    {
                        string ip = d.Address.MapToIPv4().ToString();
                        ipAddresses.Add(ip);
                    }
                }
            }

            return ipAddresses.Distinct().ToArray();
        }

        private void PingAll()
        {
            // Algorythm copied from:
            // https://www.codeproject.com/Tips/889483/How-to-List-all-devices-info-on-your-WLAN-router
            // With few changes like checking devices from all gateways, ping & responses dictionary,...

            string[] gateIpAddresses = NetworkGateway();
            this.NumberOfDevices = gateIpAddresses.Length * 255;

            // Extracting and pinging all other ip's.
            foreach (var item in gateIpAddresses)
            {
                string[] array = item.Split('.');
                for (int i = 2; i <= 255; i++)
                {
                    string ipAddressToBePinged = array[0] + "." + array[1] + "." + array[2] + "." + i;   
                    this.pingedDevicesDictionary.Add(ipAddressToBePinged, false);
                    this.Ping(ipAddressToBePinged, 1, 4000);
                }
            }
        }

        private void Ping(string host, int attempts, int timeout)
        {
            System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping();
            ping.PingCompleted += new PingCompletedEventHandler(PingCompleted);
            for (int i = 0; i < attempts; i++)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(delegate 
                    {
                        System.Net.NetworkInformation.Ping pingLocal = ping;
                        string hostLocal = host;
                        int timeoutLocal = timeout;
                        try
                        {
                            this.PingSendAsync(pingLocal, hostLocal, timeoutLocal, hostLocal);
                        }
                        catch
                        {
                            this.PingCompletedWithLock(host, isSuccess: false); 
                        }
                    }));
            }
        }

        private void PingSendAsync(Ping ping, string host, int timeout, string host2)
        {
            ping.SendAsync(host, timeout, host2);
        }

        private void PingCompleted(object sender, PingCompletedEventArgs e)
        {
            string ip = (string)e.UserState;
            bool isSuccess = e.Reply != null && e.Reply.Status == IPStatus.Success;
            this.PingCompletedWithLock(ip, isSuccess);
        }

        private void PingCompletedWithLock(string ip, bool isSuccess)
        {
            lock (this.syncLock)
            {                
                if (isSuccess)
                {
                    string macaddres = this.GetMacAddress(ip);
                    string hostname = this.GetHostName(ip, macaddres);
                    RouterDevice device = new RouterDevice(macaddres, ip, hostname);
                    bool deviceAlreadyThere = this.devices.Any(item => item.MacAddress == macaddres);
                    if (!deviceAlreadyThere)
                        this.devices.Add(device);
                }

                this.pingedDevicesDictionary[ip] = true;
                // raise event
                this.DevicePinged?.Invoke(this.devices);

                if (this.AllValuesAreTrue(this.pingedDevicesDictionary))
                {
                    this.RouterDevicesLoaded?.Invoke(this.devices);
                }
            }
        }

        private bool AllValuesAreTrue(Dictionary<string, bool> pingedDevicesDictionary)
        {
            return pingedDevicesDictionary.All(item => item.Value);
        }

        private string GetHostNameFromApi(string macaddres)
        {
            string apiUrl = ApiUrlToGetVendorNameFromMacAddress;
            string url = string.Format(apiUrl, macaddres);
            string hostName = null;
            try
            {
                using (var client = new WebClient())
                {
                    var responseString = client.DownloadString(url);
                    hostName = responseString;
                    Thread.Sleep(1000); // Because of the API limits                   
                }
            }
            catch (WebException)
            {                
            }

            return hostName;
        }

        private string GetHostName(string ipAddress, string macaddress)
        {
            try
            {
                IPHostEntry entry = Dns.GetHostEntry(ipAddress);
                if (entry != null)
                {
                    return entry.HostName;
                }
            }
            catch (SocketException)
            {
                string hostName = this.GetHostNameFromApi(macaddress);
                if (string.IsNullOrWhiteSpace(hostName))
                {
                    return "Unknown device";
                }

                return hostName;
            }

            return null;
        }

        private string GetMacAddress(string ipAddress)
        {
            string macAddress = string.Empty;
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "arp";
            process.StartInfo.Arguments = "-a " + ipAddress;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            string strOutput = process.StandardOutput.ReadToEnd();
            string[] substrings = strOutput.Split('-');
            if (substrings.Length >= 8)
            {
                macAddress = substrings[3].Substring(Math.Max(0, substrings[3].Length - 2))
                         + "-" + substrings[4] + "-" + substrings[5] + "-" + substrings[6]
                         + "-" + substrings[7] + "-"
                         + substrings[8].Substring(0, 2);
                return macAddress;
            }

            else
            {
                return "OWN Machine";
            }
        }
    }
}
