using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechSmith.Hyde.Common.DataAnnotations;

namespace SPM.Http.PackageService.Model
{
    public class PackageTag : ITableEntity
    {
        public PackageTag() { }
        public PackageTag(string packageName, string tag, string storageKey)
        {
            Package = packageName;
            Timestamp = (DateTime.MaxValue.Ticks - DateTime.Now.Ticks).ToString();
            Tag = tag;
            StorageKey = storageKey;
        }

        [PartitionKey]
        public string Package { get; set; }

        [RowKey]
        public string Timestamp { get; set; }

        public string Tag { get; set; }

        public string StorageKey { get; set; }

        public string PartitionKey => Package;
        public string RowKey => Timestamp;
    }
}
