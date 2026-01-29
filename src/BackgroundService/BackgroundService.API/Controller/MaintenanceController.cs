using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Mvc;

namespace BackgroundService.API.Controller
{
    [ApiController]
    [Route("api/maintenance")]
    public class MaintenanceController : ControllerBase
    {
        [HttpPost("delete-hangfirejob")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult ScheduleVerificationCodeRemoval([FromBody] string HangfireJobId)
        {
           
              if (!string.IsNullOrEmpty(HangfireJobId))
                     {
                         BackgroundJob.Delete(HangfireJobId); 
                     }
            return Ok(true);
        }
        
    }
}