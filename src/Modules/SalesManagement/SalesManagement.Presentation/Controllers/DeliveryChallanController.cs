using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.DeliveryChallan.Commands.CreateDeliveryChallan;
using SalesManagement.Application.DeliveryChallan.Commands.DeleteDeliveryChallan;
using SalesManagement.Application.DeliveryChallan.Queries.GetAllDeliveryChallan;
using SalesManagement.Application.DeliveryChallan.Queries.GetDeliveryChallanById;
using SalesManagement.Application.DeliveryChallan.Queries.GetDeliveryChallanAutoComplete;
using SalesManagement.Application.DeliveryChallan.Queries.GetPendingDeliveryChallan;
using SalesManagement.Application.DeliveryChallan.Queries.GetStoOpenQty;

namespace SalesManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class DeliveryChallanController : ApiControllerBase
    {
        public DeliveryChallanController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllDeliveryChallanAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllDeliveryChallanQuery
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

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingDeliveryChallanAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetPendingDeliveryChallanQuery
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
        public async Task<IActionResult> GetDeliveryChallanByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetDeliveryChallanByIdQuery { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetDeliveryChallanAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetDeliveryChallanAutoCompleteQuery(term ?? string.Empty));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("sto-open-qty")]
        public async Task<IActionResult> GetStoOpenQtyAsync([FromQuery] int stoDetailId)
        {
            var result = await Mediator.Send(new GetStoOpenQtyQuery { StoDetailId = stoDetailId });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateDeliveryChallan([FromBody] CreateDeliveryChallanCommand command)
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
        public async Task<IActionResult> DeleteDeliveryChallan(int id)
        {
            var result = await Mediator.Send(new DeleteDeliveryChallanCommand(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = "Delivery Challan deleted successfully."
            });
        }
    }
}
