#nullable disable

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.SalesSegment.Commands.CreateSalesSegment;
using SalesManagement.Application.SalesSegment.Commands.DeleteSalesSegment;
using SalesManagement.Application.SalesSegment.Commands.UpdateSalesSegment;
using SalesManagement.Application.SalesSegment.Queries.GetAllSalesSegment;
using SalesManagement.Application.SalesSegment.Queries.GetSalesSegmentAutoComplete;
using SalesManagement.Application.SalesSegment.Queries.GetSalesSegmentById;

namespace SalesManagement.Presentation.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class SalesSegmentController : ApiControllerBase
    {
        public SalesSegmentController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllSalesSegmentAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllSalesSegmentQuery
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
        public async Task<IActionResult> GetSalesSegmentByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetSalesSegmentByIdQuery { Id = id });

            if (!result.IsSuccess)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    message = result.Message
                });
            }

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result.Data
            });
        }

        [HttpGet("autocomplete")]
        public async Task<IActionResult> GetSalesSegmentAutoCompleteAsync([FromQuery] string term = null)
        {
            var result = await Mediator.Send(new GetSalesSegmentAutoCompleteQuery(term));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateSalesSegment([FromBody] CreateSalesSegmentCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await Mediator.Send(command);

            if (!result.IsSuccess)
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    isSuccess = result.IsSuccess,
                    message = result.Message
                });
            }

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result.IsSuccess,
                message = result.Message,
                data = result.Data
            });
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateSalesSegment([FromBody] UpdateSalesSegmentCommand command)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await Mediator.Send(command);

            if (!result.IsSuccess)
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    isSuccess = result.IsSuccess,
                    message = result.Message
                });
            }

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result.IsSuccess,
                message = result.Message,
                data = result.Data
            });
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteSalesSegment(int id)
        {
            var result = await Mediator.Send(new DeleteSalesSegmentCommand(id));

            if (!result)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    message = "Sales Segment not found or already deleted."
                });
            }

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = true,
                message = "Sales Segment deleted successfully."
            });
        }
    }
}
