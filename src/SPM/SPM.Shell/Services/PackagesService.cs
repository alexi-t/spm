using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SPM.Shell.Services
{
    public class PackagesService
    {
        private readonly HttpClient httpClient;

        public PackagesService(string packageServiceUrl)
        {
            this.httpClient = new HttpClient()
            {
                BaseAddress = new Uri(packageServiceUrl)
            };
        }

        public async Task UploadPackageAsync(string name, string tag, Stream fileStream)
        {
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(name), "name");
            content.Add(new StringContent(tag), "tag");
            content.Add(new StreamContent(fileStream), "packageFile", "package.wsp");

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                Content = content
            };

            var response = await httpClient.SendAsync(request);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {

            }
            else
                throw new InvalidOperationException(responseContent);
        }
    }
}
