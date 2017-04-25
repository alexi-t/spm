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

        private void WriteConfig(ConfigurationRoot config) => fileService.WriteFile(CONFIG_FILE_NAME, JsonConvert.SerializeObject(config, Formatting.Indented));

        private ConfigurationRoot ReadConfig() => JsonConvert.DeserializeObject<ConfigurationRoot>(fileService.ReadFile(CONFIG_FILE_NAME));

        public List<string> GetAllPackageNames()
        {
            ConfigurationRoot root = GetConfig();

            return root.Packages.Keys.ToList();
        }

        public void SetPackageTag(string packageName, string tag)
        {
            ConfigurationRoot root = ReadConfig();

            if (root.Packages.ContainsKey(packageName))
                root.Packages[packageName].Tag = tag;
            else
                throw new InvalidOperationException($"Can not find package with name {packageName}");

            WriteConfig(root);
        }
    }
}
