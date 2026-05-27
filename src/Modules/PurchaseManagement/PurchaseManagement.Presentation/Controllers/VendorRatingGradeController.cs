using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.VendorRatingGrade.Commands.CreateVendorRatingGrade;
using PurchaseManagement.Application.VendorRatingGrade.Commands.DeleteVendorRatingGrade;
using PurchaseManagement.Application.VendorRatingGrade.Commands.UpdateVendorRatingGrade;
using PurchaseManagement.Application.VendorRatingGrade.Queries.GetAllVendorRatingGrade;
using PurchaseManagement.Application.VendorRatingGrade.Queries.GetVendorRatingGradeAutoComplete;
using PurchaseManagement.Application.VendorRatingGrade.Queries.GetVendorRatingGradeById;

namespace PurchaseManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class VendorRatingGradeController : ApiControllerBase
    {
        public VendorRatingGradeController(ISender mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllVendorRatingGradeAsync(
            [FromQuery] int PageNumber, [FromQuery] int PageSize, [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllVendorRatingGradeQuery
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetVendorRatingGradeByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetVendorRatingGradeByIdQuery { Id = id });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetVendorRatingGradeAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetVendorRatingGradeAutoCompleteQuery(term ?? string.Empty));
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        [HttpPost]
        public async Task<IActionResult> CreateVendorRatingGrade([FromBody] CreateVendorRatingGradeCommand command)
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
        public async Task<IActionResult> UpdateVendorRatingGrade([FromBody] UpdateVendorRatingGradeCommand command)
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
        public async Task<IActionResult> DeleteVendorRatingGrade(int id)
        {
            await Mediator.Send(new DeleteVendorRatingGradeCommand(id));
            return Ok(new { StatusCode = StatusCodes.Status200OK, message = "Deleted successfully." });
        }
    }
}
