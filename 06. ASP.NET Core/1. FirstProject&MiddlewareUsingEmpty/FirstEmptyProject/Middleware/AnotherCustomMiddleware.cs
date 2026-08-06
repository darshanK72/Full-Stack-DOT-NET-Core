using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace FirstEmptyProject.Middleware
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class AnotherCustomMiddleware
    {
        private readonly RequestDelegate _next;

        public AnotherCustomMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            httpContext.Response.WriteAsync("Before Another custom middleware");
            await _next(httpContext);
            httpContext.Response.WriteAsync("After Another custom middleware");
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class AnotherCustomMiddlewareExtensions
    {
        public static IApplicationBuilder UseAnotherMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AnotherCustomMiddleware>();
        }
    }
}
