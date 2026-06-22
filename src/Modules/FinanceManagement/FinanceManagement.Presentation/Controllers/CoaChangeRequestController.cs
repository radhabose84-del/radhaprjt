using FinanceManagement.Application.CoaChangeRequest.Commands.ApproveCoaChangeImpact;
using FinanceManagement.Application.CoaChangeRequest.Commands.ApproveCoaUnfreeze;
using FinanceManagement.Application.CoaChangeRequest.Commands.CreateCoaChangeRequest;
using FinanceManagement.Application.CoaChangeRequest.Commands.CreateCoaUnfreezeRequest;
using FinanceManagement.Application.CoaChangeRequest.Commands.SealCoa;
using FinanceManagement.Application.CoaChangeRequest.Queries.GetCoaChangeRequests;
using FinanceManagement.Application.CoaChangeRequest.Queries.GetCoaUnfreezeRequestById;
using FinanceManagement.Application.CoaChangeRequest.Queries.GetPostFreezeChangeLog;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.Presentation.Controllers
{
    // US-GL02-08B — COA change-request + dual-approval unfreeze workflow that drives the 08a freeze state.
    [Route("api/finance/coa-change-request")]
    public class CoaChangeRequestController : ApiControllerBase
    {
        public CoaChangeRequestController(IMediator mediator) : base(mediator) { }

        // ── Change requests ───────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetAllAsync(
            [FromQuery] string? status = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await Mediator.Send(new GetCoaChangeRequestsQuery
            {
                Status = status,
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

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreateCoaChangeRequestCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
        }

        // AC5 — CFO approves the impact assessment.
        [HttpPost("approve-impact")]
        public async Task<IActionResult> ApproveImpactAsync([FromBody] ApproveCoaChangeImpactCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
        }

        // AC3 — the post-freeze change log report.
        [HttpGet("post-freeze-log")]
        public async Task<IActionResult> GetPostFreezeLogAsync()
        {
            var result = await Mediator.Send(new GetPostFreezeChangeLogQuery());
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        // ── Governed seal (gap G1) ────────────────────────────────────────────
        [HttpPost("seal")]
        public async Task<IActionResult> SealAsync()
        {
            var result = await Mediator.Send(new SealCoaCommand());
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
        }

        // ── Dual-approval unfreeze ────────────────────────────────────────────
        [HttpPost("unfreeze")]
        public async Task<IActionResult> CreateUnfreezeAsync([FromBody] CreateCoaUnfreezeRequestCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
        }

        // AC1/AC2 — record one approval; the second distinct approval opens the window + alerts.
        [HttpPost("unfreeze/approve")]
        public async Task<IActionResult> ApproveUnfreezeAsync([FromBody] ApproveCoaUnfreezeCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
        }

        [HttpGet("unfreeze/{id}")]
        public async Task<IActionResult> GetUnfreezeByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetCoaUnfreezeRequestByIdQuery { Id = id });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }
    }
}
