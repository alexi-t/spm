using Newtonsoft.Json;
using SPM.Shell.Commands.Base;
using SPM.Shell.Exceptions;
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
        private readonly IOnlineStoreService onlineStoreService;
        private readonly IUIService uiService;
        private readonly IConfigService configService;
        private readonly ILocalStoreService localStoreService;

        public PullCommand(
            IOnlineStoreService onlineStoreService, 
            ILocalStoreService localStoreService, 
            IFileService fileService,
            IUIService uiService, 
            IConfigService configService) 
            : base("pull", inputs: new[] { packageNameInput })
        {
            this.onlineStoreService = onlineStoreService;
            this.localStoreService = localStoreService;
            this.fileService = fileService;
            this.uiService = uiService;
            this.configService = configService;
        }

        protected async override Task RunCommandAsync()
        {
            string packageNameAndTag = GetCommandInputValue(packageNameInput);

            string[] packageTagHistory = null;

            try
            {
                Config.PackageConfiguration config = configService.GetConfig();
                string currentPackageName = $"{config.Name}@{config.Tag}";
                packageTagHistory = await onlineStoreService.GetPackageTagsAsync(packageNameAndTag, currentPackageName);
            }
            catch (ConfigFileNotFoundException)
            {
                packageTagHistory = await onlineStoreService.GetPackageTagsAsync(packageNameAndTag);
            }

            foreach (string packageVersion in packageTagHistory)
            {
                PackageInfo packageInfo = await onlineStoreService.GetPackageVersionAsync(packageVersion);

                string packageName = packageInfo.Name;
                string packageTag = packageInfo.Tag;
                FolderVersionEntry folderVersion = JsonConvert.DeserializeObject<FolderVersionEntry>(packageInfo.VersionInfo);

                if (!localStoreService.PackageExist(packageInfo))
                {
                    HttpOperationWithProgress downloadOperation = onlineStoreService.DownloadPackage(packageName, packageTag);

                    downloadOperation.OnProgress += (processedCount, totalCount) =>
                    {
                        uiService.DisplayProgress((float)processedCount * 100 / totalCount);
                    };

                    HttpResponseMessage response = await downloadOperation.GetOperationResultAsync();

                    localStoreService.SavePackage(packageInfo, await response.Content.ReadAsByteArrayAsync());
                }

                localStoreService.RestorePackage(packageName, packageTag);

                configService.SetTag(packageTag, folderVersion.Hash);
            }
        }
    }
}
