using Newtonsoft.Json;
using SPM.Shell.Services.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SPM.Shell.Services
{
    public class PackagesService : IPackagesService
    {
        private readonly HttpClient httpClient;

        public PackagesService(string packageServiceUrl)
        {
            this.httpClient = new HttpClient()
            {
                BaseAddress = new Uri(packageServiceUrl)
            };
        }

        public async Task UploadPackageAsync(string name, Stream fileStream)
        {
            var content = new MultipartFormDataContent
            {
                { new StringContent(name), "name" },
                { new StreamContent(fileStream), "packageFile", "package.wsp" }
            };
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

        public async Task<PackageInfo> SearchPackageAsync(string name)
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri($"/get?name={name}", UriKind.Relative),
                Method = HttpMethod.Get
            };

            HttpResponseMessage response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string packageJSON = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<PackageInfo>(packageJSON);
            }
            return null;
        }
    }
}
