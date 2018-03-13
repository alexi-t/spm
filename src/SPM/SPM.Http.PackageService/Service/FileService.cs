using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SPM.Http.PackageService.Service
{
    public class FileService
    {
        private readonly HttpClient httpClient;

        public FileService(string serviceUrl)
        {
            this.httpClient = new HttpClient()
            {
                BaseAddress = new Uri(serviceUrl)
            };
        }

        public async Task<string> UploadFileAsync(string packageNameAndTag, byte[] bytes)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, httpClient.BaseAddress);

            var content = new MultipartFormDataContent
            {
                { new ByteArrayContent(bytes), "data", "package.wsp" },
                { new StringContent(packageNameAndTag), "key" }
            };
            request.Content = content;

            var response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsStringAsync();

            return string.Empty;
        }

        internal string GetDowloadLinkFormat()
        {
            return $"{httpClient.BaseAddress}/{{0}}/{{1}}";
        }
    }
}
