using Api.Endpoints;
using Application.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Application;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOpenApi();

// Register Application-layer services (MediatR, validators, behaviors, etc.)
builder.Services.AddApplication();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (builder.Environment.IsDevelopment())
{
    // If no connection string is set, use in-memory database
    if (string.IsNullOrEmpty(connectionString))
    {
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase("DevInMemoryDb"));
    }
    else
    {
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString,
                npgsqlOptions => npgsqlOptions.MigrationsAssembly("Application")));  // Set the assembly where migrations are located
    }
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("/scalar/v1/api");
}

if (!string.IsNullOrEmpty(connectionString))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

// Register all ToDo endpoints defined in Api.Endpoints namespace
app.MapToDoEndpoints();

// Redirect root URL "/" to the API documentation page
app.MapGet("/", context =>
{
    context.Response.Redirect("/scalar/v1/api");
    return Task.CompletedTask;
});

app.Run();
