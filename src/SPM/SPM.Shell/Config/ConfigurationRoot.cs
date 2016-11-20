using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPM.Shell.Config
{
    public class ConfigurationRoot
    {
        [JsonProperty("packages")]
        public Dictionary<string, CofigurationPackageDescription> Packages { get; }
    }
}
