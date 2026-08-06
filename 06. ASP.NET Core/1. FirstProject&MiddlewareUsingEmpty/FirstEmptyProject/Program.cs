using FirstEmptyProject.Middleware;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;


// Builder Object
var builder = WebApplication.CreateBuilder(args);

// Register Services
builder.Services.AddTransient<MyMiddleware>();

// Application Builder
var app = builder.Build();

// Query Mapping
//app.MapGet("/", (HttpContext context) =>
//{
//    // Request
//    var userAgent = "";
//    if (context.Request.Headers.ContainsKey("User-Agent"))
//    {
//        userAgent = context.Request.Headers["User-Agent"];
//    }
//    string path = context.Request.Path;
//    string method = context.Request.Method;


//    // Response
//    context.Response.Headers["Content-Type"] = "text/html";

//    return $"<h1>Path : {path}\nMethod : {method}\nUser-Agent : {userAgent}</h1>";
//});


// Middleware 1
app.Use(async (HttpContext context,RequestDelegate next) =>
{
    if (context.Request.Path == "/" || context.Request.Path == "/Home")
    {
        context.Response.StatusCode = 200;
        await context.Response.WriteAsync($"Home Page\nResponse Status : {context.Response.StatusCode}");
    }
    else if (context.Request.Path == "/Contact")
    {
        context.Response.StatusCode = 200;
        await context.Response.WriteAsync($"Contact Page\nResponse Status : {context.Response.StatusCode}");
    }
    else if (context.Request.Path == "/Product" && context.Request.Method == "GET")
    {
        int productId = 0;
        string productName = string.Empty;
        if (context.Request.Query.ContainsKey("Id"))
        {
            productId = int.Parse(context.Request.Query["Id"]);
        }
        if (context.Request.Query.ContainsKey("Name"))
        {
            productName = context.Request.Query["Name"];
        }
        context.Response.StatusCode = 200;
        await context.Response.WriteAsync($"Contact Page\nProduct Id : {productId}\nProduct Name : {productName}\nResponse Status : {context.Response.StatusCode}");
    }
    else if (context.Request.Path == "/Product" && context.Request.Method == "POST")
    {
        int id = 0;
        string name = string.Empty;
        StreamReader reader = new StreamReader(context.Request.Body);

        string body = await reader.ReadToEndAsync();

        Dictionary<string,StringValues> dict = QueryHelpers.ParseQuery(body);

        if (dict.ContainsKey("Id"))
        {
            id = int.Parse(dict["Id"]);
        }
        if (dict.ContainsKey("Name"))
        {
            name = dict["Name"];
        }

        await context.Response.WriteAsync($"Contact Page\nName : {name}\nId : {id}");
    }
    else
    {
        context.Response.StatusCode = 404;
        await context.Response.WriteAsync($"Page Not Found\nResponse Status : {context.Response.StatusCode}");
    }

    await next(context);
});

// Middleware 2
app.Use(async (context, next) =>
{
    await context.Response.WriteAsync("Hello world This is Message form second Middle ware\n");
    next(context);

});

//app.UseMiddleware<MyMiddleware>();

app.UseMyMiddleware();

app.UseAnotherMiddleware();

// Optional Middleware
app.UseWhen(context => !context.Request.Query.ContainsKey("IsAuthorized"), app => 
{
    app.Use(async (context, next) =>
    {
        await context.Response.WriteAsync("Inside Before Optional Middleware");
        await next(context);
        await context.Response.WriteAsync("Inside After Optional Middleware");
    });
});

// Middleware 3
app.Run(async (context) =>
{
    await context.Response.WriteAsync("Hello world This is Message form Third Middle ware\n");
});

app.Run();
