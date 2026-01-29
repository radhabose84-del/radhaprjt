using Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.Common.Interfaces.IModule
{
    public interface IModuleQueryRepository
    {
        Task<Core.Domain.Entities.Modules> GetModuleByIdAsync(int id);
        Task<(List<Core.Domain.Entities.Modules>, int)> GetAllModulesAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<List<Core.Domain.Entities.Modules>> GetModule(string searchPattern);
        Task<bool> SoftDeleteValidation(int Id); 
        Task<Core.Domain.Entities.Modules> GetModuleByNameAsync(string ModuleName);
    }
}