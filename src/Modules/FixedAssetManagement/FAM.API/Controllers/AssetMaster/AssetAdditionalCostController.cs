using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.AssetMaster.AssetAdditionalCost.Commands.CreateAssetAdditionalCost;
using FAM.Application.AssetMaster.AssetAdditionalCost.Commands.UpdateAssetAdditionalCost;
using FAM.Application.AssetMaster.AssetAdditionalCost.Queries.GetAssetAdditionalCost;
using FAM.Application.AssetMaster.AssetAdditionalCost.Queries.GetAssetAdditionalCostById;
using FAM.Application.AssetMaster.AssetAdditionalCost.Queries.GetCostTypeQuery;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FAM.API.Controllers.AssetMaster
{
    [Route("api/[controller]")]
    public class AssetAdditionalCostController : ApiControllerBase
    {
        private readonly ILogger<AssetAdditionalCostController> _logger;
        private readonly IMediator _mediator;


        public AssetAdditionalCostController(ILogger<AssetAdditionalCostController> logger, IMediator mediator)
        :base(mediator)
        {
            _logger = logger;
            _mediator = mediator;
            
        }

        [HttpGet("CostType")]
        public async Task<IActionResult> GetCostTypes()
        {
            var result = await Mediator.Send(new GetCostTypeQuery());

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "Cost Types fetched successfully.",
                data = result.Data
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateAssetAdditionalCostCommand createAssetAdditionalCostCommand)
        {
            
            var CreatedAssetGroupId = await _mediator.Send(createAssetAdditionalCostCommand);

          
            return Ok(new
            {
                StatusCode = StatusCodes.Status201Created,
                message ="AssetAdditionalCost Created Successfully",
                data = CreatedAssetGroupId
            });
        
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAsync(UpdateAssetAdditionalCostCommand updateAssetAdditionalCostCommand )
        {

            await _mediator.Send(updateAssetAdditionalCostCommand);
               
              return Ok(new
            {
                message = "AssetAdditionalCost Updated Successfully",
                statusCode = StatusCodes.Status200OK
            });
                 
        }


        [HttpGet("{id}")]
        [ActionName(nameof(GetByIdAsync))]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var assetadditionalcost = await Mediator.Send(new GetAssetAdditionalCostByIdQuery() { Id = id});

              return Ok(new { StatusCode=StatusCodes.Status200OK, data = assetadditionalcost,message = assetadditionalcost });

           
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAssetAdditionalCostAsync([FromQuery] int PageNumber,[FromQuery] int PageSize,[FromQuery] string? SearchTerm = null)
        {
           var assetadditionalcost = await Mediator.Send(
            new GetAssetAdditionalCostQuery
            {
                PageNumber = PageNumber, 
                PageSize = PageSize, 
                SearchTerm = SearchTerm
            });
      
           
            return Ok( new 
            { 
                StatusCode=StatusCodes.Status200OK, 
                data = assetadditionalcost.Data,
                TotalCount = assetadditionalcost.TotalCount,
                PageNumber = assetadditionalcost.PageNumber,
                PageSize = assetadditionalcost.PageSize
            });
        }



      
    }
}