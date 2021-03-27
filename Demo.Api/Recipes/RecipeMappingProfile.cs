using AutoMapper;
using Demo.Api.Domain;
using Demo.Api.Shared;

namespace Demo.Api.Recipes
{
    public class RecipeMappingProfile: Profile
    {
        public RecipeMappingProfile()
        {
            CreateMap<CreateRecipeCommand, Domain.Recipe>()
                .IgnoreUneditableModelFields()
                .ForMember(d => d.RecipeIngredients, x => x.Ignore());

            CreateMap<Recipe, RecipeResponse>();
            CreateMap<RecipeIngredient, RecipeIngredientResponse>();
            CreateMap<Ingredient, IngredientResponse>();
        }
    }
}
