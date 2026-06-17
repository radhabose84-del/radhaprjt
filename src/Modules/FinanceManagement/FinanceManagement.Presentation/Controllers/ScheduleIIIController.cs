using FinanceManagement.Application.ScheduleIII.Commands.CreateLineItem;
using FinanceManagement.Application.ScheduleIII.Commands.CreateScheduleIII;
using FinanceManagement.Application.ScheduleIII.Commands.CreateSubTotal;
using FinanceManagement.Application.ScheduleIII.Commands.DeleteLineItem;
using FinanceManagement.Application.ScheduleIII.Commands.LockStructure;
using FinanceManagement.Application.ScheduleIII.Commands.ReorderLineItem;
using FinanceManagement.Application.ScheduleIII.Commands.UpdateLineItem;
using FinanceManagement.Application.ScheduleIII.Commands.UpdateScheduleIII;
using FinanceManagement.Application.ScheduleIII.Commands.UpdateSubTotal;
using FinanceManagement.Application.ScheduleIII.Queries.GetActivityLog;
using FinanceManagement.Application.ScheduleIII.Queries.GetScheduleIIIById;
using FinanceManagement.Application.ScheduleIII.Queries.Get03BDropdownPreview;
using FinanceManagement.Application.ScheduleIII.Queries.GetLineItemById;
using FinanceManagement.Application.ScheduleIII.Queries.GetStructure;
using FinanceManagement.Application.ScheduleIII.Queries.GetSubTotals;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.Presentation.Controllers
{
    [Route("api/finance/[controller]")]
    public class ScheduleIIIController : ApiControllerBase
    {
        public ScheduleIIIController(IMediator mediator) : base(mediator) { }

        // ── Composite (all five tables in one call) ───────────────────────────

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateScheduleIIICommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateScheduleIIICommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
        }

        [HttpGet("{structureId:int}")]
        public async Task<IActionResult> GetById(int structureId)
        {
            var result = await Mediator.Send(new GetScheduleIIIByIdQuery { Id = structureId });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        [HttpGet("activity-log")]
        public async Task<IActionResult> GetActivityLog(
            [FromQuery] string? entityName = null,
            [FromQuery] int? entityId = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await Mediator.Send(new GetActivityLogQuery
            {
                EntityName = entityName,
                EntityId = entityId,
                PageNumber = pageNumber,
                PageSize = pageSize
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

        [HttpGet("structure")]
        public async Task<IActionResult> GetStructureAsync([FromQuery] int companyId, [FromQuery] int divisionId)
        {
            var result = await Mediator.Send(new GetStructureQuery { CompanyId = companyId, DivisionId = divisionId });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        [HttpGet("preview-03b/{structureId}")]
        public async Task<IActionResult> Get03BDropdownPreviewAsync(int structureId)
        {
            var result = await Mediator.Send(new Get03BDropdownPreviewQuery { StructureId = structureId });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        [HttpGet("subtotals/{structureId}")]
        public async Task<IActionResult> GetSubTotalsAsync(int structureId)
        {
            var result = await Mediator.Send(new GetSubTotalsQuery { StructureId = structureId });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result.Data });
        }

        [HttpGet("line-item/{id}")]
        public async Task<IActionResult> GetLineItemByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetLineItemByIdQuery { Id = id });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        [HttpPost("line-item")]
        public async Task<IActionResult> CreateLineItem([FromBody] CreateLineItemCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
        }

        [HttpPut("line-item")]
        public async Task<IActionResult> UpdateLineItem([FromBody] UpdateLineItemCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
        }

        [HttpDelete("line-item")]
        public async Task<IActionResult> DeleteLineItem([FromQuery] int id)
        {
            var result = await Mediator.Send(new DeleteLineItemCommand(id));
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result, message = result ? "Line item deleted successfully." : "Failed to delete line item." });
        }

        [HttpPost("line-item/reorder")]
        public async Task<IActionResult> ReorderLineItem([FromBody] ReorderLineItemCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
        }

        [HttpPost("subtotal")]
        public async Task<IActionResult> CreateSubTotal([FromBody] CreateSubTotalCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
        }

        [HttpPut("subtotal")]
        public async Task<IActionResult> UpdateSubTotal([FromBody] UpdateSubTotalCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
        }

        [HttpPost("lock")]
        public async Task<IActionResult> LockStructure([FromBody] LockStructureCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
        }
    }
}
