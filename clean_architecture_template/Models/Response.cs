﻿using Microsoft.AspNetCore.Mvc.ModelBinding;

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
            var errorMessage = modelState?
                    .Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault(msg => msg.Contains("The JSON value could not be converted"))
                is not null
                ? "Invalid data format in the request payload."
                : modelState?.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault();

            return new Response()
            {
                Status = -1,
                Error = errorMessage ?? "Bad Request"
            };
        }

        public Response Unauthorized()
        {
            return new Response()
            {
                Status = -1,
                Error = "Unauthorized"
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
