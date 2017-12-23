using SPM.Shell.Services.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPM.Shell.Services
{
    public interface ILocalStoreService
    {
        bool PackageExist(PackageInfo packageInfo);

        void SavePackage(PackageInfo packageInfo, byte[] data);

        void RestorePackage(string name, string tag);
    }
}
