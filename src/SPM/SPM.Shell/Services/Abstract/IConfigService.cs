using System.Collections.Generic;
using SPM.Shell.Config;

namespace SPM.Shell.Services
{
    public interface IConfigService
    {
        PackageConfiguration GetConfig();
        
        void CreateConfig(string name, string versionHash);

        void SetTag(string tag, string hash);
        List<string> GetCurrentFilesList();
    }
}