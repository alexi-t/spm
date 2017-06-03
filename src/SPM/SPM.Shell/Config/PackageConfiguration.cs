using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPM.Shell.Config
{
    public class PackageConfiguration
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("tag")]
        public string Tag { get; set; }
        [JsonProperty("excludes")]
        public string[] ExcludePaths { get; set; }
        [JsonProperty("dataHsh")]
        public string Hash { get; set; }
    }
}
