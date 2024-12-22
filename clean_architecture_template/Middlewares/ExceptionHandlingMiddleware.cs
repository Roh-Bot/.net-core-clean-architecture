using clean_architecture_template.Models;

namespace clean_architecture_template.Middlewares
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
            catch (Exception ex)
            {
                logger.LogError("{ExceptionType} {ExceptionMessage}", ex.GetType().ToString(), ex.Message);

                context.Response.StatusCode = ex switch
                {
                    TimeoutException => StatusCodes.Status504GatewayTimeout,
                    _ => StatusCodes.Status500InternalServerError
                };
                context.Response.StatusCode = 500;
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
