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

        public Response BadRequest(ModelStateDictionary? modelState = null)
        {
            //The JSON value could not be converted to System.String. Path: $.username | LineNumber: 1 | BytePositionInLine: 15.
            //"JSON deserialization for type 'clean_architecture_template.Models.UserModel' was missing required properties including: 'email', 'password'."

            var errorMessages = modelState?
                .Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage);

            if (errorMessages?.FirstOrDefault(e => e.Contains("The JSON object contains a trailing comma")) is not null)
            {
                return new Response()
                {
                    Status = -1,
                    Error = "Invalid JSON format received"
                };
            }

            var dataTypeValidationError = errorMessages?.FirstOrDefault(e => e.Contains("could not be converted"));
            if (dataTypeValidationError is not null)
            {
                dataTypeValidationError = dataTypeValidationError[(dataTypeValidationError.IndexOf("$.", StringComparison.CurrentCulture) + 2)..];
                return new Response()
                {
                    Status = -1,
                    Error = $"Incorrect datatype received for parameter: '{dataTypeValidationError[..dataTypeValidationError.IndexOf(' ')]}' "
                };
            }

            var missingParametersValidationError =
                errorMessages.FirstOrDefault(e => e.Contains("missing required properties"));
            if (missingParametersValidationError is not null)
            {
                missingParametersValidationError =
                    missingParametersValidationError[missingParametersValidationError.IndexOf(':')..];
                return new Response()
                {
                    Status = -1,
                    Error = $"Payload received with missing mandatory parameters{missingParametersValidationError}"
                };
            }

            if (errorMessages.FirstOrDefault(e => e.Contains("field is required")) is not null)
            {
                return new Response()
                {
                    Status = -1,
                    Error = "Empty payload received"
                };
            }

            return new Response()
            {
                Status = -1,
                Error = "Bad Request"
            };
        }

        public Response Unauthorized(string errorMessage = "Unauthorized")
        {
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
