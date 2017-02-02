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

            var fileName = string.Join("", keyHash.Select(b => b.ToString("x2")));

            System.IO.File.WriteAllBytes(fileName, dataBytes);

            return Ok(string.Join("", dataHash.Select(b => b.ToString("X2"))));
        }

        [HttpGet("{key}")]
        public IActionResult Get(string key)
        {
            var hash = SHA256.Create();
            var keyHash = hash.ComputeHash(Encoding.UTF8.GetBytes(key));
            var fileName = string.Join("", keyHash.Select(b => b.ToString("x2")));

            if (System.IO.File.Exists(fileName))
                return File(System.IO.File.ReadAllBytes(fileName), "application/octet-stream");

            return NotFound();
        }
    }
}
