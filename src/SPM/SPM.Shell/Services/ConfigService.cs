using Newtonsoft.Json;
using SPM.Shell.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPM.Shell.Services
{
    public class ConfigService : IConfigService
    {
        private const string CONFIG_FILE_NAME = "packages.json";

        private readonly FileService fileService;

        public ConfigService(FileService fileService)
        {
            this.fileService = fileService;
        }

        public bool IsConfigExist()
        {
            return fileService.IsFileExist(CONFIG_FILE_NAME);
        }

        public void CreateConfig(List<CofigurationPackageDescription> initialPackages = null)
        {
            var root = new ConfigurationRoot(initialPackages);                

            WriteConfig(root);
        }

        public ConfigurationRoot GetConfig()
        {
            try
            {
                return ReadConfig();
            }
            catch (FileNotFoundException)
            {
                throw new ApplicationException("Config file not found");
            }
        }

        private void WriteConfig(ConfigurationRoot config) => fileService.WriteFile(CONFIG_FILE_NAME, JsonConvert.SerializeObject(config));

        private ConfigurationRoot ReadConfig() => JsonConvert.DeserializeObject<ConfigurationRoot>(fileService.ReadFile(CONFIG_FILE_NAME));
    }
}
