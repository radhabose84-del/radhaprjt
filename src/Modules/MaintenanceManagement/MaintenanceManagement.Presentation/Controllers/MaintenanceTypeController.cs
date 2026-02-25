using MaintenanceManagement.Application.MaintenanceType.Command.CreateMaintenanceType;
using MaintenanceManagement.Application.MaintenanceType.Command.DeleteMaintenanceType;
using MaintenanceManagement.Application.MaintenanceType.Command.UpdateMaintenanceType;
using MaintenanceManagement.Application.MaintenanceType.Queries.GetMaintenanceType;
using MaintenanceManagement.Application.MaintenanceType.Queries.GetMaintenanceTypeAutoComplete;
using MaintenanceManagement.Application.MaintenanceType.Queries.GetMaintenanceTypeById;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MaintenanceManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class MaintenanceTypeController : ApiControllerBase
    {
        
          private readonly IMediator _mediator;
        
        public MaintenanceTypeController(IMediator mediator)
        : base(mediator)
        {
            _mediator=mediator;
        }

         [HttpGet]
        public async Task<IActionResult> GetAllMaintenanceTypeAsync([FromQuery] int PageNumber,[FromQuery] int PageSize,[FromQuery] string? SearchTerm = null)
        {
           var maintenancetype = await Mediator.Send(
            new GetMaintenanceTypeQuery
            {
                PageNumber = PageNumber, 
                PageSize = PageSize, 
                SearchTerm = SearchTerm
            });
            return Ok( new 
            { 
                StatusCode=StatusCodes.Status200OK, 
                data = maintenancetype.Data,
                TotalCount = maintenancetype.TotalCount,
                PageNumber = maintenancetype.PageNumber,
                PageSize = maintenancetype.PageSize
                });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetMaintenanceType([FromQuery] string? Typename)
        {
        var maintenancetype = await Mediator.Send(new GetMaintenanceTypeAutoCompleteQuery 
        { 
                SearchPattern = Typename ?? string.Empty 
        });

        return Ok(new { StatusCode = StatusCodes.Status200OK, data = maintenancetype});
        }

        [HttpGet("{id}")]
        [ActionName(nameof(GetByIdAsync))]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var maintenancetype = await Mediator.Send(new GetMaintenanceTypeByIdQuery() { Id = id});
                
              return Ok(new { StatusCode=StatusCodes.Status200OK, data = maintenancetype,message = maintenancetype });
            
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateMaintenanceTypeCommand createMaintenanceTypeCommand)
        {
          
            var CreatedMaintenanceId = await _mediator.Send(createMaintenanceTypeCommand);
            
            return Ok(new
            {
                StatusCode = StatusCodes.Status201Created,
                message ="MaintenanceType Created Successfully",
                data = CreatedMaintenanceId
            });
           
        
        }
            [HttpPut]
            public async Task<IActionResult> UpdateAsync(UpdateMaintenanceTypeCommand updateMaintenanceTypeCommand)
            {
            
                await _mediator.Send(updateMaintenanceTypeCommand);
                     
                    return Ok(new
                        {
                            message = "MaintenanceType Updated Successfully",
                            statusCode = StatusCodes.Status200OK
                        });    
            }

            [HttpDelete]
            public async Task<IActionResult> DeleteMaintenanceTypeAsync(int id)
            {
               
                    var result = await _mediator.Send(new DeleteMaintenanceTypeCommand { Id = id });

                        return Ok(new
                        {
                            message = "MaintenanceType Deleted Successfully",
                            statusCode = StatusCodes.Status200OK
                        });
                  
            }

       
    }
}