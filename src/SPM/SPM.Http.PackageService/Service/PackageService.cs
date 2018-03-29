using SPM.Http.PackageService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TechSmith.Hyde.Table;
using TechSmith.Hyde;
using TechSmith.Hyde.Common;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json.Linq;

namespace SPM.Http.PackageService.Service
{
    public class PackageService
    {
        private readonly string connectionString;
        private readonly ConnectionStringCloudStorageAccount storageAccount;

        private const string PackagesTableName = "packages";
        private const string PackagesFileChangesTableName = "packageFileChanges";

        public PackageService(string connectionString)
        {
            this.connectionString = connectionString;

            this.storageAccount = new ConnectionStringCloudStorageAccount(connectionString);
        }

        public async void Init()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            CloudTable table = null;

            table = tableClient.GetTableReference(PackagesTableName);
            await table.CreateIfNotExistsAsync();

            table = tableClient.GetTableReference(PackagesFileChangesTableName);
            await table.CreateIfNotExistsAsync();
        }

        public async Task<string[]> GetAllPackagesAsync()
        {
            var allPackages = await GetAllEntitiesFromPartitionAsync<Package>(PackagesTableName, Package.PACKAGES_PARTITION_NAME);
            return allPackages.Select(p => p.Name).ToArray();
        }

        public async Task<Package> GetPackageByNameAndTagAsync(string name)
        {
            var tableStorage = new AzureTableStorageProvider(storageAccount);

            try
            {
                return await tableStorage.GetAsync<Package>(PackagesTableName, Package.PACKAGES_PARTITION_NAME, name);
            }
            catch (EntityDoesNotExistException)
            {
                return null;
            }
        }

        public Task<List<PackageTag>> GetPackageTagsAsync(string packageName) =>
            GetAllEntitiesFromPartitionAsync<PackageTag>(PackagesTableName, packageName);


        internal async Task<PackageTag> AddPackageVersionAsync(string name, string tag, string versionChanges, string fileHash)
        {
            var tableStorage = new AzureTableStorageProvider(storageAccount);

            if (name == Package.PACKAGES_PARTITION_NAME)
                throw new ArgumentException($"{name} is reserved");

            Package package = new Package(name, tag, fileHash);
            tableStorage.Upsert(PackagesTableName, package); // save as Packages | name@tag

            List<PackageTag> allTags = await GetAllEntitiesFromPartitionAsync<PackageTag>(PackagesTableName, name);

            if (allTags.Any())
            {
                if (allTags.Any(t => t.Tag == tag) && allTags.First().Tag != tag) //tag already exist and it is not last
                {
                    throw new ArgumentException($"Tag {tag} already exist and it is not last");
                }
                else if (allTags.First().Tag == tag)
                {
                    IEnumerable<PackageChange> packageChanges = await GetAllEntitiesFromPartitionAsync<PackageChange>(PackagesFileChangesTableName, name);

                    foreach (var prevChange in packageChanges.Where(c => c.Tag == tag))
                    {
                        tableStorage.Delete(PackagesFileChangesTableName, prevChange);
                    }
                    tableStorage.Delete(PackagesTableName, allTags.First());
                }
            }

            PackageTag packageVersion = new PackageTag(name, tag, fileHash);
            tableStorage.Upsert(PackagesTableName, packageVersion); //save as name | timestamp (tag, hash)

            JObject versionInfoJO = JObject.Parse(versionChanges);
            foreach (var fileEntry in versionInfoJO["files"])
            {
                string path = fileEntry["path"].Value<string>();
                string hash = fileEntry["hash"].Value<string>();
                string changeType = fileEntry["editType"].Value<string>();
                tableStorage.Add(PackagesFileChangesTableName, new PackageChange(name, tag, path, hash, changeType));
            }

            await tableStorage.SaveAsync();

            return packageVersion;
        }

        internal Task<List<PackageChange>> GetPackageFileHistory(string name) => GetAllEntitiesFromPartitionAsync<PackageChange>(PackagesFileChangesTableName, name);

        private async Task<List<T>> GetAllEntitiesFromPartitionAsync<T>(string tableName, string partitionName) where T : Model.ITableEntity, new()
        {
            List<T> result = new List<T>();

            var tableStorage = new AzureTableStorageProvider(storageAccount);

            string lastRowKey = string.Empty;
            while (true)
            {
                IEnumerable<T> entities = null;
                if (string.IsNullOrEmpty(lastRowKey))
                {
                    entities = await tableStorage
                        .CreateQuery<T>(tableName)
                        .PartitionKeyEquals(partitionName)
                        .Top(1000).Async();
                }
                else
                {
                    entities = await tableStorage
                        .CreateQuery<T>(tableName)
                        .PartitionKeyEquals(partitionName)
                        .RowKeyFrom(lastRowKey).Exclusive()
                        .Top(1000).Async();
                }

                result.AddRange(entities);

                if (entities.Count() < 1000)
                    break;
                else
                    lastRowKey = entities.Last().RowKey;
            }

            return result;
        }
    }
}
