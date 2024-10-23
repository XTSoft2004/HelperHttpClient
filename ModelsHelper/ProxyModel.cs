using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelsHelper
{
    public enum TypeProxy
    {
        None,
        Http,
        Socks4,
        Socks5
    }
    public class ProxyModel
    {
        public ProxyModel() { }
        public ProxyModel(string proxy, TypeProxy typeProxy = TypeProxy.Http)
        {
            if (string.IsNullOrEmpty(proxy))
                return;
            this.typeProxy = typeProxy;
            if (proxy.Contains("@"))
            {
                // Trường hợp 1: proxy có dạng username:password@ip:port
                string[] parts = proxy.Split('@');
                string[] authParts = parts[0].Split(':');
                string[] addressParts = parts[1].Split(':');

                Username = authParts[0];
                Password = authParts[1];
                IP = addressParts[0];
                Port = addressParts[1];
            }
            else if (proxy.Contains(":"))
            {
                // Trường hợp 2: proxy có dạng ip:port:username:password
                string[] parts = proxy.Split(':');
                if (parts.Length == 2)
                {
                    // Proxy không cần xác thực (ip:port)
                    IP = parts[0];
                    Port = parts[1];
                }
                else if (parts.Length == 4)
                {
                    // Proxy cần xác thực (ip:port:username:password)
                    IP = parts[0];
                    Port = parts[1];
                    Username = parts[2];
                    Password = parts[3];
                }
            }

        }
        public TypeProxy? typeProxy { get; set; }
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
