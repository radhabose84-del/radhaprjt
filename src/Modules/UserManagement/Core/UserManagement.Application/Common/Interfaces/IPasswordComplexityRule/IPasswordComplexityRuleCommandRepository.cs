using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserManagement.Application.Common.Interfaces.IPasswordComplexityRule
{
    public interface IPasswordComplexityRuleCommandRepository
    {
        
      Task<UserManagement.Domain.Entities.PasswordComplexityRule> CreateAsync(UserManagement.Domain.Entities.PasswordComplexityRule passwordComplexityRule);
      Task<int> UpdateAsync(int id, UserManagement.Domain.Entities.PasswordComplexityRule passwordComplexityRule);
      Task<int> DeleteAsync(int id, UserManagement.Domain.Entities.PasswordComplexityRule passwordComplexityRule);


       Task<bool> ExistsByCodeAsync(string PasswordComplexityRule);



    }
}