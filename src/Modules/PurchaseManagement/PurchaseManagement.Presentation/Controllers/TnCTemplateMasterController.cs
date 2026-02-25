using PurchaseManagement.Application.TnCTemplateMaster.Command.CreateTnCTemplateMasterCommand;
using PurchaseManagement.Application.TnCTemplateMaster.Command.DeleteTnCTemplateMasterCommand;
using PurchaseManagement.Application.TnCTemplateMaster.Command.UpdateTnCTemplateMasterCommand;
using PurchaseManagement.Application.TnCTemplateMaster.Queries.GetAllTnCTemplateMaster;
using PurchaseManagement.Application.TnCTemplateMaster.Queries.GetTnCTemplateMasterAutoComplete;
using PurchaseManagement.Application.TnCTemplateMaster.Queries.GetTnCTemplateMasterById;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace PurchaseManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class TnCTemplateMasterController : ApiControllerBase
    {
        private readonly IMediator _mediator;

        public TnCTemplateMasterController(IMediator mediator) : base(mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTnCTemplateMasterAsync([FromQuery] int PageNumber, [FromQuery] int PageSize, [FromQuery] string? SearchTerm = null)
        {
            var TnCTemplateMaster = await Mediator.Send(
             new GetAllTncTemplateQuery
             {
                 PageNumber = PageNumber,
                 PageSize = PageSize,
                 SearchTerm = SearchTerm
             });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = TnCTemplateMaster.Data,
                TotalCount = TnCTemplateMaster.TotalCount,
                PageNumber = TnCTemplateMaster.PageNumber,
                PageSize = TnCTemplateMaster.PageSize
            });
        }
        [HttpGet("{id}")]
        [ActionName(nameof(GetByIdAsync))]
        public async Task<IActionResult> GetByIdAsync(int id)
        {

            var tncTemplateMaster = await Mediator.Send(new GetTncTemplateByIdQuery() { Id = id });

            return Ok(new { StatusCode = StatusCodes.Status200OK, data = tncTemplateMaster, message = "TnC Template fetched successfully." });
        }


        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateTnCTemplateMasterCommand command)
        {

            var response = await Mediator.Send(command);

            return StatusCode(StatusCodes.Status201Created, new
            {
                StatusCode = StatusCodes.Status201Created,
                message = "Created Successfully",
                errors = "",
                data = response
            });

        }

        [HttpPut]
        public async Task<IActionResult> UpdateAsync(UpdateTnCTemplateMasterCommand command)
        {

            var response = await Mediator.Send(command);
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "Updated Successfully",
                errors = "",
                data = response
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await Mediator.Send(new DeleteTnCTemplateMasterCommand { Id = id });

            return Ok(new { StatusCode = StatusCodes.Status200OK, message = "Deleted successfully.", errors = "" });


        }  

        [HttpGet("by-name")]
        public async Task<IActionResult> GetPaymentTermMaster([FromQuery] int? templateTypeId,    [FromQuery] int? applicabilityId,    [FromQuery] string? searchPattern)
        {
          
            var tncTemplateMaster = await Mediator.Send(new TnCTemplateAutoCompleteQuery { TemplateTypeId  = templateTypeId, ApplicabilityId = applicabilityId, SearchPattern   = searchPattern});
            
            return Ok( new { StatusCode=StatusCodes.Status200OK, data = tncTemplateMaster });
            
        }
    }
}