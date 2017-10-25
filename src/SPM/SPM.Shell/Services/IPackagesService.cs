using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SPM.Shell.Services.Model;

namespace SPM.Shell.Services
{
    public interface IPackagesService
    {
        Task<PackageInfo> SearchPackageAsync(string name);
        Task PushPackageAsync(string name, byte[] packageData);

        HttpOperationWithProgress DownloadPackage(string name, string tag);
    }
}