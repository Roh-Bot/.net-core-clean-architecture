using Project.WebApi.Models;
using System.ComponentModel.DataAnnotations;
using Project.Core.Exceptions;

namespace Project.WebApi.Middlewares
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (BusinessRuleException ex)
            {
                logger.LogError("{ExceptionType} {ExceptionMessage} \n {ExceptionStackTrace}", ex.GetType().ToString(), ex.Message, ex.StackTrace);

                context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                await context.Response.WriteAsJsonAsync(new Response().BadRequest());
            }
            catch (ValidationException ex)
            {
                logger.LogError("{ExceptionType} {ExceptionMessage} \n {ExceptionStackTrace}", ex.GetType().ToString(), ex.Message, ex.StackTrace);

                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new Response().BadRequest());
            }
            catch (Exception ex)
            {
                logger.LogError("{ExceptionType} {ExceptionMessage} \n {ExceptionStackTrace}", ex.GetType().ToString(), ex.Message, ex.StackTrace);

                context.Response.StatusCode = ex switch
                {
                    TimeoutException => StatusCodes.Status504GatewayTimeout,
                    _ => StatusCodes.Status500InternalServerError
                };
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(new Response().InternalServerError());
            }
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandlingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}
