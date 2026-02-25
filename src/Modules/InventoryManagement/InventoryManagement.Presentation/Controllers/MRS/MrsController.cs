using InventoryManagement.Application.MRS.Command.CreateMrsEntry;
using InventoryManagement.Application.MRS.Command.UpdateMrsEntry;
using InventoryManagement.Application.MRS.Queries.GetMrsEntry;
using InventoryManagement.Application.MRS.Queries.GetMrsEntryById;
using InventoryManagement.Application.MRS.Queries.GetMrsPending;
using InventoryManagement.Application.MRS.Queries.GetParentWarehouse;
using InventoryManagement.Application.MRS.Queries.GetStockItemBased;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.Presentation.Controllers.MRS
{
    [Route("api/inventory/[controller]")]
    public class MrsController : ApiControllerBase
    {
        private readonly IMediator _mediator;

        public MrsController(IMediator mediator)
        : base(mediator)
        {
            _mediator = mediator;

        }
        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateMrsEntryCommand createMrsEntryCommand)
        {
            var createMrsEntry = await _mediator.Send(createMrsEntryCommand);

            return Ok(new
            {
                StatusCode = StatusCodes.Status201Created,
                message = "Created successfully.",
                data = createMrsEntry
            });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAsync(UpdateMrsEntryCommand updateMrsEntryCommand)
        {

            await _mediator.Send(updateMrsEntryCommand);

            return Ok(new
            {
                message = "Updated successfully.",
                statusCode = StatusCodes.Status200OK
            });

        }

        [HttpGet("MrsEntryDetails")]
        public async Task<IActionResult> GetMrsEntryDetails(
            int PageNumber, [FromQuery] int PageSize, [FromQuery] string? SearchTerm = null, [FromQuery] DateTimeOffset? fromDate = null, [FromQuery] DateTimeOffset? toDate = null)
        {
            // Send query to repository or mediator
            var mrsPendingHeaders = await _mediator.Send(new GetMrsEntryQuery
            {

                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm,
                FromDate = fromDate,
                ToDate = toDate,

            });

            if (mrsPendingHeaders == null)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    data = (object?)null,
                    message = "No pending GRN headers found"
                });
            }

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = mrsPendingHeaders.Data,
                TotalCount = mrsPendingHeaders.TotalCount,
                PageNumber = mrsPendingHeaders.PageNumber,
                PageSize = mrsPendingHeaders.PageSize
            });
        }


        [HttpGet("{Id}")]
        [ActionName(nameof(GetByIdAsync))]
        public async Task<IActionResult> GetByIdAsync(int Id)
        {
            var getMrsEntry = await Mediator.Send(new GetMrsEntryByIdQuery() { Id = Id });

            if (getMrsEntry == null)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    data = (object?)null,
                    message = $"Mrs with ID {Id} not found"
                });
            }

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = getMrsEntry,
                message = "ID fetched successfully"
            });
        }

        [HttpGet("StockMovement/{itemId}/{warehouseId}")]
        [ActionName(nameof(GetStockAvialble))]
        public async Task<IActionResult> GetStockAvialble(int itemId, int warehouseId, CancellationToken cancellationToken)
        {
            var logs = await _mediator.Send(new GetStockItemBasedQuery { ItemId = itemId, WarehouseId = warehouseId }, cancellationToken);

            if (logs == null || !logs.Any())
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    data = (object?)null,
                    message = $"No Stock found for PartyId {itemId}"
                });
            }

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = logs,
                message = "Stock fetched successfully"
            });
        }

        [HttpGet("ParentWarehouse/{Id}")]
        [ActionName(nameof(GetByParentWarehouseIdAsync))]
        public async Task<IActionResult> GetByParentWarehouseIdAsync(int Id)
        {
            var getMrsEntry = await Mediator.Send(new GetParentWarehouseQuery() { WarehouseId = Id });

            if (getMrsEntry == null)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    data = (object?)null,
                    message = $"Parent Warehouse with ID {Id} not found"
                });
            }

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = getMrsEntry,
                message = "ID fetched successfully"
            });
        }
        
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingMrsAsync([FromQuery] int PageNumber, [FromQuery] int PageSize, [FromQuery] string? SearchTerm = null)
        {
            var PurchaseIndent = await Mediator.Send(
             new GetMrsPendingQuery
             {
                 PageNumber = PageNumber,
                 PageSize = PageSize,
                 SearchTerm = SearchTerm
             });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = PurchaseIndent.Data,
                TotalCount = PurchaseIndent.TotalCount,
                PageNumber = PurchaseIndent.PageNumber,
                PageSize = PurchaseIndent.PageSize
            });
        }

        
    }
}