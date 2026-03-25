using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.UserFavoriteMenu.Commands.AddUserFavoriteMenu;
using UserManagement.Application.UserFavoriteMenu.Commands.RemoveUserFavoriteMenu;
using UserManagement.Application.UserFavoriteMenu.Queries.GetUserFavoriteMenus;

namespace UserManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class UserFavoriteMenuController : ApiControllerBase
    {
        public UserFavoriteMenuController(ISender mediator) : base(mediator)
        {
        }

        [HttpGet]
        public async Task<IActionResult> GetUserFavoriteMenusAsync()
        {
            var result = await Mediator.Send(new GetUserFavoriteMenusQuery());

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> AddUserFavoriteMenu([FromBody] AddUserFavoriteMenuCommand command)
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

        [HttpDelete("{menuId}")]
        public async Task<IActionResult> RemoveUserFavoriteMenu(int menuId)
        {
            await Mediator.Send(new RemoveUserFavoriteMenuCommand(menuId));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = true,
                message = "Menu removed from favorites."
            });
        }
    }
}
