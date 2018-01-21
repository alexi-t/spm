using SPM.Shell.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPM.Shell.Services
{
    public interface IVersioningService
    {
        Task<FolderVersionEntry> CreateInitialVersion(IEnumerable<string> filesList);
        FolderVersionEntry CreateDiff(IEnumerable<string> currentFilesList);
    }
}
