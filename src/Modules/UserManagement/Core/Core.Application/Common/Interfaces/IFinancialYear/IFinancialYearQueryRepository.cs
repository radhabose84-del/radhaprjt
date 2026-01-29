using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Entities;


namespace Core.Application.Common.Interfaces.IFinancialYear
{
    public interface IFinancialYearQueryRepository
    {
        Task<(List<Core.Domain.Entities.FinancialYear>,int)> GetAllFinancialYearAsync(int PageNumber, int PageSize, string? SearchTerm);  
        Task<Core.Domain.Entities.FinancialYear> GetByIdAsync(int id);  
         Task<List<Core.Domain.Entities.FinancialYear>> GetAllFinancialAutoCompleteSearchAsync(string SearchFinanceyear);                
    
    }
}