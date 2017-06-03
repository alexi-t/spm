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

        public void CreateConfig(string name, string[] excludes)
        {
            var root = new PackageConfiguration
            {
                Name = name,
                ExcludePaths = excludes
            };

            WriteConfig(root);
        }

        public PackageConfiguration GetConfig()
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

        private void WriteConfig(PackageConfiguration config) => fileService.WriteFile(CONFIG_FILE_NAME, JsonConvert.SerializeObject(config, Formatting.Indented));

        private PackageConfiguration ReadConfig() => JsonConvert.DeserializeObject<PackageConfiguration>(fileService.ReadFile(CONFIG_FILE_NAME));
        
        public void SetTag(string tag, string hash)
        {
            PackageConfiguration root = ReadConfig();

            root.Tag = tag;
            root.Hash = hash;

            WriteConfig(root);
        }
    }
}
