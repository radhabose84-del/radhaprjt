using QCManagement.Application.AuditLog.Queries.GetAuditLog;
using QCManagement.Application.AuditLog.Queries.GetAuditLogAutoComplete;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace QCManagement.Presentation.Controllers
{
    [Route("api/qc/[controller]")]
    public class AuditLogController : ApiControllerBase
    {
        public AuditLogController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllAuditLogsAsync()
        {
            var result = await Mediator.Send(new GetAuditLogQuery());
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetAuditLogAutoCompleteAsync([FromQuery] string? searchPattern = null)
        {
            var result = await Mediator.Send(new GetAuditLogAutoCompleteQuery { SearchPattern = searchPattern });
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
