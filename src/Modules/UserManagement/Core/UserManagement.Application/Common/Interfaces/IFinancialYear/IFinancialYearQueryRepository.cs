using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Domain.Entities;


namespace UserManagement.Application.Common.Interfaces.IFinancialYear
{
    public interface IFinancialYearQueryRepository
    {
        Task<(List<UserManagement.Domain.Entities.FinancialYear>,int)> GetAllFinancialYearAsync(int PageNumber, int PageSize, string? SearchTerm);  
        Task<UserManagement.Domain.Entities.FinancialYear> GetByIdAsync(int id);  
         Task<List<UserManagement.Domain.Entities.FinancialYear>> GetAllFinancialAutoCompleteSearchAsync(string SearchFinanceyear);                
    
    }
}