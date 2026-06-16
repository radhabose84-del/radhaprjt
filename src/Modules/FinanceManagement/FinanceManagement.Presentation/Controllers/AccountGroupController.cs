using FinanceManagement.Application.AccountGroup.Commands.CreateAccountGroup;
using FinanceManagement.Application.AccountGroup.Commands.DeleteAccountGroup;
using FinanceManagement.Application.AccountGroup.Commands.MoveAccountGroup;
using FinanceManagement.Application.AccountGroup.Commands.UpdateAccountGroup;
using FinanceManagement.Application.AccountGroup.Queries.GetAccountGroupAutoComplete;
using FinanceManagement.Application.AccountGroup.Queries.GetAccountGroupById;
using FinanceManagement.Application.AccountGroup.Queries.GetAccountGroupLeaves;
using FinanceManagement.Application.AccountGroup.Queries.GetAccountGroupParents;
using FinanceManagement.Application.AccountGroup.Queries.GetAccountGroupTree;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.Presentation.Controllers
{
    [Route("api/finance/[controller]")]
    public class AccountGroupController : ApiControllerBase
    {
        public AccountGroupController(IMediator mediator) : base(mediator) { }

        [HttpGet("tree")]
        public async Task<IActionResult> GetAccountGroupTreeAsync([FromQuery] int? companyId = null)
        {
            var result = await Mediator.Send(new GetAccountGroupTreeQuery { CompanyId = companyId });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result.Data,
                TotalCount = result.TotalCount
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAccountGroupByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetAccountGroupByIdQuery { Id = id });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetAccountGroupAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetAccountGroupAutoCompleteQuery(term));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("parents")]
        public async Task<IActionResult> GetAccountGroupParentsAsync([FromQuery] int level, [FromQuery] int? companyId = null)
        {
            var result = await Mediator.Send(new GetAccountGroupParentsQuery(level, companyId));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        // Assignable leaf groups for the GL-account "Account Group" picker.
        [HttpGet("leaf-groups")]
        public async Task<IActionResult> GetAccountGroupLeavesAsync([FromQuery] int? companyId = null, [FromQuery] int? accountTypeId = null)
        {
            var result = await Mediator.Send(new GetAccountGroupLeavesQuery(companyId, accountTypeId));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccountGroup([FromBody] CreateAccountGroupCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result.IsSuccess,
                message = result.Message,
                data = result.Data
            });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAccountGroup([FromBody] UpdateAccountGroupCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result.IsSuccess,
                message = result.Message,
                data = result.Data
            });
        }

        [HttpPost("move")]
        public async Task<IActionResult> MoveAccountGroup([FromBody] MoveAccountGroupCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result.IsSuccess,
                message = result.Message,
                data = result.Data
            });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAccountGroup(int id)
        {
            var result = await Mediator.Send(new DeleteAccountGroupCommand(id));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Account Group deleted successfully." : "Account Group not found."
            });
        }
    }
}
