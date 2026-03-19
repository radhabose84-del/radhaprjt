using FluentValidation;
using Hangfire;
using MediatR;
using BSOFT.Api.Configurations;
using BSOFT.Api.Filters;
using BSOFT.Api.Middleware;
using Shared.Validation.Common;
using Shared.Infrastructure.Caching;
using Shared.Infrastructure;
using UserManagement.Module;
using FixedAssetManagement.Module;
using MaintenanceManagement.Infrastructure.Jobs;
using MaintenanceManagement.Module;
using PurchaseManagement.Module;
using InventoryManagement.Module;
using PartyManagement.Module;
using SalesManagement.Module;
using WarehouseManagement.Module;
using ProjectManagement.Module;
using BudgetManagement.Module;
using BackgroundService.Presentation;
using GateEntryManagement.Module;
using FinanceManagement.Module;
using ProductionManagement.Module;
using Contracts.Common;
using BSOFT.Api.Hubs;

var builder = WebApplication.CreateBuilder(args);
var environment = builder.Environment.EnvironmentName;

builder.Configuration
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"settings/serilogsetting.{environment}.json", optional: true, reloadOnChange: true)
    .AddJsonFile("settings/jwtsetting.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddHttpContextAccessor();
builder.Services.AddSharedInfrastructureServices();
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
builder.Services.AddBackgroundServiceModule(builder.Configuration, builder.Environment);
builder.Services.AddGateEntryManagementModule(builder.Configuration, builder.Environment);
builder.Services.AddFinanceManagementModule(builder.Configuration, builder.Environment);
builder.Services.AddProductionManagementModule(builder.Configuration, builder.Environment);

// ✅ Global lookup caching (MUST be after module registrations)
builder.Services.AddLookupCaching(options =>
{
    options.CacheDuration        = TimeSpan.FromMinutes(30);
    options.AbsoluteExpiration   = TimeSpan.FromHours(24);
    options.SizeLimit            = 2000;
    options.EnableDetailedLogging = false;
});

// ✅ SignalR hub (clients connect here; BSOFT.Worker pushes via SignalR client)
builder.Services.AddSignalR();

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

// ✅ Hangfire dashboard — placed before TokenValidationMiddleware so Basic Auth
//    handles /hangfire/** requests before the JWT check can block them.
var hangfireUser = app.Configuration["HangfireServer:DashboardUser"] ?? "admin";
var hangfirePass = app.Configuration["HangfireServer:DashboardPassword"] ?? "admin";
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = [new HangfireAuthorizationFilter(hangfireUser, hangfirePass)],
    DashboardTitle = "BSOFT Hangfire Dashboard"
});

// ✅ Hangfire server — BSOFT.Api executes business-domain Hangfire jobs in-process.
//    Only listens on "maintenance-jobs" so it never competes with BSOFT.Worker's
//    infrastructure queues (forgot_password, user_unlock, sql-outbox).
//    WorkerCount is capped to avoid background threads starving API request threads.
app.UseHangfireServer(new BackgroundJobServerOptions
{
    ServerName = "BSOFT.Api",
    Queues = ["maintenance-jobs"],
    WorkerCount = 5
});

// Poll maintenance.OutboxMessages for scheduling events every minute.
// MaintenanceOutboxProcessorJob handles: MachineWiseScheduleCreationEvent,
// HeaderUpdateEvent, NextSchedulerCreatedEvent — no RabbitMQ hop.
RecurringJob.AddOrUpdate<MaintenanceOutboxProcessorJob>(
    "maintenance-outbox-processor",
    "maintenance-jobs",
    job => job.ProcessAsync(CancellationToken.None),
    Cron.Minutely());

app.UseMiddleware<TokenValidationMiddleware>();


app.UseAuthorization();

app.MapControllers();

app.MapHub<NotificationHub>("/notificationHub");

app.Run();