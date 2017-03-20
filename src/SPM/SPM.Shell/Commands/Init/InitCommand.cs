using SPM.Shell.Config;
using SPM.Shell.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPM.Shell.Commands.Init
{
    public class InitCommand : BaseCommand
    {
        private readonly IConfigService configService;
        private readonly IFileService fileService;
        private readonly IUIService uiService;

        public InitCommand(IConfigService configService, IFileService fileService, IUIService uiService) : base("init")
        {
            this.configService = configService;
            this.fileService = fileService;
            this.uiService = uiService;
        }

        protected override void RunCommand(Dictionary<CommandInput, string> parsedInput, Dictionary<CommandArgument, string> parsedArguments)
        {
            var wspFiles = fileService.SearchWorkingDirectory("*.wsp");

            var packagesConfigurationList = new List<CofigurationPackageDescription>();
            foreach (var file in wspFiles)
            {
                string fileName = Path.GetFileName(file);
                string defaultPackageName = Path.GetFileNameWithoutExtension(file);

                string name = uiService.RequestValue($"Enter name for wsp file {fileName} (default ${defaultPackageName}): ");
                if (string.IsNullOrEmpty(name))
                    name = defaultPackageName;

                var packageConfig = new CofigurationPackageDescription
                {
                    Name = name,
                    FileName = Path.GetFileName(file)
                };

                packagesConfigurationList.Add(packageConfig);
            }

            configService.CreateConfig(packagesConfigurationList);
        }
    }
}