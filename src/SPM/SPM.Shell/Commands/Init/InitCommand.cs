using SPM.Shell.Commands.Base;
using SPM.Shell.Config;
using SPM.Shell.Services;
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
        private readonly IFileService fileService;
        private readonly IUIService uiService;

        private static CommandModifier explicitIncludeModifier = new CommandModifier("explicitInclude");

        public InitCommand(IConfigService configService, IFileService fileService, IUIService uiService) 
            : base("init", modifiers: new[] { explicitIncludeModifier })
        {
            this.configService = configService;
            this.fileService = fileService;
            this.uiService = uiService;
        }

        protected async override Task RunCommandAsync()
        {
            string currentlyRunningAssemblyName = Assembly.GetExecutingAssembly().Location;
            string currentlyRunningAssemblyConfig = currentlyRunningAssemblyName.Replace(".exe", ".exe.config");

            bool implicitInclude = HasModifier(explicitIncludeModifier);

            var allFiles = fileService.ListFilesInDirectory(".");

            string packageName = uiService.RequestValue($"Enter package name: ");

            uiService.AddMessage("Package files:");

            var excludes = new List<string>();
            
            foreach (var file in allFiles)
            {
                if (file == currentlyRunningAssemblyName ||
                    file == currentlyRunningAssemblyConfig)
                {
                    excludes.Add(file);
                    continue;
                }

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