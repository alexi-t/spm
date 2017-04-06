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
        private static CommandInput packageNameInput = new CommandInput
        {
            Name = "packageName",
            Index = 0,
            Required = false
        };

        private static CommandInput[] inputs = new[] { packageNameInput };

        private readonly IConfigService configService;

        public PushCommand(IConfigService configService) : base("push", inputs)
        {
            this.configService = configService;
        }

        protected override void RunCommand()
        {
            string packageName = GetCommandInputValue(packageNameInput);

            List<string> packagesToPush = !string.IsNullOrEmpty(packageName) ?
                new List<string> { packageName } :
                configService.GetAllPackageNames();

            foreach (var package in packagesToPush)
            {

            }
        }
    }
}
