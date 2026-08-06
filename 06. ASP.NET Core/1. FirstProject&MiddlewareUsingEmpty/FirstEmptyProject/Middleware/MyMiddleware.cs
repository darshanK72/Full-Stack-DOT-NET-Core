namespace FirstEmptyProject.Middleware
{
    public class MyMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            context.Response.WriteAsync("Message before custom middle ware\n");
            await next(context);
            context.Response.WriteAsync("Message After custom middle ware\n");
        }
    }

    public static class CumstomMiddlewareExtension
    {
        public static IApplicationBuilder UseMyMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<MyMiddleware>();
        }
    }
   

}
