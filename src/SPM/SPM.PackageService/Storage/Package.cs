using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using TechSmith.Hyde.Common.DataAnnotations;

namespace SPM.PackageService.Storage
{
    public class Package
    {
        public Package() { }

        public Package(string name)
        {
            Partition = PACKAGES_PARTITION_NAME;
            Name = name;
        }

        public const string PACKAGES_PARTITION_NAME = "Packages";

        [IgnoreDataMember]
        [PartitionKey]
        public string Partition { get; set; }
        [RowKey]
        public string Name { get; set; }

        public string LastVersion { get; set; }

        public string WspName { get; set; }
    }
}
