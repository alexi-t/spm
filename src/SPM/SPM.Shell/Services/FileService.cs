using SPM.Shell.Services.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;


namespace SPM.Shell.Services
{
    public class FileService : IFileService
    {
        private readonly string localAppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create);
        private const string SPMFolderName = "SPM";
        private const string CacheFolderName = "Cache";

        public string[] SearchWorkingDirectory(string filter = null)
        {
            return Directory.GetFiles(".", filter ?? "*", SearchOption.TopDirectoryOnly);
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

        public Stream ReadFileAsStream(string fileName)
        {
            return File.OpenRead(fileName);
        }

        private string EnsureLocalDataPath(params string[] folders)
        {
            string currentPath = localAppDataFolder;
            foreach (string folderName in new[] { SPMFolderName }.Union(folders))
            {
                string folderPath = Path.Combine(currentPath, folderName);
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);
                currentPath = folderPath;
            }

            return currentPath;
        }

        public bool IsPackageExistInCache(string name, string tag)
        {
            string packagesCacheFolder = EnsureLocalDataPath("cache");

            string packageCacheFolderPath = Path.Combine(packagesCacheFolder, name);
            if (!Directory.Exists(packageCacheFolderPath))
                return false;

            string packageVersionFolderPath = Path.Combine(packagesCacheFolder, tag);
            if (!Directory.Exists(packageVersionFolderPath))
                return false;

            return true;
        }


        public void SavePackageInCache(string packageName, string packageTag, byte[] packagePayload)
        {
            string packageCachePath = EnsureLocalDataPath(CacheFolderName, packageName, packageTag);
            string packagePath = Path.Combine(packageCachePath, $"{packageName}.wsp");
            File.WriteAllBytes(packagePath, packagePayload);
        }

        public void ExtractPackageFromCache(string packageName, string packageTag)
        {
            foreach (var packageFile in Directory.GetFiles(EnsureLocalDataPath(CacheFolderName, packageName, packageTag)))
            {
                File.Copy(packageFile, Path.GetFileName(packageFile), true);
            }
        }
    }
}
