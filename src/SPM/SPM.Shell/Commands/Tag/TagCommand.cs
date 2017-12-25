using SPM.Shell.Commands.Base;
using SPM.Shell.Config;
using SPM.Shell.Services;
using SPM.Shell.Services.Model;
using System;
using System.Collections.Generic;
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
        private readonly UIService uiService;
        private readonly IVersioningService versioningService;
        private readonly IOnlineStoreService onlineStoreService;

        public TagCommand(IConfigService configService, IFileService fileService, IHashService hashService, UIService uiService, IVersioningService versioningService, IOnlineStoreService onlineStoreService)
            : base("tag", inputs: new[] { tagNameInput })
        {
            this.configService = configService;
            this.fileService = fileService;
            this.hashService = hashService;
            this.uiService = uiService;
            this.versioningService = versioningService;
            this.onlineStoreService = onlineStoreService;
        }

        private string GetTagName()
        {
            string tag = GetCommandInputValue(tagNameInput);

            if (string.IsNullOrEmpty(tag))
            {
                tag = DateTime.Now.ToString("yyyyMMdd");
            }

            return tag;
        }

        protected async override Task RunCommandAsync()
        {
            PackageConfiguration config = configService.GetConfig();

            string[] currentFilesList = fileService.GetWorkingDirectoryFiles(config.ExcludePaths);

            string currentHash = hashService.ComputeFilesHash(currentFilesList);

            if (currentHash == config.Hash)
            {
                uiService.AddMessage("No difference with previous version. Can not add tag");
                return;
            }

            string tag = GetTagName();

            FolderVersionEntry folderVersion = versioningService.CreateDiff(currentFilesList);

            configService.SetTag(tag, folderVersion.Hash);

            await onlineStoreService.PushPackageAsync($"{config.Name}@{config.Tag}", folderVersion);
        }
    }
}
