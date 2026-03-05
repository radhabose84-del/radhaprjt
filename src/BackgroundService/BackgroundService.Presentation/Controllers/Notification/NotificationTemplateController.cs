
using BackgroundService.Application.Notification.NotificationTemplate.Command.CreateNotificationTemplate;
using BackgroundService.Application.Notification.NotificationTemplate.Command.DeleteNotificationTemplate;
using BackgroundService.Application.Notification.NotificationTemplate.Command.UpdateNotificationTemplate;
using BackgroundService.Application.Notification.NotificationTemplate.Queries.GetAllNotificationTemplate;
using BackgroundService.Application.Notification.NotificationTemplate.Queries.GetNotificationTemplateAutoComplete;
using BackgroundService.Application.Notification.NotificationTemplate.Queries.GetNotificationTemplateById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BackgroundService.Presentation.Controllers.Notification
{
     [Route("api/[controller]")]
    public class NotificationTemplateController :  ApiControllerBase
    {        
        private readonly IMediator _mediator;
        public NotificationTemplateController(IMediator mediator)
        : base(mediator)
        {            
            _mediator=mediator;
        }        
        [HttpGet]
        public async Task<IActionResult> GetAllNotificationTemplateAsync([FromQuery] int PageNumber,[FromQuery] int PageSize,[FromQuery] string? SearchTerm = null)
        {
           var NotificationTemplate = await Mediator.Send(
            new GetAllNotificationTemplateQuery
            {
                PageNumber = PageNumber, 
                PageSize = PageSize, 
                SearchTerm = SearchTerm
            });
            return Ok( new 
            { 
                StatusCode=StatusCodes.Status200OK, 
                data = NotificationTemplate.Data,
                TotalCount = NotificationTemplate.TotalCount,
                PageNumber = NotificationTemplate.PageNumber,
                PageSize = NotificationTemplate.PageSize
                });
        }
        
        [HttpGet("by-name")]
        public async Task<IActionResult> GetNotificationTemplateAutoCompleteAsync([FromQuery] string? ModuleName)
        {
            var NotificationTemplate = await Mediator.Send(new GetNotificationTemplateAutoCompleteQuery 
            { 
                    SearchPattern = ModuleName ?? string.Empty 
            });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = NotificationTemplate});
        }

        [HttpGet("{id}")]        
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var NotificationTemplate = await Mediator.Send(new GetNotificationTemplateByIdQuery() { Id = id});           
            return Ok(new { StatusCode=StatusCodes.Status200OK, data = NotificationTemplate,message = NotificationTemplate });            
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateNotificationTemplateCommand createNotificationTemplateCommand)
        {            
            var CreatedNotificationId = await _mediator.Send(createNotificationTemplateCommand);            
            return Ok(new
            {
                StatusCode = StatusCodes.Status201Created,
                message ="Created successfully.",
                data = CreatedNotificationId
            });            
        
        }
        [HttpPut]
        public async Task<IActionResult> UpdateAsync(UpdateNotificationTemplateCommand updateNotificationTemplateCommand)
        {
            await _mediator.Send(updateNotificationTemplateCommand);            
            return Ok(new
            {
                message = "Updated successfully.",
                statusCode = StatusCodes.Status200OK
            });                
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            await _mediator.Send(new DeleteNotificationTemplateCommand { Id = id });
            return Ok(new
            {
                message = "Deleted successfully.",
                statusCode = StatusCodes.Status200OK
            });
        
        }
                
    }
}