using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HelperHttpClient.Models
{
    public class CookieModel
    {
        public string? Cookies { get; set; }
        public string? Path { get; set; }
        public string? Domain { get; set; }

        public IEnumerable<string[]> GetCookieList()
        {
            // Check for null or empty before splitting
            if (!string.IsNullOrEmpty(Cookies))
            {
                return Cookies
                    .Split(';')
                    .Select(x => x.Split('='))
                    .Where(x => x.Length == 2);
            }
            else
            {
                return Enumerable.Empty<string[]>();
            }
        }
    }
}
