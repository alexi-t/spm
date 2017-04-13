using System.IO;
using System.Threading.Tasks;

namespace SPM.Shell.Services
{
    public interface IFileService
    {
        bool IsFileExist(string path);
        string ReadFile(string path);
        string[] SearchWorkingDirectory(string filter);
        void WriteFile(string path, string content);
        Stream ReadFileAsStream(string packageName);
        Task DownloadPackage(string name, string tag, string downloadLink);
    }
}