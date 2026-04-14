using Contracts.Common;
using InventoryManagement.Application.Item.ItemDetail.Commands.CreateItem;
using InventoryManagement.Application.Item.ItemDetail.Commands.CreateItemTemplate;
using InventoryManagement.Application.Item.ItemDetail.Commands.CreateItemVariants;
using InventoryManagement.Application.Item.ItemDetail.Commands.DeleteItemImage;
using InventoryManagement.Application.Item.ItemDetail.Commands.UpdateItem;
using InventoryManagement.Application.Item.ItemDetail.Commands.UploadItemImage;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetItemAutoComplete;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetItemById;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetItemLogById;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetItemLogs;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetItemsByIds;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetItemsByVariantFilter;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.Presentation.Controllers.Item
{
    [Route("api/[controller]")]
    public sealed class ItemMasterController : ApiControllerBase
    {
        private readonly IMediator _mediator;
        public ItemMasterController(IMediator mediator) : base(mediator) => _mediator = mediator;

        // GET: /api/ItemMaster
        [HttpGet]
        public async Task<ActionResult<ApiResponseDTO<List<ItemListDto>>>> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] bool onlyActive = true,
            [FromQuery] int? itemGroupId = null,
            [FromQuery] int? itemCategoryId = null,
            [FromQuery] int? moduleId = null,
            [FromQuery] int? salesGroupId = null)
        {
            var (items, total) = await _mediator.Send(new GetAllItemsQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = search,
                OnlyActive = onlyActive,
                ItemGroupId = itemGroupId,
                ItemCategoryId = itemCategoryId,
                ModuleId = moduleId,
                SalesGroupId = salesGroupId
            });

            return Ok(new ApiResponseDTO<List<ItemListDto>>
            {
                StatusCode = StatusCodes.Status200OK,
                Data = items,
                TotalCount = total,
                PageNumber = pageNumber,
                PageSize = pageSize
            });
        }

        // GET: /api/ItemMaster/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponseDTO<ItemDetailsDto>>> GetById([FromRoute] int id)
        {
            var dto = await _mediator.Send(new GetItemByIdQuery { Id = id });
            if (dto is null)
                return NotFound(new ApiResponseDTO<ItemDetailsDto>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Item not found."
                });

            return Ok(new ApiResponseDTO<ItemDetailsDto>
            {
                StatusCode = StatusCodes.Status200OK,
                Data = dto
            });
        }

        // POST: /api/ItemMaster
        [HttpPost]
        public async Task<ActionResult<ApiResponseDTO<int>>> Create([FromBody] ItemDto payload, CancellationToken ct)
        {
            if (payload is null)
                return BadRequest(new ApiResponseDTO<int>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Request body is required."
                });

            var id = await _mediator.Send(new CreateItemCommand { Payload = payload }, ct);

            if (id <= 0)
                return BadRequest(new ApiResponseDTO<int>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "No variant was created for the provided selections."
                });

            return CreatedAtAction(nameof(GetById), new { id },
                new ApiResponseDTO<int>
                {
                    StatusCode = StatusCodes.Status201Created,
                    Message = "Item created successfully.",
                    Data = id
                });
        }

        // PUT: /api/ItemMaster
        [HttpPut]
        public async Task<ActionResult<ApiResponseDTO<int>>> Update([FromBody] ItemDto payload, CancellationToken ct)
        {
            await _mediator.Send(new UpdateItemCommand { Payload = payload }, ct);

            return Ok(new ApiResponseDTO<int>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Item updated successfully.",
                Data = payload.Id
            });
        }

        // POST: /api/ItemMaster/upload-logo
        [HttpPost("upload-logo")]
        public async Task<ActionResult<ApiResponseDTO<ImageDto>>> UploadLogo([FromForm] UploadFileCommand command, CancellationToken ct)
        {
            if (command.File is null || command.File.Length == 0)
                return BadRequest(new ApiResponseDTO<ImageDto>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "No file uploaded."
                });

            var dto = await _mediator.Send(command, ct);

            return Ok(new ApiResponseDTO<ImageDto>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Logo uploaded successfully.",
                Data = dto
            });
        }

        // DELETE: /api/ItemMaster/delete-logo
        [HttpDelete("delete-logo")]
        public async Task<ActionResult<ApiResponseDTO<bool>>> DeleteLogo([FromBody] DeleteFileCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(new ApiResponseDTO<bool>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Logo deleted successfully.",
                Data = result
            });
        }

        // GET: /api/ItemMaster/autocomplete
        [HttpGet("autocomplete")]
        public async Task<ActionResult<ApiResponseDTO<List<GetItemAutoCompleteDto>>>> GetAutoComplete(
            [FromQuery] string? searchPattern,
            [FromQuery] int? itemGroupId = null,
            [FromQuery] int? itemCategoryId = null,
            [FromQuery] int? sourceId = null,
            [FromQuery] int? issueRuleId = null,
            [FromQuery] int? moduleId = null,
            [FromQuery] int? salesGroupId = null,
            CancellationToken ct = default)
        {
            var items = await _mediator.Send(new GetItemAutoCompleteQuery
            {
                SearchPattern = searchPattern,
                ItemGroupId = itemGroupId,
                ItemCategoryId = itemCategoryId,
                SourceId = sourceId,
                IssueRuleId = issueRuleId,
                ModuleId = moduleId,
                SalesGroupId = salesGroupId
            }, ct);

            return Ok(new ApiResponseDTO<List<GetItemAutoCompleteDto>>
            {
                StatusCode = StatusCodes.Status200OK,
                Data = items
            });
        }

        // GET: /api/ItemMaster/variant-filter
        [HttpGet("variantFilter")]
        public async Task<ActionResult<ApiResponseDTO<List<GetItemAutoCompleteDto>>>> GetItemsByVariantFilter(
            [FromQuery] bool? hasVariant = null,
            [FromQuery] int? parentItemId = null,
            [FromQuery] string? moduleId = null,
            CancellationToken ct = default)
        {
            int? parsedModuleId = int.TryParse(moduleId, out var mid) ? mid : null;

            var items = await _mediator.Send(new GetItemsByVariantFilterQuery
            {
                HasVariant = hasVariant,
                ParentItemId = parentItemId,
                ModuleId = parsedModuleId
            }, ct);

            return Ok(new ApiResponseDTO<List<GetItemAutoCompleteDto>>
            {
                StatusCode = StatusCodes.Status200OK,
                Data = items
            });
        }

        // POST: /api/ItemMaster/variants
        [HttpPost("variants")]
        public async Task<ActionResult<ApiResponseDTO<List<int>>>> CreateVariants([FromBody] CreateItemVariantsCommand command, CancellationToken ct)
        {
            var ids = await _mediator.Send(command, ct);
            return Ok(new ApiResponseDTO<List<int>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = $"Created {ids.Count} variant(s).",
                Data = ids
            });
        }

        // POST: /api/ItemMaster/template
        [HttpPost("template")]
        public async Task<ActionResult<ApiResponseDTO<int>>> CreateTemplate([FromBody] CreateItemTemplateCommand command, CancellationToken ct)
        {
            var id = await _mediator.Send(command, ct);
            return CreatedAtAction(nameof(GetById), new { id }, new ApiResponseDTO<int>
            {
                StatusCode = StatusCodes.Status201Created,
                Message = "Template created successfully.",
                Data = id
            });
        }

        // GET: /api/ItemMaster/log/id/{id}
        [HttpGet("log/id/{id:int}")]
        public async Task<ActionResult<ApiResponseDTO<ItemLogDto>>> GetLogById([FromRoute] int id, CancellationToken ct)
        {
            var log = await _mediator.Send(new GetItemLogByIdQuery(id), ct);
            if (log is null)
                return NotFound(new ApiResponseDTO<ItemLogDto>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Log not found."
                });

            return Ok(new ApiResponseDTO<ItemLogDto>
            {
                StatusCode = StatusCodes.Status200OK,
                Data = log
            });
        }

        // GET: /api/ItemMaster/logs
        [HttpGet("logs")]
        public async Task<ActionResult<ApiResponseDTO<List<ItemLogDto>>>> GetAllLogs(
            [FromQuery] int? entityId,
            [FromQuery] string? search,
            [FromQuery] int? page,
            [FromQuery] int? size,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] int? userId,
            CancellationToken ct)
        {
            var filter = new ItemLogFilter
            {
                EntityId = entityId,
                Search = search,
                Page = page,
                Size = size,
                From = from,
                To = to,
                UserId = userId
            };

            var (items, total) = await _mediator.Send(new GetItemLogsQuery(filter), ct);

            return Ok(new ApiResponseDTO<List<ItemLogDto>>
            {
                StatusCode = StatusCodes.Status200OK,
                Data = items,
                TotalCount = total
            });
        }

        // GET: /api/ItemMaster/logs/itemid/{itemId}
        [HttpGet("logs/itemid/{itemId:int}")]
        public async Task<ActionResult<ApiResponseDTO<List<ItemLogDto>>>> GetLogsForItem(
            [FromRoute] int itemId,
            [FromQuery] int? page,
            [FromQuery] int? size,
            CancellationToken ct)
        {
            var filter = new ItemLogFilter
            {
                EntityId = itemId,
                Page = page,
                Size = size
            };

            var (items, total) = await _mediator.Send(new GetItemLogsQuery(filter), ct);

            return Ok(new ApiResponseDTO<List<ItemLogDto>>
            {
                StatusCode = StatusCodes.Status200OK,
                Data = items,
                TotalCount = total
            });
        }

        // POST: /api/ItemMaster/by-ids
        [HttpPost("by-ids")]
        public async Task<ActionResult<ApiResponseDTO<List<GetItemAutoCompleteDto>>>> GetByIds(
            [FromBody] List<int> ids,
            CancellationToken cancellationToken)
        {
            if (ids == null || ids.Count == 0)
                return Ok(new ApiResponseDTO<List<GetItemAutoCompleteDto>>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Data = new List<GetItemAutoCompleteDto>()
                });

            var result = await _mediator.Send(new GetItemsMasterByIdsQuery(ids), cancellationToken);

            return Ok(new ApiResponseDTO<List<GetItemAutoCompleteDto>>
            {
                StatusCode = StatusCodes.Status200OK,
                Data = result
            });
        }
    }
}
