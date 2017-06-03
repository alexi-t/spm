using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPM.Shell.Commands.Base
{
    public abstract class BaseCommand
    {
        private readonly string command;

        private readonly List<CommandInput> inputs = null;
        private readonly List<CommandArgument> arguments = null;
        private readonly List<CommandModifier> modifiers = null;

        private readonly int requiredInputsCount = 0;

        public BaseCommand(string command,
            IEnumerable<CommandInput> inputs = null,
            IEnumerable<CommandArgument> arguments = null,
            IEnumerable<CommandModifier> modifiers = null)
        {
            this.command = command;

            this.inputs = inputs != null ? inputs.ToList() : new List<CommandInput>();
            this.arguments = arguments != null ? arguments.ToList() : new List<CommandArgument>();

            if (inputs != null)
            {
                foreach (var input in inputs.Where(i => i.Required))
                {
                    if (input.Index != requiredInputsCount++)
                        throw new InvalidOperationException("There is a gap in reqired inputs order");
                }
            }
        }

        public string GetName()
        {
            return command;
        }

        private Dictionary<CommandInput, string> parsedInputs;
        private Dictionary<CommandArgument, string> parsedArguments;
        private List<CommandModifier> parsedModifiers;

        protected string GetCommandInputValue(CommandInput input) => parsedInputs.ContainsKey(input) ? parsedInputs[input] : string.Empty;
        protected string GetArgumentValue(CommandArgument argument) => parsedArguments.ContainsKey(argument) ? parsedArguments[argument] : string.Empty;
        public bool HasModifier(CommandModifier modifier) => parsedModifiers.Contains(modifier);

        protected abstract Task RunCommandAsync();

        public async Task RunAsync(string[] args)
        {
            var index = 0;

            parsedInputs = new Dictionary<CommandInput, string>();
            parsedArguments = new Dictionary<CommandArgument, string>();
            parsedModifiers = new List<CommandModifier>();

            while (index < args.Length)
            {
                bool tokenParsed = false;

                string currentToken = args[index];

                if (inputs.Any(i => i.Required))
                {
                    var input = inputs.FirstOrDefault(i => i.Required && i.Index == index);
                    if (input != null)
                    {
                        parsedInputs.Add(input, currentToken);
                        tokenParsed = true;
                    }
                    int? lastRequiredInputParsedIndex = parsedInputs.Keys.OrderBy(k => k.Index).LastOrDefault()?.Index;
                    if (index >= requiredInputsCount && lastRequiredInputParsedIndex.GetValueOrDefault() < index)
                        throw new InvalidOperationException($"Required inputs not provided {string.Join(", ", inputs.OrderBy(i => i.Index).Where(i => i.Required && i.Index > lastRequiredInputParsedIndex).Select(i => i.Name))}");
                }

                if (!tokenParsed)
                {
                    var argName = currentToken.TrimStart('-');
                    var argument = arguments.FirstOrDefault(a => a.Name == argName || a.Alias == argName);
                    if (argument != null)
                    {
                        index++;
                        string argValue = args[index];
                        parsedArguments.Add(argument, argValue);
                        tokenParsed = true;
                    }
                }
                
                if (!tokenParsed && inputs.Any())
                {
                    CommandInput input = inputs.FirstOrDefault(i => i.Index == index);
                    if (input != null)
                    {
                        parsedInputs.Add(input, currentToken);
                        tokenParsed = true;
                    }
                }

                if (tokenParsed)
                    index++;
                else
                    throw new InvalidOperationException($"Can not parse {args[index]}");
            }

            await RunCommandAsync();
        }
    }
}
