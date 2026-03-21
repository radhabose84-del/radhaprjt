using Contracts.Dtos.Common;
using PurchaseManagement.Application.Quotation.RfqEntry.Commands.Create;
using PurchaseManagement.Application.Quotation.RfqEntry.Commands.UpsertDraft;
using PurchaseManagement.Application.Quotation.RfqEntry.Dtos;
using PurchaseManagement.Application.Quotation.RfqEntry.DTOs;
using PurchaseManagement.Application.Quotation.RfqEntry.Queries.GetAllRfq;
using PurchaseManagement.Application.Quotation.RfqEntry.Queries.GetRfqAutoComplete;
using PurchaseManagement.Application.Quotation.RfqEntry.Queries.GetRfqAutoCompleteComparison;
using PurchaseManagement.Application.Quotation.RfqEntry.Queries.GetRfqById;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace PurchaseManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class RfqsController : ApiControllerBase
    {
        private readonly IMediator _mediator;
        public RfqsController(IMediator mediator) : base(mediator) => _mediator = mediator;

        // CREATE RFQ
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRfqCommand cmd, CancellationToken ct)
        {
            var id = await _mediator.Send(cmd, ct);
            var payload = new ApiResponse<int>(StatusCodes.Status201Created, "RFQ created successfully.", id);
            return StatusCode(StatusCodes.Status201Created, payload);
        }

        // UPDATE RFQ (full)
        [HttpPut("id")]
        public async Task<IActionResult> Update([FromBody] UpdateRfqCommand cmd, CancellationToken ct)
        {
            await _mediator.Send(cmd, ct);
            return Ok(new ApiResponse<object>(StatusCodes.Status200OK, "RFQ updated successfully.", null));
        }

        // GET RFQ BY ID
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var dto = await _mediator.Send(new GetRfqByIdQuery(id), ct);
            return Ok(new ApiResponse<RfqDto>(StatusCodes.Status200OK, "OK", dto));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? statusId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 15,
            [FromQuery] string? searchTerm = null,
            CancellationToken ct = default)
        {
            // If your query is GetAllBudgetRequestQuery use that;
            // if it's still GetAllRfqQuery, keep that name.
            var (items, total) = await _mediator.Send(new GetAllRfqQuery
            {
                StatusId = statusId,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm
            }, ct);

            var payload = new
            {
                items,
                total,
                pageNumber,
                pageSize
            };

            return Ok(new ApiResponse<object>(
                StatusCodes.Status200OK,
                "Fetched successfully.",
                payload));
        }


        // CREATE DRAFT
        [HttpPost("draft")]
        public async Task<IActionResult> CreateDraft([FromBody] UpsertRfqDraftCommand cmd, CancellationToken ct)
        {
            cmd.Id = null; // force create
            var res = await _mediator.Send(cmd, ct);
            var payload = new ApiResponse<UpsertRfqDraftResult>(StatusCodes.Status201Created, "Draft created successfully.", res);
            return StatusCode(StatusCodes.Status201Created, payload);
        }

        // UPDATE DRAFT
        [HttpPut("id/draft")]
        public async Task<IActionResult> UpdateDraft([FromBody] UpsertRfqDraftCommand cmd, CancellationToken ct)
        {
            var res = await _mediator.Send(cmd, ct);
            return Ok(new ApiResponse<UpsertRfqDraftResult>(StatusCodes.Status200OK, "Draft updated successfully.", res));
        }
        [HttpGet("autocomplete")]
        public async Task<IActionResult> GetRfqAutoComplete(
            [FromQuery] string? search,
            [FromQuery] DateOnly? lastSubmitDate,
            CancellationToken ct)
        {
            var items = await _mediator.Send(new GetRfqAutoCompleteQuery
            {
                SearchPattern = search,
                LastSubmitDate = lastSubmitDate
            }, ct);

            var response = new ApiResponse<List<RfqAutoCompleteDto>>(
                StatusCodes.Status200OK,
                "RFQ autocomplete fetched successfully",
                items
            );

            return Ok(response);
        }

        [HttpGet("autocomplete/quotation")]
        public async Task<IActionResult> NoQuotation(
            [FromQuery] string? term,
            [FromQuery] DateOnly? date,
            CancellationToken ct)
        {
            var items = await _mediator.Send(new GetRfqAutoCompleteQuotationQuery
            {
                SearchPattern = term,
                LastSubmitDate = date
            }, ct);

            var response = new ApiResponse<List<RfqAutoCompleteDto>>(
                StatusCodes.Status200OK,
                "RFQ autocomplete fetched successfully",
                items
            );

            return Ok(response);
        }

        // 🔹 RFQs WITH an Approved comparison (Comparison autocomplete)
        [HttpGet("autocomplete/comparison")]
        public async Task<IActionResult> ApprovedComparison(
            [FromQuery] string? term,
            [FromQuery] DateOnly? date,
            [FromQuery] int? statusId,
            CancellationToken ct)
        {
            var items = await _mediator.Send(new GetRfqAutoCompleteComparisonQuery
            {
                SearchPattern = term,
                LastSubmitDate = date,
                StatusId = statusId
            }, ct);

            var response = new ApiResponse<List<RfqAutoCompleteDto>>(
                StatusCodes.Status200OK,
                "RFQ autocomplete fetched successfully",
                items
            );

            return Ok(response);
        }
    }
}