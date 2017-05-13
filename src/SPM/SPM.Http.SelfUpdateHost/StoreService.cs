using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechSmith.Hyde;
using TechSmith.Hyde.Table;

namespace SPM.Http.SelfUpdateHost
{
    public class StoreService
    {
        private readonly string connectionString;
        private readonly ConnectionStringCloudStorageAccount storageAccount;

        private const string VersionsTableName = "versions";

        public StoreService(IOptions<Config> options)
        {
            this.connectionString = options.Value.StorageConnectionString;

            this.storageAccount = new ConnectionStringCloudStorageAccount(connectionString);
        }

        internal async Task<List<VersionRow>> GetLastVersions(int count)
        {
            var tableStorage = new AzureTableStorageProvider(storageAccount);
            return (await tableStorage.CreateQuery<VersionRow>(VersionsTableName).Top(count).Async()).ToList();
        }

        internal async Task<VersionRow> AddVersionAsync(string appName, string version, string fileHash)
        {
            var tableStorage = new AzureTableStorageProvider(storageAccount);

            var appVersion = new VersionRow(appName, version, fileHash);

            tableStorage.Add(VersionsTableName, appVersion);

            await tableStorage.SaveAsync();

            return appVersion;
        }

        internal async  Task<VersionRow> GetLastVersion()
        {
            var tableStorage = new AzureTableStorageProvider(storageAccount);
            var topVersion = await tableStorage.CreateQuery<VersionRow>(VersionsTableName).Top(1).Async();

            if (topVersion.Any())
                return topVersion.First();

            return null;
        }
    }
}
