using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Demo.Api.Data;
using NodaTime;

namespace Demo.Api.Domain
{
    public class Recipe: ModelBase
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Duration CookTime { get; set; }
        public Duration PrepTime { get; set; }
        private List<RecipeIngredient> _recipeIngredients = new List<RecipeIngredient>();
        public virtual IReadOnlyList<RecipeIngredient> RecipeIngredients => _recipeIngredients;

        public void AddIngredient(Ingredient ing, UnitOfMeasure uom, decimal quantity)
        {
            if (RecipeIngredients.Any(ri => ing.Id > 0 && ri.Ingredient == ri))
            {
                throw new InvalidOperationException($"Recipe [{Key}] already contains ingredient [{ing.Key}]");
            }

            var recipeIngredient = new RecipeIngredient()
            {
                Ingredient = ing,
                UnitOfMeasure = uom,
                Quantity = quantity
            };

            Debug.Assert((int) recipeIngredient.UnitOfMeasure != 0);

            _recipeIngredients.Add(recipeIngredient);
        }
    }

    public class RecipeIngredient: ModelBase
    {
        public int RecipeId { get; set; }
        public virtual Recipe Recipe { get; set; }
        public UnitOfMeasure UnitOfMeasure { get; set; }
        public int IngredientId { get; set; }
        public Ingredient Ingredient { get; set; }
        public decimal Quantity { get; set; }
    }

    public record Measurement(UnitOfMeasure Unit, decimal Quantity);

    public enum UnitOfMeasure
    {
        Teaspoon = 1,
        Tablespoon = 2,
        Cup = 3,
        Pint = 4,
        Quart = 5,
        Gallon = 6,
        Ounce = 7
    }

    public class Ingredient : ModelBase
    {
        public string Name { get; set; }
    }

    public class RecipeTag : ModelBase
    {
        public int RecipeId { get; set; }
        public int TagId { get; set; }
        public Tag Tag { get; set; }
    }

    public class Tag : ModelBase
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

}
