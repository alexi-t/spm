using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPM.Shell.Config
{
    public class CofigurationPackageDescription
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("fileName")]
        public string FileName { get; set; }
        [JsonProperty("version")]
        public string Version { get; set; }
        [JsonProperty("hash")]
        public string Hash { get; set; }
    }
}
