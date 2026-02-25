using FluentValidation;
using MediatR;
using BSOFT.Api.Configurations;
using BSOFT.Api.Middleware;
using Shared.Validation.Common;
using Shared.Infrastructure.Caching;
using UserManagement.Module;
using FixedAssetManagement.Module;
using MaintenanceManagement.Module;
using PurchaseManagement.Module;
using InventoryManagement.Module;
using PartyManagement.Module;
using SalesManagement.Module;
using WarehouseManagement.Module;
using ProjectManagement.Module;
using BudgetManagement.Module;
using Contracts.Common;

var builder = WebApplication.CreateBuilder(args);
var environment = builder.Environment.EnvironmentName;

builder.Configuration
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"settings/serilogsetting.{environment}.json", optional: true, reloadOnChange: true)
    .AddJsonFile("settings/jwtsetting.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddHttpContextAccessor();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();

// ✅ MediatR pipeline behaviors
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// ✅ Shared validation provider
builder.Services.AddSingleton<IValidationRuleProvider, JsonValidationRuleProvider>();

// ✅ FluentValidation global options
ValidatorOptions.Global.DefaultClassLevelCascadeMode = CascadeMode.Continue;
ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;

// ✅ Modules
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
builder.Services.AddLookupCaching(options =>
{
    options.CacheDuration        = TimeSpan.FromMinutes(30);
    options.AbsoluteExpiration   = TimeSpan.FromHours(24);
    options.SizeLimit            = 2000;
    options.EnableDetailedLogging = false;
});

// ✅ Controllers with filters (registered once)
builder.Services.AddControllers(o =>
{
    o.Filters.Add<DbUniqueToValidationFilter>();
});


// ✅ Infrastructure
builder.Services.AddSwaggerDocumentation();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddCorsPolicy();

var app = builder.Build();

// ✅ Swagger (always first so UI is accessible)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.DocumentTitle = "BSOFT API";
    foreach (var module in SwaggerSetup.DefaultModuleDocs)
    {
        c.SwaggerEndpoint($"{module.DocumentName}/swagger.json", module.Title);
    }
    c.RoutePrefix = "swagger";
});

// ✅ HTTPS Redirection only in Production to avoid Swagger CORS issues in dev
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// ✅ Correct middleware order
app.UseRouting();

app.UseCors("AllowAll");         // Must be after UseRouting, before UseAuthentication

app.UseAuthentication();

app.UseMiddleware<Shared.Infrastructure.Middleware.GlobalExceptionMiddleware>();
app.UseMiddleware<TokenValidationMiddleware>();


app.UseAuthorization();

app.MapControllers();

app.Run();