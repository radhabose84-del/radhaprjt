using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Application.Notification.NotificationGroup.Commands.CreateNotificationGroup;
using BackgroundService.Application.Notification.NotificationGroup.Commands.DeleteNotificationGroup;
using BackgroundService.Application.Notification.NotificationGroup.Commands.UpdateNotificationGroup;
using BackgroundService.Application.Notification.NotificationGroup.Queries.GetAllNotificationGroup;
using BackgroundService.Application.Notification.NotificationGroup.Queries.GetNotificationGroupAutoComplete;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BackgroundService.API.Controller.Notification
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationGroupController : ApiControllerBase
    {
        private readonly IMediator _mediator;
        public NotificationGroupController(IMediator mediator)
        : base(mediator)
        {
            _mediator = mediator;
        }
         [HttpGet]
        public async Task<IActionResult> GetAllNotificationGroupAsync([FromQuery] int PageNumber,[FromQuery] int PageSize,[FromQuery] string? SearchTerm = null)
        {
           var Notification = await Mediator.Send(
            new GetAllNotificationGroupQuery
            {
                PageNumber = PageNumber, 
                PageSize = PageSize, 
                SearchTerm = SearchTerm
            });
            return Ok( new 
            { 
                StatusCode=StatusCodes.Status200OK, 
                data = Notification.Data,
                TotalCount = Notification.TotalCount,
                PageNumber = Notification.PageNumber,
                PageSize = Notification.PageSize
                });
        }
        
        [HttpGet("by-name")]
        public async Task<IActionResult> GetNotificationGroupAutoCompleteAsync([FromQuery] string? ModuleName)
        {
            var Notification = await Mediator.Send(new GetNotificationGroupAutoCompleteQuery 
            { 
                    SearchPattern = ModuleName ?? string.Empty 
            });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = Notification});
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateNotificationGroupCommand createNotificationGroupCommand)
        {            
            var CreatedNotificationId = await _mediator.Send(createNotificationGroupCommand);            
            return Ok(new
            {
                StatusCode = StatusCodes.Status201Created,
                message ="Created successfully.",
                data = CreatedNotificationId
            });            
        
        }
        [HttpPut]
        public async Task<IActionResult> UpdateAsync(UpdateNotificationGroupCommand updateNotificationGroupCommand)
        {
            await _mediator.Send(updateNotificationGroupCommand);            
            return Ok(new
            {
                message = "Updated successfully.",
                statusCode = StatusCodes.Status200OK
            });                
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            await _mediator.Send(new DeleteNotificationGroupCommand { Id = id });
            return Ok(new
            {
                message = "Deleted successfully.",
                statusCode = StatusCodes.Status200OK
            });
        
        }
    }
}