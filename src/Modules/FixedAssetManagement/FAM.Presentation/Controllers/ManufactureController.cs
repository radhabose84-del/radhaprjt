using FAM.Application.Manufacture.Commands.CreateManufacture;
using FAM.Application.Manufacture.Commands.UpdateManufacture;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using FAM.Application.Manufacture.Queries.GetManufacture;
using FAM.Application.Manufacture.Queries.GetManufactureById;
using FAM.Application.Manufacture.Commands.DeleteManufacture;
using FAM.Application.Manufacture.Queries.GetManufactureAutoComplete;
using FAM.Application.DepreciationGroup.Queries.GetManufactureTypeQuery;
using Microsoft.AspNetCore.Http;

namespace FAM.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class ManufactureController  : ApiControllerBase
    { 
         
         
         
       public ManufactureController(ISender mediator) 
        : base(mediator)
        {                      
        }

        [HttpGet]                
        public async Task<IActionResult> GetAllCitiesAsync([FromQuery] int PageNumber,[FromQuery] int PageSize,[FromQuery] string? SearchTerm = null)
        {            
            var manufactures = await Mediator.Send(
            new GetManufactureQuery
            {
                PageNumber = PageNumber, 
                PageSize = PageSize, 
                SearchTerm = SearchTerm
            });
            return Ok(new 
            { 
                StatusCode=StatusCodes.Status200OK, 
                message = manufactures.Message,
                data = manufactures.Data!.ToList(),
                TotalCount = manufactures.TotalCount,
                PageNumber = manufactures.PageNumber,
                PageSize = manufactures.PageSize
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
                    message = "Invalid Manufacture ID" 
                });
            }
            var result = await Mediator.Send(new GetManufactureByIdQuery { Id = id });            
          
            return Ok(new 
            {
                StatusCode=StatusCodes.Status200OK,
                data = result
            });   
        }

        [HttpPost]               
        public async Task<IActionResult> CreateAsync(CreateManufactureCommand  command)
        { 
                 
            var result = await Mediator.Send(command);
           
                return Ok(new 
                { 
                    StatusCode=StatusCodes.Status201Created,
                    message = "Manufacture Created Successfully", 
                    data = result
                });
           
        }
        [HttpPut]        
        public async Task<IActionResult> UpdateAsync(UpdateManufactureCommand command)
        {         
                     
            var result = await Mediator.Send(command);
         
                return Ok(new 
                {   StatusCode=StatusCodes.Status200OK,
                    message = "Manufacture Updated Successfully", 
                    manufacture = result
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
                    message = "Invalid Country ID"
                });
            }            
               await Mediator.Send(new DeleteManufactureCommand { Id = id });                 
          
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data =$"Manufacture ID {id} Deleted" ,
                message = "Manufacture Deleted Successfully"
            });
        }
             
        [HttpGet("by-name")]  
        public async Task<IActionResult> GetManufacture([FromQuery] string name)
        {          
            var result = await Mediator.Send(new GetManufactureAutoCompleteQuery {SearchPattern = name}); // Pass `searchPattern` to the constructor
          
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = result,
                data = result
            });
        }
        [HttpGet("ManufactureType")]
        public async Task<IActionResult> GetManufactureTypes()
        {
            var result = await Mediator.Send(new GetManufactureTypeQuery());

            if (result == null || result.Data == null || result.Data.Count == 0)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    message = "No Manufacture Type found."
                });
            }
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "Manufacture Type fetched successfully.",
                data = result.Data
            });
        }     
    }
}