using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SPM.Shell.Services
{
    public interface IFileService
    {
        string ReadFileAsText(string path);
        byte[] ReadFileAsByteArray(string path);
        Stream ReadFileAsStream(string path);

        void WriteText(string path, string content);
        void WriteByteData(string path, byte[] content);
        void WriteStream(string path, Stream content);

        bool IsFileExist(string path);

        List<string> ListFilesInDirectory(string directory);
        Task<byte[]> ZipFiles(IEnumerable<string> packageFiles);
        void ClearWorkingDirectory();
        void Unzip(string packageZipPath);
        IEnumerable<string> GetDefaultIgnore();

        List<string> GetWorkingDirectoryFiles(IEnumerable<string> ignoreList);
    }
}