using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.SalesReturn.Commands.CreateSalesReturn;
using SalesManagement.Application.SalesReturn.Commands.DeleteSalesReturn;
using SalesManagement.Application.SalesReturn.Queries.GetAllSalesReturn;
using SalesManagement.Application.SalesReturn.Queries.GetSalesReturnById;
using SalesManagement.Application.SalesReturn.Queries.GetSalesReturnByComplaint;
using SalesManagement.Application.SalesReturn.Queries.GetComplaintReturnData;

namespace SalesManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class SalesReturnController : ApiControllerBase
    {
        public SalesReturnController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllSalesReturnQuery
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
            var result = await Mediator.Send(new GetSalesReturnByIdQuery { Id = id });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result.IsSuccess,
                message = result.Message,
                data = result.Data
            });
        }

        [HttpGet("by-complaint/{complaintHeaderId}")]
        public async Task<IActionResult> GetByComplaintAsync(int complaintHeaderId)
        {
            var result = await Mediator.Send(new GetSalesReturnByComplaintQuery
            {
                ComplaintHeaderId = complaintHeaderId
            });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result.IsSuccess,
                message = result.Message,
                data = result.Data,
                TotalCount = result.TotalCount
            });
        }

        [HttpGet("complaint-details/{complaintHeaderId}")]
        public async Task<IActionResult> GetComplaintReturnDataAsync(int complaintHeaderId)
        {
            var result = await Mediator.Send(new GetComplaintReturnDataQuery
            {
                ComplaintHeaderId = complaintHeaderId
            });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result.IsSuccess,
                message = result.Message,
                data = result.Data
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateSalesReturn([FromBody] CreateSalesReturnCommand command)
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
        public async Task<IActionResult> DeleteSalesReturn(int id)
        {
            var result = await Mediator.Send(new DeleteSalesReturnCommand(id));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }
    }
}
