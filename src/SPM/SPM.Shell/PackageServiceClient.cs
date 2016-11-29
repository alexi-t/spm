using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using SPM.Shell.Commands.Install;
using SPM.Shell.Config;
using Newtonsoft.Json;

namespace SPM.Shell
{
    public class PackageServiceClient : Commands.Pack.IVersionsService, Commands.Push.IPackageService, Commands.Install.IPackageService
    {
        private readonly string serviceUrl;

        public PackageServiceClient(string serviceUrl)
        {
            this.serviceUrl = serviceUrl;
        }

        public async Task<bool> GetCanPushPackageAsync(string packageName, CofigurationPackageDescription packageConfig)
        {
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(serviceUrl + "/GetCurrentVersion?packageName=" + packageName);
            var currentVersion = await response.Content.ReadAsStringAsync();
            return packageConfig.Version != currentVersion;
        }

        public async Task<string> GetNextVersionAsync(string name)
        {
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(serviceUrl + "/GetNextVersion?packageName=" + name);
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<PackageDescription> GetPackageAsync(string packageName)
        {
            var httpClient = new HttpClient();
            var packageResponse = await httpClient.GetAsync(serviceUrl + "/GetPackage?packageName=" + packageName);
            if (packageResponse.IsSuccessStatusCode)
            {
                var descriptionJson = await packageResponse.Content.ReadAsStringAsync();
                var description = JsonConvert.DeserializeObject<PackageDescription>(descriptionJson);
                return description;
            }
            else
            {
                return null;
            }
        }

        public async Task Push(string wspFileName, CofigurationPackageDescription packageConfig, FileStream packageFile)
        {
            var httpClient = new HttpClient();
            var content = new MultipartFormDataContent();
            content.Add(new StreamContent(packageFile), "packageData", wspFileName);
            var response = await httpClient.PostAsync(serviceUrl + $"/Push?packageName={packageConfig.Name}&version={packageConfig.Version}", content);
            if (!response.IsSuccessStatusCode)
                throw new ApplicationException("Error push package");
        }
    }
}
