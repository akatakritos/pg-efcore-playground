using System;
using System.Threading.Tasks;
using Demo.Api.Controllers;
using Demo.Api.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Api.Ingredients
{
    [Route("recipes/")]
    public class IngredientsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public IngredientsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("{recipeKey};{recipeVersion}/ingredients")]
        [ProducesResponseType(typeof(ModelUpdateIdentifier), 200)]
        [ProducesResponseType(typeof(ValidationErrorResponse), 400)]
        [ProducesErrorResponseType(typeof(ErrorResponse))]
        public async Task<IActionResult> AddIngredient(Guid recipeKey, int recipeVersion,
                                                       [FromBody] AddIngredientRequest request)
        {
            request.RecipeKey = new ModelUpdateIdentifier(recipeKey, recipeVersion);
            var result = await _mediator.Send(request);
            return Ok(result);
        }

        [HttpDelete("{recipeKey};{recipeVersion}/ingredients/{recipeIngredientKey};{recipeIngredientVersion}")]
        [ProducesResponseType(typeof(void), 204)]
        [ProducesResponseType(typeof(ValidationErrorResponse), 400)]
        [ProducesErrorResponseType(typeof(ErrorResponse))]
        public async Task<IActionResult> RemoveIngredient(Guid recipeKey, int recipeVersion, Guid recipeIngredientKey,
                                                          int recipeIngredientVersion)
        {
            var request = new RemoveIngredientRequest
            {
                RecipeModelKey = new ModelUpdateIdentifier(recipeKey, recipeVersion),
                RecipeIngredientModelKey = new ModelUpdateIdentifier(recipeIngredientKey, recipeIngredientVersion)
            };

            await _mediator.Send(request);

            return Ok(null);
        }
    }
}