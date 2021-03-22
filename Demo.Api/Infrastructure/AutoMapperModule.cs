using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using AutoMapper;
using Module = Autofac.Module;

namespace Demo.Api.Infrastructure
{
    public class AutoMapperModule : Module
    {
        private readonly bool _assertConfiguration;
        private readonly IEnumerable<Assembly> assembliesToScan;

        public AutoMapperModule(bool assertConfiguration, params Assembly[] assembliesToScan)
        {
            _assertConfiguration = assertConfiguration;
            this.assembliesToScan = assembliesToScan;
        }

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);
            var assembliesToScan = this.assembliesToScan as Assembly[] ?? this.assembliesToScan.ToArray();

            var allTypes = assembliesToScan
                .Where(a => !a.IsDynamic && a.GetName().Name != nameof(AutoMapper))
                .Distinct() // avoid AutoMapper.DuplicateTypeMapConfigurationException
                .SelectMany(a => a.DefinedTypes)
                .ToArray();

            var openTypes = new[]
            {
                typeof(IValueResolver<,,>),
                typeof(IMemberValueResolver<,,,>),
                typeof(ITypeConverter<,>),
                typeof(IValueConverter<,>),
                typeof(IMappingAction<,>)
            };

            foreach (var type in openTypes.SelectMany(openType =>
                allTypes.Where(t => t.IsClass && !t.IsAbstract && ImplementsGenericInterface(t.AsType(), openType))))
            {
                builder.RegisterType(type.AsType()).InstancePerDependency();
            }

            builder.Register<IConfigurationProvider>(ctx =>
                {
                    var config = new MapperConfiguration(cfg => cfg.AddMaps(assembliesToScan));


                    if (_assertConfiguration)
                    {
                        config.AssertConfigurationIsValid();
                    }

                    config.CompileMappings();
                    return config;
                }
            ).SingleInstance();

            builder.Register<IMapper>(ctx => new Mapper(ctx.Resolve<IConfigurationProvider>(), ctx.Resolve))
                .InstancePerDependency();
        }

        private static bool ImplementsGenericInterface(Type type, Type interfaceType)
        {
            return IsGenericType(type, interfaceType) || type.GetTypeInfo().ImplementedInterfaces
                .Any(@interface => IsGenericType(@interface, interfaceType));
        }

        private static bool IsGenericType(Type type, Type genericType)
        {
            return type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == genericType;
        }
    }
}
