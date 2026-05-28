using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.VendorEvaluationHeader.Commands.CreateVendorEvaluationHeader;
using PurchaseManagement.Application.VendorEvaluationHeader.Commands.DeleteVendorEvaluationHeader;
using PurchaseManagement.Application.VendorEvaluationHeader.Commands.UpdateVendorEvaluationHeader;
using PurchaseManagement.Application.VendorEvaluationHeader.Queries.GetAllVendorEvaluationHeader;
using PurchaseManagement.Application.VendorEvaluationHeader.Queries.GetVendorEvaluationDashboard;
using PurchaseManagement.Application.VendorEvaluationHeader.Queries.GetVendorEvaluationHeaderById;

namespace PurchaseManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class VendorEvaluationHeaderController : ApiControllerBase
    {
        public VendorEvaluationHeaderController(ISender mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllVendorEvaluationHeaderAsync(
            [FromQuery] int PageNumber, [FromQuery] int PageSize, [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllVendorEvaluationHeaderQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm
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

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetVendorEvaluationDashboardAsync(
            [FromQuery] int vendorId,
            [FromQuery] int evaluationMonth,
            [FromQuery] int evaluationYear,
            [FromQuery] int lookbackMonths = 3)
        {
            var result = await Mediator.Send(new GetVendorEvaluationDashboardQuery
            {
                VendorId = vendorId,
                EvaluationMonth = evaluationMonth,
                EvaluationYear = evaluationYear,
                LookbackMonths = lookbackMonths
            });

            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetVendorEvaluationHeaderByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetVendorEvaluationHeaderByIdQuery { Id = id });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        [HttpPost]
        public async Task<IActionResult> CreateVendorEvaluationHeader([FromBody] CreateVendorEvaluationHeaderCommand command)
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

        [HttpPut]
        public async Task<IActionResult> UpdateVendorEvaluationHeader([FromBody] UpdateVendorEvaluationHeaderCommand command)
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVendorEvaluationHeader(int id)
        {
            await Mediator.Send(new DeleteVendorEvaluationHeaderCommand(id));
            return Ok(new { StatusCode = StatusCodes.Status200OK, message = "Deleted successfully." });
        }
    }
}
