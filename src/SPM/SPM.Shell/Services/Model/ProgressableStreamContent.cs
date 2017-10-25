using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SPM.Shell.Services.Model
{
    internal class ProgressableStreamContent : HttpContent
    {

        /// <summary>
        /// Lets keep buffer of 20kb
        /// </summary>
        private const int defaultBufferSize = 1024 * 1024;

        private HttpContent content;
        private int bufferSize;
        //private bool contentConsumed;
        private Action<long, long> progress;

        public ProgressableStreamContent(HttpContent content, Action<long, long> progress) : this(content, defaultBufferSize, progress) { }

        public ProgressableStreamContent(HttpContent content, int bufferSize, Action<long, long> progress)
        {
            if (bufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException("bufferSize");
            }

            this.content = content ?? throw new ArgumentNullException("content");
            this.bufferSize = bufferSize;
            this.progress = progress;

            foreach (var h in content.Headers)
            {
                this.Headers.Add(h.Key, h.Value);
            }
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            return Task.Run(async () =>
            {
                var buffer = new Byte[this.bufferSize];
                TryComputeLength(out long size);
                var uploaded = 0;
                
                using (var sinput = await content.ReadAsStreamAsync())
                {
                    while (true)
                    {
                        var bytesReaded = await sinput.ReadAsync(buffer, 0, buffer.Length);
                        if (bytesReaded <= 0) break;
                        
                        progress?.Invoke(uploaded, size);
                        await stream.WriteAsync(buffer, 0, bytesReaded);

                        uploaded += bytesReaded;
                    }
                }
            });
        }

        protected override bool TryComputeLength(out long length)
        {
            length = content.Headers.ContentLength.GetValueOrDefault();
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                content.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}
