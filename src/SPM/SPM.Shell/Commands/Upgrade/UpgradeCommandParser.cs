using CLAP;
using SPM.Shell.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SPM.Shell.Commands.Upgrade
{
    public class PullCommandParser
    {
        private readonly IPullService pullService;

        public PullCommandParser(IPullService pullService)
        {
            this.pullService = pullService;
        }

        [Verb()]
        public async void Upgrade()
        {
            var config = ConfigManager.ReadFromWorkingDirectory();

            foreach (var package in config.Packages.Values)
            {
                var newVersion = pullService.Pull(package.Name);
                if (newVersion.Version != package.Version)
                {
                    package.Version = newVersion.Version;

                    var httpClient = new HttpClient();

                    var newPackageRequest = await httpClient.GetAsync(newVersion.PackageUrl);

                    var data = await newPackageRequest.Content.ReadAsByteArrayAsync();
                    File.WriteAllBytes(package.Name, data);

                    ConfigManager.UpdatePackageConfig(package.Name, package);
                }
            }
        }
    }
}
