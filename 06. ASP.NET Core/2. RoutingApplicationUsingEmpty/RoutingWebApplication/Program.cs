using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions()
{
    WebRootPath = "otherStaticFiles"
});

var app = builder.Build();

app.UseStaticFiles();

app.UseStaticFiles(new StaticFileOptions()
{
    FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath,"newStaticFiles"))
});

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.Map("/", async (context) =>
    {
        await context.Response.WriteAsync("This is from Default Endpoint");
    });

    endpoints.MapGet("/Home", async (context) =>
    {
        await context.Response.WriteAsync("This is form Home Endpoint");
    });

    endpoints.MapGet("/Products/{id?}", async (context) =>
    {
        var id = context.Request.RouteValues["id"];

        if(id == null)
        {
            context.Response.WriteAsync("Products Page");
        }
        else
        {
            context.Response.WriteAsync($"Products : {id}");
        }
    });

    endpoints.MapPost("/Products", async (context) =>
    {
        await context.Response.WriteAsync("This is Products Route");

    });

    endpoints.MapGet("/Product/{id=101}", async (context) =>
    {
        var id = Convert.ToInt32(context.Request.RouteValues["id"]);
        await context.Response.WriteAsync($"Product Id  : {id}");
    });

    endpoints.MapGet("/Author/{authur-name:minlength(5):maxlength(8)}/{book-id:int}", async (context) =>
    {
        var authur_name = context.Request.RouteValues["authur-name"].ToString();
        var book_id = context.Request.RouteValues["book-id"].ToString();

        await context.Response.WriteAsync($"Author Name : {authur_name}\nBook ID : {book_id}");
    });
});

app.Run( async (context) =>
{
    await context.Response.WriteAsync("This is Other Endpoint");
});

app.Run();
