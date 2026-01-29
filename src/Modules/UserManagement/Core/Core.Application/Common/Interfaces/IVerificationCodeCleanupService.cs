
namespace Core.Application.Common.Interfaces
{
    public interface IBackgroundServiceClient
    {
        Task ScheduleVerificationCodeCleanupAsync(string userName, int delayInMinutes);
        Task UserUnlock(string userName, int delayInMinutes);
        
    }
}