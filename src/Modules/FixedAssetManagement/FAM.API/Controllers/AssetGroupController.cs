using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.AssetGroup.Command.CreateAssetGroup;
using FAM.Application.AssetGroup.Command.DeleteAssetGroup;
using FAM.Application.AssetGroup.Command.UpdateAssetGroup;
using FAM.Application.AssetGroup.Queries.GetAssetGroup;
using FAM.Application.AssetGroup.Queries.GetAssetGroupAutoComplete;
using FAM.Application.AssetGroup.Queries.GetAssetGroupById;
using FAM.Infrastructure.Data;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FAM.API.Controllers
{
    [Route("api/[controller]")]
     [ApiController]
    public class AssetGroupController : ApiControllerBase
    {
        private readonly ILogger<AssetGroupController> _logger;
        private readonly IMediator _mediator;

        public AssetGroupController(ILogger<AssetGroupController> logger, IMediator mediator)
        : base(mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

[HttpPost]
public async Task<IActionResult> CreateAsync(CreateAssetGroupCommand createAssetGroupCommand)
{
    
    var CreatedAssetGroupId = await _mediator.Send(createAssetGroupCommand);

     _logger.LogInformation($"AssetGroup {createAssetGroupCommand.Code} created successfully.");
      return Ok(new
      {
          StatusCode = StatusCodes.Status201Created,
          message ="AssetGroup Created Successfully",
          data = CreatedAssetGroupId
      });
   
  
}
[HttpPut]
public async Task<IActionResult> UpdateAsync(UpdateAssetGroupCommand updateAssetGroupCommand)
{
  
       await _mediator.Send(updateAssetGroupCommand);
          _logger.LogInformation($"AssetGroup {updateAssetGroupCommand.GroupName} updated successfully.");
         return Ok(new
          {
              message = "AssetGroup Updated Successfully",
              statusCode = StatusCodes.Status200OK
          });
        
}

[HttpDelete("{id}")]
public async Task<IActionResult> DeleteAssetGroupAsync(int id)
{

      await _mediator.Send(new DeleteAssetGroupCommand { Id = id });
         _logger.LogInformation($"AssetGroup {id} deleted successfully.");
          return Ok(new
         {
             message = "AssetGroup Deleted Successfully",
             statusCode = StatusCodes.Status200OK
         });    
   
}
        [HttpGet]
        public async Task<IActionResult> GetAllAssetGroupAsync([FromQuery] int PageNumber,[FromQuery] int PageSize,[FromQuery] string? SearchTerm = null)
        {
           var assetgroups = await Mediator.Send(
            new GetAssetGroupQuery
            {
                PageNumber = PageNumber, 
                PageSize = PageSize, 
                SearchTerm = SearchTerm
            });
      
           
            return Ok( new 
            { 
                StatusCode=StatusCodes.Status200OK, 
                data = assetgroups.Data,
                TotalCount = assetgroups.TotalCount,
                PageNumber = assetgroups.PageNumber,
                PageSize = assetgroups.PageSize
                });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetAssetGroup([FromQuery] string? groupname)
        {
        var assetgroups = await Mediator.Send(new GetAssetGroupAutoCompleteQuery 
        { 
                SearchPattern = groupname ?? string.Empty 
        });

        return Ok(new { StatusCode = StatusCodes.Status200OK, data = assetgroups });
        }

        [HttpGet("{id}")]
        [ActionName(nameof(GetByIdAsync))]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var assetgroup = await Mediator.Send(new GetAssetGroupByIdQuery() { Id = id});
          
                
              return Ok(new { StatusCode=StatusCodes.Status200OK, data = assetgroup,message = assetgroup });
            
            
           
        }

    }
}