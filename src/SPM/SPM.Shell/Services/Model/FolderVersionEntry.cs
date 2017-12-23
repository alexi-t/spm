using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPM.Shell.Services.Model
{
    public class FolderVersionEntry
    {
        public FolderVersionEntry(IEnumerable<string> allFiles, string filesHash)
        {
            this.Files = allFiles.ToArray();
            this.Hash = filesHash;
            this.Timestamp = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }

        public int Timestamp { get; set; }
        public string[] Files { get; set; }
        public string Hash { get; set; }
    }
}
