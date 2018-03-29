using SPM.Shell.Commands.Base;
using SPM.Shell.Config;
using SPM.Shell.Services;
using SPM.Shell.Services.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
                string newTag = DateTime.Now.ToString("yyyyMMdd");

                string[] existingTags = await onlineStoreService.GetAllPackageTagsAsync(configuration.Name);
                string lastTag = existingTags.FirstOrDefault();

                if (string.IsNullOrEmpty(lastTag))
                    tag = newTag;

                if (lastTag == newTag)
                    newTag = lastTag + ".1";
                else if (lastTag.StartsWith(newTag))
                {
                    string[] split = newTag.Split('.');
                    if (int.TryParse(split.Last(), out int counter))
                    {
                        newTag = newTag + "." + (++counter);
                    }
                    else
                        throw new ArgumentException($"Can not create auto tag. Last tag is {lastTag}");
                }
            }

            return tag;
        }

        protected async override Task RunCommandAsync()
        {
            PackageConfiguration config = null;
            if (!configService.TryGetConfig(out config))
            {
                uiService.AddMessage("No config inited, can not compute diff");
                return;
            }

            string newTag = await GetTagName(config);

            Dictionary<string, string> actualPathToHashMap = new Dictionary<string, string>();

            foreach (var filePath in fileService.GetWorkingDirectoryFiles(config.ExcludePaths))
            {
                actualPathToHashMap.Add(filePath, hashService.ComputeFileHash(filePath));
            }

            Dictionary<string, string> tagPathToHashMap =
                await onlineStoreService.GetPackageFilesAtVersionAsync(config.Name, config.Tag);

            bool hasChanges =
                tagPathToHashMap.Keys.Except(actualPathToHashMap.Keys).Any() ||
                actualPathToHashMap.Keys.Except(tagPathToHashMap.Keys).Any();

            if (!hasChanges)
                foreach (var pathToHashMap in tagPathToHashMap)
                {
                    if (actualPathToHashMap[pathToHashMap.Key] != pathToHashMap.Value)
                    {
                        hasChanges = true;
                        break;
                    }
                }

            if (!hasChanges)
            {
                uiService.AddMessage("No difference with previous version. Can not add tag");
                return;
            }

            FolderVersionEntry folderVersion = versioningService.CreateDiff(tagPathToHashMap, actualPathToHashMap);

            uiService.AddMessage($"Created {config.Name}@{newTag}");
            uiService.AddMessage("List of changes:");
            foreach (FileHistoryEntry fileHistoryEntry in folderVersion.Files)
            {
                uiService.AddMessage($"\t[{fileHistoryEntry.EditType.ToString().ToLower()}]\t{Path.GetFileName(fileHistoryEntry.Path)}");
            }

            configService.SetTag(newTag);

            await onlineStoreService.PushPackageAsync($"{config.Name}@{newTag}", folderVersion);
        }
    }
}