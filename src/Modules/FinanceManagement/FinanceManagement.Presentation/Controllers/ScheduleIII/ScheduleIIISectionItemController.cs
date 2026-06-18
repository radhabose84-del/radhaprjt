using FinanceManagement.Application.ScheduleIII.Commands.CreateLineItem;
using FinanceManagement.Application.ScheduleIII.Commands.DeleteLineItem;
using FinanceManagement.Application.ScheduleIII.Commands.UpdateLineItem;
using FinanceManagement.Application.ScheduleIII.Queries.GetAllLineItem;
using FinanceManagement.Application.ScheduleIII.Queries.GetLineItemById;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.Presentation.Controllers
{
    // Global catalog of Schedule III line items.
    [Route("api/finance/[controller]")]
    public class ScheduleIIISectionItemController : ApiControllerBase
    {
        public ScheduleIIISectionItemController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null,
            [FromQuery] int? SectionId = null)
        {
            var result = await Mediator.Send(new GetAllLineItemQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm,
                SectionId = SectionId
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await Mediator.Send(new GetLineItemByIdQuery { Id = id });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateLineItemCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateLineItemCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromQuery] int id)
        {
            var result = await Mediator.Send(new DeleteLineItemCommand(id));
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result, message = result ? "Line item deleted successfully." : "Failed to delete line item." });
        }
    }
}
