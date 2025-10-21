using Api.Endpoints;
using Application.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Application;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOpenApi();
builder.Services.AddApplication();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(
    options => options.UseNpgsql(connectionString,
    npgsqlOptions => npgsqlOptions.MigrationsAssembly("Application")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("/scalar/v1/api");
}

app.MapToDoEndpoints();

app.MapGet("/", context =>
{
    context.Response.Redirect("/scalar/v1/api");
    return Task.CompletedTask;
});

app.Run();
