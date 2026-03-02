using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.SalesOrder.Commands.CreateSalesOrder;
using SalesManagement.Application.SalesOrder.Commands.UpdateSalesOrder;
using SalesManagement.Application.SalesOrder.Commands.DeleteSalesOrder;
using SalesManagement.Application.SalesOrder.Commands.UploadSalesOrderDocument;
using SalesManagement.Application.SalesOrder.Commands.DeleteSalesOrderDocument;
using SalesManagement.Application.SalesOrder.Queries.GetAllSalesOrder;
using SalesManagement.Application.SalesOrder.Queries.GetSalesOrderById;

namespace SalesManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class SalesOrderController : ApiControllerBase
    {
        public SalesOrderController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllSalesOrderAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllSalesOrderQuery
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
        public async Task<IActionResult> GetSalesOrderByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetSalesOrderByIdQuery { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateSalesOrder([FromBody] CreateSalesOrderCommand command)
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
        public async Task<IActionResult> UpdateSalesOrder([FromBody] UpdateSalesOrderCommand command)
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
        public async Task<IActionResult> DeleteSalesOrder(int id)
        {
            var result = await Mediator.Send(new DeleteSalesOrderCommand(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Sales Order deleted successfully." : "Failed to delete Sales Order."
            });
        }

        [HttpPost("upload-document")]
        public async Task<IActionResult> UploadSalesOrderDocument([FromForm] UploadSalesOrderDocumentCommand command)
        {
            var result = await Mediator.Send(command);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = true,
                message = "Document uploaded successfully.",
                data = result
            });
        }

        [HttpDelete("delete-document")]
        public async Task<IActionResult> DeleteSalesOrderDocument([FromQuery] string filePath)
        {
            var result = await Mediator.Send(new DeleteSalesOrderDocumentCommand { FilePath = filePath });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Document deleted successfully." : "Failed to delete document."
            });
        }
    }
}
