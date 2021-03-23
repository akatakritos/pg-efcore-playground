using System;
using System.Threading.Tasks;
using Demo.Api.Customers;
using Demo.Api.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Api.Controllers
{
    [ApiController]
    [Route("customers")]
    public class CustomerController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CustomerController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        ///     Get a customer by its key
        /// </summary>
        /// <param name="key">The key of the customer to fetch</param>
        /// <returns></returns>
        /// <response code="200">Returns the customer</response>
        [HttpGet("{key}")]
        [ProducesResponseType(typeof(GetCustomerResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        public async Task<IActionResult> Get(Guid key)
        {
            return Ok(await _mediator.Send(new GetCustomerRequest { Key = key }));
        }

        /// <summary>
        ///     Create a customer
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("")]
        [ProducesResponseType(typeof(ModelKey), 201)]
        [ProducesResponseType(typeof(ValidationErrorResponse), 400)]
        public async Task<IActionResult> Create([FromBody] CreateCustomerRequest request)
        {
            var result = await _mediator.Send(request);
            return StatusCode(201, result);
        }

        [HttpPut("{key};{version}")]
        [ProducesResponseType(typeof(ModelKey), 201)]
        [ProducesResponseType(typeof(ValidationErrorResponse), 400)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        public async Task<IActionResult> Patch(Guid key, int version, [FromBody] EditCustomerRequest request)
        {
            var cmd = new EditCustomerCommand
            {
                Name = request.Name,
                ModelKey = new ModelKey { Key = key, Version = version }
            };

            return StatusCode(201, await _mediator.Send(cmd));
        }

        [HttpGet("")]
        [ProducesResponseType(typeof(ModelKey), 201)]
        [ProducesResponseType(typeof(ValidationErrorResponse), 400)]
        public async Task<IActionResult> Search([FromQuery] SearchCustomersRequest request)
        {
            return Ok(await _mediator.Send(request));
        }

        [HttpGet("new-customers")]
        [ProducesResponseType(typeof(NewCustomerReportResult), 200)]
        public async Task<IActionResult> GetNewCustomersReport()
        {
            return Ok(await _mediator.Send(new NewCustomerReportRequest()));
        }
    }
}
