using InventoryManagement.Application.Item.PutAway.Commands.CreatePutAwayRule;
using InventoryManagement.Application.Item.PutAway.Commands.DeletePutAwayRule;
using InventoryManagement.Application.Item.PutAway.Commands.UpdatePutAwayRule;
using InventoryManagement.Application.Item.PutAway.Queries.GetPutAwayRuleById;
using InventoryManagement.Application.Item.PutAway.Queries.GetPutAwayRuleItemId;
using InventoryManagement.Application.Item.PutAway.Queries.GetPutAwayRules;
using InventoryManagement.API.Controllers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace InventoryManagement.API.Controller.Item
{
    [Route("api/[controller]")]
    public class PutAwayRuleController : ApiControllerBase
    {
        private readonly IMediator _mediator;

        public PutAwayRuleController(IMediator mediator)
            : base(mediator)
        {
            _mediator = mediator;
        }

        // GET: api/PutAwayRule
        [HttpGet]
        public async Task<IActionResult> GetAllAsync([FromQuery] int PageNumber, [FromQuery] int PageSize, [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetPutAwayRulesQuery
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
        // GET: api/PutAwayRule/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var dto = await Mediator.Send(new GetPutAwayRuleByIdQuery { Id = id });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = dto,
                message = "Put-away rule fetched successfully"
            });
        }

        // POST: api/PutAwayRule
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreatePutAwayRuleCommand command)
        {
            var createdId = await _mediator.Send(command);
            return Ok(new
            {
                StatusCode = StatusCodes.Status201Created,
                message = "Created successfully.",
                data = createdId
            });
        }

        // PUT: api/PutAwayRule
        [HttpPut]
        public async Task<IActionResult> UpdateAsync([FromBody] UpdatePutAwayRuleCommand command)
        {
            await _mediator.Send(command);
            return Ok(new
            {
                message = "Updated successfully.",
                statusCode = StatusCodes.Status200OK
            });
        }

        // DELETE: api/PutAwayRule?id=123
        [HttpDelete]
        public async Task<IActionResult> DeleteAsync([FromQuery] int id)
        {
            await _mediator.Send(new DeletePutAwayRuleCommand { Id = id });
            return Ok(new
            {
                message = "Deleted successfully.",
                statusCode = StatusCodes.Status200OK
            });
        }

        [HttpGet("PutAwayRuleLoad/{ItemIds}/{WarehouseIds}")]
        public async Task<IActionResult> GetPutAwayRuleLoad(string ItemIds, string WarehouseIds)
        {
            // --- Validate ItemIds ---
            if (string.IsNullOrWhiteSpace(ItemIds))
            {
                return BadRequest("ItemIds are required.");
            }

            // --- Validate WarehouseIds ---
            if (string.IsNullOrWhiteSpace(WarehouseIds))
            {
                return BadRequest("WarehouseIds are required.");
            }

            // --- Parse ItemIds ---
            var parsedItemIds = ItemIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => int.Parse(id.Trim()))
                .ToList();

            // --- Parse WarehouseIds ---
            var parsedWarehouseIds = WarehouseIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => int.Parse(id.Trim()))
                .ToList();

            // --- Build Query ---
            var query = new GetPutAwayRuleItemIdQuery
            {
                ItemIds = parsedItemIds,
                WarehouseIds = parsedWarehouseIds
            };

            // --- Execute Mediator call ---
            var result = await _mediator.Send(query);

            if (result == null || !result.Any())
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    data = (object?)null,
                    message = $"Put-away rule for ItemIds {ItemIds} not found"
                });
            }

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result,
                message = "Data fetched successfully"
            });
        }
            

    }
}
