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
    public class VersioningService : IVersioningService
    {
        private const string versionHistoryFileName = ".spm";

        private readonly IUIService uiService;
        private readonly IHashService hashService;

        public VersioningService(IUIService uiService, IHashService hashService, IFileService fileService)
        {
            this.uiService = uiService;
            this.hashService = hashService;

            InitVersionStoreFile();
        }

        private void InitVersionStoreFile()
        {
            if (!File.Exists(versionHistoryFileName))
            {
                using (var sw = File.CreateText(versionHistoryFileName))
                {
                    sw.Write("{\"entries\": []}");
                }
                File.SetAttributes(versionHistoryFileName, FileAttributes.Compressed | FileAttributes.Hidden);
            }
        }

        private FolderVersionEntry[] ReadCurrentHistory()
        {
            if (File.Exists(versionHistoryFileName))
            {
                string historyJson = File.ReadAllText(versionHistoryFileName);

                JObject historyJObject = JObject.Parse(historyJson);

                return historyJObject["entries"].ToObject<IEnumerable<FolderVersionEntry>>().ToArray();
            }
            return new FolderVersionEntry[0];
        }

        private void SaveHistory(FolderVersionEntry[] entries)
        {
            FolderVersionEntry[] currentHistory = ReadCurrentHistory();

            File.WriteAllText(versionHistoryFileName, JsonConvert.SerializeObject(new { entries = currentHistory.Union(entries) }));
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<string> CreateInitialVersion(bool explicitInclude, string[] ignore)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            string[] allFiles = Directory.GetFiles(".", "*.*", SearchOption.AllDirectories);

            List<string> resultFiles = new List<string>();

            foreach (string filePath in allFiles)
            {
                if (ignore.All(i => !MatchExclude(filePath, i)))
                {
                    uiService.AddMessage($"Adding {filePath}...");
                    resultFiles.Add(filePath);
                }
            }

            string filesHash = hashService.ComputeFilesHash(resultFiles);

            SaveHistory(new[] { new FolderVersionEntry(resultFiles, filesHash) });

            return filesHash;
        }

        private bool MatchExclude(string filePath, string exclude)
        {
            return filePath.Contains(exclude);
        }
    }
}