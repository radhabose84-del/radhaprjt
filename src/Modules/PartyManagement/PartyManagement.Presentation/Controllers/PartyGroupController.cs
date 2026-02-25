using PartyManagement.Application.PartyGroup.Command.CreatePartyGroup;
using PartyManagement.Application.PartyGroup.Command.DeletePartyGroup;
using PartyManagement.Application.PartyGroup.Command.UpdatePartyGroup;
using PartyManagement.Application.PartyGroup.Queries.GetChildPartyGroupAutoComplete;
using PartyManagement.Application.PartyGroup.Queries.GetPartyGroup;
using PartyManagement.Application.PartyGroup.Queries.GetPartyGroupAutoComplete;
using PartyManagement.Application.PartyGroup.Queries.GetPartyGroupById;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace PartyManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class PartyGroupController : ApiControllerBase
    {
        private readonly IMediator _mediator;

        public PartyGroupController(IMediator mediator)
        : base(mediator)
        {
            _mediator = mediator;
        }
        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreatePartyGroupCommand createPartyGroupCommand)
        {

            // Process the command
            var CreatepartygroupId = await _mediator.Send(createPartyGroupCommand);

            return Ok(new
            {
                StatusCode = StatusCodes.Status201Created,
                message = "Created successfully.",
                data = CreatepartygroupId
            });

        }
        [HttpPut]
        public async Task<IActionResult> UpdateAsync(UpdatePartyGroupCommand updatePartyGroupCommand)
        {

            await _mediator.Send(updatePartyGroupCommand);

            return Ok(new
            {
                message = "Updated successfully.",
                statusCode = StatusCodes.Status200OK
            });

        }


        [HttpDelete]
        public async Task<IActionResult> DeleteAsync(int id)
        {

            // Process the delete command
            await _mediator.Send(new DeletePartyGroupCommand { Id = id });

            return Ok(new
            {
                message = "Deleted successfully.",
                statusCode = StatusCodes.Status200OK
            });

        }
       [HttpGet]
        public async Task<IActionResult> GetAllPartyGroupAsync([FromQuery] int PageNumber,[FromQuery] int PageSize,[FromQuery] string? SearchTerm = null)
        {
           var partygroup = await Mediator.Send(
            new GetPartyGroupQuery
            {
                PageNumber = PageNumber, 
                PageSize = PageSize, 
                SearchTerm = SearchTerm
            });
            return Ok( new 
            { 
                StatusCode=StatusCodes.Status200OK, 
                data = partygroup.Data,
                TotalCount = partygroup.TotalCount,
                PageNumber = partygroup.PageNumber,
                PageSize = partygroup.PageSize
                });
        }

        [HttpGet("Parent/by-name")]
        public async Task<IActionResult> GetMainPartyGroupAsync([FromQuery] string? Typename)
        {
            var MachineMaster = await Mediator.Send(new GetPartyGroupAutoCompleteQuery
            {
                SearchPattern = Typename ?? string.Empty
            });

            return Ok(new { StatusCode = StatusCodes.Status200OK, data = MachineMaster });
        }

        [HttpGet("Child/by-name")]
        public async Task<IActionResult> GeParentPartyGroupAsync([FromQuery] string? Typename)
        {
            var MachineMaster = await Mediator.Send(new GetChildPartyGroupAutoCompleteQuery
            {
                SearchPattern = Typename ?? string.Empty
            });

            return Ok(new { StatusCode = StatusCodes.Status200OK, data = MachineMaster });
        }

       [HttpGet("{id}")]
       [ActionName(nameof(GetByIdAsync))]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var partyGroup = await Mediator.Send(new GetPartyGroupByIdQuery() { Id = id });

            if (partyGroup == null)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    data = (object?)null,
                    message = $"PartyGroup with ID {id} not found"
                });
            }

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = partyGroup,
                message = "ID fetched successfully"
            });
        }


      
    }

    
}