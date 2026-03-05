#nullable disable
using BackgroundService.Application;
using BackgroundService.Application.Interfaces.Notification;
using BackgroundService.Infrastructure;
using BSOFT.Worker.Configurations;
using BSOFT.Worker.Services;
using Hangfire;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

// ── Configuration ────────────────────────────────────────────────────────────
var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development";

builder.Configuration
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    .AddJsonFile("settings/serilogsetting.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// ── Serilog ──────────────────────────────────────────────────────────────────
builder.Services.AddSerilog(lc => lc
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Hangfire", Serilog.Events.LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/bsoft-worker-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30));

// ── Windows Service ──────────────────────────────────────────────────────────
builder.Services.AddWindowsService(options =>
    options.ServiceName = "BSOFT Worker Service");

// ── Core infrastructure ──────────────────────────────────────────────────────
builder.Services.AddHttpContextAccessor();   // required by IPAddressService
builder.Services.AddMemoryCache();

// ── Application layer: MediatR + AutoMapper + SignalR server (for IHubContext<>) ──
builder.Services.AddApplicationServices();

// ── Infrastructure layer: Hangfire + MassTransit + all repositories ───────────
builder.Services.AddInfrastructureServices(builder.Configuration, builder.Services);

// ── Hangfire server — BSOFT.Worker is the sole job executor ──────────────────
//    BSOFT.Api only uses Hangfire for the dashboard + job enqueueing (no server there).
builder.Services.AddHangfireServer(options =>
{
    options.ServerName = builder.Configuration["HangfireServer:Server"] ?? "BSOFT-Worker";
    options.Queues = new[]
    {
        "schedule_work_order_queue",
        "forgot_password_queue",
        "user_unlock_queue",
        "sql-outbox-queue",    // SqlOutboxProcessorJob — polls purchase/maintenance outbox tables
    };
});

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
