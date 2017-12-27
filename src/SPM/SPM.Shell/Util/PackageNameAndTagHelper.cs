using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPM.Shell.Util
{
    public static class PackageNameAndTagHelper
    {
        public static string GetPackageName(string packageNameAndTag) => packageNameAndTag.Split('@').First();

        public static string GetPackageTag(string packageNameAndTag)
        {
            int delimiterPosition = packageNameAndTag.IndexOf('@');
            return packageNameAndTag.Substring(delimiterPosition + 1);
        }
    }
}
