using Demo.Api.Data;

namespace Demo.Api.Domain
{
    public class Tag : ModelBase
    {
        public string Name { get; }
        public string? Description { get; set; }

        public Tag(string name)
        {
            Name = name;
        }
    }
}
