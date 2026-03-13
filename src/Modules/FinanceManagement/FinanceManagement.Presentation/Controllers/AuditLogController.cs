using FinanceManagement.Application.AuditLog.Queries.GetAuditLog;
using FinanceManagement.Application.AuditLog.Queries.GetAuditLogBySearchPattern;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.Presentation.Controllers
{
    [Route("api/finance/[controller]")]
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

        [HttpGet("search")]
        public async Task<IActionResult> GetAuditLogBySearchPatternAsync([FromQuery] string? searchPattern = null)
        {
            var result = await Mediator.Send(new GetAuditLogBySearchPatternQuery { SearchPattern = searchPattern });
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
