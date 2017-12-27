using System;

namespace SPM.Shell.Services
{
    public interface IUIService
    {
        void AddMessage(string message, bool appendNewline = true, ConsoleColor? color = null);
        string RequestValue(string message = null, bool appenNewline = false);
        void DisplayProgress(float progress);
        bool Ask(string question, bool? deafultAnswer = null);
    }
}