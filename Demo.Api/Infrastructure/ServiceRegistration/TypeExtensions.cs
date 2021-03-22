using System;
using System.Linq;

namespace Demo.Api.Infrastructure.ServiceRegistration
{
    public static class TypeExtensions
    {
        public static bool ImplementsInterface(this Type t, Type @interface)
        {
            bool MatchesInterface(Type type)
            {
                return type == @interface ||
                       (@interface.IsGenericType
                        && type.IsGenericType
                        && type.GetGenericTypeDefinition() == @interface);
            }

            return t.FindInterfaces((type, _) => MatchesInterface(type), null).Any();
        }
    }
}
