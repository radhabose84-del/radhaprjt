using FAM.Application.AssetMaster.AssetAdditionalCost.Queries.GetAssetAdditionalCostById;
using FAM.Application.AssetMaster.AssetTranferIssueApproval.Commands.UpdateAssetTranferIssueApproval;
using FAM.Application.AssetMaster.AssetTranferIssueApproval.Queries.GetAssetTransferIssueApproval;
using FAM.Application.AssetMaster.AssetTranferIssueApproval.Queries.GetAssetTransferIssueById;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FAM.Presentation.Controllers.AssetMaster
{
    [Route("api/[controller]")]
    public class AssetIssueTransferApproval : ApiControllerBase
    {
        private readonly ILogger<AssetIssueTransferApproval> _logger;
        private readonly IMediator _mediator;

        public AssetIssueTransferApproval(ILogger<AssetIssueTransferApproval> logger, IMediator mediator)
        : base(mediator)
        {
            _logger = logger;
            _mediator = mediator;
            
        }
        [HttpGet]
        public async Task<IActionResult> GetAllAssetIssueTransferPendingAsync([FromQuery] int PageNumber,[FromQuery] int PageSize,[FromQuery] string? TransferType = null,[FromQuery] DateTimeOffset? FromDate = null,
        [FromQuery] DateTimeOffset? ToDate = null)
        {
           var assetamc = await Mediator.Send(
            new GetAssetTranferIssueApprovalQuery
            {
                PageNumber = PageNumber, 
                PageSize = PageSize, 
                SearchTerm = TransferType,
                FromDate = FromDate,
                ToDate = ToDate
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

        [HttpGet("{id}")]
        [ActionName(nameof(GetByAssetTransferIdAsync))]
        public async Task<IActionResult> GetByAssetTransferIdAsync(int id)
        {
            var assettransfer = await Mediator.Send(new GetAssetTransferIssueByIdQuery() { Id = id});
          
              
              return Ok(new { StatusCode=StatusCodes.Status200OK, data = assettransfer,message = assettransfer });
           
        }
            [HttpPost("update-status")]
            public async Task<IActionResult> UpdateStatus([FromBody] UpdateAssetTranferIssueApprovalCommand command)
            {
           
                var response = await _mediator.Send(command);

                return Ok(new
                {
                    message = "Success",
                    statusCode = StatusCodes.Status200OK,
                    data = response
                });
            }
         
    }
}