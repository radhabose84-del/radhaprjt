using MediatR;

namespace UserManagement.Application.UserFavoriteMenu.Commands.RemoveUserFavoriteMenu
{
    public sealed record RemoveUserFavoriteMenuCommand(int MenuId) : IRequest<bool>;
}
