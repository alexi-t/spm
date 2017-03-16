using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPM.Shell.Commands
{
    public abstract class BaseCommand
    {
        private readonly string command;

        public BaseCommand(string command)
        {
            this.command = command;
        }

        public bool CanRun(string command)
        {
            return string.Equals(this.command, command, StringComparison.InvariantCultureIgnoreCase);
        }

        protected virtual CommandArgument[] GetSupportedArgs()
        {
            return Enumerable.Empty<CommandArgument>().ToArray();
        }

        protected virtual CommandInput[] GetInputs()
        {
            return Enumerable.Empty<CommandInput>().ToArray();
        }

        protected abstract void RunCommand(Dictionary<CommandInput, string> parsedInput, Dictionary<CommandArgument,string> parsedArguments);

        public void Run(string[] args)
        {
            var supportedArgs = GetSupportedArgs();
            var inputs = GetInputs();

            int requiredInputsCount = 0;
            foreach (var input in inputs.Where(i => i.Required))
            {
                if (input.Index == requiredInputsCount)
                    requiredInputsCount++;
                else
                    throw new InvalidOperationException("There is a gap in reqired inputs order");
            }

            var index = 0;

            var parsedInputs = new Dictionary<CommandInput, string>();
            var parsedArguments = new Dictionary<CommandArgument, string>();
            
            while (index < args.Length)
            {
                if (inputs.Any())
                {
                    var input = inputs.FirstOrDefault(i => i.Index == index);
                    if (input != null)
                    {
                        parsedInputs.Add(input, args[index]);
                        index++;
                        continue;
                    }
                    else if (input.Required)
                        throw new InvalidOperationException($"{input.Name} not provided");
                }

                var arg = args[index];
                var argName = arg.TrimStart('-');
                var argument = supportedArgs.FirstOrDefault(a => a.Name == argName || a.Alias == argName);
                if (argument != null)
                {
                    string argValue = string.Empty;
                    if (argument.HasValue)
                    {
                        index++;
                        argValue = args[index];
                    }
                    parsedArguments.Add(argument, argValue);
                    index++;
                    continue;
                }

                throw new InvalidOperationException($"Can not parse {arg}");
            }

            RunCommand(parsedInputs, parsedArguments);
        }
    }
}
