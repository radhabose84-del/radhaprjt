using PurchaseManagement.Application.PurchaseOrder.Local.Commands.Create;
using PurchaseManagement.Application.PurchaseOrder.Local.Commands.Delete;
using PurchaseManagement.Application.PurchaseOrder.Local.Commands.Update;
using PurchaseManagement.Application.PurchaseOrder.Local.Queries.GetAllPurchaseOrder;
using PurchaseManagement.Application.PurchaseOrder.Local.Queries.GetPOLocalPending;
using PurchaseManagement.Application.PurchaseOrder.Local.Queries.GetPurchaseOrderAutocomplete;
using PurchaseManagement.Application.PurchaseOrder.Local.Queries.GetPurchaseOrderById;
using PurchaseManagement.Application.PurchaseOrder.POAmendment;
using PurchaseManagement.Application.PurchaseOrder.Reports;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PurchaseLocalDetailDto.Application.PurchaseOrder.Dtos.Local;
using Microsoft.AspNetCore.Http;

namespace PurchaseManagement.API.Controllers.PurchaseOrder;

[ApiController]
[Route("api/[controller]")]
public class PurchaseOrderLocalController : ApiControllerBase
{
    public PurchaseOrderLocalController(ISender mediator) : base(mediator)
    {


    }    

   [HttpPost]
    public async Task<IActionResult> Create([FromBody] PurchaseOrderCreateDto dto, CancellationToken ct)
    {
        var response = await Mediator.Send(new CreatePurchaseOrderCommand { Data = dto }, ct);

        
        return Ok(new
        {
            statusCode = response.IsSuccess
                ? StatusCodes.Status201Created
                : StatusCodes.Status400BadRequest,  
            message = response.Message,
            data = response.Data
        });
    }


    [HttpPut]
    public async Task<IActionResult> Update([FromBody] PurchaseOrderUpdateDto dto, CancellationToken ct)
    {
        var ok = await Mediator.Send(new UpdatePurchaseOrderCommand
        { Data = dto }, ct);

        return Ok(new
        {
            StatusCode = ok ? StatusCodes.Status200OK : StatusCodes.Status404NotFound,
            message = ok ? "Updated successfully." : "Not found.",
            data = ok
        });
    }
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var ok = await Mediator.Send(new DeletePurchaseOrderCommand
        { Id = id }, ct);

        return Ok(new
        {
            StatusCode = ok ? StatusCodes.Status200OK : StatusCodes.Status404NotFound,
            message = ok ? "Deleted (soft) successfully." : "Not found.",
            data = ok
        });
    }
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1,
                                            [FromQuery] int pageSize = 20,
                                            [FromQuery] string? searchTerm = null,
                                             [FromQuery] int? poMethodId = null,  [FromQuery] int? statusId = null,[FromQuery] int? budgetGroupId = null,
                                            CancellationToken ct = default)
    {
        var data = await Mediator.Send(new GetPurchaseOrdersQuery(pageNumber, pageSize, searchTerm,poMethodId,statusId,budgetGroupId), ct);
        return Ok(new { StatusCode = StatusCodes.Status200OK, message = "Fetched", data });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var data = await Mediator.Send(new GetPurchaseOrderByIdQuery(id), ct);
        return Ok(new { StatusCode = data is null ? 404 : 200, message = data is null ? "Not found" : "Fetched", data });
    }

    [HttpGet("autocomplete")]
    public async Task<IActionResult> Autocomplete([FromQuery] string? term=null, [FromQuery] int? poMethodId=null,int? budgetGroupId=null, CancellationToken ct = default)
    {
        var data = await Mediator.Send(new GetPurchaseOrderAutocompleteQuery(term, poMethodId,budgetGroupId), ct);
        return Ok(new { StatusCode = StatusCodes.Status200OK, message = "Fetched", data });
    }
    [HttpGet("pending-po")]
    public async Task<IActionResult> GetPendingPOAsync(
        [FromQuery] int? poId =null,[FromQuery] int? PoMethodId =null,
        [FromQuery] int? pageNumber = 1,
        [FromQuery] int? pageSize = 15,
        [FromQuery] string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var (items, total) = await Mediator.Send(new GetPOLocalPendingQuery
        {
            PoId = poId,
            PoMethodId = PoMethodId,
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm
        }, cancellationToken);

        if (items == null || items.Count == 0)
        {
            return NotFound(new
            {
                StatusCode = StatusCodes.Status404NotFound,
                data = (object?)null,
                message = $"Pending PO data for PO ID {poId} not found"
            });
        }
        return Ok(new
        {
            StatusCode = StatusCodes.Status200OK,
            data = new { Items = items, TotalCount = total },
            message = $"Pending PO data for PO ID {poId} fetched successfully"
        });
    }   
    [HttpPost("amendment")]
    public async Task<IActionResult> Amend([FromBody] PurchaseOrderUpdateDto dto, CancellationToken ct)
    {
        if (dto is null)
            return BadRequest(new
            {
                StatusCode = StatusCodes.Status400BadRequest,
                message = "Invalid payload: id mismatch.",
                data = (object?)null
            });

        var newId = await Mediator.Send(new POAmendmentCommand { Data = dto }, ct);

        return Ok(new
        {
            StatusCode = StatusCodes.Status200OK,
            message = "Amendment created successfully.",
            data = new { NewPurchaseOrderId = newId }
        });
    }
    [HttpGet("reports/rfq/pdf")]
    public async Task<IActionResult> Untitled([FromQuery] int unitId ,int poId)
    {
        var pdf = await Mediator.Send(new GenerateUntitledPdfQuery(unitId, poId));       
        return File(pdf, "application/pdf", $"Po-{unitId}-{poId}.pdf");
    } 
} 
