using System.Collections.Generic;
using SPM.Shell.Config;

namespace SPM.Shell.Services
{
    public interface IConfigService
    {
        ConfigurationRoot GetConfig();
        
        void CreateConfig(List<CofigurationPackageDescription> initialPackages = null);

        bool IsConfigExist();

        List<string> GetAllPackageNames();

        void SetPackageTag(string packageName, string tag);
    }
}