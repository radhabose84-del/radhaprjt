using InventoryManagement.Application.Item.ItemGroup.Commands.CreateItemGroup;
using InventoryManagement.Application.Item.ItemGroup.Commands.DeleteItemGroup;
using InventoryManagement.Application.Item.ItemGroup.Commands.UpdateItemGroup;
using InventoryManagement.Application.Item.ItemGroup.Queries.GetItemGroup;
using InventoryManagement.Application.Item.ItemGroup.Queries.GetItemGroupAutoComplete;
using InventoryManagement.Application.Item.ItemGroup.Queries.GetItemGroupById;
using InventoryManagement.Presentation.Controllers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace InventoryManagement.Presentation.Controllers.Item
{
     [Route("api/[controller]")]
    public class ItemGroupController :  ApiControllerBase
    {        
        private readonly IMediator _mediator;
        public ItemGroupController(IMediator mediator)
        : base(mediator)
        {            
            _mediator=mediator;
        }        
        [HttpGet]
        public async Task<IActionResult> GetAllItemGroupAsync([FromQuery] int PageNumber,[FromQuery] int PageSize,[FromQuery] string? SearchTerm = null)
        {
           var notificationConfig = await Mediator.Send(
            new GetItemGroupQuery
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
        public async Task<IActionResult> GetItemGroupAutoCompleteAsync([FromQuery] string? GroupName)
        {
            var notificationConfig = await Mediator.Send(new GetItemGroupAutoCompleteQuery 
            { 
                    SearchPattern = GroupName ?? string.Empty 
            });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = notificationConfig});
        }

        [HttpGet("{id}")]        
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var notificationConfig = await Mediator.Send(new GetItemGroupByIdQuery() { Id = id});           
            return Ok(new 
            {
                StatusCode = StatusCodes.Status200OK,
                data = notificationConfig,
                message = "Item Group fetched successfully"
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreateItemGroupCommand createItemGroupCommand)
        {            
            var CreatedNotificationId = await _mediator.Send(createItemGroupCommand);            
            return Ok(new
            {
                StatusCode = StatusCodes.Status201Created,
                message ="Created successfully.",
                data = CreatedNotificationId
            });            
        
        }
        [HttpPut]
        public async Task<IActionResult> UpdateAsync(UpdateItemGroupCommand updateNotificationConfigCommand)
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
            await _mediator.Send(new DeleteItemGroupCommand { Id = id });
            return Ok(new
            {
                message = "Deleted successfully.",
                statusCode = StatusCodes.Status200OK
            });
        
        }
                
    }
}