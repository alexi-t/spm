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
        public ConfigurationRoot() { }
        public ConfigurationRoot(List<CofigurationPackageDescription> packages)
        {
            if (packages != null)
                this.Packages = packages.ToDictionary(d => d.Name);
        }

        [JsonProperty("packages")]
        public Dictionary<string, CofigurationPackageDescription> Packages { get; private set; }
    }
}
