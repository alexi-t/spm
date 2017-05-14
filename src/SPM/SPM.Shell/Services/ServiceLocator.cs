using Autofac;
using Autofac.Features.ResolveAnything;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPM.Shell.Services
{
    public class ServiceLocator
    {
        private readonly IContainer container;

        public ServiceLocator()
        {
            var builder = new ContainerBuilder();

            builder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());

            builder.RegisterType<UIService>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<FileService>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<ConfigService>().AsImplementedInterfaces().SingleInstance();
            builder.Register(ctx => new PackagesService("http://localhost:5001/packages")).AsImplementedInterfaces().SingleInstance();

            this.container = builder.Build();
        }

        public object CreateInstance(Type type)
        {
            return container.Resolve(type);
        }
    }
}
