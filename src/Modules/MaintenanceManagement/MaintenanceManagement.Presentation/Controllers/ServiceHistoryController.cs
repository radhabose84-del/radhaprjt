using MaintenanceManagement.Application.ServiceHistory.Queries.GetServiceHistory;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MaintenanceManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class ServiceHistoryController : ApiControllerBase
    {
        public ServiceHistoryController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetServiceHistoryAsync(
            [FromQuery] int? MachineId,
            [FromQuery] int? AssetId,
            [FromQuery] DateTimeOffset? FromDate,
            [FromQuery] DateTimeOffset? ToDate,
            [FromQuery] int PageNumber = 1,
            [FromQuery] int PageSize = 10)
        {
            var result = await Mediator.Send(new GetServiceHistoryQuery
            {
                MachineId = MachineId,
                AssetId = AssetId,
                FromDate = FromDate,
                ToDate = ToDate,
                PageNumber = PageNumber,
                PageSize = PageSize
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
