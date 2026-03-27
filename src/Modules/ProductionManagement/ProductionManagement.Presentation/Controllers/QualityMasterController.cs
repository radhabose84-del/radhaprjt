using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductionManagement.Application.QualityMaster.Commands.CreateQualityMaster;
using ProductionManagement.Application.QualityMaster.Commands.DeleteQualityMaster;
using ProductionManagement.Application.QualityMaster.Commands.UpdateQualityMaster;
using ProductionManagement.Application.QualityMaster.Queries.GetAllQualityMaster;
using ProductionManagement.Application.QualityMaster.Queries.GetQualityMasterAutoComplete;
using ProductionManagement.Application.QualityMaster.Queries.GetQualityMasterById;

namespace ProductionManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class QualityMasterController : ApiControllerBase
    {
        public QualityMasterController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllQualityMasterAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllQualityMasterQuery
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
        public async Task<IActionResult> GetQualityMasterByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetQualityMasterByIdQuery { Id = id });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetQualityMasterAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetQualityMasterAutoCompleteQuery(term));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateQualityMaster([FromBody] CreateQualityMasterCommand command)
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
        public async Task<IActionResult> UpdateQualityMaster([FromBody] UpdateQualityMasterCommand command)
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
        public async Task<IActionResult> DeleteQualityMaster(int id)
        {
            var result = await Mediator.Send(new DeleteQualityMasterCommand(id));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Quality Master deleted successfully." : "Quality Master not found."
            });
        }
    }
}
