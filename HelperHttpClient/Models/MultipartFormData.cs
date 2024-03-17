using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelperHttpClient.Models
{
    public class MultipartFormData
    {
        public MultipartFormDataContent content = new MultipartFormDataContent();
        public void AddStringContent(string key, string value)
        {
            content.Add(new StringContent(value), key);
        }
        public void AddFileContent(string key, string path_file)
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes(path_file);
            ByteArrayContent fileContent = new ByteArrayContent(fileBytes);
            content.Add(fileContent, key, Path.GetFileName(path_file));
        }
    }
}
