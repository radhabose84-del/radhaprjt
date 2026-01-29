using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Entities;
using System.Text;

namespace Core.Application.Common.Interfaces.IDivision
{
    public interface IDivisionQueryRepository
    {
        Task<(List<Division>,int)> GetAllDivisionAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<Division> GetByIdAsync(int id);
        Task<List<Division>> GetDivision(string searchPattern);
        Task<Division?> GetByDivisionnameAsync(string name,int? id = null);
        Task<bool> SoftDeleteValidation(int Id); 
        Task<bool> FKColumnExistValidation(int Id);
        Task<List<Division>> GetDivision_SuperAdmin(string searchPattern);
    }
}