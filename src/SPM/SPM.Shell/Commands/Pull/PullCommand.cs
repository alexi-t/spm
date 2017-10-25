using SPM.Shell.Commands.Base;
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
        private readonly ILocalStoreService localStoreService;

        public PullCommand(IPackagesService packagesService, IFileService fileService, IUIService uiService, ILocalStoreService localStoreService) 
            : base("pull", inputs: new[] { packageNameInput })
        {
            this.packagesService = packagesService;
            this.fileService = fileService;
            this.uiService = uiService;
            this.localStoreService = localStoreService;
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

            if (!localStoreService.PackageExist(packageName, packageTag))
            {
                HttpOperationWithProgress downloadOperation = packagesService.DownloadPackage(packageName, packageTag);

                downloadOperation.OnProgress += (processedCount, totalCount) =>
                {
                    uiService.DisplayProgress((float)processedCount * 100 / totalCount);
                };

                HttpResponseMessage response = await downloadOperation.GetOperationResultAsync();
                
                localStoreService.SavePackage(packageName, packageTag, await response.Content.ReadAsByteArrayAsync());
            }

            localStoreService.RestorePackage(packageName, packageTag);
        }
    }
}
