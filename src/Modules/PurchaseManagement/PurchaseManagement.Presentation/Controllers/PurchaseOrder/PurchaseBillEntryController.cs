
using PurchaseManagement.Application.PurchaseOrder.BillEntry.Commands.Create;
using PurchaseManagement.Application.PurchaseOrder.BillEntry.Commands.Update;
using PurchaseManagement.Application.PurchaseOrder.BillEntry.Dto;
using PurchaseManagement.Application.PurchaseOrder.BillEntry.Queries.GetAll;
using PurchaseManagement.Application.PurchaseOrder.BillEntry.Queries.GetById;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace PurchaseManagement.Presentation.Controllers.PurchaseOrder
{
    [Route("api/[controller]")]

    public class PurchaseBillEntryController : ApiControllerBase  
    {
        
        public PurchaseBillEntryController(ISender mediator) : base(mediator)
        {

        }
        
       [HttpPost]
        public async Task<IActionResult> CreateAsync(
            [FromBody] PurchaseBillEntryHeaderDto dto,
            CancellationToken ct = default)
        {
            var id = await Mediator.Send(new CreatePurchaseBillEntryCommand(dto), ct);

            return Ok(new
            {
                StatusCode = StatusCodes.Status201Created,
                message = "Created successfully.",
                data = id
            });
        }


        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateAsync(
            int id,
            [FromBody] PurchaseBillEntryHeaderDto dto,
            CancellationToken ct = default)
        {
            if (dto.Id is null || dto.Id.Value != id)
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = "Route id and body id must match."
                });
            }

            await Mediator.Send(new UpdatePurchaseBillEntryCommand
            {
                Data = dto
            }, ct);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "Updated successfully."
            });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetByIdAsync(
            int id,
            CancellationToken ct = default)
        {
            var result = await Mediator.Send(new GetPurchaseBillEntryByIdQuery
            {
                Id = id
            }, ct);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "Success.",
                data = result
            });
        }
        [HttpGet]
        public async Task<IActionResult> GetAllAsync(
            [FromQuery] int? vendorId,
            [FromQuery] string? search,
            [FromQuery] DateOnly? fromDate,
            [FromQuery] DateOnly? toDate,
            [FromQuery] int page = 1,
            [FromQuery] int size = 15,
            CancellationToken ct = default)
        {
            var result = await Mediator.Send(new GetAllPurchaseBillEntryQuery
            {
                VendorId = vendorId,
                Search = search,
                FromDate = fromDate,
                ToDate = toDate,
                Page = page,
                Size = size
            }, ct);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "Success.",
                data = result
            });
        }
    }
}
