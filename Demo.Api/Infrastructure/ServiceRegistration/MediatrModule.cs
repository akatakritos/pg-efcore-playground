using Autofac;
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


            var mediatrPlugins = new[]
            {
                typeof(IRequestHandler<>), typeof(IRequestHandler<,>), typeof(IPipelineBehavior<,>)
            };

            foreach (var @interface in mediatrPlugins)
            {
                builder.RegisterAssemblyTypes(ThisAssembly)
                    .AsClosedTypesOf(@interface)
                    .InstancePerDependency();
            }
        }
    }
}
