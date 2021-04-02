using System;
using System.Collections.Generic;
using System.Linq;
using Demo.Api.Data;
using Demo.Api.Shared;
using NodaTime;

namespace Demo.Api.Domain
{
    public record RecipeChangedEvent(Guid Key) : IDomainEvent;

    public record RecipeDeletedEvent(Guid Key) : IDomainEvent;


    // aggregate root
    public class Recipe : AggregateRoot
    {
        private readonly List<RecipeIngredient> _recipeIngredients = new();

        private string _name;

        // needed for EF
        // ReSharper disable once MemberCanBePrivate.Global
        protected Recipe()
        {
            _name = null!;
        }

        public Recipe(string name) : this()
        {
            Name = name;
        }

        public string Name
        {
            get => _name;
            set => _name = Verify.Param(value, nameof(Name)).IsNotNullOrEmpty().Value;
        }

        public string? Description { get; set; }

        public Duration CookTime { get; set; } = Duration.Zero;

        public Duration PrepTime { get; set; } = Duration.Zero;
        public virtual IReadOnlyList<RecipeIngredient> RecipeIngredients => _recipeIngredients;


        public RecipeIngredient AddIngredient(Ingredient ingredient, UnitOfMeasure unitOfMeasure, decimal quantity)
        {
            Verify.Param(ingredient, nameof(ingredient)).IsNotNull();
            Verify.Param(unitOfMeasure, nameof(unitOfMeasure)).IsNotNull();
            Verify.Param(quantity, nameof(quantity)).IsGreaterThan(0M);

            if (RecipeIngredients.Any(ri => ri.Ingredient == ri))
            {
                throw new InvalidOperationException($"Recipe [{Key}] already contains ingredient [{ingredient.Key}]");
            }

            var recipeIngredient = new RecipeIngredient(this, ingredient, unitOfMeasure, quantity);

            _recipeIngredients.Add(recipeIngredient);
            EnqueueDomainEvent(new RecipeChangedEvent(Key));
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
            EnqueueDomainEvent(new RecipeChangedEvent(Key));
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
            EnqueueDomainEvent(new RecipeDeletedEvent(Key));
        }
    }
}
