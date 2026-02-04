using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserManagement.Application.Common.Interfaces.IPasswordComplexityRule
{
    public interface IPasswordComplexityRuleQueryRepository 
    {
        
     
       Task<(List<UserManagement.Domain.Entities.PasswordComplexityRule>,int)> GetPasswordComplexityAsync(int PageNumber, int PageSize, string? SearchTerm);
      Task<UserManagement.Domain.Entities.PasswordComplexityRule> GetByIdAsync(int id);  

      Task<List<UserManagement.Domain.Entities.PasswordComplexityRule>> GetpwdautocompleteAsync(string searchTerm); 


      

    }
}