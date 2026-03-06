
namespace BackgroundService.Application.Interfaces
{
    public interface IVerificationCodeCleanupService
    {
        Task RemoveVerificationCode(string userName, int delayInMinutes);
    }
}