using FinanceManagement.Application.ScheduleIII.Commands.BulkCreateMaster;
using FinanceManagement.Application.ScheduleIII.Commands.BulkUpdateMaster;
using FinanceManagement.Application.ScheduleIII.Commands.CreateMaster;
using FinanceManagement.Application.ScheduleIII.Commands.DeleteMaster;
using FinanceManagement.Application.ScheduleIII.Commands.LockStructure;
using FinanceManagement.Application.ScheduleIII.Commands.ReorderDetail;
using FinanceManagement.Application.ScheduleIII.Commands.UpdateHeader;
using FinanceManagement.Application.ScheduleIII.Commands.UpdateMaster;
using FinanceManagement.Application.ScheduleIII.Queries.GetActivityLog;
using FinanceManagement.Application.ScheduleIII.Queries.Get03BDropdownPreview;
using FinanceManagement.Application.ScheduleIII.Queries.GetLinesAutoComplete;
using FinanceManagement.Application.ScheduleIII.Queries.GetStructure;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.Presentation.Controllers
{
    // Schedule III structure = one header (per token Company+Division) + its detail lines.
    // CompanyId + DivisionId always come from the token.
    [Route("api/finance/[controller]")]
    public class ScheduleIIIHeaderController : ApiControllerBase
    {
        public ScheduleIIIHeaderController(IMediator mediator) : base(mediator) { }

        // ── Structure reads ──────────────────────────────────────────────
        [HttpGet("structure")]
        public async Task<IActionResult> GetStructure()
        {
            var result = await Mediator.Send(new GetStructureQuery());
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        [HttpGet("preview-03b")]
        public async Task<IActionResult> Get03BPreview()
        {
            var result = await Mediator.Send(new Get03BDropdownPreviewQuery());
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        // Autocomplete — the structure's included lines (section + line item), ordered by DisplayOrder.
        [HttpGet("by-name")]
        public async Task<IActionResult> GetLinesAutoComplete([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetLinesAutoCompleteQuery { Term = term });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result.Data });
        }

        [HttpGet("activity-log")]
        public async Task<IActionResult> GetActivityLog(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            // Always scoped to Schedule III activity (EntityName LIKE '%schedule%').
            var result = await Mediator.Send(new GetActivityLogQuery
            {
                EntityName = "schedule",
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

        // ── Lines (ScheduleIIIDetail) ────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> AddLine([FromBody] CreateMasterCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateLine([FromBody] UpdateMasterCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
        }

        // Bulk add many lines in one call.
        [HttpPost("bulk")]
        public async Task<IActionResult> AddLines([FromBody] BulkCreateMasterCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
        }

        // Bulk update many lines in one call.
        [HttpPut("bulk")]
        public async Task<IActionResult> UpdateLines([FromBody] BulkUpdateMasterCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteLine(int id)
        {
            var result = await Mediator.Send(new DeleteMasterCommand(id));
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result, message = "Schedule III line removed successfully." });
        }

        [HttpPost("reorder")]
        public async Task<IActionResult> Reorder([FromBody] ReorderDetailCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
        }

        // ── Header (status / textile split / lock) ───────────────────────
        [HttpPut("header")]
        public async Task<IActionResult> UpdateHeader([FromBody] UpdateHeaderCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
        }

        [HttpPost("lock")]
        public async Task<IActionResult> Lock([FromBody] LockStructureCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
        }
    }
}
