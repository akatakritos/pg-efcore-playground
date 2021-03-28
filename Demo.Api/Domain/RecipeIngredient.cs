using Demo.Api.Data;

namespace Demo.Api.Domain
{
    public class RecipeIngredient : ModelBase
    {
        public int RecipeId { get; set; }
        public virtual Recipe Recipe { get; set; }
        public UnitOfMeasure UnitOfMeasure { get; set; }
        public int IngredientId { get; set; }
        public Ingredient Ingredient { get; set; }
        public decimal Quantity { get; set; }
    }
}