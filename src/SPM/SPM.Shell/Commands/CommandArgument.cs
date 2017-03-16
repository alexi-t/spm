using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPM.Shell.Commands
{
    public class CommandArgument
    {
        public string Name { get; set; }
        public string Alias { get; set; }
        public bool HasValue { get; set; }
    }
}
