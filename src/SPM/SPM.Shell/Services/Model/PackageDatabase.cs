using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPM.Shell.Services.Model
{
    public class CachedPackageInfo
    {
        public string Name { get; set; }
        public string Tag { get; set; }
        public string Path { get; set; }
        public string Hash { get; set; }
    }
}
