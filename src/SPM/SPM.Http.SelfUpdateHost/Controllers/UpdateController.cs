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

        public UpdateController(IOptions<Config> configOptions)
        {
            config = configOptions.Value;
        }

        // GET api/values
        [HttpGet("getLastVersion")]
        public string GetLastVersion()
        {
            return string.Empty;
        }

        // PUT api/values/5
        [HttpPut("{fileName}")]
        public async Task<IActionResult> PutAsync([FromBody]IFormFile file, [FromHeader(Name = "X-PackageVersion")]string version, [FromHeader(Name = "X-ServerDigest")]string digest)
        {
            if (digest != config.ServerDigest)
                return Unauthorized();

            if (string.IsNullOrEmpty(version))
                return BadRequest();

            using (var ms = new MemoryStream())
            {
                file.CopyTo(ms);

                using (var client = new HttpClient())
                {
                    HttpResponseMessage response = 
                        await client.PostAsync(config.FileServiceUrl,
                        new MultipartFormDataContent
                        {
                            { new ByteArrayContent(ms.ToArray()), "data", "package.wsp" },
                            { new StringContent($"spm_{version}"), "key" }
                        });
                    if (response.IsSuccessStatusCode)
                        return Ok();
                    else
                        return BadRequest(await response.Content.ReadAsStringAsync());
                }
            }
        }
    }
}
