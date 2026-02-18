using API.Controllers.Quotation;
using Contracts.Common;
using PurchaseManagement.Application.Item.ItemDetail.Commands.UploadItemImage;
using PurchaseManagement.Application.Quotation.QuotationEntry.Commands.DeleteImage;
using PurchaseManagement.Application.Quotations.QuotationEntry.Commands.Create;
using PurchaseManagement.Application.Quotations.QuotationEntry.Commands.Update;
using PurchaseManagement.Application.Quotations.QuotationEntry.DTOs;
using PurchaseManagement.Application.Quotations.QuotationEntry.Queries.GetAllQuotations;
using PurchaseManagement.Application.Quotations.QuotationEntry.Queries.GetQuotationAutoComplete;
using PurchaseManagement.Application.Quotations.QuotationEntry.Queries.GetQuotationById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PurchaseManagement.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuotationEntryController : ControllerBase
    {
        private readonly IMediator _mediator;
        public QuotationEntryController(IMediator mediator) => _mediator = mediator;
        [HttpGet]

        [HttpGet("GetAll")]
        public async Task<ActionResult<ApiResponse<object>>> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize   = 20,
            [FromQuery] string? searchTerm = null,
            CancellationToken ct = default)
        {
            var (items, total) = await _mediator.Send(new GetAllQuotationsQuery
            {
                PageNumber = pageNumber,
                PageSize   = pageSize,
                SearchTerm = searchTerm
            }, ct);

            var totalPages = (int)Math.Ceiling((double)total / pageSize);

            var payload = new
            {
                Items      = items,
                Total      = total,
                PageNumber = pageNumber,
                PageSize   = pageSize,
                TotalPages = totalPages
            };

            return Ok(new ApiResponse<object>(
                StatusCodes.Status200OK,
                "OK",
                payload));
        }

        [HttpGet("id")]
        public async Task<ActionResult<ApiResponse<GetQuotationHeaderDto>>> GetById(int id, CancellationToken ct)
        {
            var query = new GetQuotationByIdQuery { Id = id };
            var result = await _mediator.Send(query, ct);

            if (result is null)
            {
                return NotFound(new ApiResponse<GetQuotationHeaderDto>(
                    StatusCodes.Status404NotFound,
                    $"Quotation {id} not found.",
                    null
                ));
            }

            return Ok(new ApiResponse<GetQuotationHeaderDto>(
                StatusCodes.Status200OK,
                "Quotation fetched successfully.",
                result
            ));
        }

        [HttpGet("autocomplete")]
        public async Task<ActionResult<ApiResponse<List<QuotationAutoCompleteDto>>>> AutoComplete([FromQuery] string? search, CancellationToken ct)
        {
            var query = new GetQuotationAutoCompleteQuery { SearchPattern = search };
            var result = await _mediator.Send(query, ct);

            return Ok(new ApiResponse<List<QuotationAutoCompleteDto>>(
                StatusCodes.Status200OK,
                result.Count > 0 ? "Quotations fetched successfully." : "No results found.",
                result
            ));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateQuotationCommand command, CancellationToken ct)
        {
            var id = await _mediator.Send(command, ct);

            return StatusCode(StatusCodes.Status200OK,
                new ApiResponse<int>(
                    StatusCodes.Status200OK,
                    "Quotation created successfully.",
                    id
                )
            );
        }
/* 
        [HttpPut("id")]
        public async Task<ActionResult<ApiResponse<object>>> Update([FromBody] UpdateQuotationCommand command, CancellationToken ct)
        {
            await _mediator.Send(command, ct);
            return Ok(new ApiResponse<object>(
                StatusCodes.Status200OK,
                "Quotation updated successfully.",
                null
            ));
        } */

       [HttpPut("id")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateQuotationCommand command, CancellationToken ct)
        {            

            await _mediator.Send(command, ct);

            return StatusCode(StatusCodes.Status200OK,
                new ApiResponse<int>(
                    StatusCodes.Status200OK,
                    "Quotation updated successfully.",
                    command.Id
                )
            );
        }




        [HttpPost("upload-logo")]
        public async Task<IActionResult> UploadLogo([FromForm] UploadFileCommand command, CancellationToken ct)
        {
            if (command.File is null || command.File.Length == 0)
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = "No file uploaded.",
                    errors = "File is required."
                });
            }

            var dto = await _mediator.Send(command, ct); 

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "Logo uploaded successfully.",
                data = dto,
                errors = ""
            });
        }
        [HttpDelete("delete-logo")]
        public async Task<IActionResult> DeleteLogo([FromBody] DeleteFileCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);

            // adjust response shape to your DeleteFileCommand result type
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "Logo deleted successfully.",
                data = result,
                errors = ""
            });
        }
    }
}
