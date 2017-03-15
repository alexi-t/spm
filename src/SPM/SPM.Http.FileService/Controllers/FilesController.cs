using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using Microsoft.WindowsAzure.Storage;

namespace SPM.Http.FileService.Controllers
{
    [Route("[controller]")]
    public class FilesController : Controller
    {
        private readonly Services.CloudBlobService cloudStorage;

        public FilesController(Services.CloudBlobService cloudStorage)
        {
            this.cloudStorage = cloudStorage;
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync([FromForm]string key, IFormFile data)
        {
            if (string.IsNullOrEmpty(key))
                return BadRequest("Key is empty");

            if (data == null)
                return BadRequest("Data is empty");

            var hash = SHA256.Create();
            var keyHash = hash.ComputeHash(Encoding.UTF8.GetBytes(key));
            var dataStream = data.OpenReadStream();

            byte[] dataBytes = new byte[data.Length];
            dataStream.Read(dataBytes, 0, (int)data.Length);

            var dataHash = hash.ComputeHash(dataBytes);

            await cloudStorage.SaveFileAsync(key, dataBytes);

            return Ok(string.Join("", GetBytesHash(hash, dataBytes)));
        }

        [HttpGet("{key}/{fileHash}")]
        public async Task<IActionResult> GetAsync(string key, string fileHash)
        {
            if (string.IsNullOrEmpty(key))
                return BadRequest("Key is empty");

            if (string.IsNullOrEmpty(fileHash))
                return BadRequest("Hash is empty");

            byte[] fileData = null;

            try
            {
                fileData = await cloudStorage.GetFileAsync(key);
            }
            catch (StorageException)
            {
                return NotFound();
            }

            var hashAlgorithm = SHA256.Create();
            var fileDataHash = GetBytesHash(hashAlgorithm, fileData);

            if (fileDataHash == fileHash)
                return File(fileData, "application/octet-stream");

            return NotFound();
        }

        private string GetBytesHash(HashAlgorithm algorithm, byte[] buffer)
        {
            return string.Join("", algorithm.ComputeHash(buffer).Select(b => b.ToString("X2")));
        }
    }
}
