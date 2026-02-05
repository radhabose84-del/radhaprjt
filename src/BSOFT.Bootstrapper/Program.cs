using FluentValidation;
using MediatR;

using BSOFT.Bootstrapper.Configurations;
using BSOFT.Bootstrapper.Middleware;

using Shared.Validation.Common;

using UserManagement.Module;
using FixedAssetManagement.Module;
using MaintenanceManagement.Module;
using PurchaseManagement.Module;
using InventoryManagement.Module;
using UserManagement.Application.Common.Behaviors;

var builder = WebApplication.CreateBuilder(args);
var environment = builder.Environment.EnvironmentName;

var swaggerModuleDocs = new[]
{
    new SwaggerModuleInfo("UserManagement", "User Management API", "v1", "UserManagement.API.Controllers"),
    new SwaggerModuleInfo("FixedAssetManagement", "Fixed Asset Management API", "v1", "FAM.API.Controllers"),
    new SwaggerModuleInfo("MaintenanceManagement", "Maintenance Management API", "v1", "MaintenanceManagement.API.Controllers"),
    new SwaggerModuleInfo("PurchaseManagement", "Purchase Management API", "v1", "PurchaseManagement.API.Controllers")
};

builder.Configuration
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"settings/serilogsetting.{environment}.json", optional: true, reloadOnChange: true)
    .AddJsonFile("settings/jwtsetting.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// ✅ Global MediatR pipeline behaviors
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// ✅ Shared validation provider
builder.Services.AddSingleton<IValidationRuleProvider, JsonValidationRuleProvider>();

// ✅ FluentValidation global options
ValidatorOptions.Global.DefaultClassLevelCascadeMode = CascadeMode.Continue;
ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;

// ✅ Register modules (each module registers its own handlers/validators/mappings/infra)
builder.Services.AddUserManagementModule(builder.Configuration, builder.Environment);
builder.Services.AddFixedAssetManagementModule(builder.Configuration, builder.Environment);
builder.Services.AddMaintenanceManagementModule(builder.Configuration, builder.Environment);
builder.Services.AddPurchaseManagementModule(builder.Configuration, builder.Environment);
builder.Services.AddInventoryManagementModule(builder.Configuration, builder.Environment);



// ✅ Controllers + API
builder.Services.AddControllers();
builder.Services.AddSwaggerDocumentation(swaggerModuleDocs);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddCorsPolicy();
builder.Services.AddHttpContextAccessor();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.DocumentTitle = "BSOFT API";
        foreach (var module in swaggerModuleDocs)
        {
            c.SwaggerEndpoint($"/swagger/{module.DocumentName}/swagger.json", module.Title);
        }
        c.RoutePrefix = "swagger";
    });
}

app.UseRouting();
app.UseCors("AllowAll");

app.UseHttpsRedirection(); // ✅ recommended
app.UseAuthentication();

app.UseMiddleware<TokenValidationMiddleware>();
app.UseMiddleware<UserManagement.Infrastructure.Logging.Middleware.LoggingMiddleware>();

app.UseAuthorization();
app.MapControllers();

app.Run();
