using FinanceManagement.Application.JournalMaster.SecurityViolationLog.Queries.GetAllSecurityViolationLog;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.Presentation.Controllers.JournalMaster
{
    // US-GL01-10 — read-only view of tamper attempts (rows are written by the DB immutability triggers).
    [Route("api/finance/[controller]")]
    public class SecurityViolationLogController : FinanceManagement.Presentation.Controllers.ApiControllerBase
    {
        public SecurityViolationLogController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] int? JournalHeaderId = null)
        {
            var result = await Mediator.Send(new GetAllSecurityViolationLogQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                JournalHeaderId = JournalHeaderId
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
    }
}
