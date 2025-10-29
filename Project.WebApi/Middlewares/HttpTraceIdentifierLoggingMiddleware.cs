using Serilog.Context;

namespace Project.WebApi.Middlewares
{
    public class HttpTraceIdentifierLoggingMiddleware(RequestDelegate next, ILogger<HttpTraceIdentifierLoggingMiddleware> _)
    {
        public async Task Invoke(HttpContext context)
        {
            LogContext.PushProperty("Request Id", context.TraceIdentifier);
            await next(context);
        }
    }

    public static class HttpTraceIdentifierLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseHttpTraceIdentifierLoggingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<HttpTraceIdentifierLoggingMiddleware>();
        }
    }
}
