using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPM.Shell.Commands.Pack
{
    public static class PackCommandFactory
    {
        public static PackCommandParser CreateInstance()
        {
            return new PackCommandParser(new PackageServiceClient(string.Empty), new FileHashService());
        }
    }
}
