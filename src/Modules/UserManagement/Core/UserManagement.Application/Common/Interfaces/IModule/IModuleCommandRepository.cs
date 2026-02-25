namespace UserManagement.Application.Common.Interfaces.IModule
{
    public interface IModuleCommandRepository
    { 
    Task AddModuleAsync(UserManagement.Domain.Entities.Modules module);
    Task SaveChangesAsync(); 
    Task DeleteModuleAsync(int moduleId);
    Task<bool> UpdateModuleAsync(UserManagement.Domain.Entities.Modules module);
    }
}