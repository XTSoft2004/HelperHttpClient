using HelperHttpClient;
using HelperHttpClient.Models;
using ModelsHelper;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.WebSockets;

internal class Program
{
    private static async Task Main(string[] args)
    {
        ProxyModel proxyModel = new ProxyModel();
        RequestHttpClient _request = new RequestHttpClient();
        var response = await _request.GetAsync("http://google.com/");
        string content = _request.Content;
        Console.WriteLine(content);
    }
}