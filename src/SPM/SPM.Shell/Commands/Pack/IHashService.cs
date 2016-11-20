using System.IO;

namespace SPM.Shell.Commands.Pack
{
    public interface IHashService
    {
        string ComputeHash(Stream packageStream);
    }
}