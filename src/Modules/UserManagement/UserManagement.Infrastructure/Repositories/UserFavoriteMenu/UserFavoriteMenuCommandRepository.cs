using Microsoft.EntityFrameworkCore;
using UserManagement.Application.Common.Interfaces.IUserFavoriteMenu;
using UserManagement.Infrastructure.Data;

namespace UserManagement.Infrastructure.Repositories.UserFavoriteMenu
{
    internal sealed class UserFavoriteMenuCommandRepository : IUserFavoriteMenuCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public UserFavoriteMenuCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> CreateAsync(Domain.Entities.UserFavoriteMenu entity)
        {
            await _dbContext.UserFavoriteMenus.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<bool> HardDeleteAsync(int userId, int menuId, CancellationToken ct)
        {
            var existing = await _dbContext.UserFavoriteMenus
                .FirstOrDefaultAsync(x => x.UserId == userId && x.MenuId == menuId, ct);

            if (existing == null)
                return false;

            _dbContext.UserFavoriteMenus.Remove(existing);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
