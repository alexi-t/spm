using SPM.Shell.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SPM.Shell
{
    public class SPM
    {

        public static async Task<int> Main(string[] args)
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
                {
                    try
                    {
                        await commands[commandName].RunAsync(args.Skip(1).ToArray());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                else
                {
                    return 1;
                }
            }

            return 0;
        }
    }
}
