using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPM.Shell.Services
{
    public interface IHashService
    {
        string ComputeFilesHash(IEnumerable<string> paths);
    }
}
