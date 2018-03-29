using System.Collections.Generic;
using SPM.Shell.Config;

namespace SPM.Shell.Services
{
    public interface IConfigService
    {
        bool TryGetConfig(out PackageConfiguration packageConfiguration);
        
        void CreateConfig(string name, string versionHash);

        void SetTag(string tag);
    }
}