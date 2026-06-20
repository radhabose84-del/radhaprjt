using FinanceManagement.Application.GlAccountMaster.Commands.AddGlAccountFavourite;
using FinanceManagement.Application.GlAccountMaster.Commands.CreateGlAccountMaster;
using FinanceManagement.Application.GlAccountMaster.Commands.DeleteGlAccountMaster;
using FinanceManagement.Application.GlAccountMaster.Commands.RecordGlAccountRecent;
using FinanceManagement.Application.GlAccountMaster.Commands.RemoveGlAccountFavourite;
using FinanceManagement.Application.GlAccountMaster.Commands.UpdateGlAccountMaster;
using FinanceManagement.Application.GlAccountMaster.Queries.GetAllGlAccountMaster;
using FinanceManagement.Application.GlAccountMaster.Queries.GetGlAccountFavourites;
using FinanceManagement.Application.GlAccountMaster.Queries.GetGlAccountMasterAutoComplete;
using FinanceManagement.Application.GlAccountMaster.Queries.GetGlAccountMasterById;
using FinanceManagement.Application.GlAccountMaster.Queries.GetGlAccountSearch;
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

        // ── US-GL02-07 type-ahead component ────────────────────────────────────
        // Reusable account search; empty term returns the user's favourites + recently-used first.
        [HttpGet("search")]
        public async Task<IActionResult> SearchAccountsAsync(
            [FromQuery] string? term = null,
            [FromQuery] int? accountTypeId = null,
            [FromQuery] int? accountGroupId = null,
            [FromQuery] bool activeOnly = false,
            [FromQuery] int take = 20)
        {
            var result = await Mediator.Send(new GetGlAccountSearchQuery
            {
                Term = term,
                AccountTypeId = accountTypeId,
                AccountGroupId = accountGroupId,
                ActiveOnly = activeOnly,
                Take = take
            });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        [HttpGet("favourites")]
        public async Task<IActionResult> GetFavouritesAsync()
        {
            var result = await Mediator.Send(new GetGlAccountFavouritesQuery());
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        [HttpPost("favourites")]
        public async Task<IActionResult> AddFavourite([FromBody] AddGlAccountFavouriteCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
        }

        [HttpDelete("favourites")]
        public async Task<IActionResult> RemoveFavourite([FromQuery] int glAccountMasterId)
        {
            var result = await Mediator.Send(new RemoveGlAccountFavouriteCommand { GlAccountMasterId = glAccountMasterId });
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
        }

        // Record-on-select — the FE pings this when the user picks an account in an entry.
        [HttpPost("recent")]
        public async Task<IActionResult> RecordRecent([FromBody] RecordGlAccountRecentCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
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
