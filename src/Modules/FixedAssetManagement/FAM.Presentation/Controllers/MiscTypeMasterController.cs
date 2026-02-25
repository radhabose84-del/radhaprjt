using Microsoft.AspNetCore.Mvc;
using MediatR;
using FAM.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using FAM.Application.MiscTypeMaster.Queries.GetMiscTypeMasterById;
using FAM.Application.MiscTypeMaster.Queries.GetMiscTypeMasterAutoComplete;
using FAM.Application.MiscTypeMaster.Command.CreateMiscTypeMaster;
using FAM.Application.MiscTypeMaster.Command.UpdateMiscTypeMaster;
using FAM.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster;
using Microsoft.AspNetCore.Http;

namespace FAM.Presentation.Controllers
{
    [Route("api/fam/[controller]")]
    public class MiscTypeMasterController : ApiControllerBase
    {

      

          
          public MiscTypeMasterController(ISender mediator
          ) 
          :base(mediator)
          {
          }      
    
      [HttpGet] 
          public async Task<IActionResult> GetAllMiscTypeMasterAsync([FromQuery] int PageNumber,[FromQuery] int PageSize,[FromQuery] string? SearchTerm = null)
        {
            var misctypemaster = await Mediator.Send(
            new GetMiscTypeMasterQuery
            {
                PageNumber = PageNumber, 
                PageSize = PageSize, 
                SearchTerm = SearchTerm
            });
           // var activecompanies = companies.Data.ToList(); 

            return Ok(new 
            { 
                StatusCode=StatusCodes.Status200OK, 
                data = misctypemaster.Data,
                TotalCount = misctypemaster.TotalCount,
                PageNumber = misctypemaster.PageNumber,
                PageSize = misctypemaster.PageSize
            });
        }

         [HttpGet("{id}")]
         [ActionName(nameof(GetByIdAsync))]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
           
            var misctypemaster = await Mediator.Send(new GetMiscTypeMasterByIdQuery() { Id = id});
          
           
                 return Ok(new { StatusCode=StatusCodes.Status200OK, data = misctypemaster,message=misctypemaster});
   
           
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetMiscTypeMaster([FromQuery] string? name)
        {
          
            var misctypemaster = await Mediator.Send(new GetMiscTypeMasterAutoCompleteQuery {SearchPattern = name});
          
            return Ok( new { StatusCode=StatusCodes.Status200OK, data = misctypemaster });
        
        }

          [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateMiscTypeMasterCommand command)
        {
        
          
            var response = await Mediator.Send(command);
                                     
                return Ok(new 
                {
                     StatusCode=StatusCodes.Status201Created,
                 message = "Successfully Created",
                  errors = "",
                  data = response 
                  });
        
            
        } 


        [HttpPut]
        public async Task<IActionResult> Update(UpdateMiscTypeMasterCommand command )
        {
                  

             var misctypeExists = await Mediator.Send(new GetMiscTypeMasterByIdQuery { Id = command.Id });

             if (misctypeExists == null)
             {
                 return NotFound(new { StatusCode=StatusCodes.Status404NotFound, message = $"MiscTypeMaster ID {command.Id} not found.", errors = "" }); 
             }

              await Mediator.Send(command);
            
                 return Ok(new { StatusCode=StatusCodes.Status200OK, message = "Successfully Updated", errors = "" });
           
        }

         [HttpDelete("{id}")]
        
        public async Task<IActionResult> Delete(int id)
        {
           
            await Mediator.Send(new DeleteMiscTypeMasterCommand { Id = id });

          
            return Ok(new { StatusCode=StatusCodes.Status200OK, message = "Successfully Deleted", errors = "" });
              
            
        }

    }
}