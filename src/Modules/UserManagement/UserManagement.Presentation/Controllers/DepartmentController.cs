#nullable disable
using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.Departments.Queries.GetDepartments;
using UserManagement.Application.Departments.Queries.GetDepartmentById;
using UserManagement.Application.Departments.Commands.CreateDepartment;
using UserManagement.Application.Departments.Commands.UpdateDepartment;
using UserManagement.Application.Departments.Commands.DeleteDepartment;
using UserManagement.Application.Departments.Queries.GetDepartmentAutoCompleteSearch;
using UserManagement.Application.Departments.Queries.GetDepartmentByDepartmentGroupId;
using UserManagement.Application.Departments.Queries.GetDepartmentByGroupWithControl;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace UserManagement.Presentation.Controllers
{
    [Route("api/[controller]")]

    public class DepartmentController : ApiControllerBase
    {
       
        private readonly ILogger<DepartmentController> _logger;
        public DepartmentController( ISender mediator,ILogger<DepartmentController> logger ) : base(mediator)
        {
           
             _logger = logger;

        }
    [HttpGet]
    // public async Task<IActionResult> GetAllDepartmentAsync()
        public async Task<IActionResult> GetAllDepartmentAsync([FromQuery] int PageNumber,[FromQuery] int PageSize,[FromQuery] string SearchTerm = null)
        {
              
           _logger.LogInformation("Fetching All Department Request started.");
           
            var departments =await Mediator.Send(
                new GetDepartmentQuery
                {
                     PageNumber = PageNumber, 
                PageSize = PageSize, 
                SearchTerm = SearchTerm
                });

           if (departments.Data == null || !departments.Data.Any())
            {
               
                _logger.LogInformation($"No department records found in the database. Total count: {departments?.Data?.Count ?? 0}");
                 return NotFound(new
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        message = departments.Message
                    });
             }           
          
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = departments.Data,
                TotalCount = departments.TotalCount,
                PageNumber = departments.PageNumber,
                PageSize = departments.PageSize
            });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
            _logger.LogInformation($"Fetching single department with ID {id} request started.");

            // Retrieve the department
            var department = await Mediator.Send(new GetDepartmentByIdQuery { DepartmentId = id });

           

            // Return success response
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Data = department,
                Message = "Success"
            });
         
        }

        [HttpGet("by-name/")]
        public async Task<IActionResult> GetAllDepartmentAutoCompleteSearchAsync([FromQuery] string name)
        {
            _logger.LogInformation($"Starting GetAllDepartmentAutoCompleteSearchAsync with search pattern: {name} ");
             var query = new GetDepartmentAutoCompleteSearchQuery { SearchPattern = name ?? string.Empty };
                var result = await Mediator.Send(query);

                _logger.LogInformation($"Departments found for search pattern: {name}. Returning data." );

                return Ok(new
                {
                    StatusCode = StatusCodes.Status200OK,
                    Data = result
                });        
               
                
            
        }
        [HttpGet("withoutDatacontrol")]
        public async Task<IActionResult> GetAllDepartment([FromQuery] string name)
        {
                var query = new GetDepartmentwithoutDataControl { SearchPattern = name ?? string.Empty };
                var result = await Mediator.Send(query);

             
               if (result.IsSuccess )
               {
                _logger.LogInformation($"Departments found for search pattern: {name}. Returning data." );

                return Ok(new
                {
                    StatusCode = StatusCodes.Status200OK,
                    Data = result.Data
                });        
               }
                  _logger.LogWarning($"No departments found for search pattern: {name}");

                    return NotFound(new
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "No matching departments found."
                    });
            
        }

        [HttpGet("by-department-group/{departmentGroupName}")]
            public async Task<IActionResult> GetDepartmentsByDepartmentGroupId(string departmentGroupName)
            {
                if (string.IsNullOrEmpty(departmentGroupName))
                    {
                        return BadRequest(new
                        {
                            StatusCode = StatusCodes.Status400BadRequest,
                            Message = "Invalid Department Group ID."
                        });
                    }
                var result = await Mediator.Send(new GetDepartmentsByDepartmentGroupIdQuery
                {
                    DepartmentGroupName = departmentGroupName
                });

                var dataList = result.Data ?? new List<DepartmentWithGroupDto>();

                return Ok(new
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = dataList.Any()
                                ? "Departments retrieved successfully."
                                : "No departments found for this group.",
                    Data = dataList,
                    Count = dataList.Count,
                    IsSuccess = result.IsSuccess
                });
            }



        [HttpPost]        
        public async Task<IActionResult>CreateAsync([FromBody] CreateDepartmentCommand command)
        {
                _logger.LogInformation($"Create Department request started with data: {command}" );

          

            // Process the command
            var createdepartment = await Mediator.Send(command);
           
                return Ok(new
                {
                    StatusCode = StatusCodes.Status201Created,
                    Message = "Department created successfully",
                    Data = createdepartment
                });
                                 
        }

      [HttpPut]
        public async Task<IActionResult> UpdateAsync( UpdateDepartmentCommand command)
        {
                    _logger.LogInformation($"Update Department request started with data: {command}" );

                // Check if the department exists
                var department = await Mediator.Send(new GetDepartmentByIdQuery { DepartmentId = command.Id });
                if (department == null)
                {
                    _logger.LogWarning($"Department with ID {command.Id} not found." );

                    return NotFound(new
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Department not found"
                    });
                }
         

                // Update the department
                 await Mediator.Send(command);
               
                    _logger.LogInformation($"Department with ID { command.Id} updated successfully.");

                    return Ok(new
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "Department updated successfully"
                    
                    });
             
            
        }

        
     [HttpDelete("{id}")]         

        public async Task<IActionResult> Delete(int  id )
        {
            _logger.LogInformation($"Delete Department request started with ID: {id}");

                // Check if the department exists
                var department = await Mediator.Send(new GetDepartmentByIdQuery { DepartmentId = id });
                if (department == null)
                {
                    _logger.LogWarning($"Department with ID {id} not found.");

                    return NotFound(new
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Department not found"
                    });
                }

                _logger.LogInformation($"Department with ID {id} found. Proceeding with deletion.");

                // Attempt to delete the department
                 await Mediator.Send(new DeleteDepartmentCommand { Id = id });

              
                    _logger.LogInformation($"Department with ID {id} deleted successfully.");

                    return Ok(new
                    {
                        Message = "Department deleted successfully",
                        StatusCode = StatusCodes.Status200OK
                      
                    });
          


     
        }
     
        [HttpGet("maintenance-by-group-user-based/{departmentGroupName}")]
            public async Task<IActionResult> GetDepartmentsByDepartmentGroupWithControl(string departmentGroupName)
            {
                if (string.IsNullOrEmpty(departmentGroupName))
                    {
                        return BadRequest(new
                        {
                            StatusCode = StatusCodes.Status400BadRequest,
                            Message = "Invalid Department Group."
                        });
                    }
                var result = await Mediator.Send(new GetDepartmentByGroupWithControlQuery
                {
                    DepartmentGroupName = departmentGroupName
                });

                var dataList = result.Data ?? new List<DepartmentWithControlByGroupDto>();

                return Ok(new
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = dataList.Any()
                                ? "Departments retrieved successfully."
                                : "No departments found for this group.",
                    Data = dataList
                });
            }
      
   


    }
    

   
}