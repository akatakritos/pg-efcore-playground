using System;
using System.Diagnostics;
using Demo.Api.Data;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.FSharp.Control;

namespace Demo.Api.Controllers
{
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorController : ControllerBase
    {

        private int GetStatusCode(Exception e) => e switch
        {
            RecordNotFoundException => 404,
            _ => 500
        };

        [Route("/error")]
        public ErrorResponse Error([FromServices] IWebHostEnvironment webHostEnvironment)
        {
            var context = HttpContext.Features.Get<IExceptionHandlerFeature>();
            var exception = context.Error;

            Response.StatusCode = GetStatusCode(exception);
            return new ErrorResponse()
            {
                Message = exception.Message,
                StackTrace = webHostEnvironment.IsDevelopment() ? exception.StackTrace : null
            };
        }
    }

    public class ErrorResponse
    {
        public string Message { get; set;}
        public string StackTrace { get; set;}
    }
}
