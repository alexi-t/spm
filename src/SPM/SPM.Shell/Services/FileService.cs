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

        public async Task<byte[]> ZipFiles(List<string> packageFiles)
        {
            using (var ms = new MemoryStream())
            {
                ZipArchive archive = new ZipArchive(ms, ZipArchiveMode.Create);

                string workingDirectory = Environment.CurrentDirectory;

                float index = 0;
                int totalFilesCount = packageFiles.Count();

                uiService.AddMessage("Creating package...");

                foreach (string filePath in packageFiles)
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
    }
}
