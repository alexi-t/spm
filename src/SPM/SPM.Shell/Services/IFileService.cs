using System.IO;
using System.Threading.Tasks;

namespace SPM.Shell.Services
{
    public interface IFileService
    {
        bool IsFileExist(string path);
        string ReadFile(string path);
        void WriteFile(string path, string content);
        string[] SearchWorkingDirectory(string filter = null);
        Stream ReadFileAsStream(string packageName);
        bool IsPackageExistInCache(string packageName, string packageTag);
        void SavePackageInCache(string packageName, string packageTag, byte[] packagePayload);
        void ExtractPackageFromCache(string packageName, string packageTag);
        string ComputeHash(string[] excludePaths);
        Task<byte[]> CreatePackageAsync(string[] excludePaths);
    }
}