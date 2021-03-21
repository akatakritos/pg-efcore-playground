using System;
using System.Threading.Tasks;
using Demo.Api.Customers;
using MediatR;
using Microsoft.AspNetCore.Http;
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
        /// Get a customer by its key
        /// </summary>
        /// <param name="key">The key of the customer to fetch</param>
        /// <returns></returns>
        /// <response code="200">Returns the customer</response>
        [HttpGet("{key}")]
        [ProducesResponseType(typeof(GetCustomerResponse), 200)]
        public async Task<GetCustomerResponse> Get(Guid key)
        {
            return await _mediator.Send(new GetCustomerRequest() {Key = key});

        }
    }
}
