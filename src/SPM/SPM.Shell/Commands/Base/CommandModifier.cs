using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPM.Shell.Commands.Base
{
    public class CommandModifier
    {
        public CommandModifier(string name)
        {
            Name = name;
        }
        public string Name { get; set; }
    }
}
