using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.CustomerVisit.Commands.CreateCustomerVisit;
using SalesManagement.Application.CustomerVisit.Commands.UpdateCustomerVisit;
using SalesManagement.Application.CustomerVisit.Commands.DeleteCustomerVisit;
using SalesManagement.Application.CustomerVisit.Commands.UploadCustomerVisitImage;
using SalesManagement.Application.CustomerVisit.Commands.DeleteCustomerVisitImage;
using SalesManagement.Application.CustomerVisit.Queries.GetAllCustomerVisit;
using SalesManagement.Application.CustomerVisit.Queries.GetCustomerVisitById;
using SalesManagement.Application.CustomerVisit.Queries.GetCustomerVisitAutoComplete;

namespace SalesManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class CustomerVisitController : ApiControllerBase
    {
        public CustomerVisitController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllCustomerVisitAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllCustomerVisitQuery
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
        public async Task<IActionResult> GetCustomerVisitByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetCustomerVisitByIdQuery { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetCustomerVisitAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetCustomerVisitAutoCompleteQuery(term ?? string.Empty));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateCustomerVisit([FromBody] CreateCustomerVisitCommand command)
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
        public async Task<IActionResult> UpdateCustomerVisit([FromBody] UpdateCustomerVisitCommand command)
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
        public async Task<IActionResult> DeleteCustomerVisit(int id)
        {
            var result = await Mediator.Send(new DeleteCustomerVisitCommand(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Customer Visit deleted successfully." : "Failed to delete Customer Visit."
            });
        }

        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadCustomerVisitImage([FromForm] UploadCustomerVisitImageCommand command)
        {
            var result = await Mediator.Send(command);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = true,
                message = "Image uploaded successfully.",
                data = result
            });
        }

        [HttpDelete("delete-image")]
        public async Task<IActionResult> DeleteCustomerVisitImage([FromQuery] string imagePath)
        {
            var result = await Mediator.Send(new DeleteCustomerVisitImageCommand { ImagePath = imagePath });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Image deleted successfully." : "Failed to delete image."
            });
        }
    }
}
