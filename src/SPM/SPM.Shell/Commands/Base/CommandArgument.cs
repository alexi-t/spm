using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPM.Shell.Commands.Base
{
    public class CommandArgument
    {
        public CommandArgument(string name, string alias = null)
        {
            Name = name;
            Alias = alias;
        }

        public string Name { get; set; }
        public string Alias { get; set; }
    }
}
