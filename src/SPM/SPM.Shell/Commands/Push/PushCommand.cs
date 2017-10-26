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
        private static CommandModifier autoTagModifier = new CommandModifier("autoTag");
        
        private readonly IConfigService configService;
        private readonly IPackagesService packagesService;
        private readonly IHashService hashService;
        private readonly IFileService fileService;
        private readonly IUIService uiService;

        public PushCommand(IConfigService configService, IPackagesService packagesService, IHashService hashService, IFileService fileService, IUIService uiService) 
            : base("push", modifiers: new[] { autoTagModifier })
        {
            this.configService = configService;
            this.packagesService = packagesService;
            this.hashService = hashService;
            this.fileService = fileService;
            this.uiService = uiService;
        }

        protected override async Task RunCommandAsync()
        {
            PackageConfiguration config = configService.GetConfig();

            if (string.IsNullOrEmpty(config.Hash) || string.IsNullOrEmpty(config.Tag))
                throw new InvalidOperationException("Can not push until tag complete");

            List<string> packageFiles = configService.GetCurrentFilesList();
            string currentHash = hashService.ComputeFilesHash(packageFiles);

            if (currentHash != config.Hash)
                throw new InvalidOperationException("Files had changed, rerun tag command");

            uiService.AddMessage($"Pushing {config.Name}@{config.Tag}");
            
            await packagesService.PushPackageAsync($"{config.Name}@{config.Tag}", await fileService.ZipFiles(packageFiles));
        }
    }
}
