using CLAP;
using SPM.Shell.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPM.Shell.Commands.Push
{
    public class PushCommandParser
    {
        private readonly IPackageService packageService;

        public PushCommandParser(IPackageService packageService)
        {
            this.packageService = packageService;
        }

        [Verb()]
        public async void Push()
        {
            var config = ConfigManager.ReadFromWorkingDirectory();

            foreach (var package in config.Packages)
            {
                var packageName = package.Key;
                var packageConfig = package.Value;

                using (var packageFile = File.OpenRead(packageName))
                {
                    bool canPushPackage = await packageService.GetCanPushPackageAsync(packageName, packageConfig);
                    if (canPushPackage)
                    {
                        await packageService.Push(packageName, packageConfig, packageFile);
                    }
                }
            }
        }
    }
}
