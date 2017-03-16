using SPM.Shell.Config;
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
        public InitCommand() : base("init")
        {

        }

        protected override CommandArgument[] GetSupportedArgs()
        {
            return new[]
            {
                new CommandArgument
                {
                    Name="overwrite",
                    Alias="o"
                }
            };
        }

        protected override void RunCommand(Dictionary<CommandInput, string> parsedInput, Dictionary<CommandArgument, string> parsedArguments)
        {
            var wspFiles = Directory.GetFiles(".", "*.wsp");

            var configList = new List<CofigurationPackageDescription>();
            foreach (var file in wspFiles)
            {
                var fileName = Path.GetFileName(file);
                Console.WriteLine($"Enter name for wsp file {fileName} (default ${Path.GetFileNameWithoutExtension(file)})");
                var name = Console.ReadLine();

                if (string.IsNullOrEmpty(name))
                    name = Path.GetFileNameWithoutExtension(file);

                var packageConfig = new CofigurationPackageDescription
                {
                    Name = name,
                    FileName = Path.GetFileName(file)
                };

                configList.Add(packageConfig);
            }

            ConfigManager.CreateConfigFile(configList);
        }
    }
}
