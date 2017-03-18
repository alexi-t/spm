using System.Collections.Generic;
using SPM.Shell.Config;

namespace SPM.Shell.Services
{
    public interface IConfigService
    {
        void CreateConfig(List<CofigurationPackageDescription> initialPackages = null);
        ConfigurationRoot GetConfig();
        bool IsConfigExist();
    }
}