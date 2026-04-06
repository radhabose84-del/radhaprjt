using LogisticsManagement.Application.AuditLog.Queries;
using LogisticsManagement.Application.AuditLog.Queries.GetAuditLog;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace LogisticsManagement.Presentation.Controllers
{
    [Route("api/logistics/[controller]")]
    public class AuditLogController : ApiControllerBase
    {
        public AuditLogController(ISender mediator)
            : base(mediator)
        {
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAuditLogsAsync()
        {
            var auditLogs = await Mediator.Send(new GetAuditLogQuery());
            return Ok(auditLogs);
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetAuditLogAutoCompleteAsync([FromQuery] string? searchPattern = null)
        {
            var result = await Mediator.Send(new GetAuditLogBySearchPatternQuery { SearchPattern = searchPattern });
            return Ok(result);
        }
    }
}
