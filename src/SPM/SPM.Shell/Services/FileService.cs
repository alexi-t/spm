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

        public Stream ReadFileAsStream(string fileName)
        {
            return File.OpenRead(fileName);
        }

        private string EnsureLocalDataPath(params string[] folders)
        {
            string currentPath = localAppDataFolder;
            foreach (string folderName in folders)
            {
                string folderPath = Path.Combine(currentPath, folderName);
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);
            }

            return currentPath;
        }

        public async Task DownloadPackage(string name, string tag, string downloadLink)
        {
            using (var httpClient = new HttpClient())
            {
                string downloadFolderPath = EnsureLocalDataPath(name, tag);
                string packageDownloadPath = Path.Combine(downloadFolderPath, "package.wsp");
                byte[] buffer = new byte[4096];
                using (Stream downloadStream = await httpClient.GetStreamAsync(downloadLink))
                using (FileStream fileStream = File.Create(packageDownloadPath))
                {
                    while(downloadStream.Position != downloadStream.Length)
                    {
                        int bytesToRead = (int)Math.Min(buffer.Length, downloadStream.Length - downloadStream.Position);
                        await downloadStream.ReadAsync(buffer, 0, bytesToRead);
                        await fileStream.WriteAsync(buffer, 0, bytesToRead);
                    }
                }
                File.Copy(packageDownloadPath, "package.wsp");
            }
        }
    }
}
