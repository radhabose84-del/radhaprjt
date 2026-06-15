using UserManagement.Infrastructure.Data;
using UserManagement.Application.Common.Interfaces.IIconMaster;
using Microsoft.EntityFrameworkCore;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Infrastructure.Repositories.IconMaster
{
    public class IconMasterCommandRepository : IIconMasterCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public IconMasterCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<int> CreateAsync(UserManagement.Domain.Entities.IconMaster iconMaster)
        {
            await _applicationDbContext.IconMasters.AddAsync(iconMaster);
            await _applicationDbContext.SaveChangesAsync();
            return iconMaster.Id;
        }

        public async Task<int> UpdateAsync(int id, UserManagement.Domain.Entities.IconMaster iconMaster)
        {
            var existing = await _applicationDbContext.IconMasters.FirstOrDefaultAsync(u => u.Id == id);
            if (existing == null)
            {
                return -1;
            }

            // Keyword is immutable — never updated.
            // IsActive is intentionally NOT touched — an icon edit must never change
            // active state (admin page does not manage active/inactive).
            existing.IconName = iconMaster.IconName;
            existing.IconLibrary = iconMaster.IconLibrary;
            existing.Size = iconMaster.Size;
            existing.Style = iconMaster.Style;

            _applicationDbContext.IconMasters.Update(existing);
            await _applicationDbContext.SaveChangesAsync();
            return 1;
        }

        public async Task<bool> ExistsByKeywordAsync(string keyword)
        {
            // Only live rows reserve a keyword — a soft-deleted keyword can be reused
            return await _applicationDbContext.IconMasters
                .AnyAsync(c => c.Keyword == keyword && c.IsDeleted == IsDelete.NotDeleted);
        }

        public async Task<int> DeleteIconMasterAsync(int id, UserManagement.Domain.Entities.IconMaster iconMaster)
        {
            var existing = await _applicationDbContext.IconMasters.FirstOrDefaultAsync(u => u.Id == id);
            if (existing is null)
            {
                return -1;
            }

            existing.IsDeleted = iconMaster.IsDeleted;
            _applicationDbContext.IconMasters.Update(existing);
            await _applicationDbContext.SaveChangesAsync();
            return 1;
        }
    }
}
