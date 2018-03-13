using Newtonsoft.Json;

namespace SPM.Shell.Services.Model
{
    public enum FileHistoryType
    {
        Added, Modified, Deleted
    }

    public class FileHistoryEntry
    {
        [JsonProperty("path")]
        public string Path { get; set; }
        [JsonProperty("hash")]
        public string Hash { get; set; }
        [JsonProperty("editType")]
        public FileHistoryType EditType { get; set; }
    }
}