using BudgetManagement.Application.Common.HttpResponse;
using BudgetManagement.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace BudgetManagement.API.Controllers;

[ApiController]
[Route("api/budget/logs")]
public class ActivityLogsController : ControllerBase
{
    private readonly IMediator _mediator;
    public ActivityLogsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("{entityName}/{entityId:int}")]
    public async Task<IActionResult> GetAll(string entityName, int entityId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50, CancellationToken ct = default)
    {
        var (items, total) = await _mediator.Send(new GetActivityLogsQuery(entityName, entityId, pageNumber, pageSize), ct);
        return Ok(new ApiResponseDTO<List<ActivityLog>>
        {
            StatusCode = StatusCodes.Status200OK,
            Message = "OK",
            Data = items,
            TotalCount = total,
            PageNumber = pageNumber,
            PageSize = pageSize
        });
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken ct = default)
    {
        var row = await _mediator.Send(new GetActivityLogByIdQuery(id), ct);
        if (row is null) return NotFound(new { message = "Log not found" });

        return Ok(new ApiResponseDTO<ActivityLog>
        {
            StatusCode = StatusCodes.Status200OK,
            Message = "OK",
            Data = row
        });
    }
}
