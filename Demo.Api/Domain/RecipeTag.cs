using Demo.Api.Data;

namespace Demo.Api.Domain
{
    public class RecipeTag : ModelBase
    {
        public int RecipeId { get; set; }
        public int TagId { get; set; }
        public Tag Tag { get; set; }
    }
}