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

        public string ComputeHashUnion(IEnumerable<string> hashes)
        {
            var hashesData = new byte[20 * hashes.Count()];
            using (var ms = new MemoryStream(hashesData))
            {
                foreach (var hashString in hashes)
                {
                    byte[] hash = new byte[20];
                    for (int i = 0; i < hashString.Length; i++)
                    {
                        if (i % 2 == 0)
                        {
                            string hex = hashString.Substring(i, 2);
                            hash[i / 2] = byte.Parse(hex, System.Globalization.NumberStyles.HexNumber);
                        }
                    }
                    ms.Write(hash, 0, 20);
                }
                return SerializeHash(hashFunction.ComputeHash(ms.ToArray())); ;
            }
        }

        public string ComputeFilesHash(IEnumerable<string> paths)
        {
            var hashesData = new byte[20 * paths.Count()];
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
        string IHashService.ComputeFileHash(string path) => ComputeFilesHash(new[] { path });

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
