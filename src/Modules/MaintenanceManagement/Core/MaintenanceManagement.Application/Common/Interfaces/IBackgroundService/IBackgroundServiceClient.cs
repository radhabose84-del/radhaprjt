namespace MaintenanceManagement.Application.Common.Interfaces.IBackgroundService
{
    public interface IBackgroundServiceClient
    {
        Task<string> ScheduleWorkOrder(int preventiveScheduleDetailId, int delayInMinutes, string? token);
        Task RemoveHangFireJob(string jobId, string? token);
    }
}
