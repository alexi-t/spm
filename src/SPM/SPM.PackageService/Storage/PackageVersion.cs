using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechSmith.Hyde.Common.DataAnnotations;

namespace SPM.PackageService.Storage
{
    public class PackageVersion
    {
        [PartitionKey]
        public string PackageName { get; set; }
        [RowKey]
        public string Version { get; set; }

        public string FileID { get; set; }
    }
}
