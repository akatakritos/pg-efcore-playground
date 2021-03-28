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

            // cant use assembly scanning for registering as open generics
            // see https://github.com/jbogard/MediatR/issues/128
            builder.RegisterGeneric(typeof(ValidationBehavior<,>))
                .As(typeof(IPipelineBehavior<,>));
            builder.RegisterGeneric(typeof(LoggingBehavior<,>))
                .As(typeof(IPipelineBehavior<,>));
            builder.RegisterGeneric(typeof(CachingBehavior<,>))
                .As(typeof(IPipelineBehavior<,>));

            var mediatrPlugins = new[]
            {
                typeof(IRequestHandler<>), typeof(IRequestHandler<,>)
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