using SPM.Shell.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPM.Shell.Commands.Pull
{
    public class PullCommand : BaseCommand
    {
        private static CommandInput packageNameInput = new CommandInput
        {
            Index = 0,
            Name = "packageName",
            Required = true
        };

        private readonly IPackagesService packagesService;

        public PullCommand(IPackagesService packagesService) : base("pull", inputs: new[] { packageNameInput })
        {
            this.packagesService = packagesService;
        }

        protected override void RunCommandAsync()
        {
            string packageName = GetCommandInputValue(packageNameInput);
        }
    }
}
