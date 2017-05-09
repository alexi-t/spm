using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;

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
        public IActionResult Put(string version, IFormFile zip, [FromHeader(Name = "X-ServerDigest")]string digest)
        {
            if (digest != config.ServerDigest)
                return Unauthorized();

            if (zip == null || string.IsNullOrEmpty(version))
                return BadRequest();

            return Ok();
        }
    }
}
