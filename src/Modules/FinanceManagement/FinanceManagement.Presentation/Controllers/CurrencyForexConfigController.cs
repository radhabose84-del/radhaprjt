using FinanceManagement.Application.CurrencyForexConfig.Commands.CreateCurrencyForexConfig;
using FinanceManagement.Application.CurrencyForexConfig.Commands.DeleteCurrencyForexConfig;
using FinanceManagement.Application.CurrencyForexConfig.Commands.UpdateCurrencyForexConfig;
using FinanceManagement.Application.CurrencyForexConfig.Queries.GetAllCurrencyForexConfig;
using FinanceManagement.Application.CurrencyForexConfig.Queries.GetCurrencyForexConfigAutoComplete;
using FinanceManagement.Application.CurrencyForexConfig.Queries.GetCurrencyForexConfigById;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.Presentation.Controllers
{
    [Route("api/finance/[controller]")]
    public class CurrencyForexConfigController : ApiControllerBase
    {
        public CurrencyForexConfigController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllCurrencyForexConfigAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllCurrencyForexConfigQuery
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
        public async Task<IActionResult> GetCurrencyForexConfigByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetCurrencyForexConfigByIdQuery { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetCurrencyForexConfigAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetCurrencyForexConfigAutoCompleteQuery(term ?? string.Empty));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateCurrencyForexConfig([FromBody] CreateCurrencyForexConfigCommand command)
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
        public async Task<IActionResult> UpdateCurrencyForexConfig([FromBody] UpdateCurrencyForexConfigCommand command)
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
        public async Task<IActionResult> DeleteCurrencyForexConfig(int id)
        {
            var result = await Mediator.Send(new DeleteCurrencyForexConfigCommand(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Currency Forex Config deleted successfully." : "Failed to delete Currency Forex Config."
            });
        }
    }
}
