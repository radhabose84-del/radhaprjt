using FAM.Application.AssetCategories.Command.CreateAssetCategories;
using FAM.Application.AssetCategories.Command.DeleteAssetCategories;
using FAM.Application.AssetCategories.Command.UpdateAssetCategories;
using FAM.Application.AssetCategories.Queries.GetAssetCategories;
using FAM.Application.AssetCategories.Queries.GetAssetCategoriesAutoComplete;
using FAM.Application.AssetCategories.Queries.GetAssetCategoriesByAssetGroupId;
using FAM.Application.AssetCategories.Queries.GetAssetCategoriesById;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FAM.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class AssetCategoriesController :  ApiControllerBase
    {
        private readonly ILogger<AssetCategoriesController> _logger;
         private readonly IMediator _mediator;

        public AssetCategoriesController(ILogger<AssetCategoriesController> logger, IMediator mediator)
        : base(mediator)
        {
            _logger = logger;
            _mediator = mediator;     
         }

        [HttpGet]
        public async Task<IActionResult> GetAllAssetCategoriesAsync([FromQuery] int PageNumber,[FromQuery] int PageSize,[FromQuery] string? SearchTerm = null)
        {
           var assetcategories = await Mediator.Send(
            new GetAssetCategoriesQuery
            {
                PageNumber = PageNumber, 
                PageSize = PageSize, 
                SearchTerm = SearchTerm
            });
            return Ok( new 
            { 
                StatusCode=StatusCodes.Status200OK, 
                data = assetcategories.Data,
                TotalCount = assetcategories.TotalCount,
                PageNumber = assetcategories.PageNumber,
                PageSize = assetcategories.PageSize
                });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetAssetCategories([FromQuery] string? CategoryName)
        {
        var assetcategories = await Mediator.Send(new GetAssetCategoriesAutoCompleteQuery 
        { 
                SearchPattern = CategoryName ?? string.Empty 
        });

        return Ok(new { StatusCode = StatusCodes.Status200OK, data = assetcategories });
        }

        [HttpGet("{id}")]
        [ActionName(nameof(GetByIdAsync))]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var assetcategories = await Mediator.Send(new GetAssetCategoriesByIdQuery() { Id = id});
          
                
          return Ok(new { StatusCode=StatusCodes.Status200OK, data = assetcategories,message = assetcategories });
            
        }

[HttpPost]
public async Task<IActionResult> CreateAsync(CreateAssetCategoriesCommand createAssetCategoriesCommand)
{
   

    // Process the command
    var CreatedAssetCategoriesId = await _mediator.Send(createAssetCategoriesCommand);

      return Ok(new
      {
          StatusCode = StatusCodes.Status201Created,
          message ="AssetCategories created successfully",
          data = CreatedAssetCategoriesId
      });
}
[HttpPut]
public async Task<IActionResult> UpdateAsync(UpdateAssetCategoriesCommand updateAssetCategoriesCommand)
{
            await _mediator.Send(updateAssetCategoriesCommand);
    
     return Ok(new
     {
         message = "AssetCategories updated successfully",
         statusCode = StatusCodes.Status200OK
     });  
}

[HttpDelete("{id}")]
public async Task<IActionResult> DeleteAssetCategoriesAsync(int id)
{

     await _mediator.Send(new DeleteAssetCategoriesCommand { Id = id });
     
        return Ok(new
            {
                message = "AssetCategories deleted successfully",
                statusCode = StatusCodes.Status200OK
            });
}

        [HttpGet("group/{AssetGroupId}")]
        [ActionName(nameof(GetAssetCategoryBasedonGroupId))]
        public async Task<IActionResult> GetAssetCategoryBasedonGroupId(int AssetGroupId)
        {
            var assetcategory = await Mediator.Send(new GetAssetCategoriesByAssetGroupIdQuery() { AssetGroupId = AssetGroupId});
          
              return Ok(new { StatusCode=StatusCodes.Status200OK, data = assetcategory,message = assetcategory });
            
        }


    }
}