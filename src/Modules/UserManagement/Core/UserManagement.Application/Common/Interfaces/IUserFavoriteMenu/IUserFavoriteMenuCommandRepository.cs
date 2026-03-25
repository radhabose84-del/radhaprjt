namespace UserManagement.Application.Common.Interfaces.IUserFavoriteMenu
{
    public interface IUserFavoriteMenuCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.UserFavoriteMenu entity);
        Task<bool> HardDeleteAsync(int userId, int menuId, CancellationToken ct);
    }
}
