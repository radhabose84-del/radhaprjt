using Contracts.Events.Notifications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackgroundService.API.Controller
{
    [Route("api/email")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly IMediator _mediator;

        public EmailController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("send")]
        [AllowAnonymous]
        public async Task<IActionResult> SendEmail([FromBody] SendEmailCommand command)
        {
            var result = await _mediator.Send(command);
            if (result)
                return Ok(new { StatusCode = StatusCodes.Status200OK,Message = "Email sent successfully" });

            return BadRequest(new {StatusCode=StatusCodes.Status400BadRequest,  Message = "Email sending failed" });
        }
    }

}