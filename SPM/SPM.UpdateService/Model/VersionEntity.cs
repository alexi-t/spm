using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechSmith.Hyde.Common.DataAnnotations;

namespace SPM.UpdateService.Model
{
    public class VersionEntity
    {
        public VersionEntity() { }
        public VersionEntity(string version)
        {
            PartitionKey = "Versions";
            RowKey = (DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks).ToString();
            Version = version;
        }

        [PartitionKey]
        public string PartitionKey { get; set; }
        [RowKey]
        public string RowKey { get; set; }

        public string Version { get; set; }
    }
}
