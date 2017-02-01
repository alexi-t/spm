using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SPM.Http.PackageService.Service
{
    public class FileService
    {
        private readonly string serviceUrl;
        private readonly HttpClient httpClient;

        public FileService(string serviceUrl)
        {
            this.httpClient = new HttpClient();
            this.httpClient.BaseAddress = new Uri(serviceUrl);
        }

        public async Task<bool> UploadFileAsync(string packageNameAndTag, byte[] bytes)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, httpClient.BaseAddress);

            var content = new MultipartFormDataContent();
            content.Add(new ByteArrayContent(bytes), "data");
            content.Add(new StringContent(packageNameAndTag), "key");

            request.Content = content;

            var response = await httpClient.SendAsync(request);

            return response.IsSuccessStatusCode;
        }
    }
}
