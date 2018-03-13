using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SPM.Http.PackageService.Model
{
    public interface ITableEntity
    {
        string PartitionKey { get; }
        string RowKey { get; }
    }
}
