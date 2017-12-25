using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechSmith.Hyde.Common.DataAnnotations;

namespace SPM.Http.PackageService.Model
{
    public class PackageTag
    {
        public PackageTag() { }
        public PackageTag(string packageName, string tag, string versionInfo)
        {
            Package = packageName;
            Timestamp = (DateTime.MaxValue.Ticks - DateTime.Now.Ticks).ToString();
            Tag = tag;
            VersionInfo = versionInfo;
        }

        [PartitionKey]
        public string Package { get; set; }

        [RowKey]
        public string Timestamp { get; set; }

        public string VersionInfo { get; set; }

        public string Tag { get; set; }
    }
}
