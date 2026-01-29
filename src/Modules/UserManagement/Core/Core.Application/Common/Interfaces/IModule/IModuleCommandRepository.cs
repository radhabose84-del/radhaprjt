using Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.Common.Interfaces.IModule
{
    public interface IModuleCommandRepository
    { 
    Task AddModuleAsync(Core.Domain.Entities.Modules module);
    Task SaveChangesAsync(); 
    Task DeleteModuleAsync(int moduleId);
    Task<bool> UpdateModuleAsync(Core.Domain.Entities.Modules module);
    }
}