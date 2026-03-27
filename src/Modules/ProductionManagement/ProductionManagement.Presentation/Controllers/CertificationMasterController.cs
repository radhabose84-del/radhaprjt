using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductionManagement.Application.CertificationMaster.Commands.CreateCertificationMaster;
using ProductionManagement.Application.CertificationMaster.Commands.DeleteCertificationMaster;
using ProductionManagement.Application.CertificationMaster.Commands.UpdateCertificationMaster;
using ProductionManagement.Application.CertificationMaster.Queries.GetAllCertificationMaster;
using ProductionManagement.Application.CertificationMaster.Queries.GetCertificationMasterAutoComplete;
using ProductionManagement.Application.CertificationMaster.Queries.GetCertificationMasterById;

namespace ProductionManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class CertificationMasterController : ApiControllerBase
    {
        public CertificationMasterController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllCertificationMasterAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllCertificationMasterQuery
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
        public async Task<IActionResult> GetCertificationMasterByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetCertificationMasterByIdQuery { Id = id });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetCertificationMasterAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetCertificationMasterAutoCompleteQuery(term));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateCertificationMaster([FromBody] CreateCertificationMasterCommand command)
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
        public async Task<IActionResult> UpdateCertificationMaster([FromBody] UpdateCertificationMasterCommand command)
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
        public async Task<IActionResult> DeleteCertificationMaster(int id)
        {
            var result = await Mediator.Send(new DeleteCertificationMasterCommand(id));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Certification Master deleted successfully." : "Certification Master not found."
            });
        }
    }
}
