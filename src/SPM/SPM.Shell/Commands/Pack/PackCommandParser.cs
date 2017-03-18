using CLAP;
using SPM.Shell.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPM.Shell.Commands.Pack
{
    public class PackCommandParser
    {
        private readonly IHashService hashService;
        private readonly IVersionsService versionService;

        public PackCommandParser(IVersionsService versionService, IHashService hashService)
        {
            this.versionService = versionService;
            this.hashService = hashService;
        }

        [Verb()]
        public async void Pack()
        {
            var config = ConfigManager.ReadFromWorkingDirectory();

            foreach (var package in config.Packages)
            {
                var packageName = package.Key;
                var packageConfig = package.Value;

                var packageData = File.ReadAllBytes(packageName);

                string hash = string.Empty;

                using (var fileStream = File.OpenRead(packageName))
                    hashService.ComputeHash(fileStream);

                //if (hash == packageConfig.Hash)
                //    continue;

                var nextVersion = await versionService.GetNextVersionAsync(packageConfig.Name);

                //packageConfig.Version = nextVersion;
                //packageConfig.Hash = hash;

                ConfigManager.UpdatePackageConfig(packageName, packageConfig);
            }
        }
    }
}
