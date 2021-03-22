using System.Linq;
using Autofac;
using Demo.Api.Customers;
using MediatR;

namespace Demo.Api.Infrastructure.ServiceRegistration
{
    public class MediatrModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // Mediator itself
            builder
                .RegisterType<Mediator>()
                .As<IMediator>()
                .InstancePerLifetimeScope();

            // request & notification handlers
            builder.Register<ServiceFactory>(context =>
            {
                var c = context.Resolve<IComponentContext>();
                return t => c.Resolve(t);
            });

            // pipelines
            builder.RegisterGeneric(typeof(ValidationBehavior<,>))
                .As(typeof(IPipelineBehavior<,>))
                .InstancePerLifetimeScope();

            // handlers
            builder.RegisterAssemblyTypes(ThisAssembly)
                .Where(type => type.ImplementsInterface(typeof(IRequestHandler<>)) || type.ImplementsInterface(typeof(IRequestHandler<,>)))
                .AsImplementedInterfaces();
        }
    }
}
