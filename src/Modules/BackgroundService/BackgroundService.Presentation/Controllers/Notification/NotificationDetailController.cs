
using BackgroundService.Application.Notification.GetNotificationDetail.GetNotificationDetailById;
using BackgroundService.Application.Notification.GetNotificationDetail.UpdateNotificationStatus;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BackgroundService.Presentation.Controllers.Notification
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
        public async Task<IActionResult> GetNotificationDetailByIdAsync(
            string userId,
            [FromQuery] int PageNumber = 1,
            [FromQuery] int PageSize = 10,
            [FromQuery] DateTimeOffset? FromDate = null,
            [FromQuery] DateTimeOffset? ToDate = null,
            [FromQuery] string? ReadStatus = null)
        {
            var result = await Mediator.Send(new GetNotificationDetailByUserId
            {
                UserId = userId,
                PageNumber = PageNumber,
                PageSize = PageSize,
                FromDate = FromDate,
                ToDate = ToDate,
                ReadStatus = ReadStatus
            });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result.Data,
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
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

