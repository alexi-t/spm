using SPM.Shell.Services.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace SPM.Shell.Services
{
    public class FileService : IFileService
    {
        private readonly string localAppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create);
        private const string SPMFolderName = "SPM";
        private const string CacheFolderName = "Cache";

        private readonly IUIService uiService;

        public FileService(IUIService uiService)
        {
            this.uiService = uiService;
        }

        public string[] SearchWorkingDirectory(string filter = null)
        {
            return Directory.GetFiles(".", filter ?? "*", SearchOption.TopDirectoryOnly).Where(f => Path.GetFileName(f) != "packages.json").ToArray();
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

        public string ComputeHash(string[] excludes)
        {
            string[] allFiles = SearchWorkingDirectory().OrderBy(f => f).ToArray();

            SHA1 sha1 = SHA1.Create();

            DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            byte[] aggreagate = new byte[0];

            foreach (string filePath in allFiles)
            {
                DateTime lastWriteTime = File.GetLastWriteTimeUtc(filePath);
                double milliseconds = (lastWriteTime - unixEpoch).TotalMilliseconds;
                byte[] dateBytes = BitConverter.GetBytes(milliseconds);
                aggreagate = sha1.ComputeHash(aggreagate.Union(dateBytes).ToArray());
            }

            return string.Join("", aggreagate.Select(b => b.ToString("x2")));
        }

        public async Task<byte[]> CreatePackageAsync(string[] excludePaths)
        {
            string[] allFiles = SearchWorkingDirectory().OrderBy(f => f).ToArray();

            using (var ms = new MemoryStream())
            {
                ZipArchive archive = new ZipArchive(ms, ZipArchiveMode.Create);

                string workingDirectory = Environment.CurrentDirectory;

                float index = 0;
                int totalFilesCount = allFiles.Count();

                uiService.AddMessage("Creating package...");

                foreach (string filePath in allFiles)
                {
                    string fileRelativePath = filePath.Replace(workingDirectory, "");

                    ZipArchiveEntry fileEntry = archive.CreateEntry(fileRelativePath, CompressionLevel.Optimal);

                    using (FileStream fileStream = File.OpenRead(filePath))
                    using (Stream entryStream = fileEntry.Open())
                    {
                        await fileStream.CopyToAsync(entryStream);
                    }

                    uiService.DisplayProgress(++index * 100 / totalFilesCount);
                }

                await ms.FlushAsync();

                return ms.ToArray();
            }
        }
    }
}
