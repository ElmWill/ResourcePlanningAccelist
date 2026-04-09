using Microsoft.EntityFrameworkCore;
using ResourcePlanningAccelist.Commons.Extensions;
using ResourcePlanningAccelist.WebAPI.AuthorizationPolicies;
using ResourcePlanningAccelist.WebAPI.ExceptionHandling;
using ResourcePlanningAccelist.Entities;
using ResourcePlanningAccelist.Infrastructure.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

    options.UseNpgsql(connectionString);
});
builder.Services.AddApplicationCommons();
builder.Services.AddRoleAuthorizationPolicies();
builder.Services.AddHostedService<DatabaseMigrationHostedService>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("/docs");
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();