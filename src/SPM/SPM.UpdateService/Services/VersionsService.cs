using SPM.UpdateService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechSmith.Hyde;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;

namespace SPM.UpdateService.Services
{
    public class VersionsService
    {
        string tableName = "appversions";
        string partiionName = "Versions";

        string connectionString = string.Empty;
        
        public VersionsService(IOptions<Startup.ConnectionStrings> configuration)
        {
            this.connectionString = configuration.Value.Azure;
        }


        public string GetNextAvailableVersion()
        {
            TechSmith.Hyde.Table.AzureTableStorageProvider storage = GetStorage();

            var lastEntity = storage.CreateQuery<VersionEntity>(tableName).PartitionKeyEquals(partiionName).Take(1).FirstOrDefault();

            if (lastEntity == null)
                return "0.0.1.0";

            if (!string.IsNullOrEmpty(lastEntity.Version))
            {
                var version = new Version(lastEntity.Version);

                version = new Version(version.Major, version.Minor, version.Build, version.Revision + 1);

                return version.ToString();
            }

            return null;
        }

        internal async Task<byte[]> GetLastVersionFileStream()
        {
            TechSmith.Hyde.Table.AzureTableStorageProvider storage = GetStorage();

            var lastEntity = storage.CreateQuery<VersionEntity>(tableName).PartitionKeyEquals(partiionName).Take(1).FirstOrDefault();

            if (lastEntity == null)
                return null;

            var blob = GetVersionBlob(lastEntity.Version);

            using (var ms = new MemoryStream())
            {
                await blob.DownloadToStreamAsync(ms);
                return ms.ToArray();
            }
        }

        internal string GetConnectionString() => connectionString;

        private TechSmith.Hyde.Table.AzureTableStorageProvider GetStorage()
        {
            var account = new ConnectionStringCloudStorageAccount(connectionString);
            var storage = new TechSmith.Hyde.Table.AzureTableStorageProvider(account);
            return storage;
        }

        public string AddNewVersion(string version, byte[] data)
        {
            TechSmith.Hyde.Table.AzureTableStorageProvider storage = GetStorage();
            storage.Add(tableName, new VersionEntity(version));
            storage.Save();

            CloudBlockBlob blob = GetVersionBlob(version);

            blob.UploadFromByteArray(data, 0, data.Length);

            return blob.Uri.ToString();
        }

        private CloudBlockBlob GetVersionBlob(string version)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            var container = blobClient.GetContainerReference("updates");
            container.CreateIfNotExists();
            var blob = container.GetBlockBlobReference("update-" + version + ".zip");
            return blob;
        }
    }
}
