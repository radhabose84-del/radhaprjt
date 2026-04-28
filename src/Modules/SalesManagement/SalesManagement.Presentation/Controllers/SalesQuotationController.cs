using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.SalesQuotation.Commands.CreateSalesQuotation;
using SalesManagement.Application.SalesQuotation.Commands.UpdateSalesQuotation;
using SalesManagement.Application.SalesQuotation.Commands.DeleteSalesQuotation;
using SalesManagement.Application.SalesQuotation.Queries.GetAllSalesQuotation;
using SalesManagement.Application.SalesQuotation.Queries.GetSalesQuotationById;
using SalesManagement.Application.SalesQuotation.Queries.GetSalesQuotationAutoComplete;
using SalesManagement.Application.SalesQuotation.Queries.GetSalesQuotationPending;

namespace SalesManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class SalesQuotationController : ApiControllerBase
    {
        public SalesQuotationController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllSalesQuotationAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllSalesQuotationQuery
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
        public async Task<IActionResult> GetSalesQuotationByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetSalesQuotationByIdQuery { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetSalesQuotationAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetSalesQuotationAutoCompleteQuery(term ?? string.Empty));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingSalesQuotationAsync(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 15,
            [FromQuery] string? searchTerm = null)
        {
            var (rows, total) = await Mediator.Send(new GetSalesQuotationPendingQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm
            });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = rows,
                TotalCount = total,
                PageNumber = pageNumber,
                PageSize = pageSize
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateSalesQuotation([FromBody] CreateSalesQuotationCommand command)
        {
            var result = await Mediator.Send(command);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = true,
                message = "Sales Quotation created successfully.",
                data = result
            });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateSalesQuotation([FromBody] UpdateSalesQuotationCommand command)
        {
            var result = await Mediator.Send(command);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = true,
                message = "Sales Quotation updated successfully.",
                data = result
            });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteSalesQuotation(int id)
        {
            var result = await Mediator.Send(new DeleteSalesQuotationCommand(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Sales Quotation deleted successfully." : "Failed to delete Sales Quotation."
            });
        }
    }
}
