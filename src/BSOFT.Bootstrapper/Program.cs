using MediatR;
using Core.Application.Common.Behaviors;
using Core.Application;
using UserManagement.Infrastructure;
using UserManagement.Module;
using FixedAssetManagement.Module;
using FluentValidation;
using BSOFT.Bootstrapper.Middleware;
using BSOFT.Bootstrapper.Configurations;
using Shared.Validation.Common;
using MaintenanceManagement.Module;

var builder = WebApplication.CreateBuilder(args);

var environment = builder.Environment.EnvironmentName;

// ═══════════════════════════════════════════════════════════════
// 1. CONFIGURATION
// ═══════════════════════════════════════════════════════════════

builder.Configuration
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"settings/serilogsetting.{environment}.json", optional: true, reloadOnChange: true)
    .AddJsonFile("settings/jwtsetting.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// ═══════════════════════════════════════════════════════════════
// 2. INFRASTRUCTURE & APPLICATION SERVICES
// ═══════════════════════════════════════════════════════════════

// Infrastructure (SQL Server, MongoDB, RabbitMQ, HttpClient)
// builder.Services.AddUserManagementInfrastructure(builder.Configuration, builder.Environment);

// Application services
// builder.Services.AddApplicationServices();
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(
        typeof(Core.Application.Users.Commands.CreateUser.CreateUserCommand).Assembly,
        typeof(Core.Application.UserLogin.Commands.UserLogin.UserLoginCommandHandler).Assembly,
        typeof(FAM.Application.Location.Command.CreateLocation.CreateLocationCommand).Assembly,
        typeof(FAM.Application.Location.Command.CreateLocation.CreateLocationCommandHandler).Assembly,
        typeof(MaintenanceManagement.Application.MachineGroup.Command.CreateMachineGroup.CreateMachineGroupCommand).Assembly,
        typeof(MaintenanceManagement.Application.MachineGroup.Command.CreateMachineGroup.CreateMachineGroupCommandHandler).Assembly
    // add other module Application assemblies similarly
    );
});

// ═══════════════════════════════════════════════════════════════
// 3. MEDIATR - Pipeline Behaviors
// ═══════════════════════════════════════════════════════════════

// Note: Individual handlers are registered by each module
// This registration is for GLOBAL pipeline behaviors
builder.Services.AddTransient(typeof(IPipelineBehavior<,>),
    typeof(ValidationBehavior<,>));

// ═══════════════════════════════════════════════════════════════
// 4. SHARED VALIDATION INFRASTRUCTURE (Cross-Module)
// ═══════════════════════════════════════════════════════════════

// ✅ Register shared validation rule provider (Singleton)
builder.Services.AddSingleton<IValidationRuleProvider, JsonValidationRuleProvider>();

// ═══════════════════════════════════════════════════════════════
// 5. FLUENTVALIDATION - Global Configuration
// ═══════════════════════════════════════════════════════════════

// builder.Services.AddFluentValidationAutoValidation(config =>
// {
//     // Disable DataAnnotations - use only FluentValidation
//     config.DisableDataAnnotationsValidation = true;
// });

// Configure global validation behavior
ValidatorOptions.Global.DefaultClassLevelCascadeMode = CascadeMode.Continue;
ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;

// ═══════════════════════════════════════════════════════════════
// 6. REGISTER MODULES
// ═══════════════════════════════════════════════════════════════

// ✅ UserManagement module (includes validators, handlers, repositories, services)
builder.Services.AddUserManagementModule(builder.Configuration, builder.Environment);
builder.Services.AddFixedAssetManagementModule(builder.Configuration, builder.Environment);
builder.Services.AddMaintenanceManagementModule(builder.Configuration, builder.Environment);


// ═══════════════════════════════════════════════════════════════
// 7. CONTROLLERS & API CONFIGURATION
// ═══════════════════════════════════════════════════════════════

builder.Services.AddControllers();

// Swagger Documentation
builder.Services.AddSwaggerDocumentation();

// JWT Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

// CORS Policy
builder.Services.AddCorsPolicy();

// HTTP Context & Error Handling
builder.Services.AddHttpContextAccessor();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();

// ═══════════════════════════════════════════════════════════════
// 8. BUILD APPLICATION
// ═══════════════════════════════════════════════════════════════

var app = builder.Build();

// ═══════════════════════════════════════════════════════════════
// 9. MIDDLEWARE PIPELINE
// ═══════════════════════════════════════════════════════════════

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BSOFT API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthentication();

// Custom Middleware
app.UseMiddleware<TokenValidationMiddleware>();
app.UseMiddleware<UserManagement.Infrastructure.Logging.Middleware.LoggingMiddleware>();

app.UseAuthorization();
app.MapControllers();

app.Run();


// using Microsoft.OpenApi.Models;
// using MediatR;
// using Core.Application.Common.Behaviors;
// using Core.Application;
// using UserManagement.Infrastructure;
// using UserManagement.API.Middleware;
// using Shared.Infrastructure.Extensions;
// using Serilog;
// using UserManagement.Module;
// using UserManagement.API.Validation.Common;

// var builder = WebApplication.CreateBuilder(args);

// var environment = builder.Environment.EnvironmentName;

// builder.Configuration
//     .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
//     .AddJsonFile($"settings/serilogsetting.{environment}.json", optional: true, reloadOnChange: true)
//     .AddJsonFile("settings/jwtsetting.json", optional: true, reloadOnChange: true)
//     .AddEnvironmentVariables();

// // Infrastructure & Application
// builder.Services.AddInfrastructureServices(builder.Configuration, builder.Environment);
// builder.Services.AddApplicationServices();

// // MediatR
// builder.Services.AddMediatR(cfg =>
// {
//     cfg.RegisterServicesFromAssemblies(
//         typeof(Core.Application.Common.Interfaces.IUser.IUserCommandRepository).Assembly
//     );
// });
// builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// // Controllers
// builder.Services.AddControllers();

// // ============================================
// // CORRECT ORDER: Module FIRST, then FluentValidation
// // ============================================

// // 1. Register UserManagement module FIRST (it includes MaxLengthProvider registration)
// builder.Services.AddUserManagementModule();

// // 2. THEN register FluentValidation (it will find MaxLengthProvider from module)
// // builder.Services.AddFluentValidationForAllModules();

// // Your existing configuration methods
// builder.Services.AddSwaggerDocumentation();
// builder.Services.AddJwtAuthentication(builder.Configuration);
// builder.Services.AddCorsPolicy();

// builder.Services.AddHttpContextAccessor();
// builder.Services.AddProblemDetails();
// builder.Services.AddEndpointsApiExplorer();

// var app = builder.Build();

// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI(c =>
//     {
//         c.SwaggerEndpoint("/swagger/v1/swagger.json", "BSOFT API v1");
//         c.RoutePrefix = "swagger";
//     });
// }

// app.UseRouting();
// app.UseCors("AllowAll");
// app.UseAuthentication();
// app.UseMiddleware<TokenValidationMiddleware>();
// app.UseMiddleware<UserManagement.Infrastructure.Logging.Middleware.LoggingMiddleware>();
// app.UseAuthorization();
// app.MapControllers();

// app.Run();