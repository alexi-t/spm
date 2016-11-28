using System.IO;
using System.Threading.Tasks;
using SPM.Shell.Config;

namespace SPM.Shell.Commands.Push
{
    public interface IPackageService
    {
        Task<bool> GetCanPushPackageAsync(string packageName, CofigurationPackageDescription packageConfig);
        Task Push(string wspFileName, CofigurationPackageDescription packageConfig, FileStream packageFile);
    }
}