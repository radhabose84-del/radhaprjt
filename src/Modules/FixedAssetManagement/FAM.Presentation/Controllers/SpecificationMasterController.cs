using FAM.Application.SpecificationMaster.Commands.CreateSpecificationMaster;
using FAM.Application.SpecificationMaster.Commands.DeleteSpecificationMaster;
using FAM.Application.SpecificationMaster.Commands.UpdateSpecificationMaster;
using FAM.Application.SpecificationMaster.Queries.GetSpecificationMaster;
using FAM.Application.SpecificationMaster.Queries.GetSpecificationMasterAutoComplete;
using FAM.Application.SpecificationMaster.Queries.GetSpecificationMasterById;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FAM.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class SpecificationMasterController  : ApiControllerBase
    {
        
        
        
    public SpecificationMasterController(ISender mediator) 
        : base(mediator)
        {        
         
        }
        [HttpGet]                
        public async Task<IActionResult> GetAllSpecificationMasterAsync([FromQuery] int PageNumber,[FromQuery] int PageSize,[FromQuery] string? SearchTerm = null)
        {            
            var specificationMaster = await Mediator.Send(
            new GetSpecificationMasterQuery
            {
                PageNumber = PageNumber, 
                PageSize = PageSize, 
                SearchTerm = SearchTerm
            });
            return Ok(new 
            { 
                StatusCode=StatusCodes.Status200OK, 
                message = specificationMaster.Message,
                data = specificationMaster.Data!.ToList(),
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
                    StatusCode=StatusCodes.Status400BadRequest,
                    message = "Invalid SpecificationMaster Id" 
                });
            }
            var result = await Mediator.Send(new GetSpecificationMasterByIdQuery { Id = id });            
          
            return Ok(new 
            {
                StatusCode=StatusCodes.Status200OK,
                data = result
            });   
        }

        [HttpPost]               
        public async Task<IActionResult> CreateAsync(CreateSpecificationMasterCommand  command)
        { 
                
            var result = await Mediator.Send(command);
          
                return Ok(new 
                { 
                    StatusCode=StatusCodes.Status201Created,
                    message = "Successfully Created", 
                    data = result
                });
          
        }
        [HttpPut]        
        public async Task<IActionResult> UpdateAsync(UpdateSpecificationMasterCommand command)
        {         
                  
            var result = await Mediator.Send(command);
          
                return Ok(new 
                {   StatusCode=StatusCodes.Status200OK,
                    message = "Successfully Updated", 
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
             await Mediator.Send(new DeleteSpecificationMasterCommand { Id = id });                 
           
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data =$"SpecificationMaster ID {id} Deleted" ,
                message = "Successfully Deleted"
            });
        }
            
        [HttpGet("by-name")]
        public async Task<IActionResult> GetSpecificationMaster([FromQuery] int assetGroupId, [FromQuery] string? name = null)
        {
            var result = await Mediator.Send(new GetSpecificationMasterAutoCompleteQuery {AssetGroupId=assetGroupId,SearchPattern = name}); // Pass `searchPattern` to the constructor
          
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = result,
                data = result
            });
        }      
    }    
}