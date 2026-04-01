using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.SalesOrder.Commands.CreateSalesOrderAmendment;

using SalesManagement.Application.SalesOrder.Queries.GetAllSalesOrderAmendment;
using SalesManagement.Application.SalesOrder.Queries.GetPendingSalesOrderAmendment;
using SalesManagement.Application.SalesOrder.Queries.GetPendingSalesOrderAmendmentById;
using SalesManagement.Application.SalesOrder.Queries.GetSalesOrderAmendmentById;

namespace SalesManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class SalesOrderAmendmentController : ApiControllerBase
    {
        public SalesOrderAmendmentController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllSalesOrderAmendmentQuery
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
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetSalesOrderAmendmentByIdQuery { Id = id });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result.Data });
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var (items, total) = await Mediator.Send(new GetPendingSalesOrderAmendmentQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm
            });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = items,
                TotalCount = total,
                PageNumber,
                PageSize
            });
        }

        [HttpGet("pending/{id}")]
        public async Task<IActionResult> GetPendingByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetPendingSalesOrderAmendmentByIdQuery { Id = id });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreateSalesOrderAmendmentCommand command)
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
