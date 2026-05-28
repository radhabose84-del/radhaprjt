using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.VendorEvaluationCriteria.Commands.CreateVendorEvaluationCriteria;
using PurchaseManagement.Application.VendorEvaluationCriteria.Commands.DeleteVendorEvaluationCriteria;
using PurchaseManagement.Application.VendorEvaluationCriteria.Commands.UpdateVendorEvaluationCriteria;
using PurchaseManagement.Application.VendorEvaluationCriteria.Queries.GetAllVendorEvaluationCriteria;
using PurchaseManagement.Application.VendorEvaluationCriteria.Queries.GetVendorEvaluationCriteriaAutoComplete;
using PurchaseManagement.Application.VendorEvaluationCriteria.Queries.GetVendorEvaluationCriteriaById;

namespace PurchaseManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class VendorEvaluationCriteriaController : ApiControllerBase
    {
        public VendorEvaluationCriteriaController(ISender mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllVendorEvaluationCriteriaAsync(
            [FromQuery] int PageNumber, [FromQuery] int PageSize, [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllVendorEvaluationCriteriaQuery
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
        public async Task<IActionResult> GetVendorEvaluationCriteriaByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetVendorEvaluationCriteriaByIdQuery { Id = id });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetVendorEvaluationCriteriaAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetVendorEvaluationCriteriaAutoCompleteQuery(term ?? string.Empty));
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        [HttpPost]
        public async Task<IActionResult> CreateVendorEvaluationCriteria([FromBody] CreateVendorEvaluationCriteriaCommand command)
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
        public async Task<IActionResult> UpdateVendorEvaluationCriteria([FromBody] UpdateVendorEvaluationCriteriaCommand command)
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
        public async Task<IActionResult> DeleteVendorEvaluationCriteria(int id)
        {
            await Mediator.Send(new DeleteVendorEvaluationCriteriaCommand(id));
            return Ok(new { StatusCode = StatusCodes.Status200OK, message = "Deleted successfully." });
        }
    }
}
