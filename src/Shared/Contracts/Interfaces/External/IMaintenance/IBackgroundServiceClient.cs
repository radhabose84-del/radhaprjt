namespace Contracts.Interfaces.External.IMaintenance
{
    public interface IBackgroundServiceClient
    {
        Task<string> ScheduleWorkOrder(int preventiveScheduleId, int delayInMinutes,string token);
        Task<bool> RemoveHangFireJob(string HangfireJobId,string token);
    }
}