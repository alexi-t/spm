using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace SPM.Http.FileService.Controllers
{
    [Route("[controller]")]
    public class FilesController : Controller
    {
        [HttpPost]
        public IActionResult Post([FromForm]string key, IFormFile data)
        {
            var hash = SHA256.Create();
            var keyHash = hash.ComputeHash(Encoding.UTF8.GetBytes(key));
            var dataStream = data.OpenReadStream();

            byte[] dataBytes = new byte[data.Length];
            dataStream.Read(dataBytes, 0, (int)data.Length);

            var dataHash = hash.ComputeHash(dataBytes);
            
            System.IO.File.WriteAllBytes(key, dataBytes);

            return Ok(string.Join("", GetBytesHash(hash, dataBytes)));
        }

        [HttpGet("{key}/{fileHash}")]
        public IActionResult Get(string key, string fileHash)
        {
            if (System.IO.File.Exists(key))
            {
                var hashAlgorithm = SHA256.Create();
                var fileData = System.IO.File.ReadAllBytes(key);
                var fileDataHash = GetBytesHash(hashAlgorithm, fileData);

                if (fileDataHash == fileHash)
                    return File(fileData, "application/octet-stream");

                return NotFound();
            }
            return NotFound();
        }

        private string GetBytesHash(HashAlgorithm algorithm, byte[] buffer)
        {
            return string.Join("", algorithm.ComputeHash(buffer).Select(b => b.ToString("X2")));
        }
    }
}
