using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.DeleteDocument;
using Core.Application.MiscTypeMaster.Command.CreateMiscTypeMaster;
using Core.Application.ProjectMaster.Command.CreateProjectMaster;
using Core.Application.ProjectMaster.Command.DeleteProjectMaster;
using Core.Application.ProjectMaster.Command.UpdateProjectMaster;
using Core.Application.ProjectMaster.Queries.Dtos;
using Core.Application.ProjectMaster.Queries.GetProjectMaster;
using Core.Application.ProjectMaster.Queries.GetProjectMasterById;
using Core.Application.ProjectMaster.Queries.GetProjectPendingApprovals;
using Core.Application.ProjectMaster.Queries.ProjectMasterAutoComplete;
using Core.Application.UploadDocument;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ProjectManagement.API.Controllers
{
    [Route("api/[controller]")]
    public class ProjectMasterController : ApiControllerBase
    {
        private readonly ILogger<ProjectMasterController> _logger;

        public ProjectMasterController(ISender mediator) : base(mediator)

        {


        }

        [HttpGet]
        public async Task<IActionResult> GetAllProjectMasterAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetProjectMasterQuery
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
        [HttpGet("{id}")]
        [ActionName(nameof(GetByIdAsync))]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var project = await Mediator.Send(new GetProjectMasterByIdQuery { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = project,
                message = "Project fetched successfully."
            });
        }


        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreateProjectMasterDto dto)
        {
            // wrap DTO into command
            var command = new CreateProjectMasterCommand
            {
                Project = dto
            };





            // 🔹 2. Send to MediatR handler
            ProjectMasterDto response = await Mediator.Send(command);

            // 🔹 3. Return created response (same JSON style as your sample)
            return Ok(new
            {
                StatusCode = StatusCodes.Status201Created,
                message = "Created Successfully",
                errors = "",
                data = response.Id + " " + response.ProjectCode
            });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAsync([FromBody] UpdateProjectMasterDto dto)
        {
            // Ensure route id and body id are in sync


            var command = new UpdateProjectMasterCommand
            {
                Project = dto
            };


            var response = await Mediator.Send(command);

            // 3️⃣ Custom response
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "Updated Successfully",
                errors = "",
                data = response
            });
        }


        [HttpPost("upload-document")]
        public async Task<IActionResult> UploadDocument(UploadDocumentCommand uploadFileCommand)
        {
            var file = await Mediator.Send(uploadFileCommand);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "File uploaded successfully.",
                data = file,
                errors = ""
            });
        }

        [HttpDelete("delete-document")]
        public async Task<IActionResult> DeleteDocument([FromBody] DeleteDocumentCommand deleteFileCommand)
        {
            if (deleteFileCommand == null || string.IsNullOrWhiteSpace(deleteFileCommand.ProjectDocumentPath))
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = "Invalid request. 'ProjectDocumentPath' cannot be null or empty.",
                    errors = ""
                });
            }
            var file = await Mediator.Send(deleteFileCommand);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "File deleted successfully.",
                errors = ""
            });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var result = await Mediator.Send(new DeleteProjectMasterCommand(id));

            if (!result)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    message = $"ProjectMaster with Id {id} not found or already deleted.",
                    errors = ""
                });
            }

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "Deleted successfully (soft delete).",
                errors = ""
            });
        }

        [HttpGet("ProjectByname")]
        public async Task<IActionResult> GetAutoCompleteAsync(
            [FromQuery] int? unitId,
            [FromQuery] int? departmentId,
            [FromQuery] string? searchTerm,
            [FromQuery] int? take,
            [FromQuery] string? ProjectStatus,
            CancellationToken ct = default)
        {
            var query = new GetProjectMasterAutoCompleteQuery
            {
                UnitId = unitId,
                DepartmentId = departmentId,
                SearchTerm = searchTerm,
                Take = take ?? 10,
                ProjectStatus= ProjectStatus

            };

            var result = await Mediator.Send(query, ct);

            // If you have a common ApiResponse<T> wrapper, use it here

            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });

        }
        

        [HttpGet("pending-approvals")]
        public async Task<IActionResult> GetPendingApprovals([FromQuery] GetProjectPendingApprovalQuery query, CancellationToken ct)
        {
            var result = await Mediator.Send(query, ct); // ✅ use Mediator, not _mediator

            return Ok(new
            {
                statusCode = 200,
                message = "Project pending approvals fetched successfully.",
                data = result.Items,
                totalCount = result.TotalCount
            });
        }
        

    }
}