using SPM.Shell.Commands.Base;
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

        private static CommandModifier implicitIncludeModifier = new CommandModifier("implicitInclude");

        public InitCommand(IConfigService configService, IFileService fileService, IUIService uiService) 
            : base("init", modifiers: new[] { implicitIncludeModifier })
        {
            this.configService = configService;
            this.fileService = fileService;
            this.uiService = uiService;
        }

        protected async override Task RunCommandAsync()
        {
            bool implicitInclude = HasModifier(implicitIncludeModifier);

            var allFiles = fileService.SearchWorkingDirectory();

            string packageName = uiService.RequestValue($"Enter package name: ");

            uiService.AddMessage("Package files:");

            var excludes = new List<string>();

            foreach (var file in allFiles)
            {
                string fileName = Path.GetFileName(file);
                if (implicitInclude)
                {
                    if (!uiService.Ask($"Include {fileName}?"))
                    {
                        excludes.Add(fileName);
                    }
                }
                else
                {
                    uiService.AddMessage(fileName);
                }
            }

            configService.CreateConfig(packageName, excludes.ToArray());
        }
    }
}