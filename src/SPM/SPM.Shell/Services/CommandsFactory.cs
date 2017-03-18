using SPM.Shell.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SPM.Shell.Services
{
    public class CommandsFactory
    {
        private List<Assembly> commandAssemblies;
        private ServiceLocator serviceLocator;

        public CommandsFactory() : this(Assembly.GetExecutingAssembly()) { }

        public CommandsFactory(params Assembly[] commandAssemblies)
        {
            this.serviceLocator = new ServiceLocator();
            this.commandAssemblies = commandAssemblies.ToList();
        }

        public Dictionary<string, BaseCommand> GetCommandBindings()
        {
            var bindings = new Dictionary<string, BaseCommand>();

            foreach (var assembly in commandAssemblies)
            {
                var commandTypes = assembly.GetExportedTypes().Where(t => t.IsSubclassOf(typeof(BaseCommand)) && !t.IsAbstract);

                foreach (var commandType in commandTypes)
                {
                    var commandInstance = serviceLocator.CreateInstance(commandType) as BaseCommand;
                    var commandName = commandInstance.GetName();
                    bindings.Add(commandName, commandInstance);
                }
            }

            return bindings;
        }
    }
}
