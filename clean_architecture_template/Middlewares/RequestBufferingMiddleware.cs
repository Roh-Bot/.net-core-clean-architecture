using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace clean_architecture_template.Middlewares
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class RequestBufferingMiddleware(RequestDelegate next)
    {
        public async Task Invoke(HttpContext context)
        {
            context.Request.EnableBuffering();
            await next(context);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class RequestBufferingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestBufferingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestBufferingMiddleware>();
        }
    }
}
