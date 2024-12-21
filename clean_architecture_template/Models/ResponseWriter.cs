using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace clean_architecture_template.Models
{
    public class Response
    {
        public int Status { get; set; }
        public object? Error { get; set; }
        public object? Data { get; set; }

        public Response Write(string error, object? data = null)
        {
            return new Response()
            {
                Status = 1,
                Error = error,
                Data = data
            };
        }

        public Response Ok(object? data = null)
        {
            return new Response()
            {
                Status = 1,
                Data = data
            };
        }

        public Response BadRequest(ModelStateDictionary modelState)
        {
            var errorMessage = modelState
                .Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault();

            return new Response()
            {
                Status = -1,
                Error = errorMessage
            };
        }

        public Response InternalServerError()
        {
            return new Response()
            {
                Status = 1,
                Error = "Internal Server Error"
            };
        }
    }
}
