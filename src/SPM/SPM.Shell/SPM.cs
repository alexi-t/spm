using CLAP;
using SPM.Shell.Commands.Init;
using SPM.Shell.Commands.Pack;
using SPM.Shell.Commands.Push;
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
            int result = -1;


            var parsers = new object[] {
                new InitCommandParser(),
                PackCommandFactory.CreateInstance(),
                PushCommandFactory.CreateInstance()
            };

            int index = 0;
            while (result != 0)
            {
                try
                {
                    result = Parser.Run(args, parsers[index++]);
                }
                catch (VerbNotFoundException)
                {
                    continue;
                }
            }

            return 0;
        }

        public class Commands
        {
            [Verb(Aliases = "update-self")]
            public static void UpdateSelf()
            {
                Console.WriteLine("Update");
            }

        }
    }
}
