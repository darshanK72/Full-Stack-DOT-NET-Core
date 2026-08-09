using LocalServerWebApiApplication.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});


builder.Services.AddDbContext<Trainingdb46310114Context>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnectionString")));
var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseRouting();
app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();
