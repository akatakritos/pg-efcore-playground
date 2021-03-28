using Demo.Api.Data;
using Demo.Api.Shared;

namespace Demo.Api.Domain
{
    public class Ingredient : ModelBase
    {
        public string Name { get; }

        public Ingredient(string name)
        {
            Verify.Param(name, nameof(name)).IsNotNullOrEmpty();
            Name = name;
        }
    }
}
