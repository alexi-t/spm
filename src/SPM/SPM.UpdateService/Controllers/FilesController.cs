using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SPM.UpdateService.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace SPM.UpdateService.Controllers
{
    [Route("api/[controller]")]
    public class FilesController : Controller
    {
        private readonly VersionsService versionService;

        private readonly IHostingEnvironment _environment;
        private readonly IHttpContextAccessor contextAccessor;

        public FilesController(IHostingEnvironment environment, VersionsService versionService, IHttpContextAccessor contextAccessor)
        {
            _environment = environment;
            this.versionService = versionService;
            this.contextAccessor = contextAccessor;
        }

        [HttpGet]
        [Route("getLastVersion")]
        public async Task<IActionResult> GetLastVersion()
        {
            return File(await versionService.GetLastVersionFileStream(), "application/zip");
        }

        [HttpPost]
        [Route("upload")]
        public void Post()
        {
            var context = contextAccessor.HttpContext;
            using (var sr = new BinaryReader(context.Request.Body))
            {
                var data = sr.ReadBytes((int)context.Request.ContentLength);

                var nextVersion = versionService.GetNextAvailableVersion();
                versionService.AddNewVersion(nextVersion, data);
            }
            
        }

        [HttpGet]
        [Route("cs")]
        public IActionResult PrintConnectionString()
        {
            return Content(versionService.GetConnectionString());
        }
    }
}
