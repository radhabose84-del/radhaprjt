using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.StoReceipt.Commands.CreateStoReceipt;
using SalesManagement.Application.StoReceipt.Queries.GetAllStoReceipt;
using SalesManagement.Application.StoReceipt.Queries.GetStoReceiptById;
using SalesManagement.Application.StoReceipt.Queries.GetStoReceiptAutoComplete;
using SalesManagement.Application.StoReceipt.Queries.GetDcOpenQty;

namespace SalesManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class StoReceiptController : ApiControllerBase
    {
        public StoReceiptController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllStoReceiptAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllStoReceiptQuery
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
        public async Task<IActionResult> GetStoReceiptByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetStoReceiptByIdQuery { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetStoReceiptAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetStoReceiptAutoCompleteQuery(term ?? string.Empty));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("dc-open-qty")]
        public async Task<IActionResult> GetDcOpenQtyAsync([FromQuery] int dcDetailId)
        {
            var result = await Mediator.Send(new GetDcOpenQtyQuery { DcDetailId = dcDetailId });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateStoReceipt([FromBody] CreateStoReceiptCommand command)
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

    }
}
