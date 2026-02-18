using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.AssetMaster.AssetTranferIssueApproval.Queries.GetAssetTransferIssueById;
using FAM.Application.AssetMaster.AssetTransferReceipt.Command.CreateAssetTransferReceipt;
using FAM.Application.AssetMaster.AssetTransferReceipt.Queries.GetAssetIssueDetailsById;
using FAM.Application.AssetMaster.AssetTransferReceipt.Queries.GetAssetReceiptDetails;
using FAM.Application.AssetMaster.AssetTransferReceipt.Queries.GetAssetReceiptDetailsById;
using FAM.Application.AssetMaster.AssetTransferReceipt.Queries.GetAssetReceiptPending;
using FAM.Application.AssetMaster.AssetTransferReceipt.Queries.GetAssetRecieptDtlPending;
using Contracts.Common;
using FAM.Domain.Entities.AssetMaster;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FAM.Presentation.Controllers.AssetMaster
{
    [Route("api/[controller]")]
    public class AssetTransferReceiptController : ApiControllerBase
    {
        private readonly ILogger<AssetTransferReceiptController> _logger;
         private readonly IMediator _mediator;

        public AssetTransferReceiptController(ILogger<AssetTransferReceiptController> logger,IMediator mediator
        )
        :base(mediator)
        {
            _logger = logger;
            _mediator=mediator;
        }

        [HttpGet("GetAssetTransferReceiptPending")]
        public async Task<IActionResult> GetAssetTransferReceiptPendingAsync([FromQuery] int PageNumber,[FromQuery] int PageSize,[FromQuery] int? AssetTransferId = null,[FromQuery] string? TransferType = null,[FromQuery] DateTimeOffset? FromDate = null,
        [FromQuery] DateTimeOffset? ToDate = null)
        {
           var assetamc = await Mediator.Send(
            new GetAssetReceiptPendingQuery
            {
                PageNumber = PageNumber, 
                PageSize = PageSize, 
                AssetTransferId = AssetTransferId,
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

        [HttpGet("GetAssetTransferReceiptDtlPending/{id}")]
        public async Task<IActionResult> GetAssetTransferReceiptDtlPendingAsync(int id)
        {
            var query = new GetAssetRecieptDtlPendingQuery { AssetTransferId = id };
            var result = await Mediator.Send(query);
            if (result == null)
            {
                return NotFound($"Asset Transfer with ID {id} not found.");
            }
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Data = result
            });
        }
              

        [HttpGet]
        public async Task<IActionResult> GetAssetTransferReceiptDetails([FromQuery] int PageNumber,[FromQuery] int PageSize,[FromQuery] string? AssetReceiptId = null,[FromQuery] DateTimeOffset? FromDate = null,
        [FromQuery] DateTimeOffset? ToDate = null)
        {
           var assetamc = await Mediator.Send(
            new GetAssetReceiptDetailsQuery
            {
                PageNumber = PageNumber, 
                PageSize = PageSize, 
                SearchTerm = AssetReceiptId,
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


        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateAssetTransferReceiptCommand  createAssetTransferReceiptCommand)
        {
             
            var CreatedAssetReceiptId = await _mediator.Send(createAssetTransferReceiptCommand);

            
            return Ok(new
            {
                StatusCode = StatusCodes.Status201Created,
                message =CreatedAssetReceiptId,
                data = CreatedAssetReceiptId
            });
           
        }


        [HttpGet("{id}")]
        [ActionName(nameof(GetByAssetTransferReceiptIdAsync))]
        public async Task<IActionResult> GetByAssetTransferReceiptIdAsync(int id)
        {
            var assetreceipt = await Mediator.Send(new GetAssetReceiptDetailsByIdQuery() { AssetReceiptId = id});
          
           
                
              return Ok(new { StatusCode=StatusCodes.Status200OK, data = assetreceipt,message = assetreceipt });
            
            
           
        }

        

      
    }
}