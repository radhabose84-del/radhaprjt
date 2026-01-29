using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.AssetMaster.AssetAmc.Command.CreateAssetAmc;
using FAM.Application.AssetMaster.AssetAmc.Command.DeleteAssetAmc;
using FAM.Application.AssetMaster.AssetAmc.Command.UpdateAssetAmc;
using FAM.Application.AssetMaster.AssetAmc.Queries.GetAssetAmc;
using FAM.Application.AssetMaster.AssetAmc.Queries.GetAssetAmcById;
using FAM.Application.AssetMaster.AssetAmc.Queries.GetCoverageScope;
using FAM.Application.AssetMaster.AssetAmc.Queries.GetExistingVendorDetails;
using FAM.Application.AssetMaster.AssetAmc.Queries.GetRenewStatus;
using FAM.Infrastructure.Migrations;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FAM.API.Controllers.AssetMaster
{
   [Route("api/[controller]")]
    public class AssetAmcController : ApiControllerBase
    {
        private readonly ILogger<AssetAmcController> _logger;
        private readonly IMediator _mediator;

        public AssetAmcController(ILogger<AssetAmcController> logger, IMediator mediator)
        : base(mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [HttpGet("GetExistingVendor/{oldUnitId}/{VendorCode}")]
        public async Task<IActionResult> GetExistingVendor(string oldUnitId, string VendorCode)
        {
            if (oldUnitId == null)
            {
                return BadRequest(new { StatusCode = StatusCodes.Status400BadRequest, Message = "Invalid OldUnitId" });
            }
            if (VendorCode =="0")
            {
                return BadRequest(new { StatusCode = StatusCodes.Status400BadRequest, Message = "Invalid VendorCode" });
            }

            var result = await _mediator.Send(new GetExistingVendorDetailsQuery { OldUnitCode = oldUnitId,VendorCode = VendorCode });

            return Ok(new { StatusCode = StatusCodes.Status200OK, Data = result });
        }

         [HttpGet("RenewStatus")]
        public async Task<IActionResult> GetRenewStatusTypes()
        {
            var result = await Mediator.Send(new GetRenewStatusQuery());

            if (result == null || result.Data == null || result.Data.Count == 0)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    message = "No RenewStatus Types found."
                });
            }
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "RenewStatus fetched successfully.",
                data = result.Data
            });
        }

         [HttpGet("CoverageScope")]
        public async Task<IActionResult> GetCoverageScopeTypes()
        {
            var result = await Mediator.Send(new GetCoverageScopeQuery());

            if (result == null || result.Data == null || result.Data.Count == 0)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    message = "No CoverageScope found."
                });
            }
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "CoverageScope fetched successfully.",
                data = result.Data
            });
        }

        [HttpGet("{id}")]
        [ActionName(nameof(GetByIdAsync))]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var assetamc = await Mediator.Send(new GetAssetAmcByIdQuery() { Id = id});
          
            
              return Ok(new { StatusCode=StatusCodes.Status200OK, data = assetamc,message = assetamc });
            
           
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAssetAmcAsync([FromQuery] int PageNumber,[FromQuery] int PageSize,[FromQuery] string? SearchTerm = null)
        {
           var assetamc = await Mediator.Send(
            new GetAssetAmcQuery
            {
                PageNumber = PageNumber, 
                PageSize = PageSize, 
                SearchTerm = SearchTerm
            });
      
           
            return Ok( new 
            { 
                StatusCode=StatusCodes.Status200OK, 
                data = assetamc.Data,
                TotalCount = assetamc.TotalCount,
                PageNumber = assetamc.PageNumber,
                PageSize = assetamc.PageSize
            });
        }

         [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateAssetAmcCommand createAssetAmcCommand)
        {
            
            var CreatedAssetamcid = await _mediator.Send(createAssetAmcCommand);

            return Ok(new
            {
                StatusCode = StatusCodes.Status201Created,
                message ="Asset Amc Created Successfully",
                data = CreatedAssetamcid
            });
           
        
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAsync(UpdateAssetAmcCommand updateAssetAmcCommand )
        {
        
                 await _mediator.Send(updateAssetAmcCommand);

                return Ok(new
                    {
                        message = "Asset Amc Updated Successfully",
                        statusCode = StatusCodes.Status200OK
                    });
               
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAssetAmcAsync(int id)
        {

                await _mediator.Send(new DeleteAssetAmcCommand { Id = id });
 
                    return Ok(new
                    {
                        message = "Asset Amc Deleted Successfully",
                        statusCode = StatusCodes.Status200OK
                    });
        
        }

        
    }
}