using FAM.Application.AssetMaster.AssetTransferIssue.Command.CreateAssetTransferIssue;
using FAM.Application.AssetMaster.AssetTransferIssue.Command.UpdateAssetTransferIssue;
using FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAllAssetTransfer;
using FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAssertByCategory;
using FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAssetCustodian;
using FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAssetDtlToTransfer;
using FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAssetTranferedById;
using FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAssetTransfered;
using FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetBulkAssetToTransfer;
using FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetCategoryByCustodian;
using FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetCategoryByDeptId;
using FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetTransferType;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetTransferIssue;
using FAM.Domain.Entities.AssetMaster;
using FluentValidation;
using MassTransit.Futures.Contracts;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;



namespace FAM.Presentation.Controllers.AssetMaster
{
    [Route("api/[controller]")]
    public class AssetTransferController : ApiControllerBase

    {

        private readonly IAssetTransferQueryRepository _assetTransferQueryRepository;

        public AssetTransferController(ISender mediator, IAssetTransferQueryRepository assetTransferQueryRepository) 
        : base(mediator)
        {
            _assetTransferQueryRepository = assetTransferQueryRepository;

        }

        [HttpGet("GetAllAssetTransfers")]
        public async Task<IActionResult> GetAllAsync([FromQuery] int PageNumber, [FromQuery] int PageSize, [FromQuery] DateTimeOffset? FromDate = null, [FromQuery] DateTimeOffset? ToDate = null, [FromQuery] string? SearchTerm = null)
        {
            var assetTransferList = await Mediator.Send(
                new AssetTransferQuery
                {
                    PageNumber = PageNumber,
                    PageSize = PageSize,
                    SearchTerm = SearchTerm,
                    FromDate = FromDate,
                    ToDate = ToDate

                });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Data = assetTransferList.Data,
                TotalCount = assetTransferList.TotalCount,
                PageNumber = assetTransferList.PageNumber,
                PageSize = assetTransferList.PageSize
            });
        }

        [HttpGet("GetAllAssetTransfersByAssetTransferId/{id}")]
        public async Task<IActionResult> GetAllAssetTransfersAsync(int id)
        {
            var query = new GetAllTransferQuery { AssetTransferId = id };
            var result = await Mediator.Send(query);
            if (result == null)
            {
                return NotFound($"Asset Transfer with ID {id} not found.");
            }
            return Ok(new {
                StatusCode = StatusCodes.Status200OK,
                Data = result
            });
        }


        [HttpGet("GetCategoryByDepartmentId/{id}")]
        public async Task<IActionResult> GetCategoriesByDepartmentAsync(int id)
        {
            var assetCategoryList = await Mediator.Send(
                new GetCategoryByDeptIQuery
                {
                    DepartmentId = id

                });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Data = assetCategoryList,

            });
        }


        [HttpPost]
        // public async Task<ActionResult<ApiResponseDTO<AssetTransferIssueHdr>>> CreateAssetTransfer([FromBody] CreateAssetTransferIssueCommand command)
        public async Task<IActionResult> CreateAsync(CreateAssetTransferIssueCommand command)
        {

           
            if (command == null)
                return BadRequest(new ApiResponseDTO<AssetTransferIssueHdr>
                {
                    IsSuccess = false,
                    Message = "Invalid request data"
                });

            var response = await Mediator.Send(command);
          
                return StatusCode(StatusCodes.Status201Created, new
                {
                    StatusCode = StatusCodes.Status201Created,
                    Message = "Asset Transfer Created Successfully",
                    Data = response
                });
           
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAssetTransferByIdAsync(int id)
        {
            var query = new GetAssetTranferedByIdQuery { AssetTransferId = id };
            var result = await Mediator.Send(query);
            if (result == null)
            {
                return NotFound($"Asset Transfer with ID {id} not found.");
            }
            return Ok(new
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = result,
                    Data = result
                });
        }




        [HttpPut]
        public async Task<IActionResult> UpdateAssetTransferIssue([FromBody] UpdateAssetTransferIssueCommand command)
        {


             await Mediator.Send(command);


            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "Asset Transfer Updated Successfully",
                errors = ""
            });
        }

        [HttpGet("GetAssetsByCategory/{categoryId}/{assetDepartmentId}")]
        public async Task<IActionResult> GetAssetsByCategoryAsync(int categoryId, int assetDepartmentId)
        {
            var query = new GetAssetsByCategoryQuery { AssetCategoryId = categoryId, AssetDepartmentId = assetDepartmentId };
            var result = await Mediator.Send(query);
            return StatusCode(StatusCodes.Status200OK, new
            {
                statusCode = StatusCodes.Status200OK,
                message = "Assets retrieved successfully.",
                data = result
            });

        }
        [HttpGet("GetAssetDetailsToTransfer/{AssetId}")]
        public async Task<IActionResult> GetAssetDetailsToTransferByIdAsync(int AssetId)
        {

            // 🔹 Check if the asset is pending or approved (with AckStatus <> 1)
            bool isRestricted = await _assetTransferQueryRepository.IsAssetPendingOrApprovedAsync(AssetId);

            if (isRestricted)
            {
                var errorResponse = new ApiResponseDTO<object>
                {
                    IsSuccess = false,
                    Message = $"Asset ID {AssetId} is in 'Pending' or 'Approved' state with unacknowledged status.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                };
                return BadRequest(errorResponse);
            }

            var query = new GetAssetDetailsToTransferQuery { AssetId = AssetId };
            var result = await Mediator.Send(query);
           
                  return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "Asset details retrieved successfully.",
                data = result
            }); 

        }



        [HttpGet("TransferTypes")]
        public async Task<IActionResult> GetDisposalTypes()
        {
            var result = await Mediator.Send(new GetTransferTypeQuery());
            if (result == null || result.Data == null || result.Data.Count == 0)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    message = "No Transfer Types found."
                });
            }
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "Transfer Types fetched successfully.",
                data = result.Data
            });
        }


        [HttpGet("GetCustodiansByDepartment")]
        public async Task<IActionResult> GetCustodiansByDepartment([FromQuery] int departmentId, [FromQuery] string oldUnitId)
        {
            if (string.IsNullOrWhiteSpace(oldUnitId))
            {
                return BadRequest(new ApiResponseDTO<List<GetAssetCustodianDto>>
                {
                    IsSuccess = false,
                    Message = "OldUnitId not found in user token.",
                    Data = null
                });
            }

            var query = new GetAssetCustodianQuery
            {
                OldUnitId = oldUnitId,
                DepartmentId = departmentId
            };

            var response = await Mediator.Send(query);

            
            return Ok(new 
            {
                StatusCode = StatusCodes.Status200OK,
                data =response
            });
        }

        [HttpGet("GetCategoriesByCustodian")]
       
        public async Task<IActionResult> GetCategoriesByCustodianAsync([FromQuery] int departmentId, [FromQuery] string custodianId)
        {
            var assetCategoryList = await Mediator.Send(
                new GetCategoryByCustodianQuery
                {
                    DepartmentId = departmentId,
                    CustodianId = custodianId

                });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Data = assetCategoryList,

            });
        }
   
        [HttpGet("GetBulkAssetsToTransfer")]
        public async Task<IActionResult> GetBulkAssetsToTransfer(int departmentId, string custodianId, string categoryId)
        {
            if (string.IsNullOrWhiteSpace(custodianId))
            {
                var errorResponse = new ApiResponseDTO<List<GetAssetDetailsToTransferHdrDto>>
                {
                    IsSuccess = false,
                    Message = "CustodianId is required.",
                    Data = null,
                    StatusCode = 400,
                    Errors = new List<string> { "CustodianId cannot be null or empty." }
                };
                return BadRequest(errorResponse);
            }

            if (string.IsNullOrWhiteSpace(categoryId))
            {
                var errorResponse = new ApiResponseDTO<List<GetAssetDetailsToTransferHdrDto>>
                {
                    IsSuccess = false,
                    Message = "CategoryId is required.",
                    Data = null,
                    StatusCode = 400,
                    Errors = new List<string> { "CategoryId cannot be null or empty." }
                };
                return BadRequest(errorResponse);
            }

            var query = new GetBulkAssetToTransferQuery
            {
                DepartmentId = departmentId,
                CustodianId = custodianId,
                CategoryID = categoryId
            };

            var response = await Mediator.Send(query);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = response
            });
        }

        
        
    }
}