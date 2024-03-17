using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelperHttpClient.Models
{
    public class ProxyModel
    {
        public ProxyModel() { }
        public ProxyModel(string proxy)
        {
            if (string.IsNullOrEmpty(proxy))
                return;

            if (proxy.Split(':').Count() >= 2)
            {
                IP = proxy.Split(':')[0];
                Port = proxy.Split(':')[1];
                if (proxy.Split(':').Count() > 2)
                {
                    Username = proxy.Split(':')[2];
                    Password = proxy.Split(':')[3];
                }
            }
        }
        public string? IP { get; set; }
        public string? Port { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(IP) && !string.IsNullOrEmpty(Port) && !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password))
            {
                return $"{IP}:{Port}:{Username}:{Password}";
            }
            else if (!string.IsNullOrEmpty(IP) && !string.IsNullOrEmpty(Port))
            {
                return $"{IP}:{Port}";
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
