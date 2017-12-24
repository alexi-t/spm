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
        private readonly IUIService uiService;

        public FileService(IUIService uiService)
        {
            this.uiService = uiService;
        }

        public bool IsFileExist(string path) => File.Exists(path);

        public string ReadFile(string path) => File.ReadAllText(path);
        public Stream ReadFileAsStream(string fileName) => File.OpenRead(fileName);
        public string ReadFileAsText(string path) => File.ReadAllText(path);
        public byte[] ReadFileAsByteArray(string path) => File.ReadAllBytes(path);

        public void WriteFile(string path, string content) => File.WriteAllText(path, content);
        public void WriteText(string path, string content) => File.WriteAllText(path, content);
        public void WriteByteData(string path, byte[] content) => File.WriteAllBytes(path, content);

        public void WriteStream(string path, Stream content)
        {
            const int bufferSize = 1024 * 1024;
            using (var fs = File.OpenWrite(path))
            {
                byte[] buffer = new byte[bufferSize];
                while (true)
                {
                    int bytesRead = content.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                        break;
                    fs.Write(buffer, 0, bytesRead);
                }
            }
        }

        public List<string> ListFilesInDirectory(string directory)
        {
            return Directory.GetFiles(directory).ToList();
        }

        public async Task<byte[]> ZipFiles(IEnumerable<string> packageFiles)
        {
            string tempDirectoryName = "~temp_archive_spm" + Guid.NewGuid();
            string tempArchiveName = $"temp_archive{Guid.NewGuid()}.zip";

            float index = 0;
            int totalFilesCount = packageFiles.Count();

            uiService.AddMessage("Creating package...");

            var tempDirectory = Directory.CreateDirectory(tempDirectoryName);

            foreach (string filePath in packageFiles)
            {
                File.Copy(filePath, Path.Combine(tempDirectory.FullName, Path.GetFileName(filePath)));

                uiService.DisplayProgress(++index * 100 / totalFilesCount);
            }

            ZipFile.CreateFromDirectory(tempDirectory.FullName, tempArchiveName);

            byte[] data = File.ReadAllBytes(tempArchiveName);

            File.Delete(tempArchiveName);
            Directory.Delete(tempDirectory.FullName, true);

            return data;
        }

        public void ClearWorkingDirectory()
        {
            List<string> filesList = ListFilesInDirectory(".");
            foreach (var file in filesList)
            {
                string fileName = Path.GetFileName(file);
                if (fileName == "packages.json" ||
                    fileName == "package.json" ||
                    fileName == "")
                    continue;

                File.Delete(file);
            }
        }

        public void Unzip(string packageZipPath)
        {
            using (var fs = File.OpenRead(packageZipPath))
            {
                ZipArchive archive = new ZipArchive(fs, ZipArchiveMode.Read);
                archive.ExtractToDirectory(".");
            }
        }

        private string[] defaultIgnore = new[] { ".spm", "spm.json" };
        public IEnumerable<string> GetDefaultIgnore() => defaultIgnore;
    }
}
