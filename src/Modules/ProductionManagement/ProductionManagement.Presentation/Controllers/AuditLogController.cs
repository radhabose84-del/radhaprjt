using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProductionManagement.Application.AuditLog.Queries.GetAuditLog;
using ProductionManagement.Application.AuditLog.Queries.GetAuditLogAutoComplete;

namespace ProductionManagement.Presentation.Controllers
{
    [Route("api/production/[controller]")]
    public class AuditLogController : ApiControllerBase
    {
        public AuditLogController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllAuditLogsAsync()
        {
            var result = await Mediator.Send(new GetAuditLogQuery());
            return Ok(result);
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetAuditLogAutoCompleteAsync([FromQuery] string? searchPattern = null)
        {
            var result = await Mediator.Send(new GetAuditLogAutoCompleteQuery { SearchPattern = searchPattern });
            return Ok(result);
        }
    }
}
