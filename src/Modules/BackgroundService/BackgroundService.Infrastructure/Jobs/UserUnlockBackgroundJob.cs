using BackgroundService.Application.Interfaces;
using Hangfire;

namespace BackgroundService.Infrastructure.Jobs
{
    [Queue("user_unlock_queue")]
    public class UserUnlockBackgroundJob : IUserUnlockBackgroundJob
    {
        private readonly IUserUnlockService _userUnlockService;

        public UserUnlockBackgroundJob(IUserUnlockService userUnlockService)
        {
            _userUnlockService = userUnlockService;
        }

        public async Task Execute(string userName)
        {
            await _userUnlockService.UnlockUser(userName);
        }
    }
}