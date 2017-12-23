using SPM.Shell.Commands.Base;
using SPM.Shell.Config;
using SPM.Shell.Services;
using SPM.Shell.Services.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SPM.Shell.Commands.Init
{
    public class InitCommand : BaseCommand
    {
        private readonly IConfigService configService;
        private readonly IUIService uiService;
        private readonly IVersioningService versioningService;
        private readonly IOnlineStoreService onlineStoreService;
        private readonly IFileService fileService;

        private static CommandModifier explicitIncludeModifier = new CommandModifier("explicitInclude");
        private static CommandArgument ignoreList = new CommandArgument("ignore", "i");

        public InitCommand(IVersioningService versioningService, IConfigService configService, IUIService uiService, IOnlineStoreService onlineStoreService, IFileService fileService) 
            : base("init", 
                  modifiers: new[] { explicitIncludeModifier },
                  arguments: new[] { ignoreList })
        {
            this.configService = configService;
            this.uiService = uiService;
            this.versioningService = versioningService;
            this.onlineStoreService = onlineStoreService;
            this.fileService = fileService;
        }

        protected async override Task RunCommandAsync()
        {
            bool explicitInclude = HasModifier(explicitIncludeModifier);
            string ignoreSetup = GetArgumentValue(ignoreList);

            FolderVersionEntry version = await versioningService.CreateInitialVersion(explicitInclude, ignoreSetup.Split(','));

            string packageName = uiService.RequestValue($"Enter package name: ");
                        
            configService.CreateConfig(packageName, version.Hash);

            await onlineStoreService.PushPackageAsync($"{packageName}@initial", await fileService.ZipFiles(version.Files));
        }
    }
}