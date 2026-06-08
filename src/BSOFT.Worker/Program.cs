#nullable disable
using BackgroundService.Application;
using BackgroundService.Application.Interfaces.Notification;
using BackgroundService.Infrastructure;
using BSOFT.Worker.Configurations;
using BSOFT.Worker.Services;
using Hangfire;
using Serilog;
using Shared.Infrastructure;
using Shared.Infrastructure.Resilience;
using PurchaseManagement.Infrastructure;
using BudgetManagement.Infrastructure;
using InventoryManagement.Infrastructure;
using UserManagement.Infrastructure;
using PartyManagement.Infrastructure;
using WarehouseManagement.Infrastructure;
using ProjectManagement.Infrastructure;
using FinanceManagement.Infrastructure;
using FinanceManagement.Application;
using ProductionManagement.Infrastructure;
using QCManagement.Infrastructure;
using SalesManagement.Infrastructure;
using MaintenanceManagement.Infrastructure;
using FAM.Infrastructure;
using LogisticsManagement.Infrastructure;


var builder = Host.CreateApplicationBuilder(args);

// ── Configuration ────────────────────────────────────────────────────────────
var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development";

builder.Configuration
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    .AddJsonFile("settings/serilogsetting.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// ── Serilog ──────────────────────────────────────────────────────────────────
var mongoUrl = builder.Configuration.GetConnectionString("MongoDbConnectionString")
               ?? throw new InvalidOperationException(
                   "Connection string 'MongoDbConnectionString' is missing from configuration.");

builder.Services.AddSerilog(lc => lc
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Hangfire", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: Path.Combine(AppContext.BaseDirectory, "logs", "bsoft-worker-.log"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30)
    .WriteTo.MongoDB(
        $"{mongoUrl}/BannariERP",
        collectionName: "WorkerLogs",
        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning));

// ── Windows Service ──────────────────────────────────────────────────────────
builder.Services.AddWindowsService(options =>
    options.ServiceName = "BSOFT Worker Service");

// ── Core infrastructure ──────────────────────────────────────────────────────
builder.Services.AddHttpContextAccessor();   // required by IPAddressService
builder.Services.AddMemoryCache();
builder.Services.AddSharedInfrastructureServices();  // registers IIPAddressService
builder.Services.AddBsoftResilience(builder.Configuration);  // Polly v8 resilience pipelines

// ── Application layer: MediatR + AutoMapper + SignalR server (for IHubContext<>) ──
builder.Services.AddApplicationServices();

// ── Infrastructure layer: Hangfire + MassTransit + all repositories ───────────
builder.Services.AddInfrastructureServices(builder.Configuration, builder.Services);

// ── Module infrastructure — registers repos needed by module-specific consumers ──
// Lookup providers first (UserManagement / PartyManagement / WarehouseManagement / Production supply
// IUnitLookup, ICurrencyLookup, IPartyLookup, IRackLookup, ICountMasterLookup etc.)
builder.Services.AddUserManagementInfrastructure(builder.Configuration, builder.Environment);
builder.Services.AddPartyInfrastructure(builder.Configuration, builder.Environment);
builder.Services.AddWarehouseInfrastructure(builder.Configuration, builder.Environment);
builder.Services.AddFinanceInfrastructureServices(builder.Configuration, builder.Environment);
builder.Services.AddProductionInfrastructureServices(builder.Configuration, builder.Environment);
// QC infrastructure supplies IQualityTemplateLookup, consumed by PurchaseManagement's OCREntryQueryRepository.
builder.Services.AddQCInfrastructureServices(builder.Configuration, builder.Environment);
builder.Services.AddMaintenanceInfrastructure(builder.Configuration, builder.Environment);
builder.Services.AddFAMInfrastructure(builder.Configuration, builder.Environment);
// Business module infrastructure (consumers and their command/query repos)
builder.Services.AddPurchaseInfrastructureServices(builder.Configuration, builder.Environment);
builder.Services.AddBudgetInfrastructure(builder.Configuration, builder.Environment);
builder.Services.AddInventoryInfrastructure(builder.Configuration, builder.Environment);
builder.Services.AddProjectInfrastructure(builder.Configuration, builder.Environment);
builder.Services.AddLogisticsInfrastructure(builder.Configuration, builder.Environment);
builder.Services.AddSalesInfrastructureServices(builder.Configuration, builder.Environment);

// ── Finance Application layer — MediatR handlers + AutoMapper profiles ──────
// Required: Sales consumer (ApprovedRejectedConsumer) dispatches Finance commands
// via MediatR (CreateEInvoiceFromSalesCommand → CreateEInvoiceHeader → GenerateIrn → GenerateEwb).
// AddFinanceInfrastructureServices (above) registers repos + services only; handlers + mapper via below.
builder.Services.AddFinanceApplicationServices();

// ── Purchase Application — RTV approval-decision handler only ──────────────
// The Purchase ApprovedRejectedConsumer dispatches ProcessPurchaseReturnApprovalDecisionCommand
// via MediatR. Register ONLY that handler (not the whole assembly) — registering the full
// PurchaseManagement.Application assembly would pull in GRN handlers that depend on services
// not loaded in the Worker (e.g. IGateInwardLookup), failing DI validation on startup.
builder.Services.AddTransient<
    MediatR.IRequestHandler<
        PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Commands.ProcessApprovalDecision.ProcessPurchaseReturnApprovalDecisionCommand,
        bool>,
    PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Commands.ProcessApprovalDecision.ProcessPurchaseReturnApprovalDecisionCommandHandler>();

// ── Hangfire server — BSOFT.Worker handles infrastructure jobs only ───────────
//    BSOFT.Api runs its own Hangfire server for business-domain jobs (e.g. ScheduleWorkOrderJob)
//    because those jobs require module DI (MediatR handlers, repos) loaded only in BSOFT.Api.
//    Queues must NOT overlap with BSOFT.Api's queues ("maintenance-jobs", "default").
builder.Services.AddHangfireServer(options =>
{
    options.ServerName = builder.Configuration["HangfireServer:Server"] ?? "BSOFT-Worker";
    options.Queues = new[]
    {
        "forgot_password_queue",
        "user_unlock_queue",
    };
    // Disable RecurringJobScheduler in Worker — BSOFT.Api owns all recurring jobs
    // (e.g. maintenance-outbox-processor). Without this, Worker's scheduler tries to
    // deserialize MaintenanceOutboxProcessorJob from MaintenanceManagement.Infrastructure
    // which isn't loaded here, causing FileNotFoundException every 15 seconds.
    options.SchedulePollingInterval = TimeSpan.FromHours(24);
});

// ── Outbox polling — replaces Hangfire recurring job for sub-minute granularity ─
// Polls every 15 seconds as a fallback for outbox messages not directly published.
builder.Services.AddHostedService<OutboxPollingHostedService>();

// ── SignalR client – pushes from Worker to the hub hosted in BSOFT.Api ────────
builder.Services.AddSingleton<IWorkerNotificationService, SignalRWorkerNotificationService>();

// ── Override IInAppNotifier: InAppNotifier (from Infrastructure) uses IHubContext<NotificationHub>
//    which silently drops notifications in the Worker (Angular clients connect to BSOFT.Api's hub,
//    not the Worker). WorkerInAppNotifier uses IWorkerNotificationService (SignalR client) instead.
//    Last registration wins in ASP.NET Core DI, so this override must come AFTER AddInfrastructureServices().
builder.Services.AddScoped<IInAppNotifier, WorkerInAppNotifier>();

// ── Build and run ─────────────────────────────────────────────────────────────
var host = builder.Build();
host.RegisterRecurringJobs();
host.Run();
