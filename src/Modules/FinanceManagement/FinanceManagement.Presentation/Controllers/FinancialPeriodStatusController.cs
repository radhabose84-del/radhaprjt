using FinanceManagement.Application.PeriodStatusOverride.Commands.TransitionPeriodToHardClosed;
using FinanceManagement.Application.PeriodStatusOverride.Commands.TransitionPeriodToSoftClosed;
using FinanceManagement.Application.PeriodStatusOverride.Queries.GetFinancialPeriodStatus;
using FinanceManagement.Application.PeriodStatusOverride.Queries.GetPeriodStatusHistory;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.Presentation.Controllers
{
    [Route("api/finance/[controller]")]
    public class FinancialPeriodStatusController : ApiControllerBase
    {
        public FinancialPeriodStatusController(IMediator mediator) : base(mediator) { }

        // AC#4 — current period status, consumed by every journal entry screen.
        [HttpGet("{periodId:int}")]
        public async Task<IActionResult> GetStatusAsync(int periodId)
        {
            var result = await Mediator.Send(new GetFinancialPeriodStatusQuery(periodId));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("{periodId:int}/history")]
        public async Task<IActionResult> GetHistoryAsync(int periodId)
        {
            var result = await Mediator.Send(new GetPeriodStatusHistoryQuery(periodId));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost("{periodId:int}/soft-close")]
        public async Task<IActionResult> SoftCloseAsync(int periodId)
        {
            var result = await Mediator.Send(new TransitionPeriodToSoftClosedCommand(periodId));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result.IsSuccess,
                message = result.Message,
                data = result.Data
            });
        }

        [HttpPost("{periodId:int}/hard-close")]
        public async Task<IActionResult> HardCloseAsync(int periodId)
        {
            var result = await Mediator.Send(new TransitionPeriodToHardClosedCommand(periodId));
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
