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
        private readonly IFileService fileService;

        public PushCommand(IConfigService configService, IPackagesService packagesService, IFileService fileService) 
            : base("push", modifiers: new[] { autoTagModifier })
        {
            this.configService = configService;
            this.packagesService = packagesService;
            this.fileService = fileService;
        }

        protected override async Task RunCommandAsync()
        {
            PackageConfiguration config = configService.GetConfig();

            if (string.IsNullOrEmpty(config.Hash) || string.IsNullOrEmpty(config.Tag))
                throw new InvalidOperationException("Can not push until tag complete");

            string currentHash = fileService.ComputeHash(config.ExcludePaths);

            if (currentHash != config.Hash)
                throw new InvalidOperationException("Files had changed, rerun tag command");

            using (Stream packageStream = new MemoryStream(await fileService.CreatePackageAsync(config.ExcludePaths)))
            {
                await packagesService.UploadPackageAsync(config.Name, packageStream);
            }
        }
    }
}
