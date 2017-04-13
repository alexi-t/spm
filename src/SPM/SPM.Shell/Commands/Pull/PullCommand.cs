using SPM.Shell.Services;
using SPM.Shell.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPM.Shell.Commands.Pull
{
    public class PullCommand : BaseCommand
    {
        private static CommandInput packageNameInput = new CommandInput
        {
            Index = 0,
            Name = "packageName",
            Required = true
        };
        private readonly IFileService fileService;
        private readonly IPackagesService packagesService;

        public PullCommand(IPackagesService packagesService, IFileService fileService) : base("pull", inputs: new[] { packageNameInput })
        {
            this.packagesService = packagesService;
            this.fileService = fileService;
        }

        protected async override Task RunCommandAsync()
        {
            string packageName = GetCommandInputValue(packageNameInput);

            PackageInfo packageInfo = await packagesService.SearchPackageAsync(packageName);

            if (packageInfo == null)
            {
                return;
            }

            await fileService.DownloadPackage(packageInfo.Name, packageInfo.Tag, packageInfo.DownloadLink);
        }
    }
}
