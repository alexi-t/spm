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
using System.Net.Http.Headers;
using IOFIle = System.IO.File;
using Microsoft.ApplicationInsights;

namespace SPM.Http.FileService.Controllers
{
    [Route("[controller]")]
    public class FilesController : Controller
    {
        private readonly Services.CloudBlobService cloudStorage;
        private readonly TelemetryClient telemetry = new TelemetryClient();

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

            SHA256 hashFunction = SHA256.Create();
            
            string tempFileName = Path.GetTempFileName();
            int bufferSize = 1024 * 1024;
            
            try
            {
                using (var dataStream = data.OpenReadStream())
                using (var fs = IOFIle.OpenWrite(tempFileName))
                {
                    while (true)
                    {
                        byte[] buffer = new byte[bufferSize];
                        int bytesRead = await dataStream.ReadAsync(buffer, 0, buffer.Length);
                        if (bytesRead <= 0)
                            break;

                        await fs.WriteAsync(buffer, 0, bytesRead);
                    }
                }
                using (var fs = IOFIle.OpenRead(tempFileName))
                {
                    byte[] fileHash = hashFunction.ComputeHash(fs);

                    fs.Position = 0;

                    await cloudStorage.SaveFileAsync(key, fs);

                    return Ok(string.Join("", fileHash.Select(b => b.ToString("X2"))));
                }
            }
            finally
            {
                if (IOFIle.Exists(tempFileName))
                    IOFIle.Delete(tempFileName);
            }
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
                telemetry.TrackEvent($"File {key} not found in storage");
                return NotFound();
            }

            var hashAlgorithm = SHA256.Create();
            var fileDataHash = GetBytesHashAsString(hashAlgorithm, fileData);

            if (fileDataHash == fileHash)
            {
                Response.Headers.Append("Content-Length", new Microsoft.Extensions.Primitives.StringValues(fileData.Length.ToString()));
                return File(fileData, "application/octet-stream");
            }

            telemetry.TrackEvent($"File {key} hash from storage {fileDataHash} does not match {fileHash}");

            return NotFound();
        }

        private string GetBytesHashAsString(HashAlgorithm algorithm, byte[] buffer)
        {
            return string.Join("", algorithm.ComputeHash(buffer).Select(b => b.ToString("X2")));
        }
    }
}
