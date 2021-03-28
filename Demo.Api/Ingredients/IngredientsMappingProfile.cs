using AutoMapper;
using Demo.Api.Domain;
using Demo.Api.Shared;

namespace Demo.Api.Ingredients
{
    public class IngredientsMappingProfile : Profile
    {
        public IngredientsMappingProfile()
        {
            CreateMap<AddIngredientRequest, Ingredient>()
                .IgnoreUneditableModelFields();
        }
    }
}