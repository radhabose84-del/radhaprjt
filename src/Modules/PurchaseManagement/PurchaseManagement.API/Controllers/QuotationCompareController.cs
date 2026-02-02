using PurchaseManagement.Application.Quotation.QuotationCompare.Commands.CreateQuoteComparsion;
using PurchaseManagement.Application.Quotation.QuotationCompare.Queries.GetQuoteComparison;
using PurchaseManagement.Application.Quotation.QuotationCompare.Queries.GetQuoteComparisonById;
using PurchaseManagement.Application.Quotation.QuotationCompare.Queries.GetQuoteComparisonPending;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace PurchaseManagement.API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class QuotationCompareController : ControllerBase
    {
        private readonly IMediator _mediator;
        public QuotationCompareController(IMediator mediator) => _mediator = mediator;

        [HttpGet("comparison/{rfqId}")]
        [ActionName(nameof(GetComparisonAsync))]
        public async Task<IActionResult> GetComparisonAsync(int rfqId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetQuoteComparisonQuery(rfqId), cancellationToken);

            if (result == null)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    data = (object?)null,
                    message = $"Quote comparison for RFQ ID {rfqId} not found"
                });
            }

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result,
                message = $"Quote comparison for RFQ ID {rfqId} fetched successfully"
            });
        }
        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateQuoteComparsionCommand createQuoteComparsionCommand)
        {

            // Process the command
            var CreateQuoteId = await _mediator.Send(createQuoteComparsionCommand);

            return Ok(new
            {
                StatusCode = StatusCodes.Status201Created,
                message = "Created successfully.",
                data = CreateQuoteId
            });

        }
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingAsync(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 15,
            [FromQuery] string? searchTerm = null,
            CancellationToken ct = default)
        {
            var (items, total) = await _mediator.Send(new GetQuoteComparisonPendingQuery
            {
                PageNumber = pageNumber,
                PageSize   = pageSize,
                SearchTerm = searchTerm
            }, ct);

            return Ok(new
            {
                statusCode = StatusCodes.Status200OK,
                data = new { items, totalCount = total, pageNumber, pageSize, searchTerm },
                message = "Pending quotation comparisons fetched successfully (RFQ-wise)."
            });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetByIdAsync([FromRoute] int id, CancellationToken ct)
        {
            var dto = await _mediator.Send(new GetQuoteComparisonByIdQuery { Id = id }, ct);

            // If your handler throws on not found, you won’t reach here for null.
            // Middleware should translate KeyNotFoundException to 404.

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = dto,
                message = $"Quote comparison with ID {id} fetched successfully"
            });
        }
    }
}