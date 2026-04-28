using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.SalesQuotation.Commands.CreateSalesQuotationAmendment;
using SalesManagement.Application.SalesQuotation.Queries.GetAllSalesQuotationAmendment;
using SalesManagement.Application.SalesQuotation.Queries.GetPendingSalesQuotationAmendment;
using SalesManagement.Application.SalesQuotation.Queries.GetPendingSalesQuotationAmendmentById;
using SalesManagement.Application.SalesQuotation.Queries.GetSalesQuotationAmendmentById;

namespace SalesManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class SalesQuotationAmendmentController : ApiControllerBase
    {
        public SalesQuotationAmendmentController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllSalesQuotationAmendmentQuery
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

        [HttpGet("{salesQuotationHeaderId}")]
        public async Task<IActionResult> GetBySalesQuotationHeaderIdAsync(int salesQuotationHeaderId)
        {
            var result = await Mediator.Send(new GetSalesQuotationAmendmentByIdQuery { SalesQuotationHeaderId = salesQuotationHeaderId });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result.Data });
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var (items, total) = await Mediator.Send(new GetPendingSalesQuotationAmendmentQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm
            });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = items,
                TotalCount = total,
                PageNumber,
                PageSize
            });
        }

        [HttpGet("pending/{id}")]
        public async Task<IActionResult> GetPendingByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetPendingSalesQuotationAmendmentByIdQuery { Id = id });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreateSalesQuotationAmendmentCommand command)
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
    }
}
