using HelperHttpClient.Models;
using ModelsHelper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HelperHttpClient
{
    public class RequestHttpClient
    {
        private CancellationToken _cancellationToken;
        private HttpClient _client = new HttpClient();
        private HttpClientHandler _handler = new HttpClientHandler();
        private CookieContainer _cookieContainer = new CookieContainer();
        private HttpResponseMessage _response = new HttpResponseMessage();
        private TimeSpan? _timeout = new TimeSpan();
        public HttpResponseMessage Response
        {
            get
            {
                return _response;
            }
            set
            {
                _response = value;
            }
        }
        public string Content { get;set; }
        public RequestHttpClient()
        {
            Initialize();
        }
        public RequestHttpClient(TimeSpan timeout = default(TimeSpan))
        {
            Initialize();

            if (timeout != default(TimeSpan))
            {
                SetTimeout(timeout);
            }
        }
        public RequestHttpClient(string cookie, string path, string domain, Dictionary<string, string> headers, TimeSpan timeout = default(TimeSpan))
        {
            Initialize();
            SetCookie(cookie, path, domain);
            SetHeader(headers);
            if (timeout != default(TimeSpan))
            {
                SetTimeout(timeout);
            }
        }
        public RequestHttpClient(string? cookie = null, string? path = null, string? domain = null, Dictionary<string, string>? headers = null, ProxyModel? _proxy = null, TimeSpan timeout = default(TimeSpan))
        {
            Initialize();
            if(cookie != null && path != null && domain != null) 
                SetCookie(cookie, path, domain);
            if(headers != null)
                SetHeader(headers);
            if(_proxy != null)
                SetProxy(_proxy);
            if (timeout != default(TimeSpan))
                SetTimeout(timeout);
        }
        private void Initialize()
        {
            _cookieContainer = new CookieContainer();
            _handler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
                AutomaticDecompression = DecompressionMethods.All,
                CookieContainer = _cookieContainer,
                ServerCertificateCustomValidationCallback = (HttpRequestMessage, cert, chain, sslPolicyErrors) => true
            };

            _client = new HttpClient(_handler);
            _client.BaseAddress = new Uri("https://www.facebook.com/");
        }
        public void SetTimeout(TimeSpan timeout = default(TimeSpan))
        {
            if (timeout != default(TimeSpan))
            {
                _timeout = timeout;
                _client.Timeout = timeout;
            }
        }
        public void SetCookie(string cookie, string path, string domain)
        {
            IEnumerable<string[]> list = cookie.Split(';').Select(x => x.Split('=')).Where(x => x.Length == 2);

            foreach (string[] info in list)
            {
                SetCookie(info, path, domain);
            }
        }
        public void SetCookie(string[] cookieInfo, string path, string domain)
        {
            string name = cookieInfo[0].Trim();
            string value = cookieInfo[1].Trim();

            Cookie cookieNew = new Cookie(name, value, path, domain);
            _cookieContainer.Add(cookieNew);
        }
        public void ClearCookie()
        {
            _cookieContainer = new CookieContainer();
        }
        public void SetCookie(CookieModel? cookieModel)
        {
            if (cookieModel == null)
                return;

            IEnumerable<string[]> cookieList = cookieModel.GetCookieList();

            foreach (string[] cookie in cookieList)
            {
                SetCookie(cookie, cookieModel.Path, cookieModel.Domain);
            }
        }
        public void SetHeader(string key, string value)
        {
            if (_client.DefaultRequestHeaders.Contains(key))
                _client.DefaultRequestHeaders.Remove(key);
            _client.DefaultRequestHeaders.TryAddWithoutValidation(key, value);
        }
        public void SetHeader(Dictionary<string, string> headers, bool isClear = false)
        {
            if (headers == null)
                throw new ArgumentNullException(nameof(headers));

            if (isClear)
                _client.DefaultRequestHeaders.Clear();

            foreach (KeyValuePair<string, string> header in headers)
            {
                SetHeader(header.Key, header.Value);
            }
        }
        public void SetProxy(string? ip, string? port, string? username, string? password)
        {
            _handler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip | System.Net.DecompressionMethods.Brotli,
                CookieContainer = _cookieContainer
            };
            var _client_backup = _client;
            _client = new HttpClient(_handler);

            foreach (var header in _client_backup.DefaultRequestHeaders)
            {
                foreach (var value in header.Value)
                {
                    SetHeader(header.Key, value);
                }
            }

            WebProxy proxy = new WebProxy();

            if (!string.IsNullOrEmpty(ip) && !string.IsNullOrEmpty(port))
            {
                proxy = new WebProxy
                {
                    Address = new Uri($"http://{ip}:{port}"),
                    BypassProxyOnLocal = false,
                    UseDefaultCredentials = false,
                };
            }

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                proxy.UseDefaultCredentials = true;
                proxy.Credentials = new NetworkCredential(username, password);
            }

            if (proxy != null)
            {
                _handler.Proxy = proxy;
            }
        }

        public void SetProxy(ProxyModel proxy)
        {
            if (proxy == null || string.IsNullOrEmpty(proxy.IP) || string.IsNullOrEmpty(proxy.Port))    
                return;

            SetProxy(proxy.IP, proxy.Port, proxy.Username, proxy.Password);
        }
        public void SetCancellationToken(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
        }
        public void SetCookieContainer(CookieContainer cookieContainer)
        {
            _cookieContainer = cookieContainer;
        }
        public static async Task<string> GetTextContent(HttpResponseMessage httpResponseMessage)
        {
            if (httpResponseMessage == null)
                return string.Empty;
            //Response = httpResponseMessage;

            byte[] buffer = await httpResponseMessage.Content.ReadAsByteArrayAsync();
            string content = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
            return content;
        }
        public async Task<HttpResponseMessage> GetAsync(string url)
        {
            try
            {
                var response = _cancellationToken != default(CancellationToken) ?
                    await _client.GetAsync(url, _cancellationToken) : await _client.GetAsync(url);
                Response = response;
                //string text = await GetTextContent(response);
                Content = await GetTextContent(response);
                return response;
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                Console.WriteLine("Yêu cầu bị hủy do timeout.");
                // Xử lý nếu yêu cầu bị hủy do timeout ở đây
                //throw; // Hoặc trả về một giá trị mặc định khác
                return new HttpResponseMessage(HttpStatusCode.RequestTimeout);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi xảy ra: {ex.Message}");
                //throw;
                return new HttpResponseMessage(HttpStatusCode.NotFound); ;
            }
        }

        public async Task<HttpResponseMessage> PostAsync(string url, string json)
        {
            try
            {
                json = json.Replace("\r", "").Replace("\n", "").Trim();
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                //_client.BaseAddress = new Uri(url);
                var response = _cancellationToken != default(CancellationToken) ?
                    await _client.PostAsync(url, content, _cancellationToken) : await _client.PostAsync(url, content);
                Response = response;
                Content = await GetTextContent(response);

                //string text = await GetTextContent(response);

                return response;
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                Console.WriteLine("Yêu cầu bị hủy do timeout.");
                // Xử lý nếu yêu cầu bị hủy do timeout ở đây
                //throw; // Hoặc trả về một giá trị mặc định khác
                return new HttpResponseMessage(HttpStatusCode.RequestTimeout);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi xảy ra: {ex.Message}");
                //throw;
                return new HttpResponseMessage(HttpStatusCode.NotFound); ;
            }
        }
        public async Task<HttpResponseMessage> DeleteAsync(string url)
        {
            try
            {
                var response = _cancellationToken != default(CancellationToken) ?
                    await _client.DeleteAsync(url, _cancellationToken) : await _client.DeleteAsync(url);
                Response = response;
                Content = await GetTextContent(response);

                return response;
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                Console.WriteLine("Yêu cầu bị hủy do timeout.");
                // Xử lý nếu yêu cầu bị hủy do timeout ở đây
                //throw; // Hoặc trả về một giá trị mặc định khác
                return new HttpResponseMessage(HttpStatusCode.RequestTimeout);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi xảy ra: {ex.Message}");
                //throw;
                return new HttpResponseMessage(HttpStatusCode.NotFound); ;
            }
        }
        public async Task<HttpResponseMessage> PatchAsync(string url, dynamic? DataPost)
        {
            try
            {
                // Chuyển object thành chuỗi JSON
                string json = JsonConvert.SerializeObject(DataPost);
                json = json.Replace("\r", "").Replace("\n", "").Trim();
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = _cancellationToken != default(CancellationToken) ?
                    await _client.PatchAsync(url, content, _cancellationToken) : await _client.PatchAsync(url, content);
                Response = response;
                Content = await GetTextContent(response);

                return response;
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                Console.WriteLine("Yêu cầu bị hủy do timeout.");
                // Xử lý nếu yêu cầu bị hủy do timeout ở đây
                //throw; // Hoặc trả về một giá trị mặc định khác
                return new HttpResponseMessage(HttpStatusCode.RequestTimeout);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi xảy ra: {ex.Message}");
                //throw;
                return new HttpResponseMessage(HttpStatusCode.NotFound); ;
            }
        }
        public async Task<HttpResponseMessage> PostAsync(string url, MultipartFormData multipartForm)
        {
            try
            {
                var response = _cancellationToken != default(CancellationToken) ?
                    await _client.PostAsync(url, multipartForm.content, _cancellationToken) : await _client.PostAsync(url, multipartForm.content);
                Response = response;
                Content = await GetTextContent(response);
                //string text = await GetTextContent(response);

                return response;
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                Console.WriteLine("Yêu cầu bị hủy do timeout.");
                // Xử lý nếu yêu cầu bị hủy do timeout ở đây
                //throw; // Hoặc trả về một giá trị mặc định khác
                return new HttpResponseMessage(HttpStatusCode.RequestTimeout);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi xảy ra: {ex.Message}");
                //throw;
                return new HttpResponseMessage(HttpStatusCode.NotFound); ;
            }
        }
        public async Task<HttpResponseMessage> PostAsync(string url, Dictionary<string, string> dataPost)
        {
            try
            {
                var response = _cancellationToken != default(CancellationToken) ?
                    await _client.PostAsync(url, new FormUrlEncodedContent(dataPost), _cancellationToken) : await _client.PostAsync(url, new FormUrlEncodedContent(dataPost));
                Response = response;
                Content = await GetTextContent(response);
                //string text = await GetTextContent(response);

                return response;
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                Console.WriteLine("Yêu cầu bị hủy do timeout.");
                // Xử lý nếu yêu cầu bị hủy do timeout ở đây
                //throw; // Hoặc trả về một giá trị mặc định khác
                return new HttpResponseMessage(HttpStatusCode.RequestTimeout);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi xảy ra: {ex.Message}");
                //throw;
                return new HttpResponseMessage(HttpStatusCode.NotFound); ;
            }
        }
        public async Task<HttpResponseMessage> PostAsync(string url, MultipartFormDataContent multipartFormData)
        {
            try
            {
                var response = _cancellationToken != default(CancellationToken) ?
                    await _client.PostAsync(url, multipartFormData, _cancellationToken) : await _client.PostAsync(url, multipartFormData);
                Response = response;
                Content = await GetTextContent(response);
                //string text = await GetTextContent(response);

                return response;
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                Console.WriteLine("Yêu cầu bị hủy do timeout.");
                // Xử lý nếu yêu cầu bị hủy do timeout ở đây
                //throw; // Hoặc trả về một giá trị mặc định khác
                return new HttpResponseMessage(HttpStatusCode.RequestTimeout);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi xảy ra: {ex.Message}");
                //throw;
                return new HttpResponseMessage(HttpStatusCode.NotFound); ;
            }
        }
        public async Task<HttpResponseMessage> PostAsync(string url)
        {
            try
            {
                var response = _cancellationToken != default(CancellationToken) ?
                    await _client.PostAsync(url, null, _cancellationToken) : await _client.PostAsync(url, null);
                Response = response;
                Content = await GetTextContent(response);

                return response;
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                Console.WriteLine("Yêu cầu bị hủy do timeout.");
                // Xử lý nếu yêu cầu bị hủy do timeout ở đây
                //throw; // Hoặc trả về một giá trị mặc định khác
                return new HttpResponseMessage(HttpStatusCode.RequestTimeout);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi xảy ra: {ex.Message}");
                //throw;
                return new HttpResponseMessage(HttpStatusCode.NotFound); ;
            }
        }
        public async Task<HttpResponseMessage> PostAsync(string url, dynamic? DataPost)
        {
            try
            {
                // Chuyển object thành chuỗi JSON
                string json = JsonConvert.SerializeObject(DataPost);
                json = json.Replace("\r", "").Replace("\n", "").Trim();
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = _cancellationToken != default(CancellationToken) ?
                    await _client.PostAsync(url, content, _cancellationToken) : await _client.PostAsync(url, content);
                Response = response;
                Content = await GetTextContent(response);

                return response;
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                Console.WriteLine("Yêu cầu bị hủy do timeout.");
                // Xử lý nếu yêu cầu bị hủy do timeout ở đây
                //throw; // Hoặc trả về một giá trị mặc định khác
                return new HttpResponseMessage(HttpStatusCode.RequestTimeout);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi xảy ra: {ex.Message}");
                //throw;
                return new HttpResponseMessage(HttpStatusCode.NotFound); ;
            }
        }
        public async Task<List<Cookie>> ListGetCookies(string address)
        {
            var cookies = new List<Cookie>();

            if (_handler is HttpClientHandler handler)
            {
                var cookieContainer = handler.CookieContainer;
                //var uri = _client.BaseAddress; // Thay thế bằng địa chỉ URL bạn đã sử dụng trong yêu cầu
                Uri uri = new Uri(address);
                cookies.AddRange(cookieContainer.GetCookies(uri).Cast<Cookie>());
            }

            return cookies;
        }
        public string GetCookies(string address)
        {
            List<Cookie> cookies = ListGetCookies(address).GetAwaiter().GetResult();
            string cookie = string.Join("; ", cookies.Select(cookie => $"{cookie.Name}={cookie.Value}"));
            return cookie;
        }
        public string Address()
        {
            if (_response != null && _response.RequestMessage != null && _response.RequestMessage.RequestUri != null)
                return _response.RequestMessage.RequestUri.ToString();
            else
                return string.Empty;
        }
        private string GET_URL_FIRST(string url)
        {
            Uri uri = new Uri(url);
            return uri.GetLeftPart(UriPartial.Authority);
        }
        private string GET_URL_LAST(string url)
        {
            Uri uri = new Uri(url);
            return uri.AbsolutePath;
        }

    }
}
