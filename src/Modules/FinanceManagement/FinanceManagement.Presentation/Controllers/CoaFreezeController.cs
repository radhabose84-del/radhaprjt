using FinanceManagement.Application.CoaFreeze.Commands.SetCoaFreezeState;
using FinanceManagement.Application.CoaFreeze.Queries.GetCoaFreezeState;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.Presentation.Controllers
{
    // US-GL02-FR-008a — COA freeze engine. Enforcement is the DB triggers (AC1/AC4); this exposes the
    // freeze banner/state (AC2). Governed freeze/unfreeze (dual approval) is US-GL02-08B; set-state here
    // is a TEST/ADMIN hook so the engine is exercisable until 08B lands.
    [Route("api/finance/coa-freeze")]
    public class CoaFreezeController : ApiControllerBase
    {
        public CoaFreezeController(IMediator mediator) : base(mediator) { }

        [HttpGet("state")]
        public async Task<IActionResult> GetStateAsync()
        {
            var result = await Mediator.Send(new GetCoaFreezeStateQuery());
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        // TEST/ADMIN: IsFrozen=true seals; IsFrozen=false opens an unfreeze window. Replaced by 08B's
        // governed dual-approval flow.
        [HttpPost("set-state")]
        public async Task<IActionResult> SetStateAsync([FromBody] SetCoaFreezeStateCommand command)
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
    }
}
