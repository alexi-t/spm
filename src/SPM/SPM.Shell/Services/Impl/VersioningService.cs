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

        private void SaveHistory(FolderVersionEntry entry) => SaveHistory(new[] { entry });
        private void SaveHistory(FolderVersionEntry[] entries)
        {
            FolderVersionEntry[] currentHistory = ReadCurrentHistory();
            using (var fs = File.OpenWrite(versionHistoryFileName))
            using (var sw = new StreamWriter(fs))
            {
                fs.Position = 0;
                sw.Write(JsonConvert.SerializeObject(new { entries = currentHistory.Union(entries) }));
            }
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<FolderVersionEntry> CreateInitialVersion(IEnumerable<string> files)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var folderVersion = new FolderVersionEntry();

            foreach (string filePath in files)
            {
                uiService.AddMessage($"Adding {filePath}...");
                folderVersion.AddEntry(filePath, hashService.ComputeFilesHash(new[] { filePath }), FileHistoryType.Added);
            }

            SaveHistory(new[] { folderVersion });

            return folderVersion;
        }

        private bool MatchExclude(string filePath, string exclude)
        {
            if (string.IsNullOrEmpty(exclude))
                return false;
            return filePath.Contains(exclude);
        }

        public FolderVersionEntry CreateDiff(IEnumerable<string> currentFilesList)
        {
            FolderVersionEntry[] currentHistory = ReadCurrentHistory();

            var lastFilesVersion = new List<FileHistoryEntry>();
            foreach (var historyEntry in currentHistory)
            {
                lastFilesVersion.AddRange(historyEntry.Files.Where(f => f.EditType == FileHistoryType.Added));

                foreach (FileHistoryEntry deletedEntry in historyEntry.Files.Where(fv => fv.EditType == FileHistoryType.Deleted))
                {
                    FileHistoryEntry entryToRemove = lastFilesVersion.FirstOrDefault(fv => fv.Path == deletedEntry.Path);
                    if (entryToRemove != null)
                    {
                        lastFilesVersion.Remove(entryToRemove);
                    }
                    else
                    {
                        //delete entry without added file, an error?
                    }
                }
            }

            FolderVersionEntry currentVersion = new FolderVersionEntry();
            
            List<FileHistoryEntry> touchedEntries = new List<FileHistoryEntry>();

            foreach (string currentFilePath in currentFilesList)
            {
                string currentFileHash = hashService.ComputeFileHash(currentFilePath);
                FileHistoryEntry fileLastVersion = lastFilesVersion.FirstOrDefault(f => f.Path == currentFilePath);

                if (fileLastVersion != null)
                {
                    if (fileLastVersion.Hash != currentFileHash)
                        currentVersion.AddEntry(currentFilePath, currentFileHash, FileHistoryType.Modified);

                    touchedEntries.Add(fileLastVersion);
                }
                else
                {
                    currentVersion.AddEntry(currentFilePath, currentFileHash, FileHistoryType.Added);
                }
            }

            foreach (FileHistoryEntry deletedEntry in lastFilesVersion.Except(touchedEntries))
            {
                currentVersion.AddEntry(deletedEntry.Path, string.Empty, FileHistoryType.Deleted);
            }

            SaveHistory(currentVersion);

            return currentVersion;
        }

        public FolderVersionEntry CreateDiff(Dictionary<string, string> tagPathToHashMap, Dictionary<string, string> actualPathToHashMap)
        {
            FolderVersionEntry folderVersion = new FolderVersionEntry();

            List<string> addedPaths = actualPathToHashMap.Keys.Except(tagPathToHashMap.Keys).ToList();
            List<string> removedPaths = tagPathToHashMap.Keys.Except(actualPathToHashMap.Keys).ToList();
            List<string> changedPaths = actualPathToHashMap.Keys.Except(addedPaths).ToList();

            foreach (string path in removedPaths)
            {
                folderVersion.AddEntry(path, string.Empty, FileHistoryType.Deleted);
            }

            foreach (var path in addedPaths)
            {
                folderVersion.AddEntry(path, actualPathToHashMap[path], FileHistoryType.Added);
            }

            foreach (var path in changedPaths)
            {
                folderVersion.AddEntry(path, actualPathToHashMap[path], FileHistoryType.Modified);
            }

            return folderVersion;
        }
    }
}