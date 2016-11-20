using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SPM.Shell.Commands.Pack
{
    public class FileHashService : IHashService
    {
        public string ComputeHash(Stream packageStream)
        {
            using (var md5 = MD5.Create())
            {
                return Encoding.Default.GetString(md5.ComputeHash(packageStream));
            }
        }
    }
}
