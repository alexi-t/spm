using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SPM.Shell.Services
{
    public class HashService : IHashService
    {
        private readonly IFileService fileService;
        private readonly SHA1 hashFunction = SHA1.Create();

        public HashService(IFileService fileService)
        {
            this.fileService = fileService;
        }

        public string ComputeFilesHash(List<string> paths)
        {
            var hashesData = new byte[20 * paths.Count];
            using (var ms = new MemoryStream(hashesData))
            {
                foreach (var filePath in paths)
                {
                    byte[] hash = ComputeFileHash(filePath);
                    ms.Write(hash, 0, 20);
                }
                return SerializeHash(hashFunction.ComputeHash(ms.ToArray())); ;
            }
        }

        private string SerializeHash(byte[] hash) => string.Join("", hash.Select(b => b.ToString("x2")));

        private byte[] ComputeFileHash(string filePath)
        {
            using (var fileStream = fileService.ReadFileAsStream(filePath))
            {
                return hashFunction.ComputeHash(fileStream);
            }
        }
    }
}
