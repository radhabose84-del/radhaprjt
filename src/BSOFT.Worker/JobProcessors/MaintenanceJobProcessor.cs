#nullable disable
using BackgroundService.Application.Interfaces;

namespace BSOFT.Worker.JobProcessors;

/// <summary>
/// Thin wrapper invoked by Hangfire to process maintenance-related background jobs.
/// Delegates actual business logic to <see cref="IMaintenance"/>.
/// </summary>
public class MaintenanceJobProcessor
{
    private readonly IMaintenance _maintenanceService;
    private readonly ILogger<MaintenanceJobProcessor> _logger;

    public MaintenanceJobProcessor(IMaintenance maintenanceService, ILogger<MaintenanceJobProcessor> logger)
    {
        _maintenanceService = maintenanceService;
        _logger = logger;
    }
}
