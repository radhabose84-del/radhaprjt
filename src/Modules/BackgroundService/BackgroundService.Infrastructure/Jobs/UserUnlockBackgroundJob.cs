using BackgroundService.Application.Interfaces;

namespace BackgroundService.Infrastructure.Jobs
{
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