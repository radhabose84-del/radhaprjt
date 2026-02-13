using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.AssetSubCategories.Command.CreateAssetSubCategories;
using FAM.Application.AssetSubCategories.Command.DeleteAssetSubCategories;
using FAM.Application.AssetSubCategories.Command.UpdateAssetSubCategories;
using FAM.Application.AssetSubCategories.Queries.GetAssetSubCategories;
using FAM.Application.AssetSubCategories.Queries.GetAssetSubCategoriesAutoComplete;
using FAM.Application.AssetSubCategories.Queries.GetAssetSubCategoriesByCategoryId;
using FAM.Application.AssetSubCategories.Queries.GetAssetSubCategoriesById;
using FAM.Infrastructure.Data;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FAM.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class AssetSubCategoriesController  : ApiControllerBase
    {
        private readonly ILogger<AssetSubCategoriesController> _logger;
        private readonly IMediator _mediator;

        public AssetSubCategoriesController(ILogger<AssetSubCategoriesController> logger, IMediator mediator)
        :base(mediator)
        {
            _logger = logger;
            _mediator = mediator;

        }
         [HttpGet]
        public async Task<IActionResult> GetAllAssetSubCategoriesAsync([FromQuery] int PageNumber,[FromQuery] int PageSize,[FromQuery] string? SearchTerm = null)
        {
           var assetsubcategories = await Mediator.Send(
            new GetAssetSubCategoriesQuery
            {
                PageNumber = PageNumber, 
                PageSize = PageSize, 
                SearchTerm = SearchTerm
            });
      
           
            return Ok( new 
            { 
                StatusCode=StatusCodes.Status200OK, 
                data = assetsubcategories.Data,
                TotalCount = assetsubcategories.TotalCount,
                PageNumber = assetsubcategories.PageNumber,
                PageSize = assetsubcategories.PageSize
                });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetAssetSubCategories([FromQuery] string? subcategoryname)
        {
        var assetsubcategoriesgroups = await Mediator.Send(new GetAssetSubCategoriesAutoCompleteQuery 
        { 
                SearchPattern = subcategoryname ?? string.Empty 
        });

        return Ok(new { StatusCode = StatusCodes.Status200OK, data = assetsubcategoriesgroups });
        }

        [HttpGet("{id}")]
        [ActionName(nameof(GetByIdAsync))]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var assetsubcategoriesgroups = await Mediator.Send(new GetAssetSubCategoriesByIdQuery() { Id = id});
          
                
              return Ok(new { StatusCode=StatusCodes.Status200OK, data = assetsubcategoriesgroups,message = assetsubcategoriesgroups });
           
        }
     [HttpPost]
public async Task<IActionResult> CreateAsync(CreateAssetSubCategoriesCommand createAssetsubCategoriesCommand)
{
  
    var CreatedAssetsubCategoriesId = await _mediator.Send(createAssetsubCategoriesCommand);

  
      return Ok(new
      {
          StatusCode = StatusCodes.Status201Created,
          message ="AssetSubCategories Created Successfully",
          data = CreatedAssetsubCategoriesId
      });
   
}
[HttpPut]
public async Task<IActionResult> UpdateAsync(UpdateAssetSubCategoriesCommand updateAssetsubCategoriesCommand)
{
  
      

         await _mediator.Send(updateAssetsubCategoriesCommand);

           
           return Ok(new
            {
                message = "AssetSubCategories Updated Successfully",
                statusCode = StatusCodes.Status200OK
            });
        
}

[HttpDelete("{id}")]
public async Task<IActionResult> DeleteAssetSubCategoriesAsync(int id)
{
        
         await _mediator.Send(new DeleteAssetSubCategoriesCommand { Id = id });

             return Ok(new
            {
                message = "AssetSubCategories Deleted Successfully",
                statusCode = StatusCodes.Status200OK
            });
   
}

        [HttpGet("Category/{AssetCategoriesId}")]
        [ActionName(nameof(GetAssetSubCategoryBasedonCategoryId))]
        public async Task<IActionResult> GetAssetSubCategoryBasedonCategoryId(int AssetCategoriesId)
        {
            var assetsubcategory = await Mediator.Send(new GetAssetSubCategoriesByCategoryIdQuery() { AssetCategoriesId = AssetCategoriesId});

              return Ok(new { StatusCode=StatusCodes.Status200OK, data = assetsubcategory,message = assetsubcategory });
            
           
        }
       
    }
}
