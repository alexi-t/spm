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
        private static CommandInput packageNameInput = 
            new CommandInput
            {
                Name = "package",
                Index = 0,
                Required = false
            };

        private static CommandInput tagNameInput =
            new CommandInput
            {
                Name = "tag",
                Index = 1,
                Required = false
            };

        private static CommandInput[] commandInput = new [] { packageNameInput, tagNameInput };

        private readonly IConfigService configService;

        public TagCommand(IConfigService configService) : base("tag", inputs: commandInput)
        {
            this.configService = configService;
        }

        private string GetPackageName()
        {
            List<string> packageNames = configService.GetAllPackageNames();

            string providedName = GetCommandInputValue(packageNameInput);

            if (string.IsNullOrEmpty(providedName))
            {
                if (packageNames.Count() == 1)
                {
                    return packageNames.First();
                }
            }
            else if (packageNames.Contains(providedName))
            {
                return providedName;
            }
            else
            {
                throw new InvalidOperationException($"Package with name {providedName} not found");
            }

            throw new InvalidOperationException("Package not provided");
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

        protected override void RunCommand()
        {
            string packageName = GetPackageName();
            string tag = GetTagName();

            configService.SetPackageTag(packageName, tag);
        }
    }
}
