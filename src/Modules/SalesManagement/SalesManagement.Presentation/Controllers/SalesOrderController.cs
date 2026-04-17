using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.SalesOrder.Commands.CreateSalesOrder;
using SalesManagement.Application.SalesOrder.Commands.UpdateSalesOrder;
using SalesManagement.Application.SalesOrder.Commands.CancelSalesOrder;
using SalesManagement.Application.SalesOrder.Commands.ForecloseSalesOrder;
using SalesManagement.Application.SalesOrder.Commands.UploadSalesOrderDocument;
using SalesManagement.Application.SalesOrder.Commands.DeleteSalesOrderDocument;
using SalesManagement.Application.SalesOrder.Commands.UploadMdApprovalDocument;
using SalesManagement.Application.SalesOrder.Commands.DeleteMdApprovalDocument;
using SalesManagement.Application.SalesOrder.Commands.UploadSalesOrderImage;
using SalesManagement.Application.SalesOrder.Commands.DeleteSalesOrderImage;
using SalesManagement.Application.SalesOrder.Queries.GetAllSalesOrder;
using SalesManagement.Application.SalesOrder.Queries.GetAgentCommissions;
using SalesManagement.Application.SalesOrder.Queries.GetDiscountsBySalesGroup;
using SalesManagement.Application.SalesOrder.Queries.GetSalesOrderAutoComplete;
using SalesManagement.Application.SalesOrder.Queries.GetSalesOrderById;
using SalesManagement.Application.SalesOrder.Queries.GetPendingSalesOrder;
using SalesManagement.Application.SalesOrder.Queries.GetPendingSalesOrderById;
using SalesManagement.Application.SalesOrder.Queries.GetSalesOrderInvoices;

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
            [FromQuery] string? SearchTerm = null,
            [FromQuery] DateOnly? OrderDateFrom = null,
            [FromQuery] DateOnly? OrderDateTo = null,
            [FromQuery] string? PartyName = null,
            [FromQuery] string? StatusName = null)
        {
            var result = await Mediator.Send(new GetAllSalesOrderQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm,
                OrderDateFrom = OrderDateFrom,
                OrderDateTo = OrderDateTo,
                PartyName = PartyName,
                StatusName = StatusName
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
        public async Task<IActionResult> GetSalesOrderAutoCompleteAsync(
            [FromQuery] string? term = null,
            [FromQuery] bool proformaFilter = false)
        {
            var result = await Mediator.Send(new GetSalesOrderAutoCompleteQuery(term, proformaFilter));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("agent-commissions")]
        public async Task<IActionResult> GetAgentCommissionsAsync([FromQuery] int salesGroupId, [FromQuery] int paymentTermId, [FromQuery] int agentId)
        {
            var result = await Mediator.Send(new GetAgentCommissionsQuery
            {
                SalesGroupId = salesGroupId,
                PaymentTermId = paymentTermId,
                AgentId = agentId
            });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("discounts-by-sales-group")]
        public async Task<IActionResult> GetDiscountsBySalesGroupAsync([FromQuery] int salesGroupId, [FromQuery] int slabTypeId, [FromQuery] int paymentTermId)
        {
            var result = await Mediator.Send(new GetDiscountsBySalesGroupQuery
            {
                SalesGroupId = salesGroupId,
                SlabTypeId = slabTypeId,
                PaymentTermId = paymentTermId
            });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
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

        [HttpGet("{id}/invoices")]
        public async Task<IActionResult> GetSalesOrderInvoicesAsync(int id)
        {
            var result = await Mediator.Send(new GetSalesOrderInvoicesQuery { SalesOrderId = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingSalesOrderAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetPendingSalesOrderQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm
            });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result.Items,
                TotalCount = result.TotalCount,
                PageNumber,
                PageSize
            });
        }

        [HttpGet("pending/{id}")]
        public async Task<IActionResult> GetPendingSalesOrderByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetPendingSalesOrderByIdQuery { Id = id });

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

        [HttpPut("cancel/{id}")]
        public async Task<IActionResult> CancelSalesOrder(int id)
        {
            var result = await Mediator.Send(new CancelSalesOrderCommand(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Sales Order cancelled successfully." : "Failed to cancel Sales Order."
            });
        }

        [HttpPut("foreclose/{id}")]
        public async Task<IActionResult> ForecloseSalesOrder(int id)
        {
            var result = await Mediator.Send(new ForecloseSalesOrderCommand(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Sales Order foreclosed successfully." : "Failed to foreclose Sales Order."
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

        [HttpPost("upload-md-approval")]
        public async Task<IActionResult> UploadMdApprovalDocument([FromForm] UploadMdApprovalDocumentCommand command)
        {
            var result = await Mediator.Send(command);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = true,
                message = "MD Approval document uploaded successfully.",
                data = result
            });
        }

        [HttpDelete("delete-md-approval")]
        public async Task<IActionResult> DeleteMdApprovalDocument([FromQuery] string filePath)
        {
            var result = await Mediator.Send(new DeleteMdApprovalDocumentCommand { FilePath = filePath });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "MD Approval document deleted successfully." : "Failed to delete MD Approval document."
            });
        }

        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadSalesOrderImage([FromForm] UploadSalesOrderImageCommand command)
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
        public async Task<IActionResult> DeleteSalesOrderImage([FromQuery] string filePath)
        {
            var result = await Mediator.Send(new DeleteSalesOrderImageCommand { FilePath = filePath });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Image deleted successfully." : "Failed to delete image."
            });
        }
    }
}
