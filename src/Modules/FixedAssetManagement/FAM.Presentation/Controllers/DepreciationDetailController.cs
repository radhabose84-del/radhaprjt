

using Contracts.Common;
using FAM.Application.DepreciationDetail.Commands.CreateDepreciationDetail;
using FAM.Application.DepreciationDetail.Commands.DeleteDepreciationDetail;
using FAM.Application.DepreciationDetail.Commands.UpdateDepreciationDetail;
using FAM.Application.DepreciationDetail.Queries.GetDepreciationAbstract;
using FAM.Application.DepreciationDetail.Queries.GetDepreciationDetail;
using FAM.Application.DepreciationDetail.Queries.GetDepreciationMethod;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FAM.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class DepreciationDetailController  : ApiControllerBase
    {
         public DepreciationDetailController(ISender mediator) 
        : base(mediator) { }
        [HttpGet]
        public async Task<IActionResult> DepreciationCalculateAsync( [FromQuery] int companyId, 
        [FromQuery] int unitId, 
        [FromQuery] int finYearId,
        [FromQuery] string startDate,
        [FromQuery] string endDate,
        [FromQuery] int depreciationType,
        [FromQuery] int pageNumber,
        [FromQuery] int pageSize,
        [FromQuery] string searchTerm,
        [FromQuery] int depreciationPeriod)
        { 
            // Convert string dates to DateTimeOffset?
            DateTimeOffset? parsedStartDate = null;
            DateTimeOffset? parsedEndDate = null;

            if (!string.IsNullOrWhiteSpace(startDate))  // Allow null or empty values
            {
                if (!DateTimeOffset.TryParse(startDate, out var parsedDate))
                {
                    return BadRequest(new { message = "Invalid startDate format. Use yyyy-MM-dd." });
                }
                parsedStartDate = parsedDate;
            }

            if (!string.IsNullOrWhiteSpace(endDate))  // Allow null or empty values
            {
                if (!DateTimeOffset.TryParse(endDate, out var parsedDate))
                {
                    return BadRequest(new { message = "Invalid endDate format. Use yyyy-MM-dd." });
                }
                parsedEndDate = parsedDate;
            }
             var assetMaster = await Mediator.Send(
                new GetDepreciationDetailQuery
                {
                    companyId=companyId,
                    unitId=unitId,
                    finYearId=finYearId,
                    startDate=parsedStartDate,
                    endDate=parsedEndDate,
                    depreciationType=depreciationType,
                    PageNumber = pageNumber, 
                    PageSize = pageSize, 
                    SearchTerm = searchTerm,                    
                    depreciationPeriod=depreciationPeriod
                });
            return Ok(new 
            { 
                StatusCode = StatusCodes.Status200OK, 
                message = assetMaster.Message,
                data = assetMaster.Data.ToList(),                
                TotalCount = assetMaster.TotalCount,
                PageNumber = assetMaster.PageNumber,
                PageSize = assetMaster.PageSize
            });
        }
         [HttpGet("Abstract")]
        public async Task<IActionResult> DepreciationAbstractAsync( [FromQuery] int companyId, 
        [FromQuery] int unitId,[FromQuery] int finYearId,[FromQuery] string startDate,[FromQuery] string endDate,[FromQuery] int depreciationPeriod,[FromQuery] int depreciationType)
        {            
            DateTimeOffset? parsedStartDate = null;
            DateTimeOffset? parsedEndDate = null;

            if (!string.IsNullOrWhiteSpace(startDate))  // Allow null or empty values
            {
                if (!DateTimeOffset.TryParse(startDate, out var parsedDate))
                {
                    return BadRequest(new { message = "Invalid startDate format. Use yyyy-MM-dd." });
                }
                parsedStartDate = parsedDate;
            }

            if (!string.IsNullOrWhiteSpace(endDate))  // Allow null or empty values
            {
                if (!DateTimeOffset.TryParse(endDate, out var parsedDate))
                {
                    return BadRequest(new { message = "Invalid endDate format. Use yyyy-MM-dd." });
                }
                parsedEndDate = parsedDate;
            } 
             var assetMaster = await Mediator.Send(
                new GetDepreciationAbstractQuery
                {
                    companyId=companyId,
                    unitId=unitId,
                    finYearId=finYearId,
                    startDate=parsedStartDate,
                    endDate=parsedEndDate,
                    depreciationPeriod=depreciationPeriod,
                    depreciationType=depreciationType                    
                });
            return Ok(new 
            { 
                StatusCode = StatusCodes.Status200OK, 
                message = assetMaster,
                data = assetMaster?.ToList()               
            });
        }
        [HttpPost]               
        public async Task<IActionResult> CreateAsync(CreateDepreciationDetailCommand  command)
        {             
            var result = await Mediator.Send(command);
           return Ok(new ApiResponseDTO<string>
            {
                IsSuccess = true,
                Message = result,
                Data = result,
                StatusCode = StatusCodes.Status201Created
            });
        }        
        [HttpDelete]        
        public async Task<IActionResult> DeleteAsync(DeleteDepreciationDetailCommand  command)
        {             
            var result = await Mediator.Send(new DeleteDepreciationDetailCommand { companyId=command.companyId,unitId=command.unitId,finYearId=command.finYearId,depreciationType=command.depreciationType,depreciationPeriod=command.depreciationPeriod});                 
           
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data =$"Depreciation Details Deleted" ,
                message = result
            });
        }
         [HttpPut]        
        public async Task<IActionResult> UpdateAsync(DeleteDepreciationDetailCommand  command)
        {             
            var result = await Mediator.Send(new UpdateDepreciationDetailCommand { companyId=command.companyId,unitId=command.unitId,finYearId=command.finYearId,depreciationType=command.depreciationType,depreciationPeriod=command.depreciationPeriod});                 
           
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data =$"Depreciation Details Locked" ,
                message = result
            });
        }            // GET: api/AssetMasterGeneral/WorkingStatus
       
        [HttpGet("DeprecationPeriod")]
        public async Task<IActionResult> GetDepreciationMethod()
        {
            var result = await Mediator.Send(new GetDepreciationMethodQuery());
          
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "Working Status fetched successfully.",
                data = result.Data
            });
        }                 
    }
}