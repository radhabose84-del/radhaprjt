using Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.Common.Interfaces.IUserRole
{
    using UserRole = Core.Domain.Entities.UserRole;
    public interface IUserRoleQueryRepository
    {
        
       // Task<List<UserRole>> GetAllRoleAsync();

        Task<(List<UserRole>,int)> GetAllRoleAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<UserRole?> GetByIdAsync(int id);
        Task<List<UserRole>> GetRolesAsync(string searchTerm); 
        Task<bool> SoftDeleteValidation(int Id); 
        Task<bool> FKColumnExistValidation(int Id);
        Task<List<UserRole>> GetRoles_SuperAdmin(string searchTerm); 
        
        
         
      
    }
}