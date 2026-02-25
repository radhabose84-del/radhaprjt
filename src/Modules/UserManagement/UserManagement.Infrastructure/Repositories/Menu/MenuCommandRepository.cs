using UserManagement.Application.Common.Interfaces.IMenu;
using Microsoft.EntityFrameworkCore;
using UserManagement.Infrastructure.Data;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Infrastructure.Repositories.Menu
{
    public class MenuCommandRepository : IMenuCommand
    {
        private readonly ApplicationDbContext _applicationDbContext;
        public MenuCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<bool> BulkImportMenuAsync(List<UserManagement.Domain.Entities.Menu> menus)
        {
            await _applicationDbContext.Menus.AddRangeAsync(menus);
            await _applicationDbContext.SaveChangesAsync();
            return true;
        }

        public async Task<int> CreateAsync(UserManagement.Domain.Entities.Menu menu)
        {
            await _applicationDbContext.Menus.AddAsync(menu);
            await _applicationDbContext.SaveChangesAsync();
            return menu.Id;
        }

        public async Task<bool> DeleteAsync(int id, UserManagement.Domain.Entities.Menu menu)
        {
            var existingMenu = await _applicationDbContext.Menus.FirstOrDefaultAsync(u => u.Id == id);
            if (existingMenu != null)
            {
                existingMenu.IsDeleted = IsDelete.Deleted;
                return await _applicationDbContext.SaveChangesAsync() >0;
            }
            return false; 
        }

        public async Task<bool> UpdateAsync(UserManagement.Domain.Entities.Menu menu)
        {
            var existingMenu = await _applicationDbContext.Menus.FirstOrDefaultAsync(u => u.Id == menu.Id);
            if (existingMenu != null)
            {
                existingMenu.MenuName = menu.MenuName;
                existingMenu.ModuleId = menu.ModuleId;
                existingMenu.MenuIcon = menu.MenuIcon;
                existingMenu.MenuUrl = menu.MenuUrl;
                existingMenu.ParentId = menu.ParentId;
                existingMenu.SortOrder = menu.SortOrder;
                existingMenu.MenuIcon = menu.MenuIcon;
                existingMenu.IsActive = menu.IsActive;
                existingMenu.Type = menu.Type;

                _applicationDbContext.Menus.Update(existingMenu);
                return await _applicationDbContext.SaveChangesAsync() > 0;
            }
            return false;
        }
    }
}