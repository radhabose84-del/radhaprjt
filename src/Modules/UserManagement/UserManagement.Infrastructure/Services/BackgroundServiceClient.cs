using BackgroundService.Application.Interfaces;
using Hangfire;
using UserManagement.Application.Common.Interfaces;

namespace UserManagement.Infrastructure.Services
{
    public class BackgroundServiceClient : IBackgroundServiceClient
    {
        public Task UserUnlock(string userName, int delayInMinutes)
        {
            BackgroundJob.Schedule<IUserUnlockBackgroundJob>(
                job => job.Execute(userName),
                TimeSpan.FromMinutes(delayInMinutes));

            return Task.CompletedTask;
        }

        public Task ScheduleVerificationCodeCleanupAsync(string userName, int delayInMinutes)
        {
            // ForgotPasswordCache already stores ExpiryTime on each entry.
            // The verification code check rejects expired codes at the point of use,
            // so dictionary cleanup is memory hygiene only — not a security requirement.
            // No remote call or Hangfire job needed.
            return Task.CompletedTask;
        }
    }
}
