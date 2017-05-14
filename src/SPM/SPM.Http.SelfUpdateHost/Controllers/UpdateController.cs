using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Net.Http;

namespace SPM.Http.SelfUpdateHost.Controllers
{
    [Route("api/[controller]")]
    public class UpdateController : Controller
    {
        private readonly Config config;
        private readonly StoreService storeService;

        public UpdateController(IOptions<Config> configOptions, StoreService storeService)
        {
            config = configOptions.Value;
            this.storeService = storeService;
        }

        // GET api/values
        [HttpGet("getLastVersion")]
        public async Task<IActionResult> GetLastVersion()
        {
            VersionRow appVersion = await storeService.GetLastVersion();

            if (appVersion != null)
                return Ok(appVersion);

            return NoContent();
        }

        [HttpGet("download")]
        public async Task<IActionResult> DownloadLastest()
        {
            VersionRow appVersion = await storeService.GetLastVersion();

            if (appVersion != null)
                return Redirect($"{config.FileServiceUrl}/{GetVersionFileKey(appVersion.Version)}/{appVersion.Hash}");

            return NoContent();
        }

        // PUT api/values/5
        [HttpPut("{fileName}")]
        public async Task<IActionResult> PutAsync([FromBody]IFormFile file, [FromHeader(Name = "X-PackageVersion")]string version, [FromHeader(Name = "X-ServerDigest")]string digest)
        {
            if (digest != config.ServerDigest)
                return Unauthorized();

            if (string.IsNullOrEmpty(version))
                return BadRequest();

            byte[] buffer = new byte[file.Length];

            Stream fileStream = file.OpenReadStream();

            await fileStream.ReadAsync(buffer, 0, buffer.Length);
            
            using (var client = new HttpClient())
            {
                HttpResponseMessage response =
                    await client.PostAsync(config.FileServiceUrl,
                    new MultipartFormDataContent
                    {
                            { new ByteArrayContent(buffer), "data", file.FileName },
                            { new StringContent(GetVersionFileKey(version)), "key" }
                    });
                if (response.IsSuccessStatusCode)
                {
                    string hash = await response.Content.ReadAsStringAsync();
                    VersionRow appVersion = await storeService.AddVersionAsync("spm", version, hash);
                    return Ok(appVersion);
                }
                else
                    return BadRequest(await response.Content.ReadAsStringAsync());
            }
        }

        private string GetVersionFileKey(string version) => $"spm_{version}";
    }
}
