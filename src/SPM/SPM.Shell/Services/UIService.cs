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

        public string RequestValue(string message = null, bool appenNewline = false)
        {
            if (!string.IsNullOrEmpty(message))
                AddMessage(message, appenNewline);

            return Console.ReadLine();
        }
    }
}
