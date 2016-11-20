using Newtonsoft.Json;
using SPM.Shell.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPM.Shell.Config
{
    public static class ConfigManager
    {
        private const string ConfigFileName = "sppackages.json";

        public static ConfigurationRoot ReadFromWorkingDirectory()
        {
            if (!File.Exists(ConfigFileName))
                throw new ConfigFileNotFoundException();

            var configJson = File.ReadAllText(ConfigFileName);

            return JsonConvert.DeserializeObject<ConfigurationRoot>(configJson);
        }

        internal static void UpdatePackageConfig(string packageName, CofigurationPackageDescription packageConfig)
        {
            throw new NotImplementedException();
        }
    }
}
