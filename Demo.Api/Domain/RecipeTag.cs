using Demo.Api.Data;

namespace Demo.Api.Domain
{
    public class RecipeTag : ModelBase
    {
        public int RecipeId { get; }
        public int TagId { get; set; }
        public Tag Tag { get; }

        public RecipeTag(int recipeId, Tag tag)
        {
            RecipeId = recipeId;
            Tag = tag;
        }
    }
}
