using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.AssetMaster.AssetDisposal.Command.CreateAssetDisposal;
using FAM.Application.AssetMaster.AssetDisposal.Command.UpdateAssetDisposal;
using FAM.Application.AssetMaster.AssetDisposal.Queries.GetAssetDisposal;
using FAM.Application.AssetMaster.AssetDisposal.Queries.GetAssetDisposalById;
using FAM.Application.AssetMaster.AssetDisposal.Queries.GetDisposalType;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FAM.API.Controllers.AssetMaster
{
     [Route("api/[controller]")]
    public class AssetDisposalController : ApiControllerBase
    {
        private readonly ILogger<AssetDisposalController> _logger;
        private readonly IMediator _mediator;

        public AssetDisposalController(ILogger<AssetDisposalController> logger, IMediator mediator
        )
        : base(mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }
        [HttpGet("DisposalType")]
        public async Task<IActionResult> GetDisposalTypes()
        {
            var result = await Mediator.Send(new GetDisposalTypeQuery());

            if (result == null || result.Data == null || result.Data.Count == 0)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    message = "No Disposal Types found."
                });
            }
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "Disposal Types fetched successfully.",
                data = result.Data
            });
        }

        [HttpGet("{id}")]
        [ActionName(nameof(GetByIdAsync))]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var assetdisposal = await Mediator.Send(new GetAssetDisposalByIdQuery() { Id = id});
          
              return Ok(new { StatusCode=StatusCodes.Status200OK, data = assetdisposal,message = assetdisposal });
           
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAssetDisposalAsync([FromQuery] int PageNumber,[FromQuery] int PageSize,[FromQuery] string? SearchTerm = null)
        {
           var assetdisposal = await Mediator.Send(
            new GetAssetDisposalQuery
            {
                PageNumber = PageNumber, 
                PageSize = PageSize, 
                SearchTerm = SearchTerm
            });
      
           
            return Ok( new 
            { 
                StatusCode=StatusCodes.Status200OK, 
                data = assetdisposal.Data,
                TotalCount = assetdisposal.TotalCount,
                PageNumber = assetdisposal.PageNumber,
                PageSize = assetdisposal.PageSize
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateAssetDisposalCommand createAssetDisposalCommand)
        {
            
            var CreatedAssetDisposalId = await _mediator.Send(createAssetDisposalCommand);

           
            return Ok(new
            {
                StatusCode = StatusCodes.Status201Created,
                message ="Asset Disposal created successfully.",
                data = CreatedAssetDisposalId
            });
          
        
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAsync(UpdateAssetDisposalCommand updateAssetDisposalCommand )
        {
        
             await _mediator.Send(updateAssetDisposalCommand);
             
                return Ok(new
                    {
                        message = "Asset Disposal Updated Successfully.",
                        statusCode = StatusCodes.Status200OK
                    });
                
        }

        
    }
}