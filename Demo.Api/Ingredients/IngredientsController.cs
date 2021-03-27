using System;
using System.Threading.Tasks;
using Demo.Api.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Api.Ingredients
{
    [Route("recipes/{recipeKey}/ingredients")]
    public class IngredientsController: ControllerBase
    {
        private readonly IMediator _mediator;

        public IngredientsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("")]
        [ProducesResponseType(typeof(ModelKey), 200)]
        public async Task<IActionResult> AddIngredient(Guid recipeKey, [FromBody] AddIngredientRequest request)
        {
            request.RecipeKey = recipeKey;
            var result = await _mediator.Send(request);
            return Ok(result);
        }
    }
}
