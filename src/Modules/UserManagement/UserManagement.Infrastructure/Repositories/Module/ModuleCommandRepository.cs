using Microsoft.EntityFrameworkCore;
using UserManagement.Infrastructure.Data;
using UserManagement.Domain.Entities;
using UserManagement.Application.Common.Interfaces.IModule;

namespace UserManagement.Infrastructure.Repositories.Module
{
    public class ModuleCommandRepository : IModuleCommandRepository
    {
    private readonly ApplicationDbContext _applicationDbContext;

    public ModuleCommandRepository(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }   

    public async Task AddModuleAsync(Modules module)
    {
       // Check for existing module name before adding
            bool moduleExists = await _applicationDbContext.Modules
                .AnyAsync(m => m.ModuleName == module.ModuleName && !m.IsDeleted);

            if (moduleExists)
            {
                throw new InvalidOperationException($"A module with the name '{module.ModuleName}' already exists.");
            }

            await _applicationDbContext.Modules.AddAsync(module);
    }

    public async Task SaveChangesAsync()
    {
        await _applicationDbContext.SaveChangesAsync();
    }

        public async Task<bool> UpdateModuleAsync(Modules module)
        {
            var existingModule = await _applicationDbContext.Modules.FirstOrDefaultAsync(m => m.Id == module.Id);

            if (existingModule != null)
            {
                existingModule.ModuleName = module.ModuleName;
                return await _applicationDbContext.SaveChangesAsync() > 0;
                // Update menus
                // foreach (var menu in module.Menus)
                // {
                //     var existingMenu = existingModule.Menus.FirstOrDefault(m => m.Id == menu.Id);
                //     if (existingMenu != null)
                //     {
                //         existingMenu.MenuName = menu.MenuName;
                //     }
                // }
            }
        return false;
    }

    public async Task DeleteModuleAsync(int moduleId)
    {
        var existingModule = await _applicationDbContext.Modules.FirstOrDefaultAsync(u => u.Id == moduleId);
            if (existingModule != null)
            {
                existingModule.IsDeleted = true;
                 await _applicationDbContext.SaveChangesAsync();
            }
            
    }

    }
}