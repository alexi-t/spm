using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPM.Bootstrapper
{
    static class Program
    {
        const string spmAppFolder = "SPM";
        const string spmExecutableName = "SPM.Shell";

        [LoaderOptimization(LoaderOptimization.MultiDomainHost)]
        [STAThread]
        static void Main(params string[] args)
        {
            var localAppFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            var appDirectory = Directory.CreateDirectory(Path.Combine(localAppFolder, "SPM"));

            var cachePath = Path.Combine(appDirectory.FullName, "assemblyCache");
            var config = Path.Combine(appDirectory.FullName, "App", $"{spmExecutableName}.exe.config");
            var executable = Path.Combine(appDirectory.FullName, "App", $"{spmExecutableName}.exe");

            AppDomainSetup setup = new AppDomainSetup
            {
                ApplicationName = "SPM",
                ShadowCopyFiles = "true",
                CachePath = cachePath,
                ConfigurationFile = config
            };

            AppDomain domain = AppDomain.CreateDomain(
                "SPM",
                AppDomain.CurrentDomain.Evidence,
                setup);

            domain.ExecuteAssembly(executable, args);

            AppDomain.Unload(domain);
            Directory.Delete(cachePath, true);
        }
    }
}
