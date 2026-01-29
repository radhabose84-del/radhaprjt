using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.AssetLocation.Commands.CreateAssetLocation;
using FAM.Application.AssetLocation.Queries.GetAssetLocation;
using FAM.Application.AssetLocation.Queries.GetAssetLocationById;
using FAM.Application.AssetMaster.AssetLocation.Commands.UpdateAssetLocation;
using FAM.Application.AssetMaster.AssetLocation.Queries.GetCustodian;
using FAM.Application.AssetMaster.AssetLocation.Queries.GetSubLocationById;
using FAM.Application.AssetSubCategories.Queries.GetAssetSubCategoriesById;
using FAM.API.Validation.AssetMaster.AssetLocation;
using FAM.Infrastructure.Data;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FAM.API.Controllers.AssetMaster
{
   [Route("api/[controller]")]
    public class AssetLocationController : ApiControllerBase
    {  
         
        private readonly ILogger<AssetLocationController> _logger;
        private readonly IMediator _mediator;

        public AssetLocationController( ILogger<AssetLocationController> logger,IMediator mediator) : base(mediator)
        
        {
            _logger = logger;
            _mediator = mediator;

        }
        [HttpGet]
       
        public async Task<IActionResult> GetAllAssetLocationAsync([FromQuery] int PageNumber,[FromQuery] int PageSize,[FromQuery] string? SearchTerm = null)
        {
           var assetgroups = await Mediator.Send(
            new GetAssetLocationQuery
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
         [HttpGet("{id}")]
         [ActionName(nameof(GetByIdAsync))]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var assetLocation = await Mediator.Send(new GetAssetLocationByIdQuery() { Id = id});
           
           
              return Ok(new { StatusCode=StatusCodes.Status200OK, data = assetLocation,message = assetLocation });
          

        }
        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateAssetLocationCommand command)
        {
          

            var response = await Mediator.Send(command);

                return Ok(new 
                { 
                    StatusCode=StatusCodes.Status201Created,
                    message = "AssetLocation Created Successfully", 
                    data = response
                });
            
        }

        [HttpPut]
        public async Task<IActionResult> Update(UpdateAssetLocationCommand command)
        {
            
             await Mediator.Send(command);
           
                return Ok(new 
                { 
                    StatusCode = StatusCodes.Status200OK, 
                    Message = "AssetLocation Updated Successfully", 
                    Errors = "" 
                });
           
        }
        [HttpGet]  
        [Route("GetAllCustodian/{OldUnitId}")]      
         public async Task<IActionResult> GetAllCustodianAsync( string OldUnitId, string? SearchEmployee = null)
        {
           var custodian = await Mediator.Send(
            new GetCustodianQuery
            {
                OldUnitId = OldUnitId,                 
                SearchEmployee = SearchEmployee
              
            });
      
           
            return Ok( new 
            { 
                StatusCode=StatusCodes.Status200OK, 
                data = custodian.Data,
                SearchEmployee = SearchEmployee
            });
        }

        [HttpGet("AssetSubLocation/{id}")]
        public async Task<IActionResult>GetSubLocationByIdAsync(int id)
        {
           
            var assetLocation = await Mediator.Send(new GetSubLocationByIdQuery() { Id = id });

   
              return Ok(new { StatusCode=StatusCodes.Status200OK, data = assetLocation,message = assetLocation });

        }
         

       
    }
}