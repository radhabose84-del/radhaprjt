#nullable disable
using MediatR;
using Microsoft.AspNetCore.Mvc;
using FAM.Application.Location.Command.CreateLocation;
using FAM.Application.Location.Command.UpdateLocation;
using FAM.Application.Location.Queries.GetLocations;
using FAM.Application.Location.Command.DeleteLocation;
using FAM.Application.Location.Queries.GetLocationAutoComplete;
using FAM.Application.Location.Queries.GetLocationById;
using Microsoft.AspNetCore.Http;

namespace FAM.Presentation.Controllers
{
    [Route("api/[controller]")]

    public class LocationController : ApiControllerBase
    {
        

        public LocationController(ISender mediator)
        : base(mediator)
        {
            
        }
        [HttpGet]
        public async Task<IActionResult> GetAllLocationAsync([FromQuery] int PageNumber, [FromQuery] int PageSize, [FromQuery] string SearchTerm = null)
        {
            var locations = await Mediator.Send(
                new GetLocationQuery
                {
                    PageNumber = PageNumber,
                    PageSize = PageSize,
                    SearchTerm = SearchTerm
                });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = locations.Data.ToList(),
                TotalCount = locations.TotalCount,
                PageNumber = locations.PageNumber,
                PageSize = locations.PageSize
            });
        }
        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateLocationCommand createlocationcommand)
        {

            var result = await Mediator.Send(createlocationcommand);
         
                return Ok(new
                {
                    StatusCode = StatusCodes.Status201Created,
                    message = "Location Created Successfully",
                    data = result
                });
          

        }
        [HttpGet("{id}")]
        [ActionName(nameof(GetByIdAsync))]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = "Invalid Location ID"
                });
            }
            var result = await Mediator.Send(new GetLocationByIdQuery() { Id = id });

          
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPut]
        public async Task<IActionResult> Update(UpdateLocationCommand updateLocationcommand)
        {
           

            var locationExists = await Mediator.Send(new GetLocationByIdQuery { Id = updateLocationcommand.Id });

            if (locationExists == null)
            {
                return NotFound(new { StatusCode = StatusCodes.Status404NotFound, message = $"Location ID {updateLocationcommand.Id} not found.", errors = "" });
            }

            var result = await Mediator.Send(updateLocationcommand);
          
                return Ok(new
                {
                    StatusCode = StatusCodes.Status201Created,
                    message = "Location Updated Successfully",
                    data = result
                });
           
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = "Invalid Location ID"
                });
            }
             await Mediator.Send(new DeleteLocationCommand { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = $"Location ID {id} Deleted",
                message = "Location Deleted Successfully"
            });

        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetLocation([FromQuery] string name)
        {
            var result = await Mediator.Send(new GetLocationAutoCompleteQuery { SearchPattern = name });
         
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = result,
                data = result
            });
        }
    }
}