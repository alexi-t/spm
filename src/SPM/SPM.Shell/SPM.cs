using SPM.Shell.Services;
using System;
using System.Linq;

namespace SPM.Shell
{
    public static class SPM
    {

        public static int Main(params string[] args)
        {
#if DEBUG
            args = Console.ReadLine().Split(' ');
#endif

            var commandsFactory = new CommandsFactory();
            var commands = commandsFactory.GetCommandBindings();

            if (args.Length > 0)
            {
                var commandName = args[0];
                if (commands.ContainsKey(commandName))
                    commands[commandName].RunAsync(args.Skip(1).ToArray()).Wait();
            }

            return 0;
        }
    }
}
