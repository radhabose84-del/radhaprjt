
using BackgroundService.Application.Notification.GetNotificationDetail.GetNotificationDetailById;
using BackgroundService.Application.Notification.GetNotificationDetail.UpdateNotificationStatus;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BackgroundService.API.Controller.Notification
{
    [Route("api/[controller]")]
    public class NotificationDetailController : ApiControllerBase
    {
        private readonly IMediator _mediator;
        public NotificationDetailController(IMediator mediator)
        : base(mediator)
        {
            _mediator = mediator;
        }
       [HttpGet("detail/{userId}")]
        public async Task<IActionResult> GetNotificationDetailByIdAsync(string userId)
        {
            var data = await Mediator.Send(new GetNotificationDetailByUserId { UserId = userId });

            if (data.Count == 0)
            {
                return Ok(new
                {
                    statusCode = StatusCodes.Status200OK,
                    message = $"No notification records found for UserId {userId}.",
                    data = Array.Empty<GetNotificationDetailDto>()
                });
            }

            return Ok(new
            {
                statusCode = StatusCodes.Status200OK,
                message = $"Fetched {data.Count} notifications for UserId {userId}.",
                data
            });
        }
        [HttpPut]
        public async Task<IActionResult> UpdateAsync(UpdateNotificationStatus updateNotificationDetail)
        {
            await _mediator.Send(updateNotificationDetail);            
            return Ok(new
            {
                message = "Updated successfully.",
                statusCode = StatusCodes.Status200OK
            });                
        }
       
    }
}

