using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SPM.PackageService.Storage;
using Microsoft.AspNetCore.Http;

namespace SPM.PackageService.Controllers
{
    [Route("api/[controller]")]
    public class PackageController : Controller
    {
        private readonly PackagesService packagesService;

        public PackageController(PackagesService service)
        {
            this.packagesService = service;
        }

        [Route("GetNextVersion")]
        public async Task<string> GetNextVersion([FromQuery]string name)
        {
            var lastVersion = await packagesService.GetLastPackageVersion(name);
            var version = new Version(lastVersion);
            return new Version(version.Major, version.Minor, version.Build + 1, 0).ToString();
        }

        [Route("Push")]
        public async Task Push([FromQuery] string packageName, [FromQuery] string version, IFormFile packageData)
        {
            var fileUrl = await packagesService.AddFile(packageName, version, packageData.OpenReadStream());
            await packagesService.AddVersion(packageName, version, fileUrl);
        }
    }
}
