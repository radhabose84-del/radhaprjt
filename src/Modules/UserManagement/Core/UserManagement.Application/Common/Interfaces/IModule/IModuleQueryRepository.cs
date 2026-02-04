using UserManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserManagement.Application.Common.Interfaces.IModule
{
    public interface IModuleQueryRepository
    {
        Task<UserManagement.Domain.Entities.Modules> GetModuleByIdAsync(int id);
        Task<(List<UserManagement.Domain.Entities.Modules>, int)> GetAllModulesAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<List<UserManagement.Domain.Entities.Modules>> GetModule(string searchPattern);
        Task<bool> SoftDeleteValidation(int Id); 
        Task<UserManagement.Domain.Entities.Modules> GetModuleByNameAsync(string ModuleName);
    }
}