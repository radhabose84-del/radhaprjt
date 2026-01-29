using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Infrastructure.Data;
using Core.Application.Common.Interfaces.IPasswordComplexityRule;
using Microsoft.EntityFrameworkCore;

namespace UserManagement.Infrastructure.Repositories.PasswordComplexityRule
{
    public class PasswordComplexityRuleCommandRepository  :IPasswordComplexityRuleCommandRepository
    {
  
      private readonly ApplicationDbContext _applicationDbContext;


      public PasswordComplexityRuleCommandRepository (ApplicationDbContext applicationDbContext)
      {
          _applicationDbContext = applicationDbContext;
      }

      public async  Task<Core.Domain.Entities.PasswordComplexityRule> CreateAsync(Core.Domain.Entities.PasswordComplexityRule passwordComplexityRule)
      {
          await _applicationDbContext.PasswordComplexityRule.AddAsync(passwordComplexityRule);
          await _applicationDbContext.SaveChangesAsync();
          return passwordComplexityRule; 
      }

       public async Task<int>UpdateAsync(int id, Core.Domain.Entities.PasswordComplexityRule passwordComplexityRule)
        {
            var existingpwdcomrule  = await _applicationDbContext.PasswordComplexityRule.FirstOrDefaultAsync(p => p.Id == id);
            if (existingpwdcomrule  != null)
            {
                existingpwdcomrule.PwdComplexityRule = passwordComplexityRule.PwdComplexityRule;     
                existingpwdcomrule.IsActive = passwordComplexityRule.IsActive;                                

                _applicationDbContext.PasswordComplexityRule.Update(existingpwdcomrule);
                return await _applicationDbContext.SaveChangesAsync();
            }
            return 0; // No id found
         }

          public async Task<int> DeleteAsync(int id ,Core.Domain.Entities.PasswordComplexityRule pwdcomplexityrule )
           {                        

            var PwdcomplexityruleToDelete = await _applicationDbContext.PasswordComplexityRule.FirstOrDefaultAsync(u => u.Id == id);
            
            if (PwdcomplexityruleToDelete != null)
            {   
                 PwdcomplexityruleToDelete.IsDeleted = pwdcomplexityrule.IsDeleted;
                 await _applicationDbContext.SaveChangesAsync();
                 return 1;
            }
            return 0; 
           }


             public Task<bool> ExistsByCodeAsync(string PasswordComplexityRule)
        {
        
            return _applicationDbContext.PasswordComplexityRule.AnyAsync(c => c.PwdComplexityRule == PasswordComplexityRule);
            
        }

    }
}