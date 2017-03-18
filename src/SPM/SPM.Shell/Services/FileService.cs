using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SPM.Shell.Services
{
    public class FileService : IFileService
    {
        public string[] SearchWorkingDirectory(string filter)
        {
            return Directory.GetFiles(".", filter, SearchOption.TopDirectoryOnly);
        }

        public string ReadFile(string path)
        {
            return File.ReadAllText(path);
        }

        public void WriteFile(string path, string content)
        {
            File.WriteAllText(path, content);
        }

        public bool IsFileExist(string path)
        {
            return File.Exists(path);
        }
    }
}
