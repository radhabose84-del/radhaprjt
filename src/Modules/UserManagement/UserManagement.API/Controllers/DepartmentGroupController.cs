using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.HttpResponse;
using Core.Application.DepartmentGroup.Command.CreateDepartmentGroup;
using Core.Application.DepartmentGroup.Command.DeleteDepartmentGroup;
using Core.Application.DepartmentGroup.Command.UpdateDepartmentGroup;
using Core.Application.DepartmentGroup.Queries.GetAllDepartmentGroup;
using Core.Application.DepartmentGroup.Queries.GetDepartmentGroupAutoSearch;
using Core.Application.DepartmentGroup.Queries.GetDepartmentGroupById;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserManagement.Infrastructure.Data;

namespace UserManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentGroupController : ApiControllerBase
    {
        
        private readonly ILogger<DepartmentController> _logger;


        public DepartmentGroupController(ISender mediator, ILogger<DepartmentController> logger) : base(mediator)
        {
            
            _logger = logger;

        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreateDepartmentGroupCommand command)
        {



            // Process the command
            var createdepartmentgroup = await Mediator.Send(command);
          


                return Ok(new
                {
                    StatusCode = StatusCodes.Status201Created,
                    Message = "Department Group Created Successfully",
                    Data = createdepartmentgroup
                });
           
        }

        [HttpGet("GetAllDepartmentGroup")]
        public async Task<IActionResult> GetAllDepartmentGroupAsync([FromQuery] int PageNumber, [FromQuery] int PageSize, [FromQuery] string? SearchTerm = null)
        {


            var departmentGroups = await Mediator.Send(
                new GetAllDepartmentGroupQuery
                {
                    PageNumber = PageNumber,
                    PageSize = PageSize,
                    SearchTerm = SearchTerm
                });

            if (departmentGroups.Data == null || !departmentGroups.Data.Any())
            {


                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    message = departmentGroups.Message
                });
            }

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = departmentGroups.Data,
                totalCount = departmentGroups.TotalCount,
                pageNumber = departmentGroups.PageNumber,
                pageSize = departmentGroups.PageSize
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetDepartmentGroupByIdQuery { Id = id });

          

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Data = result
            });
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateAsync([FromBody] UpdateDepartmentGroupCommand command)
        {

            var result = await Mediator.Send(command);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Department Group Updated Successfully",
                Data = result
            });
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var command = new DeleteDepartmentGroupCommand { Id = id };


            // Check if the department group exists
             await Mediator.Send(new GetDepartmentGroupByIdQuery { Id = id });

            // Attempt deletion
             await Mediator.Send(command);

          

                return Ok(new
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Department Group Deleted Successfully",
                });
           
        }
            

      [HttpGet("by-name")]  
        public async Task<IActionResult> GetAllDepartmentGroupAutocompleteAsync([FromQuery] string? name)
        {           
            var result = await Mediator.Send(new GetDepartmentGroupAutoCompleteQuery {SearchPattern = name}); // Pass `searchPattern` to the constructor
           
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "Department Group List",
                data = result
            });
        }  



      
    }
}