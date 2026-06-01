using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QCManagement.Application.QcInspection.Commands.CreateQcInspection;
using QCManagement.Application.QcInspection.Commands.DeleteQcInspection;
using QCManagement.Application.QcInspection.Commands.SaveDisposition;
using QCManagement.Application.QcInspection.Commands.SaveParameterCollection;
using QCManagement.Application.QcInspection.Queries.GetAllQcInspection;
using QCManagement.Application.QcInspection.Queries.GetEligibleGrnLines;
using QCManagement.Application.QcInspection.Queries.GetGrnQcStatus;
using QCManagement.Application.QcInspection.Queries.GetQcInspectionById;
using QCManagement.Application.QcInspection.Queries.ResolveSpec;

namespace QCManagement.Presentation.Controllers
{
    [Route("api/qc/[controller]")]
    public class QcInspectionController : ApiControllerBase
    {
        public QcInspectionController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllQcInspectionAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null,
            [FromQuery] int? QcStatusId = null,
            [FromQuery] DateTimeOffset? InspectionDateFrom = null,
            [FromQuery] DateTimeOffset? InspectionDateTo = null)
        {
            var result = await Mediator.Send(new GetAllQcInspectionQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm,
                QcStatusId = QcStatusId,
                InspectionDateFrom = InspectionDateFrom,
                InspectionDateTo = InspectionDateTo
            });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result.Data,
                result.TotalCount,
                result.PageNumber,
                result.PageSize
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetQcInspectionByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetQcInspectionByIdQuery { Id = id });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        [HttpGet("eligible-grn-lines")]
        public async Task<IActionResult> GetEligibleGrnLinesAsync(
            [FromQuery] int? supplierId = null,
            [FromQuery] DateTimeOffset? fromDate = null,
            [FromQuery] DateTimeOffset? toDate = null)
        {
            var result = await Mediator.Send(new GetEligibleGrnLinesQuery
            {
                SupplierId = supplierId,
                FromDate = fromDate,
                ToDate = toDate
            });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        [HttpGet("resolve-spec/{grnDetailId}")]
        public async Task<IActionResult> ResolveSpecAsync(int grnDetailId)
        {
            var result = await Mediator.Send(new ResolveSpecQuery { GrnDetailId = grnDetailId });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        [HttpGet("grn-status/{grnHeaderId}")]
        public async Task<IActionResult> GetGrnQcStatusAsync(int grnHeaderId)
        {
            var result = await Mediator.Send(new GetGrnQcStatusQuery { GrnHeaderId = grnHeaderId });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        [HttpPost]
        public async Task<IActionResult> CreateQcInspection([FromBody] CreateQcInspectionCommand command)
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

        [HttpPut("parameters")]
        public async Task<IActionResult> SaveParameters([FromBody] SaveParameterCollectionCommand command)
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

        [HttpPut("disposition")]
        public async Task<IActionResult> SaveDisposition([FromBody] SaveDispositionCommand command)
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
        public async Task<IActionResult> DeleteQcInspection(int id)
        {
            var result = await Mediator.Send(new DeleteQcInspectionCommand(id));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "QC Inspection deleted successfully." : "Failed to delete QC Inspection."
            });
        }
    }
}
