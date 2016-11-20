using CLAP;
using SPM.Shell.Commands.Pack;
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
            CLAP.Parser.Run<Commands>(args);

            var packParser = PackCommandFactory.CreateInstance();

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
