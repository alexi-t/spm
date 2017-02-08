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

        public CloudBlobService(string connectionString)
        {
            this.connectionString = connectionString;
        }

        private string GetStringHash(HashAlgorithm algorithm, string str)
        {
            return string.Join("", algorithm.ComputeHash(Encoding.UTF8.GetBytes(str)).Select(b => b.ToString("x2")));
        }

        public async Task SaveFileAsync(string key, byte[] data)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);

            var sha256 = SHA256.Create();
            var hash = GetStringHash(sha256, key);

            CloudBlockBlob blockBlob = container.GetBlockBlobReference(hash);

            await blockBlob.UploadFromByteArrayAsync(data, 0, data.Length);
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
