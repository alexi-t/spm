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
    public class PackagesService : IOnlineStoreService
    {
        private readonly IUIService uiService;
        private readonly IFileService fileService;
        private readonly HttpClient httpClient;

        public PackagesService(string packageServiceUrl, IUIService uiService, IFileService fileService)
        {
            this.uiService = uiService;
            this.fileService = fileService;

            this.httpClient = new HttpClient()
            {
                BaseAddress = new Uri(packageServiceUrl),
                Timeout = TimeSpan.FromHours(1)
            };
        }

        public async Task PushPackageAsync(string name, FolderVersionEntry folderVersion)
        {
            byte[] fileData = await fileService.ZipFiles(folderVersion.Files.Where(f => f.EditType != FileHistoryType.Deleted).Select(f => f.Path));

            var content = new MultipartFormDataContent
            {
                { new StringContent(name), "nameAndTag" },
                { new StringContent(JsonConvert.SerializeObject(folderVersion)), "versionInfo"  },
                { new ByteArrayContent(fileData), "versionFile", "data.zip" }
            };

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = new ProgressableStreamContent(content, (long uploaded, long size) =>
                {
                    uiService.DisplayProgress((float)uploaded * 100 / size);
                })
            };

            request.Headers.TransferEncodingChunked = true;

            uiService.AddMessage("Uploading package...");

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
                RequestUri = new Uri($"packages/get?name={name}", UriKind.Relative),
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

        public async Task<string[]> GetPackageTagsAsync(string packageName)
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri($"packages/getAll?name={packageName}", UriKind.Relative),
                Method = HttpMethod.Get
            };
            
            HttpResponseMessage response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string arrayJSON = await response.Content.ReadAsStringAsync();

                return Newtonsoft.Json.Linq.JArray.Parse(arrayJSON).Select(t => t.ToObject<string>()).ToArray();
            }
            return new string[0];
        }

        public HttpOperationWithProgress DownloadPackage(string name, string tag)
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri($"packages/download?name={name}@{tag}", UriKind.Relative),
                Method = HttpMethod.Get
            };

            return new HttpOperationWithProgress(httpClient, request);
        }
        
    }
}
