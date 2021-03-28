using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Api.ReferenceData
{
    [Route("refdata/")]
    public class ReferenceDataController: ControllerBase
    {
        private readonly IMediator _mediator;

        public ReferenceDataController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("unit-of-measure")]
        public async Task<IActionResult> GetUnitsOfMeasure()
        {
            return Ok(await _mediator.Send(new GetUnitsOfMeasureRequest()));
        }
    }
}
