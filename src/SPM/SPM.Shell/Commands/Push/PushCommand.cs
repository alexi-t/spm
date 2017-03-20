using SPM.Shell.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPM.Shell.Commands.Push
{
    public class PushCommand : BaseCommand
    {
        private static CommandInput[] inputs = new[]
        {
            new CommandInput
            {
                Name = "packageName",
                Index = 0 ,
                Required = false
            }
        };

        private readonly IConfigService configService;

        public PushCommand(IConfigService configService) : base("push", inputs)
        {
            this.configService = configService;
        }

        protected override void RunCommand(Dictionary<CommandInput, string> parsedInput, Dictionary<CommandArgument, string> parsedArguments)
        {
            string packageName = ParsePackageName(parsedInput);

            List<string> packagesToPush = !string.IsNullOrEmpty(packageName) ?
                new List<string> { packageName } :
                configService.GetAllPackageNames();

            foreach (var package in packagesToPush)
            {

            }
        }

        private string ParsePackageName(Dictionary<CommandInput, string> parsedInput)
        {
            CommandInput packageName = GetCommandInputByName("packageName");
            if (parsedInput.ContainsKey(packageName))
                return parsedInput[packageName];

            return string.Empty;
        }
    }
}
