using BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.Commands.UpdateNotificationEventRule;
using BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.Queries.DeleteNotificationEventRule;
using BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.Queries.GetAllNotificationHierarchy;
using BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.Queries.GetNotificationHierarchyById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BackgroundService.Presentation.Controllers.Notification
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationEventRuleController : ApiControllerBase
    {
        private readonly IMediator _mediator;
        public NotificationEventRuleController(IMediator mediator)
        : base(mediator)
        {
            _mediator = mediator;
        }
         /// ✅ Get All
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] GetAllNotificationHierarchyQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(new { StatusCode=StatusCodes.Status200OK, data = result,message = result });            
        }

        /// ✅ Get By ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _mediator.Send(new GetNotificationHierarchyByIdQuery { Id = id });
            if (result == null)
                return NotFound(new { message = "Record not found" });
           
            return Ok(new { StatusCode=StatusCodes.Status200OK, data = result,message = result });            
        }

        /// ✅ Insert
        [HttpPost]
        public async Task<IActionResult> Insert([FromBody] NotificationHierarchyAndEventRuleDto dto)
        {
            var result = await _mediator.Send(new InsertNotificationHierarchyAndEventRuleCommand(dto));
            //return Ok(new { message = "Inserted successfully", success = result });
            return Ok(new { StatusCode = StatusCodes.Status200OK, message = "Inserted successfully", data = result});
        }

        /// ✅ Update
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateNotificationHierarchyAndEventRuleCommand command)
        {
            var result = await _mediator.Send(command);            
             return Ok(new
            {
                message = "Updated successfully.",
                statusCode = StatusCodes.Status200OK
            });             
        }

        /// ✅ Delete
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _mediator.Send(new DeleteNotificationLevelHierarchyCommand { Id = id });
            return Ok(new { message = "Deleted successfully", success = result ,statusCode = StatusCodes.Status200OK});
        }
        
    }
}