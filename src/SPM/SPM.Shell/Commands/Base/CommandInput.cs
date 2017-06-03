using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPM.Shell.Commands.Base
{
    public class CommandInput
    {
        public string Name { get; set; }
        public int Index { get; set; }
        public bool Required { get; set; }
    }
}
