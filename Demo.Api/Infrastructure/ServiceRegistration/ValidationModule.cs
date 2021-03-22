using Autofac;
using FluentValidation;

namespace Demo.Api.Infrastructure.ServiceRegistration
{
    public class ValidationModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(ThisAssembly)
                .Where(t => t.ImplementsInterface(typeof(IValidator<>)))
                .AsImplementedInterfaces();
        }
    }
}
