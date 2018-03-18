using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechSmith.Hyde.Common.DataAnnotations;

namespace SPM.Http.PackageService.Model
{
    public class PackageChange : ITableEntity
    {
        [PartitionKey]
        public string Package { get; set; }
        [RowKey]
        public string Order { get; set; }

        public string Tag { get; private set; }
        public string FilePath { get; private set; }
        public string Hash { get; private set; }
        public string ChangeType { get; private set; }

        public string PartitionKey => Package;

        public string RowKey => Order;

        public PackageChange() { }

        public PackageChange(string package, string version, string filePath, string hash, string changeType)
        {
            Package = package;
            Order = (DateTime.MaxValue.Ticks - DateTime.Now.Ticks).ToString();

            Tag = version;
            FilePath = filePath;
            Hash = hash;
            ChangeType = changeType;
        }
    }
}
