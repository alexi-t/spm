using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechSmith.Hyde.Common.DataAnnotations;

namespace SPM.Http.PackageService.Model
{
    public class Package
    {
        public string Hash { get; internal set; }
        [PartitionKey]
        public string Name { get; set; }
        [RowKey]
        public string Tag { get; set; }

    }
}
