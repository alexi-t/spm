using SPM.Shell.Commands.Base;
using SPM.Shell.Config;
using SPM.Shell.Services;
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

        public TagCommand(IConfigService configService, IFileService fileService) 
            : base("tag", inputs: new[] { tagNameInput })
        {
            this.configService = configService;
            this.fileService = fileService;
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

        protected override Task RunCommandAsync()
        {
            PackageConfiguration config = configService.GetConfig();

            string tag = GetTagName();

            string hash = fileService.ComputeHash(config.ExcludePaths);

            configService.SetTag(tag, hash);

            return Task.FromResult(0);
        }
    }
}
