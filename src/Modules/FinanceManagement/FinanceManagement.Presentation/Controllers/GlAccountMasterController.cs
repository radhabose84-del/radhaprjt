using FinanceManagement.Application.GlAccountMaster.Commands.CreateGlAccountMaster;
using FinanceManagement.Application.GlAccountMaster.Commands.DeleteGlAccountMaster;
using FinanceManagement.Application.GlAccountMaster.Commands.UpdateGlAccountMaster;
using FinanceManagement.Application.GlAccountMaster.Queries.GetAllGlAccountMaster;
using FinanceManagement.Application.GlAccountMaster.Queries.GetGlAccountMasterAutoComplete;
using FinanceManagement.Application.GlAccountMaster.Queries.GetGlAccountMasterById;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.Presentation.Controllers
{
    [Route("api/finance/[controller]")]
    public class GlAccountMasterController : ApiControllerBase
    {
        public GlAccountMasterController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllGlAccountMasterAsync(
            [FromQuery] int? PageNumber = null,
            [FromQuery] int? PageSize = null,
            [FromQuery] string? SearchTerm = null,
            [FromQuery] int? AccountTypeId = null,
            [FromQuery] int? AccountGroupId = null)
        {
            var result = await Mediator.Send(new GetAllGlAccountMasterQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm,
                AccountTypeId = AccountTypeId,
                AccountGroupId = AccountGroupId
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
        public async Task<IActionResult> GetGlAccountMasterByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetGlAccountMasterByIdQuery { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetGlAccountMasterAutoCompleteAsync(
            [FromQuery] string? term = null,
            [FromQuery] string? AccountTypeCode = null)
        {
            var result = await Mediator.Send(new GetGlAccountMasterAutoCompleteQuery(term ?? string.Empty, AccountTypeCode));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateGlAccountMaster([FromBody] CreateGlAccountMasterCommand command)
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
        public async Task<IActionResult> UpdateGlAccountMaster([FromBody] UpdateGlAccountMasterCommand command)
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
        public async Task<IActionResult> DeleteGlAccountMaster(int id)
        {
            var result = await Mediator.Send(new DeleteGlAccountMasterCommand(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "GL Account deleted successfully." : "Failed to delete GL Account."
            });
        }
    }
}
