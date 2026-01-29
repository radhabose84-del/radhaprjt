using BackgroundService.Application.Interfaces;
using BackgroundService.Infrastructure.Jobs;
using BackgroundService.Infrastructure.Services;
using Contracts.Events.Notifications;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackgroundService.API.Controllers
{
    [ApiController]
    [Route("api/userhangfirejobs")]
    public class UserHangfireJobsController : ControllerBase
    {
        [HttpPost("user-verification-code-removal")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [AllowAnonymous]
        public IActionResult ScheduleVerificationCodeRemoval([FromBody] ScheduleRemoveCodeCommand command)
        {

            BackgroundJob.Schedule<VerificationCodeCleanupService>(
                job => job.RemoveVerificationCode(command.UserName, command.DelayInMinutes),
               TimeSpan.FromMinutes(1)
           );

            return Ok("Verification code removal scheduled successfully.");
        }

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
