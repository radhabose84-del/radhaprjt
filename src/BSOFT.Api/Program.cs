using FluentValidation;
using Hangfire;
using Serilog;
using MediatR;
using BSOFT.Api.Configurations;
using BSOFT.Api.Filters;
using BSOFT.Api.Middleware;
using Shared.Validation.Common;
using Shared.Infrastructure.Caching;
using Shared.Infrastructure;
using Shared.Infrastructure.Resilience;
using UserManagement.Module;
using FixedAssetManagement.Module;
using MaintenanceManagement.Infrastructure.Jobs;
using BackgroundService.Infrastructure.Jobs;
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
using LogisticsManagement.Module;
using QCManagement.Module;
using Contracts.Common;
using BSOFT.Api.Hubs;

var builder = WebApplication.CreateBuilder(args);
var environment = builder.Environment.EnvironmentName;

builder.Configuration
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"settings/serilogsetting.{environment}.json", optional: true, reloadOnChange: true)
    .AddJsonFile("settings/jwtsetting.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));

builder.Services.AddHttpContextAccessor();
builder.Services.AddSharedInfrastructureServices();
builder.Services.AddBsoftResilience(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();

// ✅ MediatR pipeline behaviors (order: PermissionBehavior runs first, then ValidationBehavior)
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(Shared.Infrastructure.Behaviors.PermissionBehavior<,>));

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
builder.Services.AddLogisticsManagementModule(builder.Configuration, builder.Environment);
builder.Services.AddQCManagementModule(builder.Configuration, builder.Environment);

// ✅ Global lookup caching with write-invalidate (MUST be after module registrations)
// Reads serve from IMemoryCache; CacheInvalidationBehavior auto-evicts after every successful
// Create*/Update*/Delete*Command, so newly added master data is visible immediately.
// TTL values act as a safety net for out-of-band changes (direct SQL updates, sync jobs).
// IWorkflowLookup and IDocumentSequenceLookup are excluded — they always hit SQL.
builder.Services.AddLookupCaching(options =>
{
    options.CacheDuration         = TimeSpan.FromMinutes(30); // sliding — safety net for out-of-band changes
    options.AbsoluteExpiration    = TimeSpan.FromHours(4);    // hard ceiling on staleness if eviction is missed
    options.SizeLimit             = 8000;                     // 71 lookup interfaces × ~50 method+arg combos ≈ 3,550; 8,000 gives 2× headroom
    options.EnableDetailedLogging = false;                    // flip to true only when debugging cache behavior
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

// ✅ Hangfire server — maintenance-jobs queue only; never competes with BSOFT.Worker's queues
builder.Services.AddHangfireServer(options =>
{
    options.ServerName  = "BSOFT.Api";
    options.Queues      = ["maintenance-jobs", "coa-refreeze-queue"];
    options.WorkerCount = 5;
});

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
if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("QA"))
{
    app.UseHttpsRedirection();
}
// ✅ Strip invisible characters (BOM, NBSP) from JSON request bodies
app.UseMiddleware<JsonBodySanitizingMiddleware>();

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


// Poll maintenance.OutboxMessages for scheduling events every minute.
// MaintenanceOutboxProcessorJob handles: MachineWiseScheduleCreationEvent,
// HeaderUpdateEvent, NextSchedulerCreatedEvent — no RabbitMQ hop.
RecurringJob.AddOrUpdate<MaintenanceOutboxProcessorJob>(
    "maintenance-outbox-processor",
    "maintenance-jobs",
    job => job.ProcessAsync(CancellationToken.None),
    Cron.Minutely());

// US-GL02-FR-008a — auto-re-freeze the COA when an unfreeze window lapses (AC3). Every minute.
RecurringJob.AddOrUpdate<CoaAutoReFreezeJob>(
    "coa-auto-refreeze",
    "coa-refreeze-queue",
    job => job.ProcessAsync(CancellationToken.None),
    Cron.Minutely());

app.UseMiddleware<TokenValidationMiddleware>();


app.UseAuthorization();

app.MapControllers();

app.MapHub<NotificationHub>("/notificationHub");

app.Run();