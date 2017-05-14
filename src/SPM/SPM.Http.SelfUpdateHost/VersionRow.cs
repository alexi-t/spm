using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using TechSmith.Hyde.Common.DataAnnotations;

namespace SPM.Http.SelfUpdateHost
{
    public class VersionRow
    {
        public VersionRow() { }

        [IgnoreDataMember]
        [PartitionKey]
        public string AppName { get; set; }

        [IgnoreDataMember]
        [RowKey]
        public string Timestamp { get; set; }

        [DataMember(Name = "Created")]
        [Timestamp]
        public DateTimeOffset CreateTime { get; set; }

        public VersionRow(string appName, string version, string hash)
        {
            AppName = appName;
            Timestamp = (DateTime.MaxValue.Ticks - DateTime.Now.Ticks).ToString();
            Version = version;
            Hash = hash;
            CreateTime = DateTime.UtcNow;
        }

        [DataMember(Name = "version")]
        public string Version { get; set; }

        [IgnoreDataMember]
        public string Hash { get; set; }
    }
}
