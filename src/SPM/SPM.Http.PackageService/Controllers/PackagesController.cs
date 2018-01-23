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
        public async Task<IActionResult> PostAsync([FromForm]string nameAndTag, [FromForm]string tagHash, [FromForm]string versionInfo, IFormFile versionFile)
        {
            int separatorIndex = nameAndTag.LastIndexOf('@');
            string name = nameAndTag.Substring(0, separatorIndex);
            string tag = nameAndTag.Substring(separatorIndex + 1);

            var package = await packageService.GetPackageByNameAndTagAsync(name, tag);

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
                if (package == null)
                    return Ok(await packageService.AddPackageAsync(name, tag, tagHash, versionInfo, zipHash));
                else
                {
                    package.Hash = zipHash;
                    await packageService.UpdatePackageAsync(package);
                    return Ok(package);
                }
            }
            return BadRequest("Internal error in file service.");
        }

        [HttpGet("{packageName}/tags")]
        public async Task<IActionResult> GetTagsAsync(string packageName, [FromQuery]string to, [FromQuery]string from)
        {
            if (string.IsNullOrEmpty(to))
                return BadRequest();

            List<PackageTag> tags = new List<PackageTag>();

            while (true)
            {
                IEnumerable<PackageTag> intermediateResult = await packageService.GetPackageTagsAsync(packageName, 3, tags.LastOrDefault()?.Timestamp);

                if (!intermediateResult.Any())
                    break;

                if (!string.IsNullOrEmpty(from))
                {
                    tags.AddRange(intermediateResult.TakeWhile(t => t.Tag != from));
                    if (intermediateResult.Any(t => t.Tag == from))
                        break;
                }
                else
                    tags.AddRange(intermediateResult);
            }

            return Json(tags.SkipWhile(t => t.Tag != to).Select(t => t.Tag));
        }

        [HttpGet("download")]
        public async Task<IActionResult> GetDownloadLink(string name)
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

            return Redirect(packageInfo.DownloadLink);
        }
    }
}
