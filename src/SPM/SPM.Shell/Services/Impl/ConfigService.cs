using Newtonsoft.Json;
using SPM.Shell.Config;
using SPM.Shell.Exceptions;
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
        private const string CONFIG_FILE_NAME = "spm.json";

        private readonly FileService fileService;

        public ConfigService(FileService fileService)
        {
            this.fileService = fileService;
        }
        
        public void CreateConfig(string name, string hash)
        {
            var root = new PackageConfiguration
            {
                Name = name
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
                throw new ConfigFileNotFoundException();
            }
        }

        private void WriteConfig(PackageConfiguration config) => fileService.WriteFile(CONFIG_FILE_NAME, JsonConvert.SerializeObject(config, Formatting.Indented));

        private PackageConfiguration ReadConfig() => JsonConvert.DeserializeObject<PackageConfiguration>(fileService.ReadFile(CONFIG_FILE_NAME));
        
        public List<string> GetCurrentFilesList()
        {
            PackageConfiguration config = ReadConfig();
            return Directory.GetFiles(Environment.CurrentDirectory).Except(config.ExcludePaths).Where(f => Path.GetFileName(f) != CONFIG_FILE_NAME).ToList();
        }

        public bool TryGetConfig(out PackageConfiguration packageConfiguration)
        {
            packageConfiguration = null;
            try
            {
                packageConfiguration = ReadConfig();
                return true;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
        }

        public void SetTag(string tag)
        {
            PackageConfiguration root = ReadConfig();

            root.Tag = tag;

            WriteConfig(root);
        }
    }
}
