using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechSmith.Hyde.Common.DataAnnotations;

namespace SPM.Http.PackageService.Model
{
    public class Package
    {
        public const string PACKAGES_PARTITION_NAME = "Packages";

        [PartitionKey]
        public string Partition { get; set; } = PACKAGES_PARTITION_NAME;

        [RowKey]
        public string Name { get; set; }
        
        public string FileHash { get; set; }

        public Package() { }

        public Package(string name, string tag, string fileHash)
        {
            Name = $"{name}@{tag}";
            FileHash = fileHash;
        }
    }
}
