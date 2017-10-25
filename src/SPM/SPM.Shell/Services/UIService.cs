using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPM.Shell.Services
{
    public class UIService : IUIService
    {
        private readonly ConsoleColor defaultBackColor;
        private readonly ConsoleColor defaultForeColor;

        public UIService()
        {
            this.defaultBackColor = Console.BackgroundColor;
            this.defaultForeColor = Console.ForegroundColor;
        }

        public void AddMessage(string message, bool appendNewline = true)
        {
            if (appendNewline)
                Console.WriteLine(message);
            else
                Console.Write(message);
        }

        public bool Ask(string question, bool? deafultAnswer = null)
        {
            start: Console.Write(question + " (y/n)");
            string answer = Console.ReadLine();
            switch (answer)
            {
                case "y":
                case "Y":
                    return true;
                case "n":
                case "N":
                    return false;
                default:
                    if (deafultAnswer != null)
                        return deafultAnswer.GetValueOrDefault();
                    else
                        goto start;
            }
        }

        public void DisplayProgress(float progress)
        {
            int progressInt = (int)(progress * .5);
            Console.Write("\r");
            Console.Write($"[{new string('.', progressInt).PadRight(50)}]");
            if (progressInt == 50)
                Console.WriteLine();
        }

        public string RequestValue(string message = null, bool appenNewline = false)
        {
            if (!string.IsNullOrEmpty(message))
                AddMessage(message, appenNewline);

            return Console.ReadLine();
        }
    }
}
