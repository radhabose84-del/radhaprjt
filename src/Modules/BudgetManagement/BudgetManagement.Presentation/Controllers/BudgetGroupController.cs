using BudgetManagement.Application.BudgetGroup.Command.DeleteBudgetGroup;
using BudgetManagement.Application.BudgetGroups;
using BudgetManagement.Application.BudgetGroups.Commands.CreateBudgetGroup;
using BudgetManagement.Application.BudgetGroups.Commands.UpdateBudgetGroup;
using BudgetManagement.Application.BudgetGroups.Queries.GetBudgetGroup;
using BudgetManagement.Application.BudgetGroups.Queries.GetBudgetGroupAutoComplete;
using BudgetManagement.Application.BudgetGroups.Queries.GetBudgetGroupById;
using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace BudgetManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class BudgetGroupController : ApiControllerBase
    {
        private readonly IMediator _mediator;

        public BudgetGroupController(IMediator mediator) : base(mediator)
        {
            _mediator = mediator;
        }

        // 🔹 LIST – GetBudgetGroupQuery
        // GET: api/BudgetGroup
        [HttpGet]
        public async Task<ActionResult<ApiResponseDTO<List<BudgetGroupListItemDto>>>> GetBudgetGroups(
            [FromQuery] GetBudgetGroupQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        // 🔹 GET BY ID – GetBudgetGroupByIdQuery
        // GET: api/BudgetGroup/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponseDTO<BudgetGroupDto>>> GetBudgetGroupById(int id)
        {
            var query = new GetBudgetGroupByIdQuery { Id = id };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        // 🔹 AUTOCOMPLETE – GetBudgetGroupAutoCompleteQuery
        // GET: api/BudgetGroup/autocomplete
        [HttpGet("autocomplete")]
        public async Task<ActionResult<ApiResponseDTO<List<BudgetGroupAutoCompleteDto>>>> GetBudgetGroupAutoComplete([FromQuery] GetBudgetGroupAutoCompleteQuery query)
        {
            var result = await _mediator.Send(query);

            var response = new ApiResponseDTO<List<BudgetGroupAutoCompleteDto>>
            {
                StatusCode = StatusCodes.Status200OK,
                IsSuccess = true,
                Message = "Budget Group autocomplete fetched successfully.",
                Data = result
            };
            return Ok(response);
        }


        // 🔹 CREATE – CreateBudgetGroupCommand
        // POST: api/BudgetGroup
        [HttpPost]
        public async Task<IActionResult> CreateBudgetGroup([FromBody] CreateBudgetGroupCommand command)
        {
            var result = await _mediator.Send(command);

            return Ok(new ApiResponseDTO<int>
            {
                StatusCode = StatusCodes.Status201Created,
                IsSuccess = true,
                Message = "Budget Group created successfully.",
                Data = result
            });
        }

        // 🔹 UPDATE – UpdateBudgetGroupCommand
        // PUT: api/BudgetGroup/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateBudgetGroup(int id, [FromBody] UpdateBudgetGroupCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("ID mismatch.");
            }

            var result = await _mediator.Send(command);

            return Ok(new ApiResponseDTO<int>
            {
                StatusCode = StatusCodes.Status200OK,
                IsSuccess = true,
                Message = "Budget Group updated successfully.",
                Data = result
            });
        }

        // 🔹 DELETE – DeleteBudgetGroupCommand
        // DELETE: api/BudgetGroup/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteBudgetGroup(int id)
        {
            var command = new DeleteBudgetGroupCommand { Id = id };
            var result = await _mediator.Send(command);

            // Check if the result was successful
            if (result > 0)
            {
                // Successfully deleted the group
                return Ok(new ApiResponseDTO<int>
                {
                    IsSuccess = true,
                    Message = "Budget Group deleted successfully.",
                    Data = result,
                    StatusCode = StatusCodes.Status200OK,
                });
            }
            else
            {
                // Failed to delete the group 
                return NotFound(new ApiResponseDTO<int>
                {
                    IsSuccess = false,
                    Message = "Budget Group not found.",
                    Data = result,
                    StatusCode = StatusCodes.Status404NotFound,
                });
            }
        }

        // 🔹 GET BY ID – GetBudgetGroupByDepartmentQuery
        [HttpGet("by-department")]
        public async Task<IActionResult> GetByDepartment([FromQuery] int departmentId, [FromQuery] string? searchPattern, CancellationToken ct)
        {
            var data = await _mediator.Send(
                new BudgetManagement.Application.BudgetGroups.Queries.GetBudgetGroupByDepartment.GetBudgetGroupByDepartmentQuery(
                    departmentId, searchPattern),
                ct);

            return Ok(new ApiResponseDTO<List<BudgetGroupAutoCompleteDto>>
            {
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Success",
                Data = data,
                TotalCount = data.Count,
                PageSize = data.Count
            });
        }
    }
}