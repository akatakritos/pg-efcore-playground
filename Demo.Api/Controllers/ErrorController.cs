using System;
using System.Collections.Generic;
using System.Linq;
using Demo.Api.Data;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace Demo.Api.Controllers
{
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorController : ControllerBase
    {
        private (int, ErrorResponse) GetStatusCode(Exception e)
        {
            return e switch
            {
                RecordNotFoundException => (404, new ErrorResponse(e)),
                ValidationException v => (400, new ValidationErrorResponse(v)),
                _ => (500, new ErrorResponse(e))
            };
        }

        [Route("/error")]
        public ErrorResponse Error([FromServices] IWebHostEnvironment webHostEnvironment)
        {
            var context = HttpContext.Features.Get<IExceptionHandlerFeature>();
            var exception = context.Error;

            var (statusCode, response) = GetStatusCode(exception);

            Response.StatusCode = statusCode;
            if (webHostEnvironment.IsProduction())
            {
                response.StackTrace = null;
            }

            return response;
        }
    }

    public class ErrorResponse
    {
        public ErrorResponse(Exception e)
        {
            Message = e.Message;
            StackTrace = e.StackTrace;
        }

        public string Message { get; }
        public string StackTrace { get; set; }
    }

    public class ValidationErrorResponse : ErrorResponse
    {
        public ValidationErrorResponse(ValidationException v) : base(v)
        {
            ValidationFailures = v.Errors.Select(e => new ValidationFailure
                { PropertyName = e.PropertyName, ErrorMessage = e.ErrorMessage });
        }

        public IEnumerable<ValidationFailure> ValidationFailures { get; }
    }

    public class ValidationFailure
    {
        public string PropertyName { get; set; }
        public string ErrorMessage { get; set; }
    }
}