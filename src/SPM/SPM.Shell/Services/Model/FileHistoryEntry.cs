namespace SPM.Shell.Services.Model
{
    public enum FileHistoryType
    {
        Added, Modified, Deleted
    }

    public class FileHistoryEntry
    {
        public string Path { get; set; }
        public string Hash { get; set; }
        public FileHistoryType EditType { get; set; }
    }
}