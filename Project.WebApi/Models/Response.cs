using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Project.WebApi.Models;

public class Response
{
    public int Status { get; set; }
    public object? Message { get; set; }
    public object? Data { get; set; }

    public Response Write(int status, object? error, object? data = null)
    {
        return new Response()
        {
            Status = status,
            Message = error,
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

    public Response Ok(object? message, object? data)
    {
        return new Response()
        {
            Status = 1,
            Message = message,
            Data = data
        };
    }

    public Response Ok(int status, string? message = null, object? data = null)
    {
        return new Response()
        {
            Status = status,
            Message = message,
            Data = data
        };
    }

    public Response BadRequest(ModelStateDictionary? modelState = null)
    {
        //The JSON value could not be converted to System.String. Path: $.username | LineNumber: 1 | BytePositionInLine: 15.
        //"JSON deserialization for type 'clean_architecture_template.Models.UserModel' was missing required properties including: 'email', 'password'."
        var errors = modelState?
            .Where(ms => ms.Value?.Errors.Count > 0)
            .SelectMany(kvp => kvp.Value!.Errors.Select(e => new
            {
                Field = kvp.Key.Length > 1 ? kvp.Key[2..] : "",
                Message = e.ErrorMessage
            }))
            .ToList();


        if (errors?.FirstOrDefault(e => e.Message.Contains("The JSON object contains a trailing comma")) is not null)
        {
            return new Response()
            {
                Status = -1,
                Message = "Invalid JSON format received"
            };
        }

        var missingParametersValidationError = errors?.Where(e => e.Message.Contains("missing required properties")).Select(e => e.Field.IsNullOrEmpty() ? e.Message[e.Message.IndexOf("including: ")..] : e.Field);
        if (missingParametersValidationError != null && missingParametersValidationError.Any())
        {
            return new Response()
            {
                Status = -1,
                Message = $"Payload received with missing mandatory parameters {string.Join(',', missingParametersValidationError)}"
            };
        }

        var dataTypeValidationError = errors?.Where(e => e.Message.Contains("could not be converted")).Select(e => e.Field);
        if (dataTypeValidationError != null && dataTypeValidationError.Any())
        {
            return new Response()
            {
                Status = -1,
                Message = $"Incorrect datatype received for parameters: {string.Join(',', dataTypeValidationError)} "
            };
        }

        return new Response()
        {
            Status = -1,
            Message = string.Join(',', errors?.Select(e => e.Message) ?? ["BadRequest"])
        };
    }

    public Response BadRequest(string message)
    {
        return new Response()
        {
            Status = -1,
            Message = message
        };
    }

    public Response Unauthorized(string errorMessage = "Unauthorized")
    {
        return new Response()
        {
            Status = -1,
            Message = errorMessage
        };
    }

    public Response InternalServerError(object? errorMessage = null)
    {
        return new Response()
        {
            Status = -1,
            Message = errorMessage ?? "Internal Server Error"
        };
    }

    public Response NotFound(string errorMessage)
    {
        return new Response()
        {
            Status = -1,
            Message = errorMessage ?? "Resource not found"
        };
    }
}
