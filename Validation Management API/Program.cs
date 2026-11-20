using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;
using Validation_Management_API.Data;
using Validation_Management_API.Services;
using Validation_Management_API.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(option =>
{
    option.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddControllers().AddJsonOptions(o =>
    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddScoped<IComputerSystemService, ComputerSystemService>();
builder.Services.AddScoped<IProjectService, ProjectService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    // Development only: apply any pending migrations, then fill an empty database with test data.
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
    await DataSeeder.SeedAsync(db);
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
