using FAM.Application.AssetMaster.AssetSpecification.Commands.CreateAssetSpecification;
using FAM.Application.AssetMaster.AssetSpecification.Commands.DeleteAssetSpecification;
using FAM.Application.AssetMaster.AssetSpecification.Commands.UpdateAssetSpecification;
using FAM.Application.AssetMaster.AssetSpecification.Queries.GetAssetSpecification;
using FAM.Application.AssetMaster.AssetSpecification.Queries.GetAssetSpecificationAutoComplete;
using FAM.Application.AssetMaster.AssetSpecification.Queries.GetAssetSpecificationBasedMachineNo;
using FAM.Application.AssetMaster.AssetSpecification.Queries.GetAssetSpecificationById;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FAM.API.Controllers.AssetMaster
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssetSpecificationController : ApiControllerBase
    {
       


        public AssetSpecificationController(ISender mediator)
            : base(mediator)
        {
            
        }
        [HttpGet]
        public async Task<IActionResult> GetAllAssetSpecificationAsync([FromQuery] int PageNumber, [FromQuery] int PageSize, [FromQuery] string? SearchTerm = null)
        {
            var specificationMaster = await Mediator.Send(
            new GetAssetSpecificationQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm
            });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = specificationMaster.Message,
                data = specificationMaster.Data.ToList(),
                TotalCount = specificationMaster.TotalCount,
                PageNumber = specificationMaster.PageNumber,
                PageSize = specificationMaster.PageSize
            });
        }

        [HttpGet("{id}")]
        [ActionName(nameof(GetByIdAsync))]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = "Invalid SpecificationMaster Id"
                });
            }
            var result = await Mediator.Send(new GetAssetSpecificationByIdQuery { Id = id });
          
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateAssetSpecificationCommand command)
        {
           
            var result = await Mediator.Send(command);
         
                return Ok(new
                {
                    StatusCode = StatusCodes.Status201Created,
                    message = "Asset Specification Created Successfully",
                    data = result
                });
            
        }
        [HttpPut]
        public async Task<IActionResult> UpdateAsync(UpdateAssetSpecificationCommand command)
        {
            
            var result = await Mediator.Send(command);
           
                return Ok(new
                {
                    StatusCode = StatusCodes.Status200OK,
                    message = "Asset Specification Updated Successfully",
                    asset = result
                });
           

        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            
          
            if (id <= 0)
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = "Invalid Asset ID"
                });
            }
             await Mediator.Send(new DeleteAssetSpecificationCommand { Id = id });
         
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = $"Asset Specification ID {id} Deleted",
                message = "Asset Specification Deleted Successfully"
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetAssetSpecification([FromQuery] string? name)
        {
            var result = await Mediator.Send(new GetAssetSpecificationAutoCompleteQuery { SearchPattern = name }); // Pass `searchPattern` to the constructor
           
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = result,
                data = result
            });
        }
         [HttpGet("GetAllAssetSpecificationBasedOnMachineNo")]                
        public async Task<IActionResult> GetAllAssetSpecificationBasedOnMachineNo([FromQuery] int PageNumber,[FromQuery] int PageSize,[FromQuery] string? SearchTerm = null)
        {            
            var specificationMaster = await Mediator.Send(
            new GetAssetSpecificationBasedMachineNoQuery
            {
                PageNumber = PageNumber, 
                PageSize = PageSize, 
                SearchTerm = SearchTerm
            });
            return Ok(new 
            { 
                StatusCode=StatusCodes.Status200OK, 
                message = specificationMaster.Message,
                data = specificationMaster.Data.ToList(),
                TotalCount = specificationMaster.TotalCount,
                PageNumber = specificationMaster.PageNumber,
                PageSize = specificationMaster.PageSize
            });
        }      
    }    
}