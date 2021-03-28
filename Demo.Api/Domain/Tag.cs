using Demo.Api.Data;

namespace Demo.Api.Domain
{
    public class Tag : ModelBase
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}