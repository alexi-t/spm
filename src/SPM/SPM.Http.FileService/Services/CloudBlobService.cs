using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SPM.Http.FileService.Services
{
    public class CloudBlobService
    {
        private const string containerName = "files";

        private readonly string connectionString;
        private readonly TelemetryClient telemetryClient = new TelemetryClient(TelemetryConfiguration.Active);

        public CloudBlobService(string connectionString)
        {
            this.connectionString = connectionString;
        }

        private string GetStringHash(HashAlgorithm algorithm, string str)
        {
            return string.Join("", algorithm.ComputeHash(Encoding.UTF8.GetBytes(str)).Select(b => b.ToString("x2")));
        }

        public async Task SaveFileAsync(string key, Stream dataStream)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);

            var sha256 = SHA256.Create();
            var hash = GetStringHash(sha256, key);

            var operation = telemetryClient.StartOperation<RequestTelemetry>($"upload key {key}, hash: {hash}");
            
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(hash);
            
            int bufferSize = 4 * 1024 * 1024;

            int blockId = 0;
            telemetryClient.TrackTrace($"Starting upload to blob {blockBlob.Uri}");
            while (true)
            {
                byte[] buffer = new byte[bufferSize];
                int bytesRead = await dataStream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead <= 0)
                    break;

                using (var md5 = MD5.Create())
                using (var ms = new MemoryStream())
                {
                    await ms.WriteAsync(buffer, 0, bytesRead);
                    ms.Position = 0;

                    try
                    {
                        await blockBlob.PutBlockAsync(
                                            blockId: Convert.ToBase64String(Encoding.Default.GetBytes($"{++blockId:d7}")),
                                            blockData: ms,
                                            contentMD5: Convert.ToBase64String(md5.ComputeHash(buffer, 0, bytesRead)));
                        telemetryClient.TrackTrace($"uploaded {dataStream.Position / 1024} KB");
                    }
                    catch (StorageException ex)
                    {
                        telemetryClient.TrackException(ex);
                    }
                }
            }
            telemetryClient.StopOperation(operation);

            await blockBlob.PutBlockListAsync(Enumerable.Range(1, blockId).Select(id => Convert.ToBase64String(Encoding.Default.GetBytes($"{id:d7}"))));
        }

        public async Task<byte[]> GetFileAsync(string key)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);

            var sha256 = SHA256.Create();
            var hash = GetStringHash(sha256, key);

            CloudBlockBlob blockBlob = container.GetBlockBlobReference(hash);

            using (var ms = new MemoryStream())
            {
                await blockBlob.DownloadToStreamAsync(ms);

                return ms.ToArray();
            }
        }
    }
}
