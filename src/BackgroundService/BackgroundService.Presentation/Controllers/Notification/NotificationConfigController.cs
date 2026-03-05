
using BackgroundService.Application.Notification.NotificationConfig.Command.CreateNotificationConfig;
using BackgroundService.Application.Notification.NotificationConfig.Command.DeleteNotificationConfig;
using BackgroundService.Application.Notification.NotificationConfig.Command.UpdateNotificationConfig;
using BackgroundService.Application.Notification.NotificationConfig.Queries.GetAllNotificationConfig;
using BackgroundService.Application.Notification.NotificationConfig.Queries.GetNotificationConfigAutoComplete;
using BackgroundService.Application.Notification.NotificationConfig.Queries.GetNotificationConfigById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BackgroundService.Presentation.Controllers.Notification
{
     [Route("api/[controller]")]
    public class NotificationConfigController :  ApiControllerBase
    {        
        private readonly IMediator _mediator;
        public NotificationConfigController(IMediator mediator)
        : base(mediator)
        {            
            _mediator=mediator;
        }        
        [HttpGet]
        public async Task<IActionResult> GetAllNotificationConfigAsync([FromQuery] int PageNumber,[FromQuery] int PageSize,[FromQuery] string? SearchTerm = null)
        {
           var notificationConfig = await Mediator.Send(
            new GetAllNotificationConfigQuery
            {
                PageNumber = PageNumber, 
                PageSize = PageSize, 
                SearchTerm = SearchTerm
            });
            return Ok( new 
            { 
                StatusCode=StatusCodes.Status200OK, 
                data = notificationConfig.Data,
                TotalCount = notificationConfig.TotalCount,
                PageNumber = notificationConfig.PageNumber,
                PageSize = notificationConfig.PageSize
                });
        }
        
        [HttpGet("by-name")]
        public async Task<IActionResult> GetNotificationConfigAutoCompleteAsync([FromQuery] string? ModuleName)
        {
            var notificationConfig = await Mediator.Send(new GetNotificationConfigAutoCompleteQuery 
            { 
                    SearchPattern = ModuleName ?? string.Empty 
            });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = notificationConfig});
        }

        [HttpGet("{id}")]        
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var notificationConfig = await Mediator.Send(new GetNotificationConfigByIdQuery() { Id = id});           
            return Ok(new { StatusCode=StatusCodes.Status200OK, data = notificationConfig,message = notificationConfig });            
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateNotificationConfigCommand createNotificationConfigCommand)
        {            
            var CreatedNotificationId = await _mediator.Send(createNotificationConfigCommand);            
            return Ok(new
            {
                StatusCode = StatusCodes.Status201Created,
                message ="Created successfully.",
                data = CreatedNotificationId
            });            
        
        }
        [HttpPut]
        public async Task<IActionResult> UpdateAsync(UpdateNotificationConfigCommand updateNotificationConfigCommand)
        {
            await _mediator.Send(updateNotificationConfigCommand);            
            return Ok(new
            {
                message = "Updated successfully.",
                statusCode = StatusCodes.Status200OK
            });                
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            await _mediator.Send(new DeleteNotificationConfigCommand { Id = id });
            return Ok(new
            {
                message = "Deleted successfully.",
                statusCode = StatusCodes.Status200OK
            });
        
        }
                
    }
}