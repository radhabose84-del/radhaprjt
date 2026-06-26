using FinanceManagement.Application.ScheduleIII.Commands.CreateSection;
using FinanceManagement.Application.ScheduleIII.Commands.ReorderSection;
using FinanceManagement.Application.ScheduleIII.Commands.UpdateSection;
using FinanceManagement.Application.ScheduleIII.Queries.GetAllSection;
using FinanceManagement.Application.ScheduleIII.Queries.GetSectionById;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.Presentation.Controllers
{
    // Standalone master for Schedule III sections (separate from ScheduleIIIController).
    [Route("api/finance/[controller]")]
    public class ScheduleIIISectionController : ApiControllerBase
    {
        public ScheduleIIISectionController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null,
            [FromQuery] int? ScheduleIIIMasterId = null)
        {
            var result = await Mediator.Send(new GetAllSectionQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm,
                ScheduleIIIMasterId = ScheduleIIIMasterId
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
            var result = await Mediator.Send(new GetSectionByIdQuery { Id = id });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSectionCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateSectionCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
        }

        // Move a section up (Direction = 1) or down (Direction = -1) within its statement type.
        [HttpPost("reorder")]
        public async Task<IActionResult> Reorder([FromBody] ReorderSectionCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
        }
    }
}
