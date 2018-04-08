using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SPM.Shell.Services.Model;
using SPM.Shell.Util;
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
                { new StringContent(JsonConvert.SerializeObject(folderVersion)), "packageChanges"  },
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
                uiService.AddMessage("/r/nUpload done");
            }
            else
                throw new InvalidOperationException(responseContent);
        }
        

        public async Task<PackageInfo> GetPackageVersionAsync(string name)
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri($"packages/{name}/info", UriKind.Relative),
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

        public async Task<string[]> GetPackageTagsAsync(string packageNameTo, string packageNameFrom = null)
        {
            string packageName = PackageNameAndTagHelper.GetPackageName(packageNameTo);

            string toTag = PackageNameAndTagHelper.GetPackageTag(packageNameTo);
            string fromTag = packageNameFrom != null ? PackageNameAndTagHelper.GetPackageTag(packageNameFrom) : string.Empty;

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri($"packages/{packageName}/tags?to={toTag}&from={fromTag}", UriKind.Relative),
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

        public async Task<string[]> GetAllPackageTagsAsync(string packageName)
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri($"packages/{packageName}/tags", UriKind.Relative),
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

        public async Task<Dictionary<string, string>> GetPackageFilesAtVersionAsync(string name, string tag)
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri($"packages/{name}/files", UriKind.Relative),
                Method = HttpMethod.Get
            };

            HttpResponseMessage response = await httpClient.SendAsync(request);

            var changes = new Dictionary<string, string>();

            if (response.IsSuccessStatusCode)
            {
                string arrayJSON = await response.Content.ReadAsStringAsync();
                JArray jArray = JArray.Parse(arrayJSON);

                string prevChangeTag = string.Empty;
                bool stopAtTagChange = false;

                foreach (var changeEl in jArray.Reverse())
                {
                    string changeTag = changeEl["tag"].ToObject<string>();
                    string path = changeEl["filePath"].ToObject<string>();
                    string hash = changeEl["hash"].ToObject<string>();
                    FileHistoryType changeType = changeEl["changeType"].ToObject<FileHistoryType>();

                    if (changeTag != prevChangeTag && stopAtTagChange)
                        break;

                    switch (changeType)
                    {
                        case FileHistoryType.Added:
                        case FileHistoryType.Modified:
                            if (changes.ContainsKey(path))
                                changes[path] = hash;
                            else
                                changes.Add(path, hash);
                            break;
                        case FileHistoryType.Deleted:
                            if (changes.ContainsKey(path))
                                changes.Remove(path);
                            break;
                        default:
                            break;
                    }
                    prevChangeTag = changeTag;

                    if (changeTag == tag)
                        stopAtTagChange = true;
                }
                return changes;
            }
            return new Dictionary<string, string>();
        }
    }
}
