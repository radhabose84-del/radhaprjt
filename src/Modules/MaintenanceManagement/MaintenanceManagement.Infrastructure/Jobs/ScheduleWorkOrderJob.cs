using Hangfire;
using MaintenanceManagement.Application.PreventiveSchedulers.Commands.ScheduleWorkOrder;
using MediatR;

namespace MaintenanceManagement.Infrastructure.Jobs
{
    /// <summary>
    /// Hangfire job executed by BSOFT.Api's Hangfire server.
    /// Runs in-process: all MaintenanceManagement DI (MediatR handlers, repos) is available
    /// because BSOFT.Api loads AddMaintenanceManagementModule() at startup.
    /// BSOFT.Worker does NOT reference this assembly — it only handles infrastructure queues
    /// (forgot_password, user_unlock, sql-outbox). Maintenance jobs run on BSOFT.Api.
    /// </summary>
    public class ScheduleWorkOrderJob
    {
        private readonly ISender _sender;

        public ScheduleWorkOrderJob(ISender sender)
        {
            _sender = sender;
        }

        [Queue("maintenance-jobs")]
        public async Task ExecuteAsync(int preventiveScheduleDetailId)
        {
            await _sender.Send(new ScheduleWorkOrderCommand { PreventiveScheduleId = preventiveScheduleDetailId });
        }
    }
}
