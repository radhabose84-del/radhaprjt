using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.Reports.SalesProjection.Queries.GetSalesProjection;

namespace SalesManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class SalesProjectionController : ApiControllerBase
    {
        public SalesProjectionController(ISender mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetSalesProjectionAsync(
            [FromQuery] ProjectionPeriodType PeriodType = ProjectionPeriodType.Monthly,
            [FromQuery] DateOnly? DateFrom = null,
            [FromQuery] DateOnly? DateTo = null)
        {
            var result = await Mediator.Send(new GetSalesProjectionQuery
            {
                PeriodType = PeriodType,
                DateFrom = DateFrom,
                DateTo = DateTo
            });

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
