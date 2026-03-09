using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.DispatchAdvice.Commands.CreateDispatchAdvice;
using SalesManagement.Application.DispatchAdvice.Queries.GetAllDispatchAdvice;
using SalesManagement.Application.DispatchAdvice.Queries.GetDispatchAdviceById;
using SalesManagement.Application.DispatchAdvice.Queries.GetDispatchAdviceStock;
using SalesManagement.Application.DispatchAdvice.Queries.GetDispatchAdvicePackNoValidation;
using SalesManagement.Application.DispatchAdvice.Queries.GetDispatchAdviceAutoComplete;
using SalesManagement.Application.DispatchAdvice.Commands.DeleteDispatchAdvice;

namespace SalesManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class DispatchAdviceController : ApiControllerBase
    {
        public DispatchAdviceController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllDispatchAdviceAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllDispatchAdviceQuery
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

        [HttpGet("by-name")]
        public async Task<IActionResult> GetDispatchAdviceAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetDispatchAdviceAutoCompleteQuery(term ?? string.Empty));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDispatchAdviceByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetDispatchAdviceByIdQuery { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateDispatchAdvice([FromBody] CreateDispatchAdviceCommand command)
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

        [HttpGet("stock")]
        public async Task<IActionResult> GetDispatchAdviceStockAsync(
            [FromQuery] int itemId,
            [FromQuery] int lotId)
        {
            var result = await Mediator.Send(new GetDispatchAdviceStockQuery
            {
                ItemId = itemId,
                LotId = lotId
            });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("validate-packno")]
        public async Task<IActionResult> ValidatePackNoAsync(
            [FromQuery] int itemId,
            [FromQuery] int lotId,
            [FromQuery] int startPackNo,
            [FromQuery] int endPackNo,
            [FromQuery] int packTypeId)
        {
            var result = await Mediator.Send(new GetDispatchAdvicePackNoValidationQuery
            {
                ItemId = itemId,
                LotId = lotId,
                StartPackNo = startPackNo,
                EndPackNo = endPackNo,
                PackTypeId = packTypeId
            });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteDispatchAdvice(int id)
        {
            var result = await Mediator.Send(new DeleteDispatchAdviceCommand(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = "Dispatch Advice deleted successfully."
            });
        }

    }
}
