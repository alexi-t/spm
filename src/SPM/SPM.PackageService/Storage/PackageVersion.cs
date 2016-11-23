using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechSmith.Hyde.Common.DataAnnotations;

namespace SPM.PackageService.Storage
{
    public class PackageVersion
    {
        public PackageVersion() { }

        public PackageVersion(string name, string version, string fileUrl)
        {
            PackageName = name;
            RowKey = (DateTime.MaxValue.Ticks - DateTime.Now.Ticks).ToString();
            Version = version;
            FileUrl = fileUrl;
        }

        [PartitionKey]
        public string PackageName { get; set; }
        [RowKey]
        public string RowKey { get; set; }
        public string Version { get; set; }

        public string FileUrl { get; set; }
    }
}
