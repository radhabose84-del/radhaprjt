using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.Common.Interfaces.IPasswordComplexityRule
{
    public interface IPasswordComplexityRuleCommandRepository
    {
        
      Task<Core.Domain.Entities.PasswordComplexityRule> CreateAsync(Core.Domain.Entities.PasswordComplexityRule passwordComplexityRule);
      Task<int> UpdateAsync(int id, Core.Domain.Entities.PasswordComplexityRule passwordComplexityRule);
      Task<int> DeleteAsync(int id, Core.Domain.Entities.PasswordComplexityRule passwordComplexityRule);


       Task<bool> ExistsByCodeAsync(string PasswordComplexityRule);



    }
}