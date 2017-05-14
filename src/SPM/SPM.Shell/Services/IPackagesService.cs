using System.IO;
using System.Threading.Tasks;
using SPM.Shell.Services.Model;

namespace SPM.Shell.Services
{
    public interface IPackagesService
    {
        Task<PackageInfo> SearchPackageAsync(string name);
        Task UploadPackageAsync(string name, Stream fileStream);

        HttpOperationWithProgress DownloadPackage(string name, string tag);
    }
}