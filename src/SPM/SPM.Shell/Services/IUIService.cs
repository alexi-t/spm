namespace SPM.Shell.Services
{
    public interface IUIService
    {
        void AddMessage(string message, bool appendNewline = true);
        string RequestValue(string message = null, bool appenNewline = false);
        void DisplayProgress(float progress);
    }
}