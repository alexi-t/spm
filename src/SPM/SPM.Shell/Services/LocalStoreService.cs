using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SPM.Shell.Services.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPM.Shell.Services
{
    public class LocalStoreService : ILocalStoreService
    {
        private const string dbFile = "packages.json";

        private readonly string localAppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create);

        private readonly IConfigService configService;
        private readonly IFileService fileService;

        private readonly string localCacheFolder;
        private readonly string dbFilePath;

        public LocalStoreService(IConfigService configService, IFileService fileService)
        {
            this.configService = configService;
            this.fileService = fileService;

            localCacheFolder = Path.Combine(localAppDataFolder, "SPM");
            if (!Directory.Exists(localCacheFolder))
                Directory.CreateDirectory(localCacheFolder);

            dbFilePath = Path.Combine(localCacheFolder, dbFile);
            if (!File.Exists(dbFilePath))
                File.WriteAllText(dbFilePath, "[]");
        }
        
        private List<CachedPackageInfo> GetAllCachedPackages()
            => JArray.Parse(File.ReadAllText(Path.Combine(localCacheFolder, dbFile))).ToObject<List<CachedPackageInfo>>();
        private void AddCachedPackage(string name, string tag, string path)
        {
            var info = new CachedPackageInfo
            {
                Name = name,
                Tag = tag,
                Path = path
            };

            List<CachedPackageInfo> allPackages = GetAllCachedPackages();
            allPackages.Add(info);
            File.WriteAllText(dbFilePath, JsonConvert.SerializeObject(allPackages));
        }

        public bool PackageExist(string name, string tag)
        {
            return GetAllCachedPackages().Any(p => p.Name == name && p.Tag == tag);
        }

        public void RestorePackage(string name, string tag)
        {
            CachedPackageInfo cachedPackage = GetAllCachedPackages().FirstOrDefault(p => p.Name == name && p.Tag == tag);

            if (cachedPackage == null)
                return;

            fileService.ClearWorkingDirectory();
            
            fileService.Unzip(cachedPackage.Path);
        }

        public void SavePackage(string name, string tag, byte[] data)
        {
            if (PackageExist(name, tag))
                return;

            string packageDirectoryPath = Path.Combine(localCacheFolder, name);
            string tagDirectoryPath = Path.Combine(packageDirectoryPath, tag);
            if (!Directory.Exists(packageDirectoryPath))
                Directory.CreateDirectory(packageDirectoryPath);
            if (!Directory.Exists(tagDirectoryPath))
                Directory.CreateDirectory(tagDirectoryPath);

            string packageZipPath = Path.Combine(tagDirectoryPath, "package.zip");

            File.WriteAllBytes(packageZipPath, data);

            AddCachedPackage(name, tag, packageZipPath);
        }


    }
}
