using FinanceManagement.Application.ScheduleIII.Commands.CreateMaster;
using FinanceManagement.Application.ScheduleIII.Commands.LockStructure;
using FinanceManagement.Application.ScheduleIII.Commands.UpdateMaster;
using FinanceManagement.Application.ScheduleIII.Queries.GetActivityLog;
using FinanceManagement.Application.ScheduleIII.Queries.Get03BDropdownPreview;
using FinanceManagement.Application.ScheduleIII.Queries.GetStructure;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.Presentation.Controllers
{
    // The Schedule III master (statement structure/version). CompanyId + DivisionId come from the token.
    [Route("api/finance/[controller]")]
    public class ScheduleIIIMasterController : ApiControllerBase
    {
        public ScheduleIIIMasterController(IMediator mediator) : base(mediator) { }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateMasterCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateMasterCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
        }

        [HttpGet("structure")]
        public async Task<IActionResult> GetStructure()
        {
            var result = await Mediator.Send(new GetStructureQuery());
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        [HttpGet("preview-03b/{scheduleIIIMasterId}")]
        public async Task<IActionResult> Get03BPreview(int scheduleIIIMasterId)
        {
            var result = await Mediator.Send(new Get03BDropdownPreviewQuery { ScheduleIIIMasterId = scheduleIIIMasterId });
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

        [HttpPost("lock")]
        public async Task<IActionResult> Lock([FromBody] LockStructureCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
        }
    }
}
