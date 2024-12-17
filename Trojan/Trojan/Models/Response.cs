using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Trojan.Models
{
    public class Response
    {
        public int StatusCode { get; set; }
        public object? ErrorMessage { get; set; }
        public object? Data { get; set; }

    }
    public class ResponseWriter
    {
        public static IActionResult Ok(object? data)
        {
            var response = new Response
            {
                StatusCode = 1,
                Data = data
            };
            return new ObjectResult(response) { StatusCode = StatusCodes.Status200OK };
        }

        public static IActionResult BadRequest(ModelStateDictionary modelState)
        {
            var errorMessage = modelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage);
            var response = new Response()
            {
                StatusCode = -1,
                ErrorMessage = errorMessage
            };
            return new ObjectResult(response)
            {
                StatusCode = StatusCodes.Status400BadRequest
            };
        }

        public static IActionResult InternalServerError()
        {
            var response = new Response();
            return new ObjectResult(response)
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }
}
