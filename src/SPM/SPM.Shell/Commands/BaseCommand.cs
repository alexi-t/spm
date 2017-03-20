﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPM.Shell.Commands
{
    public abstract class BaseCommand
    {
        private readonly string command;

        private readonly List<CommandInput> inputs = null;
        private readonly List<CommandArgument> arguments = null;

        public BaseCommand(string command, IEnumerable<CommandInput> inputs = null, IEnumerable<CommandArgument> arguments = null)
        {
            this.command = command;

            this.inputs = inputs != null ? inputs.ToList() : new List<CommandInput>();
            this.arguments = arguments != null ? arguments.ToList() : new List<CommandArgument>();
        }

        public string GetName()
        {
            return command;
        }

        protected CommandInput GetCommandInputByName(string name) => inputs.FirstOrDefault(i => i.Name == name);

        protected abstract void RunCommand(Dictionary<CommandInput, string> parsedInput, Dictionary<CommandArgument, string> parsedArguments);

        public void Run(string[] args)
        {
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
                if (inputs.Any(i => i.Required))
                {
                    var input = inputs.FirstOrDefault(i => i.Required && i.Index == index);
                    if (input != null)
                    {
                        parsedInputs.Add(input, args[index]);
                        index++;
                        continue;
                    }
                }

                var arg = args[index];
                var argName = arg.TrimStart('-');
                var argument = arguments.FirstOrDefault(a => a.Name == argName || a.Alias == argName);
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

                if (inputs.Any())
                {
                    CommandInput input = inputs.FirstOrDefault(i => i.Index == index);
                    if (input != null)
                    {
                        parsedInputs.Add(input, args[index]);
                    }
                }

                throw new InvalidOperationException($"Can not parse {arg}");
            }

            RunCommand(parsedInputs, parsedArguments);
        }
    }
}