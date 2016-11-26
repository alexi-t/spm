using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPM.Shell.Commands.Pull
{
    public interface IPullService
    {
        NewPackage Pull(string name);
    }
}
