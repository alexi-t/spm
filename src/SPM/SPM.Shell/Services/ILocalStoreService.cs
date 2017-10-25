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
        bool PackageExist(string name, string tag);

        void SavePackage(string name, string tag, byte[] data);

        void RestorePackage(string name, string tag);
    }
}
