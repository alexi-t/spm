using CLAP;
using SPM.Shell.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPM.Shell.Commands.Init
{
    public class InitCommandParser
    {
        [Verb()]
        public void Init()
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

                var desc = new CofigurationPackageDescription
                {
                    Name = name
                };

                configList.Add(desc);
            }

            ConfigManager.CreateConfigFile(configList);
        }
    }
}
