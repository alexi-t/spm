using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechSmith.Hyde;

namespace SPM.PackageService.Storage
{
    public class PackagesService
    {
        private readonly string connectionString;

        private const string PackagesVersionsTableName = "packageversion";
        private const string PackagesTableName = "packages";

        public PackagesService(IOptions<Startup.ConnectionStrings> azureConnectionString)
        {
            this.connectionString = azureConnectionString.Value.AzureStorage;
        }

        private TechSmith.Hyde.Table.AzureTableStorageProvider GetStorage()
        {
            var account = new ConnectionStringCloudStorageAccount(connectionString);
            var storage = new TechSmith.Hyde.Table.AzureTableStorageProvider(account);
            return storage;
        }

        public async Task<string> GetLastPackageVersion(string packageName)
        {
            var storage = GetStorage();
            var packageVersions = await storage.CreateQuery<PackageVersion>(PackagesTableName).PartitionKeyEquals(packageName).Top(1).Async();
            if (packageVersions.Any())
                return packageVersions.First().Version;
            return "0.0.0.0";
        }
    }
}
