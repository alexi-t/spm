using SPM.Shell.Commands.Base;
using SPM.Shell.Config;
using SPM.Shell.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPM.Shell.Commands.Push
{
    public class PushCommand : BaseCommand
    {
        private static CommandInput packageNameInput = new CommandInput
        {
            Name = "packageName",
            Index = 0,
            Required = false
        };

        private static CommandInput[] inputs = new[] { packageNameInput };

        private readonly IConfigService configService;
        private readonly IPackagesService packagesService;
        private readonly IFileService fileService;

        public PushCommand(IConfigService configService, IPackagesService packagesService, IFileService fileService) : base("push", inputs)
        {
            this.configService = configService;
            this.packagesService = packagesService;
            this.fileService = fileService;
        }

        protected override async Task RunCommandAsync()
        {
            string providedPackageName = GetCommandInputValue(packageNameInput);

            List<CofigurationPackageDescription> availablePackages = configService.GetConfig().Packages.Select(p => p.Value).ToList();

            if (!string.IsNullOrEmpty(providedPackageName) &&
                !availablePackages.Any(p => p.Name == providedPackageName))
                throw new ArgumentOutOfRangeException();

            List<CofigurationPackageDescription> packagesToPush = !string.IsNullOrEmpty(providedPackageName) ?
                availablePackages.Where(p => p.Name == providedPackageName).ToList() :
                availablePackages;

            foreach (var package in packagesToPush)
            {
                using (Stream fileStream = fileService.ReadFileAsStream(package.FileName))
                    await this.packagesService.UploadPackageAsync($"{package.Name}@{package.Tag}", fileStream);
            }
        }
    }
}
