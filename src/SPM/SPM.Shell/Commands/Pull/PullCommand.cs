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
        private readonly IHashService hashService;
        private readonly ILocalStoreService localStoreService;

        public PullCommand(
            IOnlineStoreService onlineStoreService, 
            ILocalStoreService localStoreService, 
            IFileService fileService,
            IUIService uiService, 
            IConfigService configService,
            IHashService hashService) 
            : base("pull", inputs: new[] { packageNameInput })
        {
            this.onlineStoreService = onlineStoreService;
            this.localStoreService = localStoreService;
            this.fileService = fileService;
            this.uiService = uiService;
            this.configService = configService;
            this.hashService = hashService;
        }

        protected async override Task RunCommandAsync()
        {
            string packageNameAndTag = GetCommandInputValue(packageNameInput);

            string packageName = packageNameAndTag.Split('@').FirstOrDefault();

            string[] packageTagHistory = null;

            bool createConfig = false;

            try
            {
                Config.PackageConfiguration config = configService.GetConfig();

                if (packageName != config.Name)
                    throw new InvalidOperationException($"Folder binded to another package {config.Name}");

                string currentPackageName = $"{config.Name}@{config.Tag}";
                packageTagHistory = await onlineStoreService.GetPackageTagsAsync(packageNameAndTag, currentPackageName);
            }
            catch (ConfigFileNotFoundException)
            {
                createConfig = true;
                packageTagHistory = await onlineStoreService.GetPackageTagsAsync(packageNameAndTag);
            }

            foreach (string packageTag in packageTagHistory.Reverse())
            {
                PackageInfo packageInfo = await onlineStoreService.GetPackageVersionAsync($"{packageName}@{packageTag}");
                
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

                if (createConfig)
                {
                    configService.CreateConfig(packageNameAndTag, packageInfo.Hash);
                    createConfig = false;
                }
                else
                    configService.SetTag(packageTag, packageInfo.Hash);
            }
        }
    }
}