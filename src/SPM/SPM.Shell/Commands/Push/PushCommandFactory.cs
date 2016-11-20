using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPM.Shell.Commands.Push
{
    public static class PushCommandFactory
    {
        public static PushCommandParser CreateInstance()
        {
            return new PushCommandParser(new PackageServiceClient(""));
        }
    }
}
