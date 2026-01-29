using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Infrastructure.Data;
using Core.Application.Common.Interfaces.IFinancialYear;
using Microsoft.EntityFrameworkCore;
using Core.Domain.Entities;


namespace UserManagement.Infrastructure.Repositories.FinancialYear
{
    public class FinancialYearCommandRepository :IFinancialYearCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        public FinancialYearCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;

        }
        public async Task<Core.Domain.Entities.FinancialYear> CreateAsync(Core.Domain.Entities.FinancialYear financialYear)
         {
            await _applicationDbContext.FinancialYear.AddAsync(financialYear);
            await _applicationDbContext.SaveChangesAsync();
            return financialYear;
        }

          public async Task<int> UpdateAsync(int id, Core.Domain.Entities.FinancialYear financialYear)
            {  

              var financialYearToUpdate = await _applicationDbContext.FinancialYear.FirstOrDefaultAsync(u => u.Id == id);
               // If the entity does not exist, throw a CustomException
                if (financialYearToUpdate == null)
                {
                    return -1; //indicate failure
                }
            
                  financialYearToUpdate.StartYear = financialYear.StartYear;
                  financialYearToUpdate.StartDate = financialYear.StartDate;
                  financialYearToUpdate.EndDate = financialYear.EndDate;
                  financialYearToUpdate.FinYearName = financialYear.FinYearName;
                  financialYearToUpdate.IsActive = financialYear.IsActive;

                  _applicationDbContext.FinancialYear.Update(financialYearToUpdate);
                   await _applicationDbContext.SaveChangesAsync();             
                   return 1; // Indicate success
            }

              public async Task<int> DeleteAsync(int id ,Core.Domain.Entities.FinancialYear financialYear )
    {
        
            var financialYearToDelete = await _applicationDbContext.FinancialYear.FirstOrDefaultAsync(u => u.Id == id);
            if (financialYearToDelete != null)
            {
               
                financialYearToDelete.IsDeleted = financialYear.IsDeleted;
                return await _applicationDbContext.SaveChangesAsync();
            }
            return 0; // No user found
    }
   

      
    }
}