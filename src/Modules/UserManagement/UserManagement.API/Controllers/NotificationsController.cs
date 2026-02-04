using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Infrastructure.Data;
using UserManagement.Application.Notification.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace UserManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ApiControllerBase
    {
         private readonly NotificationsQueryHandler _NotificationsQueryHandler;
         private readonly IMediator _mediator;
         private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(NotificationsQueryHandler NotificationsQueryHandler, IMediator mediator, ILogger<NotificationsController> logger) 
        : base(mediator)
        {
            _NotificationsQueryHandler = NotificationsQueryHandler; 
            _mediator = mediator; 
            _logger = logger;
        }
        [HttpPost("PasswordResetNotifications")]
        public async Task<IActionResult> PasswordResetNotifications([FromBody] NotificationRequest request)
        {
            var response = await _NotificationsQueryHandler.Handle(request, CancellationToken.None);
           
                _logger.LogInformation("User {Username} Password Reset Notifications information.", request.Username);

                return Ok(new
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Password Reset Notifications information.",
                  
                });
           
        }      
    }
}