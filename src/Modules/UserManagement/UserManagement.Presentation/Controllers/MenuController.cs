using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Application.Menu.Commands.CreateMenu;
using UserManagement.Application.Menu.Commands.DeleteMenu;
using UserManagement.Application.Menu.Commands.UpdateMenu;
using UserManagement.Application.Menu.Commands.UploadMenu;
using UserManagement.Application.Menu.Queries.GetChildMenuByModule;
using UserManagement.Application.Menu.Queries.GetMenu;
using UserManagement.Application.Menu.Queries.GetMenuByModule;
using UserManagement.Application.Menu.Queries.GetParentMenu;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace UserManagement.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MenuController : ApiControllerBase
    {
        public MenuController(ISender mediator) : base(mediator)
        {
        }
        [HttpPost("by-module")]
        public async Task<IActionResult> GetParentMenuByModule([FromBody] List<int> id)
        {
            if (id.Count <= 0 || id == null)
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = "Invalid Module ID",
                    errors = ""
                });
            var menus = await Mediator.Send(new GetMenuByModuleQuery() { ModuleId = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = menus.ToList()
            });
        }
        [HttpPost("by-parent")]
        public async Task<IActionResult> GetChildMenuByModule([FromBody] List<int> id)
        {
            if (id.Count <= 0 || id == null)
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = "Invalid ParentId Menu ID",
                    errors = ""
                });
            var menus = await Mediator.Send(new GetChildMenuByModuleQuery() { ParentId = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = menus.ToList()
            });
        }
        [HttpGet]
        public async Task<IActionResult> GetAllMenusAsync([FromQuery] int PageNumber, [FromQuery] int PageSize, [FromQuery] string? SearchTerm = null)
        {
            var menus = await Mediator.Send(
             new GetMenuQuery
             {
                 PageNumber = PageNumber,
                 PageSize = PageSize,
                 SearchTerm = SearchTerm
             });


            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = menus.Data,
                TotalCount = menus.TotalCount,
                PageNumber = menus.PageNumber,
                PageSize = menus.PageSize
            });
        }
        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateMenuCommand command)
        {

            
            var response = await Mediator.Send(command);
          

                return Ok(new { StatusCode = StatusCodes.Status201Created, message = "Menu created successfully", errors = "", data = response });
       

        }
        //  [HttpGet("{id}")]
        //  [ActionName(nameof(GetByIdAsync))]
        // public async Task<IActionResult> GetByIdAsync(int id)
        // {

        //     var division = await Mediator.Send(new GetDivisionByIdQuery() { Id = id});

        //      if(division == null)
        //     {
        //         return NotFound( new { StatusCode=StatusCodes.Status404NotFound, message = $"Division ID {id} not found.", errors = "" });
        //     }
        //     return Ok(new { StatusCode=StatusCodes.Status200OK, data = division.Data});
        // }

        [HttpPut]
        public async Task<IActionResult> Update(UpdateMenuCommand command)
        {
           

             await Mediator.Send(command);
          
                return Ok(new { StatusCode = StatusCodes.Status200OK, message = "Menu updated successfully", errors = "" });

        }


        [HttpDelete("{id}")]

        public async Task<IActionResult> Delete(int id)
        {
            var command = new DeleteMenuCommand { Id = id };

             await Mediator.Send(command);

                return Ok(new { StatusCode = StatusCodes.Status200OK, message = "Menu deleted successfully", errors = "" });

           

        }

        [HttpPost("bulk-upload-menu")]
        public async Task<IActionResult> UploadPreventiveSchedule(UploadMenuCommand command)
        {

            var result = await Mediator.Send(command);
            return Ok(result);
        }
            [HttpGet("by-name")]
        public async Task<IActionResult> GetMenu([FromQuery] string? name)
        {
           
            var MenuList = await Mediator.Send(new GetParentMenuQuery {SearchPattern = name});
            return Ok( new { StatusCode=StatusCodes.Status200OK, data = MenuList });
        }
    }
}