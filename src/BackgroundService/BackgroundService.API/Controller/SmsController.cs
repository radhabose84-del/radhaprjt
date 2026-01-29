using Contracts.Events.Notifications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackgroundService.API.Controller
{
    [Route("api/sms")]
    [ApiController]
    public class SmsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SmsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("send")]
        [AllowAnonymous]
        public async Task<IActionResult> SendSms([FromBody] SendSmsCommand command)
        {
            var result = await _mediator.Send(command);
            if (result)
                return Ok(new { Message = "sms sent successfully" });

            return BadRequest(new { Message = "sms sending failed" });
        }
    }

}