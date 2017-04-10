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

        protected override async void RunCommandAsync()
        {
            string providedPackageName = GetCommandInputValue(packageNameInput);

            List<string> availablePackages = configService.GetAllPackageNames();

            if (!string.IsNullOrEmpty(providedPackageName) &&
                !availablePackages.Contains(providedPackageName))
                throw new ArgumentOutOfRangeException();

            List<string> packagesToPush = !string.IsNullOrEmpty(providedPackageName) ?
                new List<string> { providedPackageName } :
                availablePackages;

            foreach (var packageName in packagesToPush)
            {
                using (Stream fileStream = fileService.ReadFileAsStream(packageName))
                    await this.packagesService.UploadPackageAsync(packageName, fileStream);
            }
        }
    }
}
