using MediatR;
using UserManagement.Application.UserFavoriteMenu.Dto;

namespace UserManagement.Application.UserFavoriteMenu.Queries.GetUserFavoriteMenus
{
    public class GetUserFavoriteMenusQuery : IRequest<List<UserFavoriteMenuDto>>
    {
    }
}
