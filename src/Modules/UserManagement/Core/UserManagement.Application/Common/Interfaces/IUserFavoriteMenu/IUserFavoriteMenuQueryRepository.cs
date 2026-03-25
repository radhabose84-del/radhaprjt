using UserManagement.Application.UserFavoriteMenu.Dto;

namespace UserManagement.Application.Common.Interfaces.IUserFavoriteMenu
{
    public interface IUserFavoriteMenuQueryRepository
    {
        Task<List<UserFavoriteMenuDto>> GetByUserIdAsync(int userId);
        Task<bool> AlreadyFavoritedAsync(int userId, int menuId);
        Task<bool> MenuExistsAndActiveAsync(int menuId);
        Task<bool> FavoriteExistsAsync(int userId, int menuId);
    }
}
