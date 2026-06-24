using FinanceManagement.Application.AccountAuditTrail.Queries.ExportAccountAudit;
using FinanceManagement.Application.AccountAuditTrail.Queries.GetAccountAuditHistory;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.Presentation.Controllers
{
    // US-GL02-09 — read-only viewer + export over the immutable Finance.AccountAuditTrail.
    // No write endpoints: rows are written only by AccountAuditTrailSaveChangesInterceptor, in the
    // same transaction as the change. Company scope comes from the token.
    [Route("api/finance/account-audit")]
    public class AccountAuditTrailController : ApiControllerBase
    {
        public AccountAuditTrailController(IMediator mediator) : base(mediator) { }

        // AC-3 — per-account, field-level change history in chronological order.
        [HttpGet("{entityName}/{entityId:int}")]
        public async Task<IActionResult> GetHistoryAsync(string entityName, int entityId)
        {
            var result = await Mediator.Send(new GetAccountAuditHistoryQuery(entityName, entityId));
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        // AC-4 — export for a date range with a tamper-evident record-count checksum.
        [HttpGet("export")]
        public async Task<IActionResult> ExportAsync(
            [FromQuery] DateTimeOffset from,
            [FromQuery] DateTimeOffset to,
            [FromQuery] string? entityName = null)
        {
            var result = await Mediator.Send(new ExportAccountAuditQuery(from, to, entityName));
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }
    }
}
