using Contracts.Common;
using MediatR;

namespace UserManagement.Application.UserFavoriteMenu.Commands.AddUserFavoriteMenu
{
    public class AddUserFavoriteMenuCommand : IRequest<ApiResponseDTO<int>>
    {
        public int MenuId { get; set; }
    }
}
