using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.Interfaces.IProjectWorkBreakdownStructure;
using Core.Application.ProjectWorkBreakdownStructure.Command.CreateProjectWorkBreakdownStructureCommand;
using Core.Application.ProjectWorkBreakdownStructure.Command.SoftDeleteProjectWorkBreakdownStructureCommand;
using Core.Application.ProjectWorkBreakdownStructure.Command.UpdateProjectWorkBreakdownStructureCommand;
using Core.Application.ProjectWorkBreakdownStructure.Queries.Autocomplete;
using Core.Application.ProjectWorkBreakdownStructure.Queries.GetAllProjectWBS;
using Core.Application.ProjectWorkBreakdownStructure.Queries.GetById;
using Core.Application.ProjectWorkBreakdownStructure.Queries.GetByProject;
using Core.Application.ProjectWorkBreakdownStructure.Queries.GetWbsLookup;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ProjectManagement.API.Controllers
{
    [Route("api/[controller]")]
    public class ProjectWorkBreakdownStructureController : ApiControllerBase
    {

        private readonly IProjectWorkBreakdownStructureCommandRepository _commandRepository;

        public ProjectWorkBreakdownStructureController(
            ISender mediator,

            IProjectWorkBreakdownStructureCommandRepository commandRepository)
            : base(mediator)
        {

            _commandRepository = commandRepository;
        }

        // -------------------------------- GET: Paged list (all WBS) --------------------------------
        // GET: api/ProjectWorkBreakdownStructure?pageNumber=1&pageSize=20&searchTerm=xxx
        [HttpGet]
        public async Task<IActionResult> GetAllAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllprojectWBSQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm
            });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result.Data,
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            });
        }

        // -------------------------------- GET: By Id --------------------------------
        // GET: api/ProjectWorkBreakdownStructure/5
        [HttpGet("{id:int}")]
        [ActionName(nameof(GetByIdAsync))]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var wbs = await Mediator.Send(new GetProjectWorkBreakdownStructureByIdQuery(id));

            if (wbs == null)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    message = $"Project WBS with Id {id} not found.",
                    data = (object?)null
                });
            }

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = wbs,
                message = "Success"
            });
        }

        // -------------------------------- GET: By ProjectId (tree/list for a project) --------------------------------
        // GET: api/ProjectWorkBreakdownStructure/by-project?projectId=1
        [HttpGet("by-project")]
        public async Task<IActionResult> GetByProjectAsync([FromQuery] int projectId)
        {
            var items = await Mediator.Send(new GetProjectWorkBreakdownStructureByProjectQuery(projectId));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = items
            });
        }

        // -------------------------------- GET: Autocomplete --------------------------------
        // GET: api/ProjectWorkBreakdownStructure/by-name?projectId=1&name=Elec
        [HttpGet("by-name")]
        public async Task<IActionResult> GetProjectWbsByName(
            [FromQuery] int projectId,
            [FromQuery] string? name)
        {
            var result = await Mediator.Send(
                new GetProjectWorkBreakdownStructureAutocompleteQuery(projectId, name));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        // -------------------------------- POST: Create --------------------------------
        // POST: api/ProjectWorkBreakdownStructure
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreateProjectWorkBreakdownStructureCommand command)
        {


            var response = await Mediator.Send(command);

            return Ok(new
            {
                StatusCode = StatusCodes.Status201Created,
                message = "Created Successfully",
                errors = "",
                data = response
            });
        }

        // -------------------------------- PUT: Update --------------------------------
        // PUT: api/ProjectWorkBreakdownStructure
        [HttpPut]
        public async Task<IActionResult> UpdateAsync([FromBody] UpdateProjectWorkBreakdownStructureCommand command)
        {


            await Mediator.Send(command);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Updated Successfully",
                Errors = ""
            });
        }

        // -------------------------------- DELETE: Soft Delete --------------------------------
        // DELETE: api/ProjectWorkBreakdownStructure/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var command = new DeleteProjectWorkBreakdownStructureCommand(id);


            var success = await Mediator.Send(command);

            if (!success)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    message = $"Project WBS with Id {id} not found.",
                    errors = ""
                });
            }

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "Deleted successfully.",
                errors = ""
            });
        }
        

        [HttpGet("ProjectWbsParentLookup")]
      public async Task<IActionResult> GetLookup([FromQuery] int? projectId, CancellationToken ct)
        {
            var data = await Mediator.Send(new GetProjectWbsLookupQuery
            {
                ProjectId = projectId
            }, ct);

            return Ok(new
            {
                statusCode = 200,
                message = "WBS parent fetched successfully.",
                data
            });
        }
    }
}
