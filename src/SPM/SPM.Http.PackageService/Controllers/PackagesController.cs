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

        // GET api/values
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

        // GET api/values/5
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
                tag = name.Substring(separatorIndex);
            else
                tag = "lastest";

            var package = await packageService.GetPackageByNameAndTagAsync(packageName, tag);

            if (package == null)
                return NotFound();

            return Ok(package);
        }

        // POST api/values
        [HttpPost]
        public async Task<IActionResult> PostAsync(string name, string tag, IFormFile packageFile)
        {
            var package = await packageService.GetPackageByNameAndTagAsync(name, tag);
            if (package != null)
                return BadRequest();

            byte[] packageData = new byte[packageFile.Length];
            using (var stream = packageFile.OpenReadStream())
            {
                await stream.ReadAsync(packageData, 0, (int)packageFile.Length);
            }

            var uploadSuccess = await fileService.UploadFileAsync($"{name}@{tag}", packageData);

            if (uploadSuccess)
            {
                return Ok(await packageService.AddPackageAsync(name, tag));
            }
            return BadRequest();
        }
    }
}
