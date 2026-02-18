using BackgroundService.Infrastructure;
using BackgroundService.Application;
using BackgroundService.API.Configurations;
using BackgroundService.API;
using BackgroundService.API.GrpcServices;
using BackgroundService.Application.Interfaces;
using BackgroundService.Infrastructure.Services;
using BackgroundService.API.Validation.Common;
using MediatR;
using BackgroundService.Application.Hubs;
using BackgroundService.API.Middleware;
using Contracts.Common;

var builder = WebApplication.CreateBuilder(args);
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?? "Development";

builder.Configuration
.AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
.AddJsonFile("settings/jwtsetting.json", optional: false, reloadOnChange: true)
.AddEnvironmentVariables();

builder.Host.ConfigureSerilog(builder.Configuration);
// Add validation services
var validationService = new ValidationService();
validationService.AddValidationServices(builder.Services);
// Add services
builder.Services.AddControllers();
builder.Services.AddSwaggerDocumentation();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddCorsPolicy();
builder.Services.AddApplicationServices();
//builder.Services.AddHttpClients(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration, builder.Services);
builder.Services.AddHttpContextAccessor();
builder.Services.AddProblemDetails();
builder.Services.AddMemoryCache();


// Load configuration
builder.Services.AddGrpc();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddHttpContextAccessor();
var app = builder.Build();

// Enable Swagger in Development
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();   
    app.UseDeveloperExceptionPage();
//}

app.UseHttpsRedirection();
app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });
app.UseMiddleware<BackgroundService.Infrastructure.Logging.Middleware.LoggingMiddleware>();
app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseMiddleware<TokenValidationMiddleware>();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapGrpcService<MaintenanceJobGrpcService>().EnableGrpcWeb();
    endpoints.MapGrpcService<MaintenanceHangfireRemoveGrpcService>().EnableGrpcWeb();
    endpoints.MapGrpcService<ApprovalRequestStatusAllGrpcService>().EnableGrpcWeb();
    endpoints.MapGrpcService<ApprovalLineRequestStatusGrpcService>().EnableGrpcWeb();
    endpoints.MapGrpcService<ApproverListGrpcService>().EnableGrpcWeb();
    endpoints.MapControllers();
    endpoints.MapHub<NotificationHub>("/notificationHub");    
});

// app.MapControllers();
app.MapGet("/healthz", () => Results.Ok("OK")).AllowAnonymous();
app.ConfigureHangfireDashboard();
app.Run();
