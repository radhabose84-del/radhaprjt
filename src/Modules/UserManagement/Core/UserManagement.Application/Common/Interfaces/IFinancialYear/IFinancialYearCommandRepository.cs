using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserManagement.Application.Common.Interfaces.IFinancialYear
{
    public interface IFinancialYearCommandRepository
    {
        Task<UserManagement.Domain.Entities.FinancialYear> CreateAsync(UserManagement.Domain.Entities.FinancialYear financialYear);
        Task<int> UpdateAsync(int id, UserManagement.Domain.Entities.FinancialYear financialYear);

         Task<int> DeleteAsync(int id,UserManagement.Domain.Entities.FinancialYear financialYear);

      
      
    }
}