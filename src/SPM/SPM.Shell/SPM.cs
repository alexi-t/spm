using CLAP;
using SPM.Shell.Commands.Init;
using SPM.Shell.Commands.Pack;
using SPM.Shell.Commands.Push;
using SPM.Shell.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPM.Shell
{
    public static class SPM
    {

        public static int Main(params string[] args)
        {
            var commandsFactory = new CommandsFactory();
            var commands = commandsFactory.GetCommandBindings();

            if (args.Length > 0)
            {
                var commandName = args[0];
                if (commands.ContainsKey(commandName))
                    commands[commandName].Run(args.Skip(1).ToArray());
            }
            
            return 0;
        }
    }
}
