using Hangfire;
using MaintenanceManagement.Application.Common.Interfaces.IBackgroundService;
using MaintenanceManagement.Infrastructure.Jobs;

namespace MaintenanceManagement.Infrastructure.Services
{
    internal sealed class BackgroundServiceClient : IBackgroundServiceClient
    {
        private readonly IBackgroundJobClient _backgroundJobClient;

        public BackgroundServiceClient(IBackgroundJobClient backgroundJobClient)
        {
            _backgroundJobClient = backgroundJobClient;
        }

        public Task<string> ScheduleWorkOrder(int preventiveScheduleDetailId, int delayInMinutes, string? token)
        {
            var jobId = _backgroundJobClient.Schedule<ScheduleWorkOrderJob>(
                job => job.ExecuteAsync(preventiveScheduleDetailId),
                TimeSpan.FromMinutes(delayInMinutes));

            return Task.FromResult(jobId);
        }

        public Task RemoveHangFireJob(string jobId, string? token)
        {
            _backgroundJobClient.Delete(jobId);
            return Task.CompletedTask;
        }
    }
}
