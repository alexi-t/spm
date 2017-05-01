using SPM.Shell.Services;
using SPM.Shell.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
        private readonly IUIService uiService;

        public PullCommand(IPackagesService packagesService, IFileService fileService, IUIService uiService) 
            : base("pull", inputs: new[] { packageNameInput })
        {
            this.packagesService = packagesService;
            this.fileService = fileService;
            this.uiService = uiService;
        }

        protected async override Task RunCommandAsync()
        {
            string packageNameArg = GetCommandInputValue(packageNameInput);

            PackageInfo packageInfo = await packagesService.SearchPackageAsync(packageNameArg);

            if (packageInfo == null)
            {
                return;
            }

            string packageName = packageInfo.Name;
            string packageTag = packageInfo.Tag;

            if (!fileService.IsPackageExistInCache(packageName, packageTag))
            {
                HttpOperationWithProgress downloadOperation = packagesService.DownloadPackage(packageName, packageTag);

                downloadOperation.OnProgress += (processedCount, totalCount) =>
                {
                    uiService.DisplayProgress((float)processedCount * 100 / totalCount);
                };

                HttpResponseMessage response = await downloadOperation.GetOperationResultAsync();

                fileService.SavePackageInCache(packageName, packageTag, await response.Content.ReadAsByteArrayAsync());
            }

            fileService.ExtractPackageFromCache(packageName, packageTag);
        }
    }
}
