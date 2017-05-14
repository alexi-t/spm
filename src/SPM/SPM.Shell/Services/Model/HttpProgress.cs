using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SPM.Shell.Services.Model
{
    public class HttpOperationWithProgress
    {
        private readonly HttpRequestMessage request;
        private readonly HttpClient httpClient;

        public delegate void ProgressChanged(int processedCount, long totalCount);
        
        public event ProgressChanged OnProgress;
        public event Action<Exception> OnError;

        public HttpOperationWithProgress(HttpClient client, HttpRequestMessage request)
        {
            this.httpClient = client;
            this.request = request;
        }

        public async Task<HttpResponseMessage> GetOperationResultAsync()
        {
            HttpResponseMessage response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            if (response.IsSuccessStatusCode)
            {
                var contentLength = response.Content.Headers.ContentLength ?? 0;
                if (contentLength == 0)
                    OnError(new ApplicationException($"No content-length header in response from {request.RequestUri}"));
                else
                {
                    byte[] buffer = new byte[contentLength];
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        int position = 0;
                        while (position < contentLength)
                        {
                            int bytesRed = await stream.ReadAsync(buffer, position, (int)Math.Min(1024, contentLength - position));
                            position += bytesRed;
                            OnProgress(position, contentLength);
                        }
                    }
                    response.Content = new ByteArrayContent(buffer);
                    return response;
                }
            }
            else
                OnError(new ApplicationException($"Error send {request.RequestUri} {response.StatusCode} {await response.Content.ReadAsStringAsync()}"));

            return null;
        }
    }
}
