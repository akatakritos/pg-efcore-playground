using System;
using Demo.Api.Data;
using Demo.Api.Shared;

namespace Demo.Api.Domain
{
    public class RecipeIngredient : ModelBase
    {
#pragma warning disable 169
        // for EF
        // private int _recipeId;
        // private int _ingredientId;
#pragma warning restore 169

        public virtual Recipe Recipe { get; } = null!;

        public UnitOfMeasure UnitOfMeasure { get; } = null!;


        public Ingredient Ingredient { get; } = null!;

        private decimal _quantity = Decimal.One;
        public decimal Quantity
        {
            get => _quantity;
            set => _quantity = Verify.Param(value, nameof(Quantity)).IsGreaterThan(0M).Value;
        }

        // for EF
        protected RecipeIngredient(){}

        public RecipeIngredient(Recipe recipe, Ingredient ingredient, UnitOfMeasure unitOfMeasure, decimal quantity): this()
        {
            Verify.Param(recipe, nameof(recipe)).IsNotNull();
            Verify.Param(ingredient, nameof(ingredient)).IsNotNull();
            Verify.Param(unitOfMeasure, nameof(unitOfMeasure)).IsNotNull();

            Recipe = recipe;
            Ingredient = ingredient;
            UnitOfMeasure = unitOfMeasure;
            Quantity = quantity;
        }
    }
}
