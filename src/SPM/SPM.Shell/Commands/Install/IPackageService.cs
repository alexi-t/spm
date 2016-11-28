using System.Threading.Tasks;

namespace SPM.Shell.Commands.Install
{
    public interface IPackageService
    {
        Task<PackageDescription> GetPackageAsync(string packageName);
    }
}