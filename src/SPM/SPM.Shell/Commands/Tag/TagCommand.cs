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
        private static CommandInput[] commandInput = new[]
            {
                new CommandInput
                {
                    Name = "package",
                    Index = 0,
                    Required = false
                },
                new CommandInput
                {
                    Name = "tag",
                    Index = 1,
                    Required = false
                }
            };

        private readonly IConfigService configService;

        public TagCommand(IConfigService configService) : base("tag", inputs: commandInput)
        {
            this.configService = configService;
        }

        private string ParsePackage(Dictionary<CommandInput, string> parsedInput)
        {
            List<string> packageNames = configService.GetAllPackageNames();

            CommandInput packageNameInput = GetCommandInputByName("packageName");
            if (parsedInput.ContainsKey(packageNameInput))
            {
                string packageName = parsedInput[packageNameInput];

                if (!packageNames.Contains(packageName))
                    throw new InvalidOperationException($"Package with name {packageName} not found");

                return packageName;
            }
            else if (packageNames.Count() == 1)
            {
                return packageNames.First();
            }
            else
                throw new InvalidOperationException("Package not provided");
        }

        private string ParseTag(Dictionary<CommandInput, string> parsedInput)
        {
            string tag = string.Empty;

            CommandInput tagInput = GetCommandInputByName("tag");

            if (parsedInput.ContainsKey(tagInput))
                tag = parsedInput[tagInput];

            if (string.IsNullOrEmpty(tag))
            {
                tag = DateTime.Now.ToString("yyyyMMdd");
            }

            return tag;
        }

        protected override void RunCommand(Dictionary<CommandInput, string> parsedInput, Dictionary<CommandArgument, string> parsedArguments)
        {
            string packageName = ParsePackage(parsedInput);
            string tag = ParseTag(parsedInput);

            configService.SetPackageTag(packageName, tag);
        }
    }
}
