using InventoryManagement.Application.Item.ItemCategory.Commands.CreateItemCategory;
using InventoryManagement.Application.Item.ItemCategory.Commands.DeleteItemCategory;
using InventoryManagement.Application.Item.ItemCategory.Commands.UpdateItemCategory;
using InventoryManagement.Application.Item.ItemCategory.Queries.GetItemCategory;
using InventoryManagement.Application.Item.ItemCategory.Queries.GetItemCategoryAutoComplete;
using InventoryManagement.Application.Item.ItemCategory.Queries.GetItemCategoryById;
using InventoryManagement.Presentation.Controllers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace InventoryManagement.Presentation.Controllers.Item
{
     [Route("api/[controller]")]
    public class ItemCategoryController :  ApiControllerBase
    {        
        private readonly IMediator _mediator;
        public ItemCategoryController(IMediator mediator)
        : base(mediator)
        {            
            _mediator=mediator;
        }        
        [HttpGet]
        public async Task<IActionResult> GetAllItemCategoryAsync([FromQuery] int PageNumber,[FromQuery] int PageSize,[FromQuery] string? SearchTerm = null)
        {
           var notificationConfig = await Mediator.Send(
            new GetItemCategoryQuery
            {
                PageNumber = PageNumber, 
                PageSize = PageSize, 
                SearchTerm = SearchTerm
            });
            return Ok( new 
            { 
                StatusCode=StatusCodes.Status200OK, 
                data = notificationConfig.Data,
                TotalCount = notificationConfig.TotalCount,
                PageNumber = notificationConfig.PageNumber,
                PageSize = notificationConfig.PageSize
                });
        }
        
        [HttpGet("by-name")]
        public async Task<IActionResult> GetItemCategoryAutoCompleteAsync([FromQuery] string? CategoryName, [FromQuery] int? groupId, [FromQuery] bool? isGroup = false, [FromQuery] int? excludeId = 0)
        {
            var notificationConfig = await Mediator.Send(new GetItemCategoryAutoCompleteQuery
            {
                SearchPattern = CategoryName ?? string.Empty,
                        GroupId = groupId  ,IsGroup = isGroup ,excludeId = excludeId ?? 0
            });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = notificationConfig});
        }

        [HttpGet("{id}")]        
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var notificationConfig = await Mediator.Send(new GetItemCategoryByIdQuery() { Id = id});           
            return Ok(new 
            {
                StatusCode = StatusCodes.Status200OK,
                data = notificationConfig,
                message = "Item category fetched successfully"
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateItemCategoryCommand createItemCategoryCommand)
        {            
            var CreatedNotificationId = await _mediator.Send(createItemCategoryCommand);            
            return Ok(new
            {
                StatusCode = StatusCodes.Status201Created,
                message ="Created successfully.",
                data = CreatedNotificationId
            });            
        
        }
        [HttpPut]
        public async Task<IActionResult> UpdateAsync(UpdateItemCategoryCommand updateNotificationConfigCommand)
        {
            await _mediator.Send(updateNotificationConfigCommand);            
            return Ok(new
            {
                message = "Updated successfully.",
                statusCode = StatusCodes.Status200OK
            });                
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            await _mediator.Send(new DeleteItemCategoryCommand { Id = id });
            return Ok(new
            {
                message = "Deleted successfully.",
                statusCode = StatusCodes.Status200OK
            });
        
        }
                
    }
}