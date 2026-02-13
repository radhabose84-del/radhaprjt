using FAM.Application.AssetGroup.Queries.GetAssetGroupById;
using FAM.Application.AssetSubGroup.Command.CreateAssetSubGroup;
using FAM.Application.AssetSubGroup.Command.DeleteAssetSubGroup;
using FAM.Application.AssetSubGroup.Command.UpdateAssetSubGroup;
using FAM.Application.AssetSubGroup.Queries.GetAssetGroupById;
using FAM.Application.AssetSubGroup.Queries.GetAssetSubGroup;
using FAM.Application.AssetSubGroup.Queries.GetAssetSubGroupAutoComplete;
using FAM.Application.AssetSubGroup.Queries.GetAssetSubGroupById;
using FAM.Infrastructure.Data;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FAM.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssetSubGroupController : ApiControllerBase
    {
        private readonly ILogger<AssetSubGroupController> _logger;
        private readonly IMediator _mediator;

        public AssetSubGroupController(ILogger<AssetSubGroupController> logger, IMediator mediator
        )
        : base(mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateAssetSubGroupCommand createAssetSubGroupCommand)
        {
            
            var CreatedAssetSubGroupId = await _mediator.Send(createAssetSubGroupCommand);

          
                _logger.LogInformation($"AssetSubGroup {createAssetSubGroupCommand.Code} created successfully.");
                return Ok(new
                {
                    StatusCode = StatusCodes.Status201Created,
                    message = "AssetSubGroup created successfully.",
                    data = CreatedAssetSubGroupId
                });
            

        }
        [HttpPut]
        public async Task<IActionResult> UpdateAsync(UpdateAssetSubGroupCommand updateAssetSubGroupCommand)
        {

          
             await _mediator.Send(updateAssetSubGroupCommand);

                _logger.LogInformation($"AssetSubGroup {updateAssetSubGroupCommand.SubGroupName} updated successfully.");
                return Ok(new
                {
                    message = "AssetSubGroup updated successfully.",
                    statusCode = StatusCodes.Status200OK
                });
            
           
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAssetSubGroupAsync(int id)
        {

             await _mediator.Send(new DeleteAssetSubGroupCommand { Id = id });

                _logger.LogInformation($"AssetSubGroup {id} deleted successfully.");
                return Ok(new
                {
                    message = "AssetSubGroup deleted successfully.",
                    statusCode = StatusCodes.Status200OK
                });


        }
        [HttpGet]
        public async Task<IActionResult> GetAllAssetSubGroupAsync([FromQuery] int PageNumber, [FromQuery] int PageSize, [FromQuery] string? SearchTerm = null)
        {
            var assetSubGroups = await Mediator.Send(
             new GetAssetSubGroupQuery
             {
                 PageNumber = PageNumber,
                 PageSize = PageSize,
                 SearchTerm = SearchTerm
             });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = assetSubGroups.Data,
                TotalCount = assetSubGroups.TotalCount,
                PageNumber = assetSubGroups.PageNumber,
                PageSize = assetSubGroups.PageSize
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetAssetSubGroup([FromQuery] string? SubGroupName)
        {
            var assetSubGroups = await Mediator.Send(new GetAssetSubGroupAutoCompleteQuery
            {
                SearchPattern = SubGroupName ?? string.Empty
            });

            return Ok(new { StatusCode = StatusCodes.Status200OK, data = assetSubGroups });
        }

        [HttpGet("{id}")]
        [ActionName(nameof(GetByIdAsync))]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var assetSubGroup = await Mediator.Send(new GetAssetSubGroupByIdQuery() { Id = id });

                return Ok(new { StatusCode = StatusCodes.Status200OK, data = assetSubGroup, message = assetSubGroup });
          

        }
        [HttpGet("groupId")]    
        public async Task<IActionResult> GetByGroupIdAsync(int groupId)
        {
            var assetSubGroup = await Mediator.Send(new GetGroupByIdQuery() { GroupId = groupId });

            return Ok(new { StatusCode = StatusCodes.Status200OK, data = assetSubGroup, message = assetSubGroup });
        }

    }
}