using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SPM.Shell.Services.Model;

namespace SPM.Shell.Services
{
    public interface IOnlineStoreService
    {
        Task<PackageInfo> GetPackageVersionAsync(string name);
        HttpOperationWithProgress DownloadPackage(string name, string tag);
        Task PushPackageAsync(string packageNameAndTag, string tagHash, FolderVersionEntry folderVersion);
        Task<string[]> GetPackageTagsAsync(string packageNameTo, string packageNameFrom = null);
    }
}