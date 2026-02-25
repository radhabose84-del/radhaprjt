using MaintenanceManagement.Application.MaintenanceCategory.Command.CreateMaintenanceCategory;
using MaintenanceManagement.Application.MaintenanceCategory.Command.DeleteMaintenanceCategory;
using MaintenanceManagement.Application.MaintenanceCategory.Command.UpdateMaintenanceCategory;
using MaintenanceManagement.Application.MaintenanceCategory.Queries.GetMaintenanceCategory;
using MaintenanceManagement.Application.MaintenanceCategory.Queries.GetMaintenanceCategoryAutoComplete;
using MaintenanceManagement.Application.MaintenanceCategory.Queries.GetMaintenanceCategoryById;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MaintenanceManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class MaintenanceCategoryController : ApiControllerBase
    {
        
        private readonly IMediator _mediator;


        public MaintenanceCategoryController(IMediator mediator)
        : base(mediator)
        {
            
            _mediator=mediator;
        }
         [HttpGet]
        public async Task<IActionResult> GetAllMaintenanceCategoryAsync([FromQuery] int PageNumber,[FromQuery] int PageSize,[FromQuery] string? SearchTerm = null)
        {
           var maintenanceCategory = await Mediator.Send(
            new GetMaintenanceCategoryQuery
            {
                PageNumber = PageNumber, 
                PageSize = PageSize, 
                SearchTerm = SearchTerm
            });
            return Ok( new 
            { 
                StatusCode=StatusCodes.Status200OK, 
                data = maintenanceCategory.Data,
                TotalCount = maintenanceCategory.TotalCount,
                PageNumber = maintenanceCategory.PageNumber,
                PageSize = maintenanceCategory.PageSize
                });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetMaintenanceCategory([FromQuery] string? CateggoryName)
        {
        var maintenancetype = await Mediator.Send(new GetMaintenanceCategoryAutoCompleteQuery 
        { 
                SearchPattern = CateggoryName ?? string.Empty 
        });

        return Ok(new { StatusCode = StatusCodes.Status200OK, data = maintenancetype});
        }

        [HttpGet("{id}")]
        [ActionName(nameof(GetByIdAsync))]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var maintenancecategory = await Mediator.Send(new GetMaintenanceCategoryByIdQuery() { Id = id});
          
            
                
              return Ok(new { StatusCode=StatusCodes.Status200OK, data = maintenancecategory,message = maintenancecategory });
            
            
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateMaintenanceCategoryCommand createMaintenanceCategoryCommand)
        {
            // Process the command
            var CreatedMaintenanceId = await _mediator.Send(createMaintenanceCategoryCommand);

         
            return Ok(new
            {
                StatusCode = StatusCodes.Status201Created,
                message ="MaintenanceCategory Created Successfully",
                data = CreatedMaintenanceId
            });
        
        }
            [HttpPut]
            public async Task<IActionResult> UpdateAsync(UpdateMaintenanceCategoryCommand updateMaintenanceCategoryCommand)
            {
            
                     await _mediator.Send(updateMaintenanceCategoryCommand);
                        
                    return Ok(new
                        {
                            message = "MaintenanceCategory Updated Successfully",
                            statusCode = StatusCodes.Status200OK
                        });
                            
            }

            [HttpDelete]
            public async Task<IActionResult> DeleteMaintenanceTypeAsync(int id)
            {
               
                     await _mediator.Send(new DeleteMaintenanceCategoryCommand { Id = id });
                        
                        return Ok(new
                        {
                            message = "MaintenanceCategory Deleted Successfully",
                            statusCode = StatusCodes.Status200OK
                        });
            }

      
    }
}