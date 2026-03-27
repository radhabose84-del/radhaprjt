using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.ComplaintQCReview.Commands.SubmitQCReview;
using SalesManagement.Application.ComplaintQCReview.Commands.UpdateQCReview;
using SalesManagement.Application.ComplaintQCReview.Queries.GetAllQCReview;
using SalesManagement.Application.ComplaintQCReview.Queries.GetQCReviewByComplaintId;
using SalesManagement.Application.ComplaintQCReview.Queries.GetQCReviewById;

namespace SalesManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class ComplaintQCReviewController : ApiControllerBase
    {
        public ComplaintQCReviewController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllQCReviewAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null,
            [FromQuery] string? StatusFilter = null)
        {
            var result = await Mediator.Send(new GetAllQCReviewQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm,
                StatusFilter = StatusFilter
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
        public async Task<IActionResult> GetQCReviewByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetQCReviewByIdQuery { Id = id });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-complaint/{complaintHeaderId}")]
        public async Task<IActionResult> GetQCReviewByComplaintIdAsync(int complaintHeaderId)
        {
            var result = await Mediator.Send(new GetQCReviewByComplaintIdQuery
            {
                ComplaintHeaderId = complaintHeaderId
            });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> SubmitQCReview([FromBody] SubmitQCReviewCommand command)
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
        public async Task<IActionResult> UpdateQCReview([FromBody] UpdateQCReviewCommand command)
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
