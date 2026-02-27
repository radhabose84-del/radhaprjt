using System.Threading.Tasks;
using BackgroundService.Application.Notification.NotificationWhatsAppGroup.Commands.CreateNotificationWhatsAppGroup;
using BackgroundService.Application.Notification.NotificationWhatsAppGroup.Commands.UpdateNotificationWhatsAppGroup;
using BackgroundService.Application.Notification.NotificationWhatsAppGroup.Commands.DeleteNotificationWhatsAppGroup;
using BackgroundService.Application.Notification.NotificationWhatsAppGroup.Queries.GetAllNotificationWhatsAppGroup;
using BackgroundService.Application.Notification.NotificationWhatsAppGroup.Queries.GetNotificationWhatsAppGroupById;
using BackgroundService.Application.Notification.NotificationWhatsAppGroup.Queries.GetNotificationWhatsAppGroupAutoComplete;
using BackgroundService.Application.Notification.NotificationWhatsAppGroup.Queries.GetNotificationWhatsAppGroupByDepartment;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BackgroundService.API.Controller.Notification
{    
    [Route("api/[controller]")]
    public class NotificationWhatsAppGroupController : ApiControllerBase
    {
        private readonly IMediator _mediator;

        public NotificationWhatsAppGroupController(IMediator mediator)
            : base(mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync(
            [FromQuery] int pageNumber,
            [FromQuery] int pageSize,
            [FromQuery] string? searchTerm = null,
            [FromQuery] int? departmentId = null)
        {
            var query = new GetAllNotificationWhatsAppGroupQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                DepartmentId = departmentId
            };

            var (items, totalCount, currentPage, currentSize) = await _mediator.Send(query);

            return Ok(new
            {
                statusCode = StatusCodes.Status200OK,
                data = items,
                totalCount,
                pageNumber = currentPage,
                pageSize = currentSize
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateNotificationWhatsAppGroupCommand command)
        {
            var createdId = await _mediator.Send(command);

            return Ok(new
            {
                StatusCode = StatusCodes.Status201Created,
                message = "Created successfully.",
                IsSuccess = true,
                data = createdId
            });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAsync([FromBody] UpdateNotificationWhatsAppGroupCommand command)
        {
            var result = await _mediator.Send(command);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "Updated successfully.",
                IsSuccess = true,
                data = result
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var result = await _mediator.Send(new DeleteNotificationWhatsAppGroupCommand { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "Deleted successfully.",
                IsSuccess = true,
                data = result
            });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var dto = await _mediator.Send(new GetNotificationWhatsAppGroupByIdQuery { Id = id });

            return Ok(new
            {
                statusCode = StatusCodes.Status200OK,
                data = dto
            });
        }

        [HttpGet("autocomplete")]
        public async Task<IActionResult> AutoCompleteAsync([FromQuery] string? searchTerm)
        {
            var list = await _mediator.Send(new GetNotificationWhatsAppGroupAutoCompleteQuery
            {
                SearchTerm = searchTerm
            });

            return Ok(new
            {
                statusCode = StatusCodes.Status200OK,
                data = list
            });
        }

        [HttpGet("by-department/{departmentId:int}")]
        public async Task<IActionResult> GetByDepartmentAsync(int departmentId, [FromQuery] string? searchTerm)
        {
            var list = await _mediator.Send(new GetNotificationWhatsAppGroupByDepartmentQuery
            {
                DepartmentId = departmentId,
                SearchTerm = searchTerm
            });

            return Ok(new
            {
                statusCode = StatusCodes.Status200OK,
                data = list
            });
        }

    }
}
