using System;
using System.Collections.Generic;
using System.Linq;
using Demo.Api.Data;
using Demo.Api.Shared;
using NodaTime;

namespace Demo.Api.Domain
{
    public class Recipe : ModelBase
    {
        private readonly List<RecipeIngredient> _recipeIngredients = new();
        public string Name { get; set; }

        public string Description { get; set; }

        public Duration CookTime { get; set; }

        public Duration PrepTime { get; set; }
        public virtual IReadOnlyList<RecipeIngredient> RecipeIngredients => _recipeIngredients;

        public RecipeIngredient AddIngredient(Ingredient ingredient, UnitOfMeasure unitOfMeasure, decimal quantity)
        {
            Verify.Param(ingredient, nameof(ingredient)).IsNotNull();
            Verify.Param(unitOfMeasure, nameof(unitOfMeasure)).IsDefinedEnum();
            Verify.Param(quantity, nameof(quantity)).IsGreaterThan(0M);

            if (RecipeIngredients.Any(ri => ri.Ingredient == ri))
            {
                throw new InvalidOperationException($"Recipe [{Key}] already contains ingredient [{ingredient.Key}]");
            }

            var recipeIngredient = new RecipeIngredient
            {
                Ingredient = ingredient,
                UnitOfMeasure = unitOfMeasure,
                Quantity = quantity
            };

            _recipeIngredients.Add(recipeIngredient);
            return recipeIngredient;
        }

        public void RemoveIngredient(ModelUpdateIdentifier identifier)
        {
            var recipeIngredient = _recipeIngredients.FirstOrDefault(identifier.Matches);
            if (recipeIngredient == null)
            {
                throw new RecordNotFoundException(nameof(RecipeIngredient), identifier);
            }

            recipeIngredient.SoftDelete();
            _recipeIngredients.Remove(recipeIngredient);
            MarkUpdated();
        }
    }
}
