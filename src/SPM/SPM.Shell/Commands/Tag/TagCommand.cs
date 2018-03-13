using SPM.Shell.Commands.Base;
using SPM.Shell.Config;
using SPM.Shell.Services;
using SPM.Shell.Services.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPM.Shell.Commands.Tag
{
    public class TagCommand : BaseCommand
    {
        private static CommandInput tagNameInput =
            new CommandInput
            {
                Name = "tag",
                Index = 0,
                Required = false
            };

        private readonly IConfigService configService;
        private readonly IFileService fileService;
        private readonly IHashService hashService;
        private readonly IUIService uiService;
        private readonly IVersioningService versioningService;
        private readonly IOnlineStoreService onlineStoreService;

        public TagCommand(
            IConfigService configService, 
            IFileService fileService, 
            IHashService hashService, 
            IUIService uiService, 
            IVersioningService versioningService, 
            IOnlineStoreService onlineStoreService)
            : base("tag", inputs: new[] { tagNameInput })
        {
            this.configService = configService;
            this.fileService = fileService;
            this.hashService = hashService;
            this.uiService = uiService;
            this.versioningService = versioningService;
            this.onlineStoreService = onlineStoreService;
        }

        private async Task<string> GetTagName(PackageConfiguration configuration)
        {
            string tag = GetCommandInputValue(tagNameInput);

            if (string.IsNullOrEmpty(tag))
            {
                tag = DateTime.Now.ToString("yyyyMMdd");

                string[] tags = await onlineStoreService.GetAllPackageTagsAsync(configuration.Name, 1);
                string lastTag = tags.FirstOrDefault();
            }

            return tag;
        }

        protected async override Task RunCommandAsync()
        {
            PackageConfiguration config = configService.GetConfig();

            List<string> currentFilesList = fileService.GetWorkingDirectoryFiles(config.ExcludePaths);

            string currentHash = hashService.ComputeFilesHash(currentFilesList);

            if (currentHash == config.Hash)
            {
                uiService.AddMessage("No difference with previous version. Can not add tag");
                return;
            }

            string tag =  await GetTagName(config);

            FolderVersionEntry folderVersion = versioningService.CreateDiff(currentFilesList);

            uiService.AddMessage($"Created {config.Name}@{tag}");
            uiService.AddMessage("List of changes:");
            foreach (FileHistoryEntry fileHistoryEntry in folderVersion.Files)
            {
                uiService.AddMessage($"\t[{fileHistoryEntry.EditType.ToString().ToLower()}]\t{Path.GetFileName(fileHistoryEntry.Path)}");
            }

            configService.SetTag(tag, currentHash);

            await onlineStoreService.PushPackageAsync($"{config.Name}@{tag}", currentHash, folderVersion);
        }
    }
}