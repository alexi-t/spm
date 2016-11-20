using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPM.Shell.Commands.Pack
{
    public interface IVersionsService
    {
        Task<string> GetNextVersionAsync(string name);
    }
}
