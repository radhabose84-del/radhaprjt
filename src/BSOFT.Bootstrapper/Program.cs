using FluentValidation;
using MediatR;

using BSOFT.Bootstrapper.Configurations;
using BSOFT.Bootstrapper.Middleware;

using Shared.Validation.Common;
using Shared.Infrastructure.Caching;

using UserManagement.Module;
using FixedAssetManagement.Module;
using MaintenanceManagement.Module;
using PurchaseManagement.Module;
using InventoryManagement.Module;
using Contracts.Common;
using PartyManagement.Module;
using SalesManagement.Module;
using WarehouseManagement.Module;
using ProjectManagement.Module;
using BudgetManagement.Module;

var builder = WebApplication.CreateBuilder(args);
var environment = builder.Environment.EnvironmentName;


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
builder.Services.AddPartyManagementModule(builder.Configuration, builder.Environment);
builder.Services.AddSalesManagementModule(builder.Configuration, builder.Environment);
builder.Services.AddWarehouseManagementModule(builder.Configuration, builder.Environment);
builder.Services.AddProjectManagementModule(builder.Configuration, builder.Environment);
builder.Services.AddBudgetManagementModule(builder.Configuration, builder.Environment);

// ✅ Global lookup caching (MUST be after module registrations)
// Automatically decorates ALL I*Lookup interfaces with caching - zero changes needed in handlers!
builder.Services.AddLookupCaching(options =>
{
    options.CacheDuration = TimeSpan.FromMinutes(30);       // Cache for 30 min if not accessed
    options.AbsoluteExpiration = TimeSpan.FromHours(24);    // Force refresh after 24 hours
    options.SizeLimit = 2000;                                // Max 2000 cached lookup entries
    options.EnableDetailedLogging = false;                   // Set to true for debugging
});

// ✅ Controllers + API
//builder.Services.AddControllers();
builder.Services.AddControllers(o =>
{
    o.Filters.Add<DbUniqueToValidationFilter>();
});
// builder.Services.AddSwaggerDocumentation(swaggerModuleDocs);
builder.Services.AddSwaggerDocumentation();
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
        foreach (var module in SwaggerSetup.DefaultModuleDocs)
        {
            c.SwaggerEndpoint($"../swagger/{module.DocumentName}/swagger.json", module.Title);
        }
        c.RoutePrefix = "swagger";
    });
}

app.UseRouting();
app.UseCors("AllowAll");

app.UseHttpsRedirection(); // ✅ recommended
app.UseAuthentication();

app.UseMiddleware<Shared.Infrastructure.Middleware.GlobalExceptionMiddleware>();
app.UseMiddleware<TokenValidationMiddleware>();

app.UseAuthorization();
app.MapControllers();

app.Run();

