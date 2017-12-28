using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SPM.Http.PackageService.Model
{
    public class PackageInfo
    {
        public PackageInfo(Package package, string downloadLinkFormat)
        {
            Name = package.Name;
            Tag = package.Tag;
            Hash = package.Hash;
            VersionInfo = package.VersionInfo;

            DownloadLink = string.Format(downloadLinkFormat, Name + "@" + Tag, package.Hash);
        }

        public string Name { get; set; }
        public string Tag { get; set; }
        public string Hash { get; set; }
        public string VersionInfo { get; set; }
        public string DownloadLink { get; set; }
    }
}
