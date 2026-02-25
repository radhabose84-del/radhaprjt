using FAM.Application.AssetMaster.AssetPurchase.Commands.CreateAssetPurchaseDetails;
using FAM.Application.AssetMaster.AssetPurchase.Commands.UpdateAssetPurchaseDetails;
using FAM.Application.AssetMaster.AssetPurchase.Queries.GetAssetGRN;
using FAM.Application.AssetMaster.AssetPurchase.Queries.GetAssetGrnDetails;
using FAM.Application.AssetMaster.AssetPurchase.Queries.GetAssetGRNItem;
using FAM.Application.AssetMaster.AssetPurchase.Queries.GetAssetPurchase;
using FAM.Application.AssetMaster.AssetPurchase.Queries.GetAssetPurchaseById;
using FAM.Application.AssetMaster.AssetPurchase.Queries.GetAssetSourceAutoComplete;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;


namespace FAM.Presentation.Controllers.AssetPurchase
{
    [Route("api/[controller]")]
    public class AssetPurchaseController : ApiControllerBase
    {
        private readonly ILogger<AssetPurchaseController> _logger;
        private readonly IMediator _mediator;

        public AssetPurchaseController(ILogger<AssetPurchaseController> logger, IMediator mediator
       )
         : base(mediator)
        {
            _logger = logger;
           _mediator = mediator;
        }

         [HttpGet("AssetSource/by-name")]
        public async Task<IActionResult> GetAssetSource([FromQuery] string? SourceName)
        {
        var assetsource = await Mediator.Send(new GetAssetSourceAutoCompleteQuery 
        { 
                SearchPattern = SourceName ?? string.Empty 
        });

        return Ok(new { StatusCode = StatusCodes.Status200OK, data = assetsource });
        }
        [HttpGet("{userName}")]
        public async Task<IActionResult> GetAssetUnitByUser(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                return BadRequest(new 
                { 
                    StatusCode = StatusCodes.Status400BadRequest, 
                    Message = "Username is required."
                });
            }

            var assetUnits = await _mediator.Send(new GetAssetUnitAutoCompleteQuery 
            { 
                Username = userName
            });

            return Ok(new 
            { 
                StatusCode = StatusCodes.Status200OK, 
                Data = assetUnits 
            });
        }


         [HttpGet("GetGrnNo/{oldUnitId}/{assetSourceId}")]
        public async Task<IActionResult> GetGrnNo(string oldUnitId,int assetSourceId, [FromQuery] string? searchGrnNo)
        {
            if (oldUnitId == null)
            {
                return BadRequest(new { StatusCode = StatusCodes.Status400BadRequest, Message = "Invalid OldUnitId" });
            }
            if (assetSourceId > 2)
            {
                return BadRequest(new { StatusCode = StatusCodes.Status400BadRequest, Message = "Invalid AssetSuourceId" });
            }

            var result = await _mediator.Send(new GetAssetGrnQuery { OldUnitId = oldUnitId,AssetSourceId = assetSourceId ,SearchGrnNo = searchGrnNo });

            
            return Ok(new { StatusCode = StatusCodes.Status200OK, Data = result });
        }
            [HttpGet("GetGrnItems/{oldUnitId}/{assetSourceId}/{grnNo}")]
            public async Task<IActionResult> GetGrnItems(string oldUnitId, int assetSourceId,  int grnNo)
            {
            if (oldUnitId == null)
            {
                return BadRequest(new { StatusCode = StatusCodes.Status400BadRequest, Message = "Invalid OldUnitId" });
            }
            if (assetSourceId > 2)
            {
                return BadRequest(new { StatusCode = StatusCodes.Status400BadRequest, Message = "Invalid AssetSuourceId" });
            }
                var query = new GetAssetGrnItemQuery { OldUnitId = oldUnitId,AssetSourceId = assetSourceId, GrnNo = grnNo };
                var result = await _mediator.Send(query);

              

                return Ok(new { StatusCode = StatusCodes.Status200OK, Data = result });
            }

            [HttpGet("GetGrnDetails/{oldUnitId}/{assetSourceId}/{grnNo}/{grnSerialNo}")]
            public async Task<IActionResult> GetGrnDetails(string oldUnitId,int assetSourceId,int grnNo, int grnSerialNo)
            {
                if (oldUnitId == null)
                {
                return BadRequest(new { StatusCode = StatusCodes.Status400BadRequest, Message = "Invalid OldUnitId" });
                }
                if (assetSourceId > 2)
                {
                return BadRequest(new { StatusCode = StatusCodes.Status400BadRequest, Message = "Invalid AssetSuourceId" });
                }
                var query = new GetAssetDetailsQuery { OldUnitId = oldUnitId,AssetSourceId = assetSourceId,GrnNo = grnNo, GrnSerialNo = grnSerialNo };
                var result = await _mediator.Send(query);

               

                return Ok(new { StatusCode = StatusCodes.Status200OK, Data = result }); 
            }

            [HttpPost]
            public async Task<IActionResult> CreateAsync(CreateAssetPurchaseDetailCommand createAssetPurchaseDetailCommand)
            {
                
                var CreatedAssetPurchaseDetailId = await _mediator.Send(createAssetPurchaseDetailCommand);

              
                
                return Ok(new
                {
                    StatusCode = StatusCodes.Status201Created,
                    message ="AssetPurchaseDetail Created Successfully",
                    data = CreatedAssetPurchaseDetailId
                });
             
            
            }

            [HttpPut]
            public async Task<IActionResult> UpdateAsync(UpdateAssetPurchaseDetailCommand updateAssetPurchaseDetailCommand)
            {
            

                     await _mediator.Send(updateAssetPurchaseDetailCommand);

                    
                    return Ok(new
                        {
                            message = "AssetPurchaseDetail Updated Successfully",
                            statusCode = StatusCodes.Status200OK
                        });
                  
            }

        [HttpGet("AssetPurchase/{id}")]
        [ActionName(nameof(GetByIdAsync))]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var assetpurchase = await Mediator.Send(new GetAssetPurchaseByIdQuery() { Id = id});
          
          
                
              return Ok(new { StatusCode=StatusCodes.Status200OK, data = assetpurchase,message = assetpurchase });
            
            
           
        }

         [HttpGet]
        public async Task<IActionResult> GetAllAssetPurchaseDetails([FromQuery] int PageNumber,[FromQuery] int PageSize,[FromQuery] string? SearchTerm = null)
        {
           var assetpurchase = await Mediator.Send(
            new GetAssetPurchaseQuery
            {
                PageNumber = PageNumber, 
                PageSize = PageSize, 
                SearchTerm = SearchTerm
            });
      
           
            return Ok( new 
            { 
                StatusCode=StatusCodes.Status200OK, 
                data = assetpurchase.Data,
                TotalCount = assetpurchase.TotalCount,
                PageNumber = assetpurchase.PageNumber,
                PageSize = assetpurchase.PageSize
                });
        }        
    }
}