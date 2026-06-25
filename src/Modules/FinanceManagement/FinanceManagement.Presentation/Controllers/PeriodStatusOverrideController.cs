using FinanceManagement.Application.PeriodStatusOverride.Commands.ApprovePeriodReversal;
using FinanceManagement.Application.PeriodStatusOverride.Commands.RejectPeriodReversal;
using FinanceManagement.Application.PeriodStatusOverride.Commands.RequestPeriodReversal;
using FinanceManagement.Application.PeriodStatusOverride.Queries.GetPendingPeriodReversals;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.Presentation.Controllers
{
    [Route("api/finance/[controller]")]
    public class PeriodStatusOverrideController : ApiControllerBase
    {
        public PeriodStatusOverrideController(IMediator mediator) : base(mediator) { }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingAsync()
        {
            var result = await Mediator.Send(new GetPendingPeriodReversalsQuery());
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost("request")]
        public async Task<IActionResult> RequestAsync([FromBody] RequestPeriodReversalCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result.IsSuccess,
                message = result.Message,
                data = result.Data
            });
        }

        [HttpPost("{overrideId:int}/approve")]
        public async Task<IActionResult> ApproveAsync(int overrideId, [FromBody] ApprovePeriodReversalCommand command)
        {
            command.OverrideId = overrideId;
            var result = await Mediator.Send(command);
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result.IsSuccess,
                message = result.Message,
                data = result.Data
            });
        }

        [HttpPost("{overrideId:int}/reject")]
        public async Task<IActionResult> RejectAsync(int overrideId, [FromBody] RejectPeriodReversalCommand command)
        {
            command.OverrideId = overrideId;
            var result = await Mediator.Send(command);
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result.IsSuccess,
                message = result.Message,
                data = result.Data
            });
        }
    }
}
