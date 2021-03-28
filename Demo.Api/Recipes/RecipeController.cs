using System;
using System.Threading.Tasks;
using Demo.Api.Controllers;
using Demo.Api.Data;
using Demo.Api.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Api.Recipes
{
    [Route("recipes")]
    public class RecipeController: ControllerBase
    {
        private readonly IMediator _mediator;

        public RecipeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ModelUpdateIdentifier), 200)]
        [ProducesResponseType(typeof(ValidationErrorResponse), 400)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<IActionResult> Create([FromBody] CreateRecipeCommand cmd)
        {
            var result = await _mediator.Send(cmd);
            return Ok(result);
        }

        [HttpGet("{key}")]
        [ProducesResponseType(typeof(RecipeResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<IActionResult> Get(Guid key)
        {
            var result = await _mediator.Send(new GetRecipeRequest() { Key = key });
            return Ok(result);
        }
    }
}
