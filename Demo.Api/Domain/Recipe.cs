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

        private string _name;
        public string Name
        {
            get => _name;
            set => _name = Verify.Param(value, nameof(Name)).IsNotNullOrEmpty().Value;
        }

        public string Description { get; set; }

        public Duration CookTime { get; set; } = Duration.Zero;

        public Duration PrepTime { get; set; } = Duration.Zero;
        public virtual IReadOnlyList<RecipeIngredient> RecipeIngredients => _recipeIngredients;

        // needed for EF
        // ReSharper disable once MemberCanBePrivate.Global
        protected Recipe(){}

        public Recipe(string name): this()
        {
            Name = name;
        }


        public RecipeIngredient AddIngredient(Ingredient ingredient, UnitOfMeasure unitOfMeasure, decimal quantity)
        {
            Verify.Param(ingredient, nameof(ingredient)).IsNotNull();
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
            Verify.Param(identifier, nameof(identifier)).IsNotNull();

            var recipeIngredient = _recipeIngredients.FirstOrDefault(identifier.Matches);
            if (recipeIngredient == null)
            {
                throw new RecordNotFoundException(nameof(RecipeIngredient), identifier);
            }

            recipeIngredient.SoftDelete();
            _recipeIngredients.Remove(recipeIngredient);
            MarkUpdated();
        }

        public override void SoftDelete()
        {
            base.SoftDelete();
            foreach (var recipe in _recipeIngredients)
            {
                recipe.SoftDelete();
            }
            _recipeIngredients.Clear();
        }
    }
}
