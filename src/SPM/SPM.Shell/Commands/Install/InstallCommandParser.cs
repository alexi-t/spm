using CLAP;
using SPM.Shell.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SPM.Shell.Commands.Install
{
    public class InstallCommandParser
    {
        private readonly IPackageService packageService;

        public InstallCommandParser(IPackageService packageService)
        {
            this.packageService = packageService;
        }

        [Verb]
        public async void Install([Aliases("p")]string packageName)
        {
            PackageDescription package = await packageService.GetPackageAsync(packageName);

            if (package != null)
            {
                var config = ConfigManager.ReadFromWorkingDirectory();

                var httpClient = new HttpClient();

                var wspFileData = await httpClient.GetByteArrayAsync(package.FileUrl);

                File.WriteAllBytes(package.WspName, wspFileData);

                ConfigManager.UpdatePackageConfig(packageName, new CofigurationPackageDescription
                {
                    Name = packageName,
                    Hash = "",
                    Version = package.Version
                });
            }
        }
    }
}
