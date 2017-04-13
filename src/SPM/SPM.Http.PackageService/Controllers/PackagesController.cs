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

        [HttpGet("getAll")]
        public async Task<IActionResult> GetAllTagsAsync([FromQuery]string name, [FromQuery]int count = 10)
        {
            if (string.IsNullOrEmpty(name))
                return BadRequest();

            var tags = await packageService.GetPackageTagsAsync(name, count);

            if (tags == null)
                return NotFound();

            return Ok(tags.Select(t => $"{name}@{t}"));
        }

        [HttpGet("get")]
        public async Task<IActionResult> GetAsync([FromQuery]string name)
        {
            if (string.IsNullOrEmpty(name))
                return BadRequest();

            var separatorIndex = name.IndexOf('@');
            var packageName = name.Substring(0, separatorIndex);
            if (string.IsNullOrEmpty(packageName))
                return BadRequest();

            string tag = string.Empty;
            if (separatorIndex > -1)
                tag = name.Substring(separatorIndex + 1);
            else
                tag = "lastest";

            var package = await packageService.GetPackageByNameAndTagAsync(packageName, tag);

            if (package == null)
                return NotFound();

            var packageInfo = new PackageInfo(package, fileService.GetDowloadLinkFormat());

            return Ok(packageInfo);
        }

        // POST api/values
        [HttpPost]
        public async Task<IActionResult> PostAsync([FromForm(Name = "name")]string nameAndTag, IFormFile packageFile)
        {
            int separatorIndex = nameAndTag.LastIndexOf('@');
            string name = nameAndTag.Substring(0, separatorIndex);
            string tag = nameAndTag.Substring(separatorIndex + 1);

            var package = await packageService.GetPackageByNameAndTagAsync(name, tag);
            if (package != null)
                return BadRequest("Package with same name and tag already exist!");

            if (packageFile == null)
                return BadRequest("No package file provided");

            byte[] packageData = new byte[packageFile.Length];
            using (var stream = packageFile.OpenReadStream())
            {
                await stream.ReadAsync(packageData, 0, (int)packageFile.Length);
            }

            var fileHash = await fileService.UploadFileAsync($"{name}@{tag}", packageData);

            if (!string.IsNullOrEmpty(fileHash))
            {
                return Ok(await packageService.AddPackageAsync(name, tag, fileHash));
            }
            return BadRequest("Internal error in file service.");
        }
    }
}
