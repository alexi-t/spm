using SPM.Http.PackageService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TechSmith.Hyde.Table;
using TechSmith.Hyde;
using TechSmith.Hyde.Common;

namespace SPM.Http.PackageService.Service
{
    public class PackageService
    {
        private readonly string connectionString;
        private readonly ConnectionStringCloudStorageAccount storageAccount;

        private const string PackagesTableName = "packages";
        private const string PackagesTagsTableName = "packageTags";

        public PackageService(string connectionString)
        {
            this.connectionString = connectionString;

            this.storageAccount = new ConnectionStringCloudStorageAccount(connectionString);
        }

        public async Task<Package> GetPackageByNameAndTagAsync(string name, string tag)
        {
            var tableStorage = new AzureTableStorageProvider(storageAccount);

            try
            {
                return await tableStorage.GetAsync<Package>(PackagesTableName, name, tag);
            }
            catch (EntityDoesNotExistException)
            {
                return null;
            }
        }

        public async Task<List<string>> GetPackageTagsAsync(string packageName, int count = 0, string fromRowId = null)
        {
            var tableStorage = new AzureTableStorageProvider(storageAccount);

            IRowKeyFilterable<PackageTag> filteredTags = tableStorage.CreateQuery<PackageTag>(PackagesTagsTableName).PartitionKeyEquals(packageName);

            var tags = await filteredTags.Async();

            return tags.Any() ? tags.Select(t => t.Tag).ToList() : null;
        }

        internal async Task<Package> AddPackageAsync(string name, string tag, string versionInfo, string fileHash)
        {
            var tableStorage = new AzureTableStorageProvider(storageAccount);

            var package = new Package { Name = name, Tag = tag, Hash = fileHash };

            tableStorage.Add(PackagesTableName, package);
            tableStorage.Add(PackagesTagsTableName, new PackageTag(name, tag, versionInfo));

            await tableStorage.SaveAsync();

            return package;
        }

        internal async Task UpdatePackageAsync(Package package)
        {
            var tableStorage = new AzureTableStorageProvider(storageAccount);
            tableStorage.Update(PackagesTableName, package);
            await tableStorage.SaveAsync();
        }
    }
}
