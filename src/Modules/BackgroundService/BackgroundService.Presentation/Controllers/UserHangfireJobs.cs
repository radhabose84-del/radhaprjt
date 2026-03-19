using BackgroundService.Infrastructure.Services;
using Contracts.Events.Notifications;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackgroundService.Presentation.Controllerss
{
    [ApiController]
    [Route("api/userhangfirejobs")]
    public class UserHangfireJobsController : ControllerBase
    {
        [HttpPost("user-unlock")]
        [AllowAnonymous]
        public IActionResult ScheduleUserUnlock([FromBody] ScheduleRemoveCodeCommand command)
        {
            BackgroundJob.Schedule<UserUnlockService>(
                 job => job.UnlockUser(command.UserName),
                TimeSpan.FromMinutes(1)
            );
            return Ok("User unlock scheduled successfully.");
        }
    }
}
