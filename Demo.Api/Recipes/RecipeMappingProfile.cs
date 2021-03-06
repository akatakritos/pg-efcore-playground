using AutoMapper;
using Demo.Api.Domain;
using Demo.Api.Shared;

namespace Demo.Api.Recipes
{
    public class RecipeMappingProfile : Profile
    {
        public RecipeMappingProfile()
        {
            CreateMap<CreateRecipeCommand, Recipe>()
                .IgnoreUneditableModelFields()
                .ForMember(d => d.RecipeIngredients, x => x.Ignore());
            CreateMap<Recipe, RecipeResponse>()
                .IncludeModelId();
            CreateMap<RecipeIngredient, RecipeIngredientResponse>()
                .IncludeModelId();
            CreateMap<Ingredient, IngredientResponse>()
                .IncludeModelId();
        }
    }
}