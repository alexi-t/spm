using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SPM.Http.PackageService.Model;

namespace SPM.Http.PackageService.Controllers
{
    [Route("packages")]
    public class PackagesController : Controller
    {
        private readonly Service.PackageService packageService;
        private readonly Service.FileService fileService;

        public PackagesController(Service.PackageService packageService, Service.FileService fileService)
        {
            this.packageService = packageService;
            this.fileService = fileService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllPackagesAsync()
        {
            return Ok(await packageService.GetAllPackagesAsync());
        }
        
        [HttpGet("{name}/info")]
        public async Task<IActionResult> GetAsync([FromQuery]string name)
        {
            if (string.IsNullOrEmpty(name))
                return BadRequest();
            
            var package = await packageService.GetPackageByNameAndTagAsync(name);

            if (package == null)
                return NotFound();

            var packageInfo = new PackageInfo(package, fileService.GetDowloadLinkFormat());

            return Ok(packageInfo);
        }

        // POST api/values
        [HttpPost]
        public async Task<IActionResult> PostAsync([FromForm]string nameAndTag, [FromForm]string packageChanges, IFormFile versionFile)
        {
            int separatorIndex = nameAndTag.LastIndexOf('@');
            string name = nameAndTag.Substring(0, separatorIndex);
            string tag = nameAndTag.Substring(separatorIndex + 1);
            
            if (versionFile == null)
                return BadRequest("No package file provided");

            byte[] packageData = new byte[versionFile.Length];
            using (var stream = versionFile.OpenReadStream())
            {
                await stream.ReadAsync(packageData, 0, (int)versionFile.Length);
            }

            var zipHash = await fileService.UploadFileAsync($"{name}@{tag}", packageData);

            if (!string.IsNullOrEmpty(zipHash))
            {
                return Ok(await packageService.AddPackageVersionAsync(name, tag, packageChanges, zipHash));
            }
            return BadRequest("Internal error in file service.");
        }

        [HttpGet("{packageName}/tags")]
        public async Task<IActionResult> GetTagsAsync(string packageName, [FromQuery]string to, [FromQuery]string from)
        {
            if (string.IsNullOrEmpty(packageName))
                return BadRequest();

            List<PackageTag> tags = await packageService.GetPackageTagsAsync(packageName);

            if (!string.IsNullOrEmpty(to))
                tags = tags.SkipWhile(t => t.Tag != to).ToList();

            if (!string.IsNullOrEmpty(from))
                tags = tags.TakeWhile(t => t.Tag != from).ToList();

            return Json(tags.Select(t => t.Tag));
        }

        [HttpGet("{name}/download")]
        public async Task<IActionResult> GetDownloadLink(string name)
        {
            if (string.IsNullOrEmpty(name))
                return BadRequest();
            
            var package = await packageService.GetPackageByNameAndTagAsync(name);

            if (package == null)
                return NotFound();

            var packageInfo = new PackageInfo(package, fileService.GetDowloadLinkFormat());

            return Redirect(packageInfo.DownloadLink);
        }
    }
}
