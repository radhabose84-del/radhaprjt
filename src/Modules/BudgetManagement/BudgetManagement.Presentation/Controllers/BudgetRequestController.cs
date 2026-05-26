using BudgetManagement.Application.BudgetRequest;
using BudgetManagement.Application.BudgetRequest.Commands;
using BudgetManagement.Application.BudgetRequest.Commands.Create;
using BudgetManagement.Application.BudgetRequest.Commands.Delete;
using BudgetManagement.Application.BudgetRequest.Commands.DeleteImage;
using BudgetManagement.Application.BudgetRequest.Commands.Update;
using BudgetManagement.Application.BudgetRequest.Queries.GetAll;
using BudgetManagement.Application.BudgetRequest.Queries.GetBudgetRequestPending;
using BudgetManagement.Application.BudgetRequest.Queries.GetById;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace BudgetManagement.Presentation.Controllers;

[Route("api/[controller]")]
public class BudgetRequestController : ApiControllerBase
{
    private readonly IMediator _mediator;
    public BudgetRequestController(IMediator mediator) : base(mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBudgetRequestCommand cmd)
    {
        var id = await _mediator.Send(cmd);

        return Ok(new
        {
            StatusCode = StatusCodes.Status201Created,
            message = "Created successfully.",
            errors = string.Empty,
            id
        });
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateBudgetRequestCommand cmd)
    {
        if (cmd.Id == 0)
        {
            return BadRequest(new
            {
                StatusCode = StatusCodes.Status400BadRequest,
                message = "Id mismatch.",
                errors = "Enter correct id."
            });
        }

        await _mediator.Send(cmd);

        return Ok(new
        {
            StatusCode = StatusCodes.Status200OK,
            message = "Updated successfully.",
            errors = string.Empty
        });
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(int id)
    {
        await _mediator.Send(new DeleteBudgetRequestCommand { Id = id });

        return Ok(new
        {
            StatusCode = StatusCodes.Status200OK,
            message = "Deleted successfully.",
            errors = string.Empty
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] int? statusId = null,
        [FromQuery] string? searchTerm = null)
    {
        var (items, total) = await _mediator.Send(new GetAllBudgetRequestQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            StatusId = statusId,
            SearchTerm = searchTerm
        });

        return Ok(new
        {
            statusCode = StatusCodes.Status200OK,
            data = items,
            totalCount = total,
            pageNumber,
            pageSize
        });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetBudgetRequestByIdQuery { Id = id });

        if (result == null)
        {
            return NotFound(new
            {
                StatusCode = StatusCodes.Status404NotFound,
                message = "Budget Request not found.",
                errors = $"No Budget Request with Id = {id}",
                data = (BudgetRequestDto?)null
            });
        }

        return Ok(new
        {
            StatusCode = StatusCodes.Status200OK,
            message = "Fetched successfully.",
            errors = string.Empty,
            data = result
        });
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

        return Ok(new
        {
            StatusCode = StatusCodes.Status200OK,
            message = "Logo deleted successfully.",
            data = result,
            errors = ""
        });
    }    
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending([FromQuery] GetBudgetRequestPendingQuery query, CancellationToken ct)
    {
        var (items, total) = await _mediator.Send(query, ct);

        return Ok(new
        {
            statusCode = StatusCodes.Status200OK,
            data = items,
            totalCount = total,
            pageNumber = query.PageNumber ?? 1,
            pageSize = query.PageSize ?? 15
        });
    }
}